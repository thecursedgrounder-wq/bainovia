using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Editorial tooling that assembles the playable Bainovia prototype scene,
/// wires up all game scripts, bakes the NavMesh, and produces a Windows build.
/// </summary>
public static class BainoviaSceneBuilder
{
    static readonly string ScenePath = "Assets/Scenes/BainoviaPrototype.unity";
    static readonly string BuildDir = "Builds/Windows";
    static readonly string BuildExe = "Bainovia.exe";

    // Dialogue UI refs captured by CreateUI and bound to the DialogueSystem
    // by WireDialogueUI after the DialogueManager exists (wiring order bug: CreateUI
    // otherwise runs before CreateSystems and silently binds nothing).
    static GameObject uiDialogueSpeaker;
    static GameObject uiDialogueBody;
    static GameObject uiDialogueHint;
    static GameObject[] uiDialogueOptions;

    public static void FullBuild()
    {
        try
        {
            // Ensure URSIAN model+anim import as Generic BEFORE any scene code
            // loads them, so FitModel/validation see the true skinned bounds.
            ConfigureWerebearImports();

            BuildPrototypeScene();

            var dir = Path.GetDirectoryName(ScenePath);
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(dir) ?? "Assets", Path.GetFileName(dir));

            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Directory.Exists(BuildDir))
                Directory.CreateDirectory(BuildDir);

            var report = BuildPipeline.BuildPlayer(
                new[] { ScenePath },
                Path.Combine(BuildDir, BuildExe),
                BuildTarget.StandaloneWindows64,
                BuildOptions.None);

            Debug.Log("Build result: " + report.summary.result);
            Debug.Log("Total errors: " + report.summary.totalErrors);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("BUILD FAILED");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("BAINOVIA BUILD SUCCEEDED -> " + Path.GetFullPath(Path.Combine(BuildDir, BuildExe)));
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    [MenuItem("Bainovia/Build Prototype Scene + Windows Build")]
    public static void FullBuildMenu()
    {
        FullBuild();
    }

    [MenuItem("Bainovia/Validate 3D Models")]
    public static void ValidateModels()
    {
        string[] models =
        {
            "kaykit/knight.glb", "kaykit/mage.glb", "kaykit/barbarian.glb",
            "kaykit/skeleton_rogue.glb", "kaykit/pillar.glb", "kaykit/coin.glb",
            "kenney/wolf.glb", "quaternius/crystal_small.glb", "kenney/bear.glb",
            "ursian/UNITY CHAR.fbx",
            "decor/campfire_stones.glb", "decor/tent.glb", "decor/log_stack.glb",
            "decor/barrel.glb", "decor/torch_lit.glb", "decor/shrine.glb",
            "decor/shrine_candles.glb", "decor/statue_obelisk.glb",
            "decor/statue_column_damaged.glb", "decor/arch_gothic.glb",
            "decor/column_broken.glb", "decor/rock_large_c.glb",
            "decor/rock_small_b.glb", "decor/tree_pine_round_a.glb",
            "decor/tree_default_dark.glb", "decor/tree_blocks.glb",
            "decor/grass.glb", "decor/flower_yellow_a.glb", "decor/flower_purple_a.glb",
            "local/well.gltf", "local/medieval_houses.gltf"
        };
        int failed = 0;
        foreach (var m in models)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(m));
            if (prefab == null)
            {
                Debug.LogError("MODEL MISSING: " + m);
                failed++;
                continue;
            }
            var renderers = CollectRenderers(prefab);
            if (renderers.Length == 0)
            {
                Debug.LogError("MODEL NO RENDERERS: " + m);
                failed++;
                continue;
            }
            var b = CombineBounds(renderers);
            Debug.Log($"MODEL OK: {m} size=({b.size.x:F2}, {b.size.y:F2}, {b.size.z:F2}) renderers={renderers.Length}");
        }
        Debug.Log(failed == 0 ? "BAINOVIA MODELS OK" : $"BAINOVIA MODELS FAILED: {failed}");
        EditorApplication.Exit(failed == 0 ? 0 : 1);
    }

    [MenuItem("Bainovia/Build Prototype Scene Only")]
    public static void BuildSceneOnly()
    {
        try
        {
            BuildPrototypeScene();

            var dir = Path.GetDirectoryName(ScenePath);
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(dir) ?? "Assets", Path.GetFileName(dir));

            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            Debug.Log("Scene saved: " + ScenePath);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    static void WireDialogueUI()
    {
        var dm = GameObject.Find("DialogueManager");
        if (dm == null)
        {
            Debug.LogWarning("WireDialogueUI: DialogueManager not found - dialogue UI left unbound.");
            return;
        }

        var dialogue = dm.GetComponent<DialogueSystem>();
        if (dialogue == null) return;

        var so = new SerializedObject(dialogue);
        if (uiDialogueSpeaker != null) so.FindProperty("speakerText").objectReferenceValue = uiDialogueSpeaker;
        if (uiDialogueBody != null) so.FindProperty("dialogueText").objectReferenceValue = uiDialogueBody;
        if (uiDialogueHint != null) so.FindProperty("hintText").objectReferenceValue = uiDialogueHint;
        var optProp = so.FindProperty("optionTexts");
        int optionCount = uiDialogueOptions != null ? uiDialogueOptions.Length : 0;
        optProp.arraySize = optionCount;
        for (int i = 0; i < optionCount; i++)
            optProp.GetArrayElementAtIndex(i).objectReferenceValue = uiDialogueOptions[i];
        so.ApplyModifiedProperties();

        Debug.Log($"Wired dialogue UI: speaker={uiDialogueSpeaker != null} body={uiDialogueBody != null} hint={uiDialogueHint != null} options={optionCount}");
    }

    static void BuildPrototypeScene()
    {
        EnsureLayersAndTags();
        CreateAnimatorControllers();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var ground = CreateGround();
        var light = CreateLighting();
        CreateAtmosphere();
        CreateDecor();
        BuildGroundDetail();

        var navBaker = new GameObject("NavMeshBaker");
        navBaker.AddComponent<PrototypeNavMeshBaker>();

        var player = CreatePlayer();
        CreateEnemies();
        CreateBoss();
        CreateSanctuary(player);
        var ui = CreateUI();
        CreateSystems(player, null, ui);
        CreateNPC(player, ui);
        CreateItemPickup();
        WireDialogueUI();

        // Bake references UI -> player systems
        var gameUI = ui.GetComponent<GameUI>();
        if (gameUI != null)
        {
            if (gameUI.player == null) gameUI.player = player.GetComponent<BainoviaCharacterController>();
            if (gameUI.runeSystem == null) gameUI.runeSystem = player.GetComponent<RuneMagicSystem>();
            var qm = GameObject.Find("QuestManager");
            if (qm != null) gameUI.questSystem = qm.GetComponent<QuestSystem>();
        }

        // Ensure default light/camera duplicates are gone
        var allCameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        foreach (var c in allCameras)
        {
            if (c.GetComponentInParent<BainoviaCharacterController>() == null && c != null && c.tag != "MainCamera")
            {
                Object.DestroyImmediate(c.gameObject);
            }
        }
        foreach (var o in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
        {
            if (o == null) continue;
            if (o.name == "Main Camera" && o.GetComponent<CameraController>() == null)
            {
                Object.DestroyImmediate(o);
                continue;
            }
            if (o.name == "Directional Light" && o.GetComponent<Light>() != null && o.transform.parent == null && o.name == "Directional Light")
            {
                // if our lighting was created differently, skip; otherwise keep default
            }
        }
    }

    // ---------------------------------------------------------------- layers

    static void EnsureLayersAndTags()
    {
        var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (asset == null || asset.Length == 0) return;

        var so = new SerializedObject(asset[0]);

        var layers = so.FindProperty("layers");
        layers.GetArrayElementAtIndex(6).stringValue = "Player";
        layers.GetArrayElementAtIndex(7).stringValue = "Enemy";
        layers.GetArrayElementAtIndex(8).stringValue = "Interactable";
        layers.GetArrayElementAtIndex(9).stringValue = "Environment";

        var tags = so.FindProperty("tags");
        string[] wanted = { "Enemy", "NPC", "Interactable", "QuestItem" };
        foreach (var w in wanted)
        {
            bool exists = false;
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == w) { exists = true; break; }
            }
            if (!exists)
            {
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = w;
            }
        }

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ---------------------------------------------------------------- animators

    static void CreateAnimatorControllers()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Animators"))
            AssetDatabase.CreateFolder("Assets", "Animators");

        var pPath = "Assets/Animators/PlayerAnimator.controller";
        if (!File.Exists(pPath))
        {
            var pc = AnimatorController.CreateAnimatorControllerAtPath(pPath);
            pc.AddParameter("Speed", AnimatorControllerParameterType.Float);
            pc.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            pc.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            pc.AddParameter("HeavyAttack", AnimatorControllerParameterType.Trigger);
            pc.AddParameter("IsBerserk", AnimatorControllerParameterType.Bool);
            pc.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
            pc.AddParameter("Horizontal", AnimatorControllerParameterType.Float);
            pc.AddParameter("Vertical", AnimatorControllerParameterType.Float);
            var idle = pc.layers[0].stateMachine.AddState("Idle");
            pc.layers[0].stateMachine.defaultState = idle;
        }

        var ePath = "Assets/Animators/EnemyAnimator.controller";
        if (!File.Exists(ePath))
        {
            var ec = AnimatorController.CreateAnimatorControllerAtPath(ePath);
            ec.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
            ec.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
            ec.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
            ec.AddParameter("IsStunned", AnimatorControllerParameterType.Bool);
            ec.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
            ec.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            var idle = ec.layers[0].stateMachine.AddState("Idle");
            ec.layers[0].stateMachine.defaultState = idle;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ---------------------------------------------------------------- helpers

    static Shader PickShader()
    {
        var s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Legacy Shaders/Diffuse");
        return s;
    }

    static Material MakeMat(string name, Color c)
    {
        var m = new Material(PickShader());
        m.name = name;
        m.color = c;
        return m;
    }

    // ---------------------------------------------------------------- 3d models

    static readonly string ModelRoot = "Assets/Models";
    static string ModelPath(string file) => ModelRoot + "/" + file;

    /// <summary>
    /// Instantiates a gltfast-imported GLB model as a child of <paramref name="parent"/>,
    /// normalizes it to the requested height (or width when fitToWidth) and plants its
    /// feet at the parent's local Y=0. Returns null when the model asset is missing so
    /// callers can fall back to a placeholder primitive.
    /// </summary>
    static GameObject AttachModel(GameObject parent, string childName, string modelFile, float targetSize, bool fitToWidth)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(modelFile));
        if (prefab == null)
        {
            Debug.LogWarning($"{childName}: model not found at {ModelPath(modelFile)} - using placeholder.");
            return null;
        }
        var inst = Object.Instantiate(prefab);
        inst.name = childName;
        inst.transform.SetParent(parent.transform, false);
        FitModel(inst, targetSize, fitToWidth);
        return inst;
    }

    static void FitModel(GameObject model, float targetSize, bool fitToWidth)
    {
        var renderers = CollectRenderers(model);
        if (renderers == null || renderers.Length == 0) return;
        var b = CombineBounds(renderers);
        float current = fitToWidth ? b.size.x : b.size.y;
        if (current < 0.0001f) return;
        float k = targetSize / current;
        model.transform.localScale = new Vector3(k, k, k);
        var after = CombineBounds(renderers);
        model.transform.localPosition = new Vector3(0f, -after.min.y, 0f);
    }

    static Bounds ModelLocalBounds(GameObject model)
    {
        var renderers = CollectRenderers(model);
        return renderers != null && renderers.Length > 0 ? CombineBounds(renderers) : new Bounds();
    }

    /// <summary>
    /// Gathers every renderer that contributes visible geometry. Plain GLB/primitive
    /// models expose MeshRenderer, whereas rigged character FBX (e.g. URSIAN) expose
    /// SkinnedMeshRenderer — both must participate in bounds fitting.
    /// </summary>
    static Renderer[] CollectRenderers(GameObject model)
    {
        List<Renderer> list = new List<Renderer>();
        foreach (var r in model.GetComponentsInChildren<MeshRenderer>(true))
            list.Add(r);
        foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            list.Add(r);
        return list.ToArray();
    }

    static Bounds CombineBounds(Renderer[] renderers)
    {
        var b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    /// <summary>
    /// Adds a BoxCollider to <paramref name="root"/> covering the given (world) bounds so
    /// AIM/AOE detection and the interaction raycast can still hit objects that now render
    /// GLB models instead of collider-bearing primitives.
    /// </summary>
    static Collider AddColliderFromBounds(GameObject root, Bounds b)
    {
        var center = root.transform.InverseTransformPoint(b.center);
        var size = root.transform.InverseTransformVector(b.size);
        var col = root.AddComponent<BoxCollider>();
        col.center = center;
        col.size = new Vector3(Mathf.Max(0.05f, Mathf.Abs(size.x)), Mathf.Max(0.05f, Mathf.Abs(size.y)), Mathf.Max(0.05f, Mathf.Abs(size.z)));
        return col;
    }

    /// <summary>
    /// Drops any colliders cloned from GLB imports / primitive fallbacks so a single
    /// purpose-built collider on the root object is the only physics surface.
    /// </summary>
    static void DestroyChildColliders(GameObject model)
    {
        foreach (var c in model.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(c);
    }

    static void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 anchoredPos, Vector2 size)
    {
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }

    static Text MakeText(RectTransform parent, string name, string txt, int fontSize, Color color, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, TextAnchor align = TextAnchor.MiddleCenter)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        SetRect(rt, aMin, aMax, pos, size);
        var t = go.GetComponent<Text>();
        t.text = txt;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null) t.font = font;
        return t;
    }

    static Image MakeImage(RectTransform parent, string name, Color color, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        SetRect(rt, aMin, aMax, pos, size);
        var im = go.GetComponent<Image>();
        im.color = color;
        return im;
    }

    static RectTransform MakeStretch(RectTransform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    static Slider MakeSlider(RectTransform parent, string name, Color fillColor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        var slider = go.GetComponent<Slider>();

        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        var bgr = (RectTransform)bg.transform;
        bgr.SetParent(rt, false);
        SetRect(bgr, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bgr.offsetMin = Vector2.zero;
        bgr.offsetMax = Vector2.zero;
        bgr.gameObject.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        var fr = (RectTransform)fill.transform;
        fr.SetParent(rt, false);
        SetRect(fr, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        fr.offsetMin = new Vector2(2f, 2f);
        fr.offsetMax = new Vector2(-2f, -2f);
        var fi = fill.GetComponent<Image>();
        fi.color = fillColor;
        fi.type = Image.Type.Filled;
        fi.fillMethod = Image.FillMethod.Horizontal;
        fi.fillOrigin = 0;

        slider.fillRect = fr;
        slider.targetGraphic = bgr.gameObject.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = false;

        // backing store so slider.value reflects fill
        slider.value = 1f;
        return slider;
    }

    // ---------------------------------------------------------------- world

    static GameObject CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        var mr = ground.GetComponent<MeshRenderer>();
        mr.sharedMaterial = MakeMat("GroundMat", new Color(0.80f, 0.84f, 0.92f, 1f));
        ground.isStatic = true;
        GroundNavStatic(ground);
        return ground;
    }

    static void GroundNavStatic(GameObject ground)
    {
        try
        {
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.BatchingStatic);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("GroundNavStatic: " + e.Message);
        }
    }

    /// <summary>
    /// Breaks up the flat snow sheet with wind-swept drifts and a worn path to
    /// the rest camp, so the floor reads as terrain instead of a single quad.
    /// Drifts are flattened spheres: cheap, smooth, and snow-lit by the moon.
    /// </summary>
    static void BuildGroundDetail()
    {
        var driftMat = MakeMat("SnowDriftMat", new Color(0.84f, 0.88f, 0.95f, 1f));
        driftMat.SetFloat("_Metallic", 0f);
        driftMat.SetFloat("_Glossiness", 0.12f);

        var rng = new System.Random(20240917);

        // Broad drifts across the play floor.
        for (int i = 0; i < 26; i++)
        {
            double ang = rng.NextDouble() * Mathf.PI * 2f;
            float rad = 8f + (float)rng.NextDouble() * 36f;
            float sx = 4f + (float)rng.NextDouble() * 9f;
            float sy = 0.5f + (float)rng.NextDouble() * 1.7f;
            float sz = 3.5f + (float)rng.NextDouble() * 8f;
            PlaceDrift("Drift" + i,
                new Vector3(Mathf.Cos((float)ang) * rad, 0f, Mathf.Sin((float)ang) * rad),
                new Vector3(sx, sy, sz), (float)rng.NextDouble() * 360f, driftMat);
        }

        // Small dusting shelves hugging the crag bases.
        for (int i = 0; i < 22; i++)
        {
            double ang = rng.NextDouble() * Mathf.PI * 2f;
            float rad = 40f + (float)rng.NextDouble() * 22f;
            float sy = 0.25f + (float)rng.NextDouble() * 0.6f;
            PlaceDrift("DriftFoot" + i,
                new Vector3(Mathf.Cos((float)ang) * rad, 0f, Mathf.Sin((float)ang) * rad),
                new Vector3(2.2f + (float)rng.NextDouble() * 4.5f, sy, 1.6f + (float)rng.NextDouble() * 3f),
                (float)rng.NextDouble() * 360f, driftMat);
        }

        // Taller lee drifts flanking the beaten path.
        PlaceDrift("LeeDriftA", new Vector3(1.6f, 0f, 4.2f), new Vector3(3.4f, 1.5f, 2.6f), 20f, driftMat);
        PlaceDrift("LeeDriftB", new Vector3(5.6f, 0f, 7.6f), new Vector3(3.8f, 1.8f, 2.4f), -25f, driftMat);

        BuildSnowPath();
    }

    static void PlaceDrift(string name, Vector3 pos, Vector3 scale, float yaw, Material mat)
    {
        var drift = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        drift.name = name;
        drift.transform.position = new Vector3(pos.x, scale.y * 0.5f, pos.z);
        // Widen the slope: stretch the X/Z a touch under the cap for a mound curve.
        drift.transform.localScale = new Vector3(scale.x * 1.3f, scale.y, scale.z * 1.3f);
        drift.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        var mr = drift.GetComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        Object.DestroyImmediate(drift.GetComponent<Collider>());
        drift.isStatic = true;
    }

    static void BuildSnowPath()
    {
        var pathMat = MakeMat("SnowPathMat", new Color(0.57f, 0.59f, 0.64f, 1f));
        pathMat.SetFloat("_Metallic", 0f);
        pathMat.SetFloat("_Glossiness", 0.28f);

        // Beaten trail from the spawn apron to the campfire.
        PlacePathTile("PathToCamp", new Vector3(3.5f, 0.06f, 3.3f), Quaternion.Euler(0f, 42.5f, 0f), new Vector3(2.0f, 0.12f, 13f), pathMat);
        // Trampled spur on toward the Restless Shrine.
        PlacePathTile("PathToShrine", new Vector3(10.2f, 0.06f, 5.4f), Quaternion.Euler(0f, -9.5f, 0f), new Vector3(1.6f, 0.12f, 7f), pathMat);
    }

    static void PlacePathTile(string name, Vector3 pos, Quaternion rot, Vector3 scale, Material mat)
    {
        var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = name;
        tile.transform.position = pos;
        tile.transform.localRotation = rot;
        tile.transform.localScale = scale;
        var mr = tile.GetComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        Object.DestroyImmediate(tile.GetComponent<Collider>());
        tile.isStatic = true;
    }

    static GameObject CreateLighting()
    {
        // Bainovia North: perpetual storm night. Distant cold moon so the snow
        // reads blue, with a faint aurora-coloured fill from the rear.
        var moonGO = new GameObject("Moon");
        var moon = moonGO.AddComponent<Light>();
        moon.type = LightType.Directional;
        // Behind-left of the hero camera so the snow plane is front-lit, not
        // side-lit into a flat fog wash.
        moonGO.transform.rotation = Quaternion.Euler(65f, -160f, 0f);
        moon.intensity = 0.72f;
        moon.color = new Color(0.72f, 0.74f, 0.9f);

        var fillGO = new GameObject("AuroraFill");
        var fill = fillGO.AddComponent<Light>();
        fill.type = LightType.Directional;
        fillGO.transform.rotation = Quaternion.Euler(30f, 20f, 0f);
        fill.intensity = 0.08f;
        fill.color = new Color(0.6f, 0.8f, 0.85f);

        RenderSettings.ambientLight = new Color(0.18f, 0.22f, 0.34f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.0025f;
        RenderSettings.fogColor = new Color(0.15f, 0.19f, 0.28f);

        // Kill the default bright skybox so a stale clearFlags/Camera can never
        // flood the frame with pale blue in the built player.
        RenderSettings.skybox = null;

        return moonGO;
    }

    /// <summary>
    /// Dresses the sky for the North: a procedural storm-night skybox, broad
    /// aurora ribbons overhead, drifting snowfall, and bluespines that will
    /// catch the aurora glow at the rim of the scene.
    /// </summary>
    static void CreateAtmosphere()
    {
        // Deterministic night sky: an inverted dome enclosing the whole canyon,
        // painted near-black storm blue and fogged out to the storm fog colour at
        // distance. Reliable in batch/built contexts where skybox shaders are not.
        var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "NightSkyDome";
        dome.transform.position = new Vector3(0f, 40f, 0f);
        dome.transform.localScale = new Vector3(-420f, -420f, -420f);
        var domeMat = MakeMat("NightSkyMat", new Color(0.025f, 0.04f, 0.075f, 1f));
        domeMat.EnableKeyword("_EMISSION");
        domeMat.SetColor("_EmissionColor", new Color(0.04f, 0.06f, 0.12f));
        domeMat.SetFloat("_Glossiness", 0f);
        domeMat.SetFloat("_Metallic", 0f);
        dome.GetComponent<MeshRenderer>().sharedMaterial = domeMat;
        Object.DestroyImmediate(dome.GetComponent<Collider>());
        dome.isStatic = true;

        // Aurora as billboard glow: particles always face the camera, so we
        // get visible teal-green curtains without fighting plane orientation.
        var auroraTex = ProceduralGlowTexture(128, new Color(0.55f, 1f, 0.9f));
        var auroraMat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        if (auroraMat == null) auroraMat = new Material(Shader.Find("Sprites/Default"));
        auroraMat.mainTexture = auroraTex;
        auroraMat.color = new Color(0.45f, 0.95f, 0.8f, 0.5f);

        var aurora = new GameObject("AuroraField");
        aurora.transform.position = new Vector3(0f, 26f, 42f);
        aurora.transform.rotation = Quaternion.Euler(-16f, 0f, 0f); // lean back toward zenith
        var aps = aurora.AddComponent<ParticleSystem>();
        var amain = aps.main;
        amain.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
        amain.startSpeed = 0.6f;
        amain.startSize = new ParticleSystem.MinMaxCurve(2.2f, 5.5f);
        amain.startColor = new Color(0.55f, 1f, 0.9f, 0.7f);
        amain.maxParticles = 260;
        amain.simulationSpace = ParticleSystemSimulationSpace.World;
        amain.gravityModifier = 0f;
        var anoise = aps.noise;
        anoise.enabled = true;
        anoise.strength = new ParticleSystem.MinMaxCurve(3.8f);
        anoise.frequency = 1.4f;
        anoise.scrollSpeed = 7f;
        // Constant-mode velocity curves keep every curve family aligned and
        // silence "Particle Velocity curves must all be in the same mode".
        var avel = aps.velocityOverLifetime;
        avel.enabled = true;
        avel.x = new ParticleSystem.MinMaxCurve(0.5f);
        avel.y = new ParticleSystem.MinMaxCurve(0.05f);
        avel.z = new ParticleSystem.MinMaxCurve(0.5f);
        var aem = aps.emission;
        aem.rateOverTime = 14f;
        var ashape = aps.shape;
        ashape.shapeType = ParticleSystemShapeType.Box;
        ashape.scale = new Vector3(92f, 5f, 40f);
        var arend = aps.GetComponent<ParticleSystemRenderer>();
        arend.renderMode = ParticleSystemRenderMode.Billboard;
        arend.sharedMaterial = auroraMat;
        aps.Emit(120);

        // A single soft core so the glow reads even where particles are sparse.
        var core = new GameObject("AuroraCore");
        core.transform.position = new Vector3(2f, 37f, 72f);
        var sr = core.AddComponent<SpriteRenderer>();
        var coreTex = ProceduralGlowTexture(128, new Color(0.5f, 1f, 0.85f));
        sr.sprite = Sprite.Create(coreTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 50f);
        var coreMat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        if (coreMat == null) coreMat = new Material(Shader.Find("Sprites/Default"));
        coreMat.color = new Color(1f, 1f, 1f, 0.42f);
        sr.sharedMaterial = coreMat;
        core.transform.localScale = new Vector3(85f, 30f, 1f);

        // Teal point glow so the atmosphere around the skyline picks up light.
        var aurGlow = new GameObject("AuroraGlowLight");
        aurGlow.transform.position = new Vector3(0f, 26f, 42f);
        var al = aurGlow.AddComponent<Light>();
        al.type = LightType.Point;
        al.color = new Color(0.4f, 1f, 0.8f);
        al.range = 48f;
        al.intensity = 1.3f;

        BuildCragSkyline();

        // Warm counterpart to the cold light: the rest-camp fire so the scene
        // has a focal warmth source instead of being uniformly glacial.
        var campFire = new GameObject("CampfireLight");
        campFire.transform.position = new Vector3(6.5f, 1.4f, 6f);
        var cf = campFire.AddComponent<Light>();
        cf.type = LightType.Point;
        cf.color = new Color(1f, 0.58f, 0.25f);
        cf.range = 9f;
        cf.intensity = 1.4f;

        // Cold blue glow over the spawn keeps the hero readable.
        var spawnLight = new GameObject("SpawnCoolLight");
        spawnLight.transform.position = new Vector3(0f, 3f, 0f);
        var fl = spawnLight.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.color = new Color(0.45f, 0.6f, 0.9f);
        fl.range = 9f;
        fl.intensity = 0.7f;

        // Cold electric-blue accent marking the Restless Shrine.
        var shrineLight = new GameObject("ShrineBlueLight");
        shrineLight.transform.position = new Vector3(13.5f, 2.6f, 4.8f);
        var sl = shrineLight.AddComponent<Light>();
        sl.type = LightType.Point;
        sl.color = new Color(0.35f, 0.7f, 1f);
        sl.range = 10f;
        sl.intensity = 1.6f;

        // Drifting snow across the whole canyon floor.
        var snow = new GameObject("Snowfall");
        snow.transform.position = new Vector3(0f, 12f, 0f);
        var ps = snow.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 4f;
        main.startSpeed = 3.2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor = new Color(0.9f, 0.94f, 1f, 0.85f);
        main.maxParticles = 900;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission;
        em.rateOverTime = 320f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(70f, 1f, 70f);
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);
        vel.z = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        var snowMat = new Material(PickShader());
        snowMat.color = new Color(0.88f, 0.92f, 1f, 0.8f);
        renderer.sharedMaterial = snowMat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }

    /// <summary>
    /// Radial additive glow texture (soft core to transparent rim), generated
    /// at edit time so the scene needs no external art assets.
    /// </summary>
    static Texture2D ProceduralGlowTexture(int size, Color tint)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        tex.name = "ProcGlow";
        float half = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float core = Mathf.Clamp01((1f - d) / 0.35f);
                float glow = Mathf.Pow(Mathf.Clamp01(1f - d / 1.15f), 2.6f);
                float a = Mathf.Lerp(core, glow, 0.45f) * 0.85f;
                tex.SetPixel(x, y, new Color(tint.r, tint.g, tint.b, a));
            }
        }
        tex.Apply(false, true);
        return tex;
    }

    /// <summary>
    /// Rings the play space with tall rock silhouettes so the night skyline has
    /// mountains to read against, instead of flat horizon falling into fog.
    /// Deterministic angles from a fixed seed for reproducible shots.
    /// </summary>
    static void BuildCragSkyline()
    {
        var rng = new System.Random(1337);
        for (int i = 0; i < 14; i++)
        {
            double a = rng.NextDouble() * Mathf.PI * 2f;
            float r = 48f + (float)rng.NextDouble() * 46f;
            float h = 16f + (float)rng.NextDouble() * 15f;
            SpawnDecor("decor/rock_large_c.glb", "CragMountain" + i,
                new Vector3(Mathf.Cos((float)a) * r, 0f, Mathf.Sin((float)a) * r),
                h, false, (float)rng.NextDouble() * 360f, false);
        }
        for (int i = 0; i < 12; i++)
        {
            double a = rng.NextDouble() * Mathf.PI * 2f;
            float r = 34f + (float)rng.NextDouble() * 18f;
            SpawnDecor("decor/rock_small_b.glb", "MidCrag" + i,
                new Vector3(Mathf.Cos((float)a) * r, 0f, Mathf.Sin((float)a) * r),
                2.2f + (float)rng.NextDouble() * 2.6f, false, (float)rng.NextDouble() * 360f, false);
        }
        Debug.Log("Crag skyline placed (" + 14 + " peaks, " + 12 + " mid crags).");
    }

    static void SpawnDecor(string modelFile, string name, Vector3 pos, float targetSize, bool fitToWidth, float yaw, bool blocking)
    {
        var root = new GameObject(name);
        root.transform.position = pos;

        var model = AttachModel(root, "Model", modelFile, targetSize, fitToWidth);
        if (model == null)
        {
            GameObject.DestroyImmediate(root);
            return;
        }
        if (Mathf.Abs(yaw) > 0.01f)
            model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        DestroyChildColliders(model);
        // Non-trigger collider from the model bounds so the runtime NavMesh baker
        // still carves around blocking decor (trees, rocks, ruins, camp props).
        if (blocking)
            AddColliderFromBounds(root, ModelLocalBounds(model)).isTrigger = false;
        root.isStatic = true;
    }

    static void CreateDecor()
    {
        // Rest camp a short walk from the spawn.
        SpawnDecor("decor/campfire_stones.glb", "Campfire", new Vector3(6.5f, 0f, 6f), 0.8f, false, 0f, true);
        SpawnDecor("decor/tent.glb", "Tent", new Vector3(10f, 0f, 7.5f), 2.2f, false, 180f, true);
        SpawnDecor("decor/log_stack.glb", "LogStack", new Vector3(5f, 0f, 8.5f), 0.9f, false, 40f, true);
        SpawnDecor("decor/barrel.glb", "Barrel1", new Vector3(4f, 0f, 5.5f), 0.8f, false, 30f, true);
        SpawnDecor("decor/barrel.glb", "Barrel2", new Vector3(12f, 0f, 5.2f), 0.8f, false, -20f, true);
        SpawnDecor("decor/torch_lit.glb", "TorchCamp1", new Vector3(5.5f, 0f, 4.3f), 1.6f, false, 20f, true);
        SpawnDecor("decor/torch_lit.glb", "TorchCamp2", new Vector3(8.5f, 0f, 8.8f), 1.6f, false, -30f, true);
        SpawnDecor("local/well.gltf", "Well", new Vector3(8.5f, 0f, 9.5f), 2.2f, false, 0f, true);

        // The Restless Shrine, beyond the old columns.
        SpawnDecor("decor/shrine.glb", "Shrine", new Vector3(13.5f, 0f, 4.8f), 2.0f, false, 0f, true);
        SpawnDecor("decor/shrine_candles.glb", "ShrineCandles", new Vector3(13.5f, 0f, 4.2f), 0.8f, false, 0f, true);
        SpawnDecor("decor/statue_obelisk.glb", "Obelisk1", new Vector3(15.5f, 0f, 2.2f), 3.0f, false, 0f, true);
        SpawnDecor("decor/statue_obelisk.glb", "Obelisk2", new Vector3(15.5f, 0f, 7.2f), 3.0f, false, 0f, true);
        SpawnDecor("decor/arch_gothic.glb", "GothicArch", new Vector3(17.2f, 0f, 4.7f), 4.0f, false, 0f, true);
        SpawnDecor("decor/torch_lit.glb", "TorchShrine1", new Vector3(11.8f, 0f, 3.1f), 1.6f, false, 0f, true);
        SpawnDecor("decor/torch_lit.glb", "TorchShrine2", new Vector3(14.4f, 0f, 6.6f), 1.6f, false, 0f, true);

        // Ruined columns framing the old approach.
        SpawnDecor("kaykit/pillar.glb", "Pillar1", new Vector3(-12f, 0f, -8f), 4.0f, false, 0f, true);
        SpawnDecor("kaykit/pillar.glb", "Pillar2", new Vector3(12f, 0f, 10f), 4.0f, false, 0f, true);
        SpawnDecor("decor/column_broken.glb", "BrokenColumn1", new Vector3(9f, 0f, -12f), 2.5f, false, 120f, true);
        SpawnDecor("decor/column_broken.glb", "BrokenColumn2", new Vector3(-14f, 0f, 12f), 2.5f, false, -70f, true);
        SpawnDecor("decor/statue_column_damaged.glb", "RuinedColumn1", new Vector3(20f, 0f, 2f), 2.4f, false, 45f, true);
        SpawnDecor("decor/statue_column_damaged.glb", "RuinedColumn2", new Vector3(-20f, 0f, -5f), 2.4f, false, -30f, true);

        // Rocks across the highland floor.
        SpawnDecor("decor/rock_large_c.glb", "Rock1", new Vector3(-18f, 0f, -14f), 1.6f, false, 80f, true);
        SpawnDecor("decor/rock_large_c.glb", "Rock2", new Vector3(24f, 0f, 12f), 1.6f, false, -30f, true);
        SpawnDecor("decor/rock_large_c.glb", "Rock3", new Vector3(2f, 0f, -16f), 1.8f, false, 10f, true);
        SpawnDecor("decor/rock_large_c.glb", "Rock4", new Vector3(-22f, 0f, 16f), 1.7f, false, 160f, true);
        Vector3[] smallRocks =
        {
            new Vector3(-9f, 0f, -18f), new Vector3(18f, 0f, -14f), new Vector3(20f, 0f, -2f), new Vector3(-16f, 0f, -4f),
            new Vector3(-26f, 0f, 20f), new Vector3(26f, 0f, 18f), new Vector3(12f, 0f, 20f), new Vector3(-10f, 0f, 22f)
        };
        for (int i = 0; i < smallRocks.Length; i++)
            SpawnDecor("decor/rock_small_b.glb", "SmallRock" + i, smallRocks[i], 0.7f, false, i * 47f, true);

        // Rim treeline to suggest the canyon walls.
        SpawnDecor("decor/tree_pine_round_a.glb", "Pine1", new Vector3(0f, 0f, -32f), 6f, false, 20f, true);
        SpawnDecor("decor/tree_pine_round_a.glb", "Pine2", new Vector3(-28f, 0f, -6f), 6f, false, -40f, true);
        SpawnDecor("decor/tree_pine_round_a.glb", "Pine3", new Vector3(8f, 0f, 34f), 6f, false, 160f, true);
        SpawnDecor("decor/tree_pine_round_a.glb", "Pine4", new Vector3(22f, 0f, -24f), 5.5f, false, 260f, true);
        SpawnDecor("decor/tree_pine_round_a.glb", "Pine5", new Vector3(-34f, 0f, 18f), 6f, false, 90f, true);
        SpawnDecor("decor/tree_default_dark.glb", "Tree1", new Vector3(28f, 0f, 2f), 7f, false, 30f, true);
        SpawnDecor("decor/tree_default_dark.glb", "Tree2", new Vector3(-12f, 0f, 30f), 7f, false, 0f, true);
        SpawnDecor("decor/tree_default_dark.glb", "Tree3", new Vector3(34f, 0f, -18f), 6.5f, false, 180f, true);
        SpawnDecor("decor/tree_default_dark.glb", "Tree4", new Vector3(-2f, 0f, 30f), 6.5f, false, 0f, true);
        SpawnDecor("decor/tree_blocks.glb", "BlockTree1", new Vector3(18f, 0f, 26f), 5.5f, false, 0f, true);
        SpawnDecor("decor/tree_blocks.glb", "BlockTree2", new Vector3(-24f, 0f, 30f), 5.5f, false, 20f, true);

        // Distant village clusters on the far rim (facing the camp).
        SpawnDecor("local/medieval_houses.gltf", "Village1", new Vector3(22f, 0f, -40f), 7f, false, 15f, true);
        SpawnDecor("local/medieval_houses.gltf", "Village2", new Vector3(-36f, 0f, -30f), 7f, false, -30f, true);

        // Grass tufts and flowers (cosmetic, non-blocking).
        Vector3[] tufts =
        {
            new Vector3(4f, 0f, 3f), new Vector3(6f, 0f, 7f), new Vector3(12f, 0f, 8f), new Vector3(14f, 0f, 3f),
            new Vector3(-4f, 0f, 2f), new Vector3(-6f, 0f, 5f), new Vector3(11f, 0f, 3.5f)
        };
        for (int i = 0; i < tufts.Length; i++)
            SpawnDecor("decor/grass.glb", "Grass" + i, tufts[i], 0.4f, false, i * 53f, false);
        SpawnDecor("decor/flower_yellow_a.glb", "FlowerY1", new Vector3(5f, 0f, 2f), 0.3f, false, 0f, false);
        SpawnDecor("decor/flower_yellow_a.glb", "FlowerY2", new Vector3(9f, 0f, 5f), 0.3f, false, 0f, false);
        SpawnDecor("decor/flower_yellow_a.glb", "FlowerY3", new Vector3(12f, 0f, 6f), 0.3f, false, 0f, false);
        SpawnDecor("decor/flower_purple_a.glb", "FlowerP1", new Vector3(7f, 0f, 4f), 0.3f, false, 0f, false);
        SpawnDecor("decor/flower_purple_a.glb", "FlowerP2", new Vector3(10f, 0f, 6f), 0.3f, false, 0f, false);
        SpawnDecor("decor/flower_purple_a.glb", "FlowerP3", new Vector3(3f, 0f, 1f), 0.3f, false, 0f, false);
    }

    // ---------------------------------------------------------------- player

    static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        player.transform.position = new Vector3(0f, 1f, 0f);

        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0f, 1f, 0f);

        // League-style movement is NavMeshAgent-driven. Keep the CharacterController
        // (required by the controller script) disabled so they never fight over the transform.
        var nav = player.AddComponent<NavMeshAgent>();
        nav.radius = 0.45f;
        nav.height = 2f;
        nav.baseOffset = 0f;
        nav.speed = 8f;
        nav.angularSpeed = 720f;
        nav.acceleration = 40f;
        nav.stoppingDistance = 2.5f;
        cc.enabled = false;

        var human = AttachModel(player, "HumanModel", "kaykit/knight.glb", 1.8f, false);
        if (human == null)
        {
            human = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            human.name = "HumanModel";
            human.transform.SetParent(player.transform, false);
            human.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            human.transform.localScale = new Vector3(0.65f, 0.9f, 0.65f);
            human.GetComponent<MeshRenderer>().sharedMaterial = MakeMat("HumanMat", new Color(0.25f, 0.4f, 0.75f, 1f));
        }
        DestroyChildColliders(human);
        // Cool, reduced-specular instance tint so the hero reads against the
        // moon without clipping to a featureless white blob.
        foreach (var r in human.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (r.sharedMaterial == null) continue;
            r.sharedMaterial = new Material(r.sharedMaterial);
            r.sharedMaterial.color = new Color(0.6f, 0.64f, 0.8f, 1f);
            r.sharedMaterial.SetFloat("_Glossiness", 0.16f);
            r.sharedMaterial.SetFloat("_Metallic", 0.05f);
        }

        var werebear = new GameObject("WerebearModel");
        werebear.transform.SetParent(player.transform, false);
        werebear.transform.localPosition = new Vector3(0f, 0f, 0f);
        var bear = AttachModel(werebear, "Model", "ursian/UNITY CHAR.fbx", 2.3f, false);
        if (bear == null)
        {
            bear = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bear.name = "Model";
            bear.transform.SetParent(werebear.transform, false);
            bear.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            bear.transform.localScale = new Vector3(1.1f, 0.95f, 1.1f);
            bear.GetComponent<MeshRenderer>().sharedMaterial = MakeMat("WerebearMat", new Color(0.45f, 0.28f, 0.16f, 1f));
        }
        DestroyChildColliders(bear);
        WireWerebearAnim(bear);
        werebear.SetActive(false);

        var ap = new GameObject("AttackPoint");
        ap.transform.SetParent(player.transform, false);
        ap.transform.localPosition = new Vector3(0f, 1.3f, 1.6f);

        var spawn = new GameObject("SpiritSpawnPoint");
        spawn.transform.SetParent(player.transform, false);
        spawn.transform.localPosition = new Vector3(0f, 2.5f, 0f);

        var animator = player.AddComponent<Animator>();
        var pCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animators/PlayerAnimator.controller");
        animator.runtimeAnimatorController = pCtrl;
        // Destroy the root animator entirely: it would otherwise claim the werebear's
        // SkinnedMeshRenderer (nearest-ancestor rule) and force bind/T-pose. The human
        // model is a static glb with no rig, so no animation is lost by removing it.
        Object.DestroyImmediate(animator);

        player.AddComponent<AudioSource>();

        var ctrl = player.AddComponent<BainoviaCharacterController>();
        ctrl.humanModel = human;
        ctrl.werebearModel = werebear;
        ctrl.attackPoint = ap.transform;
        ctrl.enemyLayers = LayerMask.GetMask("Enemy");

        var rune = player.AddComponent<RuneMagicSystem>();
        rune.availableRunes = new[]
        {
            RuneMagicSystem.RuneType.Storm,
            RuneMagicSystem.RuneType.Frost,
            RuneMagicSystem.RuneType.Wind,
            RuneMagicSystem.RuneType.Spirit,
            RuneMagicSystem.RuneType.Blood,
            RuneMagicSystem.RuneType.Forbidden
        };
        rune.characterController = ctrl;
        rune.audioSource = player.GetComponent<AudioSource>();

        var skill = player.AddComponent<SkillTreeSystem>();
        skill.player = ctrl;
        skill.runeSystem = rune;

        var inv = player.AddComponent<InventorySystem>();
        inv.player = ctrl;
        inv.runeSystem = rune;

        var combat = player.AddComponent<CombatSystem>();
        combat.attackPoint = ap.transform;
        combat.enemyLayers = LayerMask.GetMask("Enemy");
        combat.characterController = ctrl;

        var camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        // Matches CameraController's defaultPitch=16 / distance=13: cinematic
        // eye level where hero, snow and the aurora sky share the frame.
        camObj.transform.position = new Vector3(0f, 6.6f, -12.5f);
        camObj.transform.rotation = Quaternion.Euler(16f, 0f, 0f);
        var cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        // Storm night: clear to near-black subtle storm blue (no bright default
        // skybox). The fogged NightSkyDome and aurora ribbons layer on top of it.
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.045f, 0.07f, 0.11f);
        camObj.AddComponent<AudioListener>();
        var camCtrl = camObj.AddComponent<CameraController>();
        var camBody = new SerializedObject(camCtrl);
        camBody.FindProperty("playerBody").objectReferenceValue = player.transform;
        camBody.ApplyModifiedProperties();

        var inter = player.AddComponent<InteractionSystem>();
        var interSO = new SerializedObject(inter);
        interSO.FindProperty("playerCamera").objectReferenceValue = cam;
        interSO.FindProperty("player").objectReferenceValue = ctrl;
        interSO.ApplyModifiedProperties();

        var sp = player.AddComponent<SpiritCompanionSystem>();
        sp.player = ctrl;
        sp.spiritSpawnPoint = spawn.transform;

        // Controller needs references to its sibling magic/combat systems (found in code
        // via GetComponent, but wiring them explicitly keeps it deterministic).
        ctrl.runeSystem = rune;
        ctrl.combatSystem = combat;
        combat.cameraController = camCtrl;

        // Starter spirit - a hidden wolf spirit the player can summon with 7.
        WireStarterSpirit(sp);

        return player;
    }

    /// <summary>
    /// Gives the player a starter Wolf Spirit (hidden model) so summoning works immediately.
    /// </summary>
    static void WireStarterSpirit(SpiritCompanionSystem sp)
    {
        var wolfModel = AttachModel(sp.gameObject, "WolfSpiritModel", "kenney/wolf.glb", 0.9f, false);
        if (wolfModel == null)
        {
            wolfModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wolfModel.name = "WolfSpiritModel";
            wolfModel.transform.SetParent(sp.transform, false);
            wolfModel.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            wolfModel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            wolfModel.GetComponent<MeshRenderer>().sharedMaterial = MakeMat("WolfSpiritMat", new Color(0.7f, 0.85f, 1f, 0.9f));
        }
        else
        {
            // Pale spirit-blue instanced tint. Must not bleed into the shared enemy
            // wolf material, so instantiate per-renderer copies here.
            var spiritTint = new Color(0.65f, 0.85f, 1f, 0.9f);
            foreach (var r in wolfModel.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.sharedMaterial == null) continue;
                r.sharedMaterial = new Material(r.sharedMaterial) { color = spiritTint };
            }
        }
        DestroyChildColliders(wolfModel);
        wolfModel.SetActive(false);

        var so = new SerializedObject(sp);

        var spirits = so.FindProperty("collectedSpirits");
        spirits.arraySize = 1;
        spirits.GetArrayElementAtIndex(0).FindPropertyRelative("spiritId").stringValue = "wolf_spirit";
        spirits.GetArrayElementAtIndex(0).FindPropertyRelative("spiritName").stringValue = "Wolf Spirit";
        spirits.GetArrayElementAtIndex(0).FindPropertyRelative("description").stringValue = "A loyal spirit of the highlands.";
        spirits.GetArrayElementAtIndex(0).FindPropertyRelative("type").enumValueIndex = (int)SpiritCompanionSystem.SpiritType.Offensive;
        spirits.GetArrayElementAtIndex(0).FindPropertyRelative("spiritModel").objectReferenceValue = wolfModel;
        spirits.GetArrayElementAtIndex(0).FindPropertyRelative("relationshipLevel").intValue = 1;
        spirits.GetArrayElementAtIndex(0).FindPropertyRelative("maxRelationshipLevel").intValue = 5;
        spirits.GetArrayElementAtIndex(0).FindPropertyRelative("isActive").boolValue = true;

        var ability = spirits.GetArrayElementAtIndex(0).FindPropertyRelative("ability");
        ability.FindPropertyRelative("abilityName").stringValue = "Claw Swipe";
        ability.FindPropertyRelative("cooldown").floatValue = 20f;
        ability.FindPropertyRelative("duration").floatValue = 8f;
        ability.FindPropertyRelative("power").floatValue = 45f;

        so.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------------- werebear animation

    /// <summary>
    /// Configures the URSIAN model + idle FBX + RPG clip FBX as Humanoid so Unity
    /// retargets the animation pack clips onto the CC/AccuRig skeleton at runtime.
    /// </summary>
    static void ConfigureWerebearImports()
    {
        var files = new[] { "ursian/UNITY CHAR.fbx", "ursian/idle.fbx" };
        foreach (var file in files)
        {
            var path = ModelPath(file);
            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) continue;
            bool dirty = false;
            if (imp.animationType != ModelImporterAnimationType.Human)
            {
                imp.animationType = ModelImporterAnimationType.Human;
                dirty = true;
            }
            if (!imp.optimizeGameObjects)
            {
                imp.optimizeGameObjects = false;
                dirty = true;
            }
            if (SetImportedLoop(imp, "idle", true, true)) dirty = true;
            if (dirty)
            {
                imp.SaveAndReimport();
                Debug.Log($"Werebear import configured (Human) -> {file}");
            }
        }

        // The outsourced ability clips (RPG Character Animation Pack) are humanoid
        // so they retarget onto the URSIAN avatar.
        string rpgDir = "Assets/Animations/RPG";
        if (AssetDatabase.IsValidFolder(rpgDir))
        {
            foreach (var f in AssetDatabase.FindAssets("t:Model", new[] { rpgDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(f);
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;
                bool dirty = false;
                if (imp.animationType != ModelImporterAnimationType.Human)
                {
                    imp.animationType = ModelImporterAnimationType.Human;
                    dirty = true;
                }
                if (!imp.optimizeGameObjects)
                {
                    imp.optimizeGameObjects = false;
                    dirty = true;
                }
                // The HumanoidRun locomotion clip loops; all ability clips are one-shots.
                if (SetImportedLoop(imp, "Run", false, true)) dirty = true;
                if (SetImportedLoop(imp, "Idle", false, true)) dirty = true;
                if (SetImportedLoop(imp, "Attack", false, false)) dirty = true;
                if (SetImportedLoop(imp, "Death", false, false)) dirty = true;
                if (SetImportedLoop(imp, "GetHit", false, false)) dirty = true;
                if (SetImportedLoop(imp, "Jump", false, false)) dirty = true;
                if (SetImportedLoop(imp, "Fall", false, false)) dirty = true;
                if (dirty)
                {
                    imp.SaveAndReimport();
                    Debug.Log($"RPG clip import configured (Human) -> {Path.GetFileName(path)}");
                }
            }
        }
    }

    /// <summary>
    /// Builds the URSIAN werebear AnimatorController from the outsourced RPG
    /// animation pack. The model, its idle clip and the RPG clips are imported as
    /// Humanoid; Unity retargets the packed clips onto the CC/AccuRig skeleton.
    /// Falls back to a generic avatar when the model exposes no humanoid avatar.
    /// </summary>
    static void WireWerebearAnim(GameObject bear)
    {
        AnimationClip idle = FindAnimClip("ursian/idle.fbx", "idle");
        if (idle == null)
        {
            Debug.LogWarning("Werebear: no animation clip found in idle.fbx - static pose.");
            return;
        }

        var anim = bear.GetComponent<Animator>();
        bool hadAnimator = anim != null;
        if (anim == null) anim = bear.AddComponent<Animator>();

        // Prefer the humanoid avatar baked into the URSIAN model import.
        Avatar avatar = null;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ModelPath("ursian/UNITY CHAR.fbx")))
        {
            if (o is Avatar a && a.isHuman) { avatar = a; break; }
        }
        if (avatar == null)
        {
            Transform rootBone = null;
            var smr0 = bear.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr0 != null) rootBone = smr0.rootBone;
            var av = AvatarBuilder.BuildGenericAvatar(bear, rootBone != null ? rootBone.name : null);
            if (av != null)
            {
                av.name = "WerebearGenericAvatar";
                string avPath = "Assets/Animators/WerebearGenericAvatar.asset";
                var existing = AssetDatabase.LoadAssetAtPath<Avatar>(avPath);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(av, avPath);
                    existing = AssetDatabase.LoadAssetAtPath<Avatar>(avPath);
                }
                avatar = existing != null ? existing : av;
            }
        }
        if (avatar != null) anim.avatar = avatar;

        string avatarFinal = anim.avatar == null ? "none" : (anim.avatar.isHuman ? "human" : "generic");
        Debug.Log($"Werebear animator: hadAnimator={hadAnimator} finalAvatar={avatarFinal}");
        anim.applyRootMotion = false;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        var smr = bear.GetComponentInChildren<SkinnedMeshRenderer>(true);
        string smrInfo = smr == null ? "none" :
            $"{smr.sharedMesh.boneWeights.Length} weights, rootBone={(smr.rootBone != null ? smr.rootBone.name : "null")}, {smr.bones.Length} bones";
        Debug.Log($"Werebear debug: avatar={anim.avatar != null} smr={smrInfo} root={bear.name} children={bear.transform.childCount}");

        // Outsourced ability clips (retargeted onto the URSIAN avatar).
        var clipAttack = FindRpgClip("2Hand-Sword-Attack1");
        var clipHeavy = FindRpgClip("2Hand-Sword-Attack2");
        var clipCast = FindRpgClip("Unarmed-Attack-R1");
        var clipHurt = FindRpgClip("Unarmed-GetHit-B1");
        var clipDeath = FindRpgClip("2Hand-Sword-Death1");
        Debug.Log($"Werebear rpg clips: idle={idle.name} attack={(clipAttack != null ? clipAttack.name : "none")} " +
            $"heavy={(clipHeavy != null ? clipHeavy.name : "none")} cast={(clipCast != null ? clipCast.name : "none")} " +
            $"hurt={(clipHurt != null ? clipHurt.name : "none")} death={(clipDeath != null ? clipDeath.name : "none")}");

        SetLoop(idle, true);
        if (clipAttack != null) SetLoop(clipAttack, false);
        if (clipHeavy != null) SetLoop(clipHeavy, false);
        if (clipCast != null) SetLoop(clipCast, false);
        if (clipHurt != null) SetLoop(clipHurt, false);
        if (clipDeath != null) SetLoop(clipDeath, false);

        const string ctrlPath = "Assets/Animators/WerebearAnimator.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath) != null)
            AssetDatabase.DeleteAsset(ctrlPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

        ctrl.parameters = new AnimatorControllerParameter[0];
        AddTrigger(ctrl, "Attack");
        AddTrigger(ctrl, "HeavyAttack");
        AddTrigger(ctrl, "Cast");
        AddTrigger(ctrl, "Hurt");
        AddBool(ctrl, "IsBerserk", false);
        AddBool(ctrl, "IsDead", false);
        AddBool(ctrl, "IsGrounded", true);
        AddFloat(ctrl, "Speed", 0f);

        var root = ctrl.layers[0].stateMachine;
        root.states = new ChildAnimatorState[0];
        root.anyStateTransitions = new AnimatorStateTransition[0];
        root.defaultState = null;

        var runClip = FindRpgClip("HumanoidRun") ?? FindRpgClip("Run");

        // Hub state: 1D blend of idle/run driven by the runtime Speed param.
        AnimationClip hubMotion = null;
        var stLoc = root.AddState("Locomotion");
        if (runClip != null)
        {
            SetLoop(runClip, true);
            var locomotion = new BlendTree();
            locomotion.name = "LocomotionBT";
            locomotion.blendParameter = "Speed";
            locomotion.AddChild(idle, 0f);
            locomotion.AddChild(runClip, 1f);
            AssetDatabase.AddObjectToAsset(locomotion, ctrl);
            stLoc.motion = locomotion;
            hubMotion = runClip;
            Debug.Log($"Werebear run clip: id={runClip.name} len={runClip.length:F2}s");
        }
        else
        {
            stLoc.motion = idle;
            Debug.LogWarning("Werebear: no run clip found - locomotion blend skipped.");
        }
        root.defaultState = stLoc;

        var stAttack = AddClipState(root, "Attack", clipAttack);
        var stHeavy = AddClipState(root, "HeavyAttack", clipHeavy);
        var stCast = AddClipState(root, "Cast", clipCast);
        var stHurt = AddClipState(root, "Hurt", clipHurt);
        var stDeath = AddClipState(root, "Death", clipDeath);

        // Berserk reuses a closet attack pose as a looping frenzy stance. Clone the
        // clip so the one-shot Attack state stays non-looping, and add the clone to
        // the controller asset so its motion reference survives serialization.
        AnimationClip berserkClip = clipAttack != null ? Object.Instantiate(clipAttack) : null;
        if (berserkClip != null)
        {
            berserkClip.name = "BerserkFrenzy-" + clipAttack.name;
            berserkClip.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(berserkClip, ctrl);
        }
        SetLoop(berserkClip, true);
        var stBerserk = AddClipState(root, "Berserk", berserkClip);

        // Locomotion transitions into abilities and berserk.
        AddTriggerTransition(stLoc, stAttack, "Attack");
        AddTriggerTransition(stLoc, stHeavy, "HeavyAttack");
        AddTriggerTransition(stLoc, stCast, "Cast");
        AddTriggerTransition(stLoc, stHurt, "Hurt");
        AddConditionTransition(stLoc, stBerserk, "IsBerserk", true);
        AddConditionTransition(stBerserk, stLoc, "IsBerserk", false);

        // Abilities return to locomotion (which continues blending idle/run).
        AddReturnTransition(stAttack, stLoc);
        AddReturnTransition(stHeavy, stLoc);
        AddReturnTransition(stCast, stLoc);
        AddReturnTransition(stHurt, stLoc);

        // Any state -> Death when IsDead flips.
        foreach (var s in new[] { stLoc, stAttack, stHeavy, stCast, stHurt, stBerserk })
            AddConditionTransition(s, stDeath, "IsDead", true);

        anim.runtimeAnimatorController = ctrl;
        anim.updateMode = AnimatorUpdateMode.Normal;

        // Re-fit after reimport so the skinned bounds' lowest point lands on y=0.
        FitModel(bear, 2.3f, false);
        Debug.Log($"Werebear animation wired: idle={idle.name} ({idle.length:F2}s) run={(hubMotion != null ? hubMotion.name : "none")} states=Attack/HeavyAttack/Cast/Hurt/Death/Berserk.");
    }

    static AnimationClip FindAnimClip(string modelRelativePath, string namePart)
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(ModelPath(modelRelativePath));
        List<AnimationClip> clips = new List<AnimationClip>();
        foreach (var o in all) if (o is AnimationClip c) clips.Add(c);
        foreach (var c in clips) Debug.Log($"Werebear {modelRelativePath} clip: '{c.name}' ({c.length:F3}s)");
        foreach (var c in clips) if (c.name.ToLowerInvariant().Contains(namePart)) return c;
        if (clips.Count == 1) return clips[0];
        return clips.Find(c =>
            !c.name.ToLowerInvariant().Contains("preview") &&
            !c.name.ToLowerInvariant().Contains("pose") &&
            c.length > 0.05f) ?? clips.Find(c => !c.name.ToLowerInvariant().StartsWith("__preview__")) ?? (clips.Count > 0 ? clips[0] : null);
    }

    static AnimationClip FindRpgClip(string namePart)
    {
        string dir = "Assets/Animations/RPG";
        if (!AssetDatabase.IsValidFolder(dir)) return null;
        foreach (var f in AssetDatabase.FindAssets("t:Model", new[] { dir }))
        {
            var path = AssetDatabase.GUIDToAssetPath(f);
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (o is AnimationClip c && !c.name.StartsWith("__preview__") &&
                    c.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0) return c;
            }
        }
        return null;
    }

    static void SetLoop(AnimationClip clip, bool loop)
    {
        var so = new SerializedObject(clip);
        so.FindProperty("m_AnimationClipSettings.m_LoopTime").boolValue = loop;
        so.ApplyModifiedProperties();
    }

    /// <summary>
    /// Persists Loop Time on an FBX-importer clip (requires SaveAndReimport when dirty).
    /// 'matchAll' forces every clip in the file; otherwise clips whose name contains
    /// 'namePart' (case-insensitive) are updated. Returns whether any setting changed.
    /// </summary>
    static bool SetImportedLoop(ModelImporter imp, string namePart, bool matchAll, bool loop)
    {
        if (imp == null) return false;
        ModelImporterClipAnimation[] clips = imp.clipAnimations;
        if (clips == null || clips.Length == 0)
        {
            clips = imp.defaultClipAnimations;
            if (clips == null || clips.Length == 0) return false;
            imp.clipAnimations = clips;
        }
        bool dirty = false;
        foreach (var c in clips)
        {
            if (c == null) continue;
            bool match = matchAll || c.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (match && c.loopTime != loop)
            {
                c.loopTime = loop;
                dirty = true;
            }
        }
        if (dirty) imp.clipAnimations = clips;
        return dirty;
    }

    static void AddTrigger(AnimatorController ctrl, string name)
    {
        ctrl.AddParameter(new AnimatorControllerParameter { name = name, type = AnimatorControllerParameterType.Trigger });
    }

    static void AddBool(AnimatorController ctrl, string name, bool def)
    {
        ctrl.AddParameter(new AnimatorControllerParameter { name = name, type = AnimatorControllerParameterType.Bool, defaultBool = def });
    }

    static void AddFloat(AnimatorController ctrl, string name, float def)
    {
        ctrl.AddParameter(new AnimatorControllerParameter { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = def });
    }

    static AnimatorState AddClipState(AnimatorStateMachine sm, string name, AnimationClip clip)
    {
        var st = sm.AddState(name);
        st.motion = clip;
        return st;
    }

    static void AddTriggerTransition(AnimatorState from, AnimatorState to, string trigger)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.1f;
        t.AddCondition(AnimatorConditionMode.If, 0f, trigger);
    }

    static void AddConditionTransition(AnimatorState from, AnimatorState to, string param, bool value)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.15f;
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, param);
    }

    static void AddReturnTransition(AnimatorState from, AnimatorState to)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true;
        t.exitTime = 0.9f;
        t.duration = 0.15f;
    }

    // ---------------------------------------------------------------- enemy

    static void CreateEnemies()
    {
        CreateEnemy("Frost Brute", new Vector3(6f, 0f, 8f), "kaykit/barbarian.glb", 2.2f,
            new Color(0.75f, 0.16f, 0.16f, 1f), 120, 14, "frost_brute");

        CreateEnemy("Thunder Wolf", new Vector3(-8f, 0f, 10f), "kenney/wolf.glb", 1.1f,
            new Color(0.85f, 0.75f, 0.2f, 1f), 60, 8, "thunder_wolf");
        CreateEnemy("Thunder Wolf", new Vector3(-10f, 0f, 6f), "kenney/wolf.glb", 1.1f,
            new Color(0.85f, 0.75f, 0.2f, 1f), 60, 8, "thunder_wolf");

        CreateEnemy("Rune Stalker", new Vector3(8f, 0f, -6f), "kaykit/skeleton_rogue.glb", 1.9f,
            new Color(0.4f, 0.2f, 0.6f, 1f), 80, 12, "rune_stalker");
    }

    static GameObject CreateEnemy(string name, Vector3 position, string modelFile, float modelHeight, Color color, int maxHealth, int damage, string targetId)
    {
        var enemy = new GameObject(name);
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");
        enemy.transform.position = new Vector3(position.x, position.y, position.z);

        var model = AttachModel(enemy, "Model", modelFile, modelHeight, false);
        if (model == null)
        {
            model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "Model";
            model.transform.SetParent(enemy.transform, false);
            model.transform.localPosition = new Vector3(0f, modelHeight * 0.5f, 0f);
            model.transform.localScale = new Vector3(1f, modelHeight / 2f, 1f);
            model.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(name + "Mat", color);
        }
        DestroyChildColliders(model);
        AddColliderFromBounds(enemy, ModelLocalBounds(model));

        var agent = enemy.AddComponent<NavMeshAgent>();
        agent.speed = 5f;
        agent.angularSpeed = 360f;
        agent.acceleration = 20f;
        agent.stoppingDistance = 2f;

        var animator = enemy.AddComponent<Animator>();
        var eCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animators/EnemyAnimator.controller");
        animator.runtimeAnimatorController = eCtrl;

        var health = enemy.AddComponent<Health>();
        health.maxHealth = maxHealth;
        health.destroyOnDeath = true;
        health.destroyDelay = 2f;
        health.targetId = targetId;

        var ai = enemy.AddComponent<EnemyAI>();
        ai.health = health;
        ai.animator = animator;
        ai.damage = damage;
        ai.patrolPoints = new Transform[0];

        var bar = enemy.AddComponent<EnemyHealthBar>();
        bar.heightAbove = modelHeight + 0.6f;
        bar.health = health;

        enemy.AddComponent<EnemyStatus>();

        return enemy;
    }

    // ---------------------------------------------------------------- boss

    static void CreateBoss()
    {
        var boss = new GameObject("Alpha Loup-Tonnerre");
        boss.tag = "Enemy";
        boss.layer = LayerMask.NameToLayer("Enemy");
        boss.transform.position = new Vector3(30f, 0f, -26f);

        var model = AttachModel(boss, "Model", "kenney/wolf.glb", 2.4f, false);
        if (model == null)
        {
            model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "Model";
            model.transform.SetParent(boss.transform, false);
            model.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            model.transform.localScale = new Vector3(1.8f, 1.2f, 1.8f);
            model.GetComponent<MeshRenderer>().sharedMaterial = MakeMat("BossMat", new Color(0.55f, 0.5f, 0.6f, 1f));
        }
        DestroyChildColliders(model);

        // Dark storm-tinted hide so the pack leader reads as "not a normal wolf".
        var tint = MakeMat("AlphaWolfMat", new Color(0.3f, 0.29f, 0.42f, 1f));
        foreach (var r in model.GetComponentsInChildren<MeshRenderer>(true))
            r.sharedMaterial = tint;

        AddColliderFromBounds(boss, ModelLocalBounds(model));

        var agent = boss.AddComponent<NavMeshAgent>();
        agent.speed = 6f;
        agent.angularSpeed = 720f;
        agent.acceleration = 24f;
        agent.stoppingDistance = 2f;

        var animator = boss.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animators/EnemyAnimator.controller");

        var health = boss.AddComponent<Health>();
        health.maxHealth = 1500;
        health.destroyOnDeath = true;
        health.destroyDelay = 4f;
        health.targetId = "alpha_loup_tonnerre";

        var bar = boss.AddComponent<EnemyHealthBar>();
        bar.heightAbove = 3.2f;
        bar.health = health;

        boss.AddComponent<EnemyStatus>();

        var bossAI = boss.AddComponent<BossAI>();
        bossAI.health = health;
        bossAI.animator = animator;
        bossAI.tempestAura = CreateTempestAura(boss);
        bossAI.lightingWarningPrefab = CreateLightningWarning();

        // Static storm glow ring under the boss as a visual "arena marker".
        var glow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glow.name = "StormGlow";
        glow.transform.SetParent(boss.transform, false);
        glow.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        glow.transform.localScale = new Vector3(6f, 0.05f, 6f);
        glow.GetComponent<MeshRenderer>().sharedMaterial = MakeMat("BossStormGlow", new Color(0.35f, 0.6f, 1f, 0.35f));
        DestroyChildColliders(glow);

        Debug.Log("Boss Alpha Loup-Tonnerre placed at " + boss.transform.position);
    }

    static GameObject CreateTempestAura(GameObject boss)
    {
        var aura = new GameObject("TempestAura");
        aura.transform.SetParent(boss.transform, false);
        aura.transform.localPosition = new Vector3(0f, 2.4f, 0f);

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "AuraRing";
        ring.transform.SetParent(aura.transform, false);
        ring.transform.localScale = new Vector3(5f, 0.12f, 5f);
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.4f, 0.65f, 1f, 0.4f);
        ring.GetComponent<MeshRenderer>().sharedMaterial = mat;
        DestroyChildColliders(ring);

        aura.SetActive(false);
        return aura;
    }

    static GameObject CreateLightningWarning()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "LightningWarning";
        var mr = go.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.92f, 0.35f, 0.5f);
        mr.sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    // ---------------------------------------------------------------- spirit sanctuary

    static void CreateSanctuary(GameObject player)
    {
        var sanctuary = new GameObject("Spirit Sanctuary");
        sanctuary.tag = "NPC";
        sanctuary.layer = LayerMask.NameToLayer("Interactable");
        Vector3 pos = new Vector3(-3.5f, 0f, 7f);
        sanctuary.transform.position = pos;

        // Stone altar base.
        var altar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        altar.name = "Altar";
        altar.transform.SetParent(sanctuary.transform, false);
        altar.transform.localPosition = new Vector3(0f, 0.22f, 0f);
        altar.transform.localScale = new Vector3(1.8f, 0.22f, 1.8f);
        altar.GetComponent<MeshRenderer>().sharedMaterial = MakeMat("SanctuaryAltar", new Color(0.24f, 0.26f, 0.34f, 1f));
        DestroyChildColliders(altar);

        // Brazier holding the soul-flame.
        var brazier = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        brazier.name = "Brazier";
        brazier.transform.SetParent(sanctuary.transform, false);
        brazier.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        brazier.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        brazier.GetComponent<MeshRenderer>().sharedMaterial = MakeMat("SanctuaryBrazier", new Color(0.1f, 0.12f, 0.16f, 1f));
        DestroyChildColliders(brazier);

        // Flame glow (hidden until kindled).
        var flame = new GameObject("Flame");
        flame.transform.SetParent(sanctuary.transform, false);
        flame.transform.localPosition = new Vector3(0f, 1.05f, 0f);

        var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "FlameGlow";
        orb.transform.SetParent(flame.transform, false);
        orb.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
        var orbMat = new Material(Shader.Find("Sprites/Default"));
        orbMat.color = new Color(1f, 0.62f, 0.15f, 0.95f);
        orb.GetComponent<MeshRenderer>().sharedMaterial = orbMat;
        DestroyChildColliders(orb);

        var flameLight = flame.AddComponent<Light>();
        flameLight.type = LightType.Point;
        flameLight.color = new Color(1f, 0.55f, 0.2f);
        flameLight.range = 7f;
        flameLight.intensity = 2.4f;
        flame.SetActive(false);

        AddColliderFromBounds(sanctuary, new Bounds(pos + Vector3.up * 0.5f, new Vector3(2.2f, 1.2f, 2.2f))).isTrigger = false;

        var script = sanctuary.AddComponent<SpiritSanctuary>();
        var so = new SerializedObject(script);
        so.FindProperty("flameVFX").objectReferenceValue = flame;
        so.FindProperty("sanctuaryId").stringValue = "spirit_sanctuary";
        so.FindProperty("sanctuaryName").stringValue = "Spirit Sanctuary";
        so.FindProperty("isLit").boolValue = false;
        so.ApplyModifiedProperties();

        Debug.Log("Spirit Sanctuary placed near spawn.");
    }

    // ---------------------------------------------------------------- NPC

    static void CreateNPC(GameObject player, GameObject ui)
    {
        var npc = new GameObject("Rune Scholar");
        npc.tag = "NPC";
        npc.layer = LayerMask.NameToLayer("Interactable");
        npc.transform.position = new Vector3(-5f, 0f, 3f);

        var model = AttachModel(npc, "Model", "kaykit/mage.glb", 1.8f, false);
        if (model == null)
        {
            model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "Model";
            model.transform.SetParent(npc.transform, false);
            model.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            model.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
            model.GetComponent<MeshRenderer>().sharedMaterial = MakeMat("NPCMat", new Color(0.2f, 0.55f, 0.35f, 1f));
        }
        DestroyChildColliders(model);
        AddColliderFromBounds(npc, ModelLocalBounds(model)).isTrigger = false;

        var qg = npc.AddComponent<QuestGiverInteractable>();
        var so = new SerializedObject(qg);
        so.FindProperty("npcName").stringValue = "Rune Scholar";
        so.FindProperty("startDialogueNodeId").stringValue = "greeting";

        var dm = GameObject.Find("DialogueManager");
        if (dm != null)
            so.FindProperty("dialogueSystem").objectReferenceValue = dm.GetComponent<DialogueSystem>();

        var qm = GameObject.Find("QuestManager");
        if (qm != null)
            so.FindProperty("questSystem").objectReferenceValue = qm.GetComponent<QuestSystem>();

        so.FindProperty("questId").stringValue = "protect_camp";
        so.FindProperty("canRepeat").boolValue = false;
        so.ApplyModifiedProperties();

        EnsureScriptReference(qg);
    }

    // ---------------------------------------------------------------- item pickup

    static void CreateItemPickup()
    {
        var item = CreatePickup("Ancient Token", new Vector3(-3f, 0f, 1f), "kaykit/coin.glb", 0.5f, true);
        AddItem(item, "ancient_token", "Ancient Token", "A fragment of a forgotten rune shrine.", InventorySystem.ItemType.QuestItem, 5, 10);

        // Rune fragments for the "Restless Shrine" collect quest.
        Vector3[] fragmentPositions =
        {
            new Vector3(10f, 0f, 2f),
            new Vector3(12f, 0f, 4f),
            new Vector3(-12f, 0f, -9f)
        };
        foreach (Vector3 pos in fragmentPositions)
        {
            CreateFragment(pos);
        }
    }

    static GameObject CreatePickup(string name, Vector3 position, string modelFile, float targetSize, bool fitToWidth)
    {
        var go = new GameObject(name);
        go.layer = LayerMask.NameToLayer("Interactable");
        go.transform.position = position;

        var model = AttachModel(go, "Model", modelFile, targetSize, fitToWidth);
        if (model == null)
        {
            model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            model.name = "Model";
            model.transform.SetParent(go.transform, false);
            model.transform.localPosition = new Vector3(0f, targetSize * 0.5f, 0f);
            model.transform.localScale = new Vector3(targetSize, targetSize, targetSize);
            model.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(name + "Mat", new Color(1f, 0.8f, 0.2f, 1f));
        }
        DestroyChildColliders(model);
        AddColliderFromBounds(go, ModelLocalBounds(model)).isTrigger = false;
        return go;
    }

    static void CreateFragment(Vector3 position)
    {
        var frag = CreatePickup("Rune Fragment", position, "quaternius/crystal_small.glb", 0.6f, false);
        AddItem(frag, "rune_fragment", "Rune Fragment", "A shard of pure rune energy from the canyon floor.", InventorySystem.ItemType.QuestItem, 10, 5);
    }

    static void AddItem(GameObject go, string itemId, string itemName, string description, InventorySystem.ItemType type, int maxStack, int value)
    {
        var pickup = go.AddComponent<ItemPickupInteractable>();
        var so = new SerializedObject(pickup);
        var itemProp = so.FindProperty("item");
        itemProp.FindPropertyRelative("itemId").stringValue = itemId;
        itemProp.FindPropertyRelative("itemName").stringValue = itemName;
        itemProp.FindPropertyRelative("description").stringValue = description;
        itemProp.FindPropertyRelative("type").enumValueIndex = (int)type;
        itemProp.FindPropertyRelative("maxStack").intValue = maxStack;
        itemProp.FindPropertyRelative("currentStack").intValue = 1;
        itemProp.FindPropertyRelative("value").intValue = value;
        so.ApplyModifiedProperties();

        EnsureScriptReference(pickup);
    }

    /// <summary>
    /// Secondary classes in a multi-class .cs file can be added at runtime with a
    /// broken (GUID-less) m_Script reference in batch mode. Re-bind the component
    /// to the exact MonoScript asset for its type so the saved scene references it
    /// correctly (and the built player doesn't drop the component).
    /// </summary>
    static void EnsureScriptReference(MonoBehaviour mb)
    {
        try
        {
            var type = mb.GetType();
            if (type == null) return;

            var editorScript = MonoScript.FromMonoBehaviour(mb);
            if (editorScript == null) return;

            string path = AssetDatabase.GetAssetPath(editorScript);
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var script = asset as MonoScript;
                if (script == null) continue;
                if (script.name != type.Name) continue;

                var so = new SerializedObject(mb);
                var prop = so.FindProperty("m_Script");
                if (prop == null) continue;
                prop.objectReferenceValue = script;
                so.ApplyModifiedProperties();
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"EnsureScriptReference({mb.GetType().Name}): {e.Message}");
        }
    }

    // ---------------------------------------------------------------- systems

    static void CreateSystems(GameObject player, GameObject enemy, GameObject ui)
    {
        // Faction manager
        var fm = new GameObject("FactionManager");
        var faction = fm.AddComponent<FactionSystem>();
        faction.player = player.GetComponent<BainoviaCharacterController>();

        var f1 = new FactionSystem.Faction { factionId = "stormguard", factionName = "Storm Guard", reputation = 1, maxReputation = 5, minReputation = -5, alliedFactions = new List<string>(), enemyFactions = new List<string>(), availableQuests = new List<string>() };
        var f2 = new FactionSystem.Faction { factionId = "bainovia", factionName = "Bainovian Clans", reputation = 2, maxReputation = 5, minReputation = -5, alliedFactions = new List<string>(), enemyFactions = new List<string>(), availableQuests = new List<string>() };
        var f3 = new FactionSystem.Faction { factionId = "spikslia", factionName = "Spikslian Houses", reputation = 0, maxReputation = 5, minReputation = -5, alliedFactions = new List<string>(), enemyFactions = new List<string>(), availableQuests = new List<string>() };
        f1.enemyFactions.Add("storm_cult");
        f1.alliedFactions.Add("bainovia");
        f2.enemyFactions.Add("storm_cult");
        f2.alliedFactions.Add("stormguard");
        faction.factions = new List<FactionSystem.Faction> { f1, f2, f3 };

        // Quest manager
        var qm = new GameObject("QuestManager");
        var quest = qm.AddComponent<QuestSystem>();
        quest.factionSystem = faction;
        quest.player = player.GetComponent<BainoviaCharacterController>();

        var q1 = new QuestSystem.Quest
        {
            questId = "protect_camp",
            questName = "Protect the Camp",
            description = "Hunt down the Frost Brute lurking near the canyon camp.",
            factionId = "bainovia",
            requiredReputation = 1,
            isMainQuest = true,
            isActive = false,
            isCompleted = false,
            objectives = new List<QuestSystem.QuestObjective>
            {
                new QuestSystem.QuestObjective { objectiveId = "kill_brute", description = "Kill the Frost Brute", targetCount = 1, currentCount = 0, type = QuestSystem.QuestObjective.ObjectiveType.Kill, targetId = "frost_brute" }
            },
            rewards = new List<QuestSystem.QuestReward>
            {
                new QuestSystem.QuestReward { type = QuestSystem.QuestReward.RewardType.Gold, amount = 50 },
                new QuestSystem.QuestReward { type = QuestSystem.QuestReward.RewardType.SpiritEssence, amount = 25 }
            }
        };
        var q2 = new QuestSystem.Quest
        {
            questId = "restless_shrine",
            questName = "The Restless Shrine",
            description = "The old shrine beyond the columns demands tribute. Gather rune fragments along the canyon floor.",
            factionId = "bainovia",
            requiredReputation = 0,
            isMainQuest = false,
            isActive = false,
            isCompleted = false,
            objectives = new List<QuestSystem.QuestObjective>
            {
                new QuestSystem.QuestObjective { objectiveId = "collect_fragments", description = "Collect rune fragments", targetCount = 3, currentCount = 0, type = QuestSystem.QuestObjective.ObjectiveType.Collect, targetId = "rune_fragment" }
            },
            rewards = new List<QuestSystem.QuestReward>
            {
                new QuestSystem.QuestReward { type = QuestSystem.QuestReward.RewardType.Gold, amount = 40 },
                new QuestSystem.QuestReward { type = QuestSystem.QuestReward.RewardType.SpiritEssence, amount = 15 }
            }
        };
        quest.availableQuests = new List<QuestSystem.Quest> { q1, q2 };

        // Dialogue manager
        var dm = new GameObject("DialogueManager");
        var dialogue = dm.AddComponent<DialogueSystem>();
        dialogue.player = player.GetComponent<BainoviaCharacterController>();
        dialogue.factionSystem = faction;
        dialogue.questSystem = quest;
        var dlgPanel = ui.transform.Find("DialoguePanel");
        dialogue.dialogueUI = dlgPanel != null ? dlgPanel.gameObject : ui;

        var nodeGreeting = new DialogueSystem.DialogueNode
        {
            nodeId = "greeting",
            speakerName = "Rune Scholar",
            dialogueText = "The storm runes grow restless, Storm-Breaker. A Frost Brute has wandered near the canyon camp. Hunt it, and I will share the shrine's secrets with you.",
            options = new List<DialogueSystem.DialogueOption>
            {
                new DialogueSystem.DialogueOption { optionText = "I will hunt it.", nextNodeId = "accept", givesQuest = true, questId = "protect_camp", endsDialogue = true },
                new DialogueSystem.DialogueOption { optionText = "Tell me of the old shrine.", nextNodeId = "shrine", givesQuest = false, endsDialogue = false },
                new DialogueSystem.DialogueOption { optionText = "Another time.", nextNodeId = "", endsDialogue = true }
            }
        };
        var nodeAccept = new DialogueSystem.DialogueNode
        {
            nodeId = "accept",
            speakerName = "Rune Scholar",
            dialogueText = "May the old spirits guide your claws.",
            options = new List<DialogueSystem.DialogueOption>
            {
                new DialogueSystem.DialogueOption { optionText = "Farewell.", nextNodeId = "", endsDialogue = true }
            }
        };
        var nodeShrine = new DialogueSystem.DialogueNode
        {
            nodeId = "shrine",
            speakerName = "Rune Scholar",
            dialogueText = "Beyond the columns, the Restless Shrine wakes. Gather three rune fragments from the canyon floor and it may calm.",
            options = new List<DialogueSystem.DialogueOption>
            {
                new DialogueSystem.DialogueOption { optionText = "I will gather them.", nextNodeId = "", givesQuest = true, questId = "restless_shrine", endsDialogue = true },
                new DialogueSystem.DialogueOption { optionText = "Perhaps later.", nextNodeId = "", endsDialogue = true }
            }
        };
        dialogue.dialogueNodes = new List<DialogueSystem.DialogueNode> { nodeGreeting, nodeAccept, nodeShrine };

        // Central save system (F5 quick save / F9 quick load). Aggregates the
        // player's live state plus each manager's SaveData()/LoadData().
        var saveGO = new GameObject("SaveSystem");
        var save = saveGO.AddComponent<GameSaveSystem>();
        var saveSO = new SerializedObject(save);
        saveSO.FindProperty("player").objectReferenceValue = player.GetComponent<BainoviaCharacterController>();
        saveSO.FindProperty("dialogueSystem").objectReferenceValue = dialogue;
        saveSO.FindProperty("factionSystem").objectReferenceValue = faction;
        saveSO.FindProperty("inventorySystem").objectReferenceValue = player.GetComponent<InventorySystem>();
        saveSO.FindProperty("questSystem").objectReferenceValue = quest;
        saveSO.FindProperty("skillTreeSystem").objectReferenceValue = player.GetComponent<SkillTreeSystem>();
        saveSO.FindProperty("spiritCompanionSystem").objectReferenceValue = player.GetComponent<SpiritCompanionSystem>();
        saveSO.FindProperty("runeMagicSystem").objectReferenceValue = player.GetComponent<RuneMagicSystem>();
        saveSO.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------------- UI

    static GameObject CreateUI()
    {
        EnsureRuneIconSprites();
        var canvas = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.transform.SetParent(null);
        var c = canvas.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        var rt = (RectTransform)canvas.transform;

        // ---------------- top-left status bars ----------------
        var health = MakeSlider(rt, "HealthBar", new Color(0.8f, 0.15f, 0.15f, 1f));
        SetRect((RectTransform)health.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -30), new Vector2(300, 22));
        AddAsChild(rt, health.gameObject, new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -30), new Vector2(300, 22));
        var healthTxt = MakeText(rt, "HealthText", "100%", 12, new Color(1, 1, 1, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(165, -30), new Vector2(150, 20), TextAnchor.MiddleLeft);

        var spirit = MakeSlider(rt, "SpiritBar", new Color(0.2f, 0.5f, 0.9f, 1f));
        AddAsChild(rt, spirit.gameObject, new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -62), new Vector2(300, 22));

        var rage = MakeSlider(rt, "RageBar", new Color(0.9f, 0.5f, 0.1f, 1f));
        AddAsChild(rt, rage.gameObject, new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -94), new Vector2(300, 22));

        var shield = MakeSlider(rt, "ShieldBar", new Color(0.5f, 0.75f, 1f, 1f));
        AddAsChild(rt, shield.gameObject, new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -126), new Vector2(300, 22));
        var shieldTxt = MakeText(rt, "ShieldText", "", 12, new Color(0.8f, 0.9f, 1f, 1f), new Vector2(0, 1), new Vector2(0, 1), new Vector2(165, -126), new Vector2(150, 20), TextAnchor.MiddleLeft);

        // Berserk indicator
        var berserk = MakeImage(rt, "BerserkIndicator", new Color(0.95f, 0.35f, 0.05f, 0.9f), new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -158), new Vector2(120, 26));
        var bText = MakeText((RectTransform)berserk.transform, "BerserkText", "BERSERK", 12, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bText.transform.SetParent(berserk.transform, false);
        berserk.gameObject.SetActive(false);

        // Rune cooldown icons (top-right): static base + radial fill overlay.
        // Base shows the per-rune icon art (Assets/UI/Icons); the overlay is
        // tinted in the rune's element color and drains radially on cooldown.
        var runeImages = new Image[6];
        for (int i = 0; i < 6; i++)
        {
            var baseIcon = MakeImage(rt, "RuneCooldown" + i, new Color(0.12f, 0.15f, 0.22f, 0.95f), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-120 - i * 70, -40), new Vector2(50, 50));
            var iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Rune_" + RuneName(i) + ".png");
            if (iconSprite != null) baseIcon.sprite = iconSprite;
            else Debug.LogWarning($"Rune HUD icon missing (not a Sprite yet?): Assets/UI/Icons/Rune_{RuneName(i)}.png");
            var overlay = MakeImage((RectTransform)baseIcon.transform, "Overlay" + i, RuneOverlayColor(i), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Radial360;
            overlay.fillOrigin = 0;
            overlay.fillClockwise = true;
            overlay.fillAmount = 0f;
            var overlayRect = (RectTransform)overlay.transform;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            MakeText(rt, "RuneLabel" + i, RuneLabel(i), 10, new Color(0.8f, 0.9f, 1f, 1f), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-120 - i * 70, -18), new Vector2(60, 16));
            MakeText(rt, "RuneCooldownText" + i, "", 14, Color.white, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-120 - i * 70, -40), new Vector2(50, 50));
            runeImages[i] = overlay;
        }

        // Quest panel (left)
        var questPanel = MakeImage(rt, "QuestPanel", new Color(0.08f, 0.1f, 0.16f, 0.85f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(160, -150), new Vector2(360, 150));
        var qt = MakeText((RectTransform)questPanel.transform, "QuestTitle", "", 15, new Color(1f, 0.9f, 0.5f, 1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -18), new Vector2(330, 22), TextAnchor.MiddleLeft);
        qt.transform.SetParent(questPanel.transform, false);
        var qd = MakeText((RectTransform)questPanel.transform, "QuestDescription", "", 12, Color.white, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, -14), new Vector2(330, 44), TextAnchor.MiddleLeft);
        qd.transform.SetParent(questPanel.transform, false);
        var qObj = MakeText((RectTransform)questPanel.transform, "QuestObjective", "", 12, new Color(0.9f, 0.95f, 1f, 1f), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 26), new Vector2(330, 16), TextAnchor.MiddleLeft);
        qObj.transform.SetParent(questPanel.transform, false);
        var qp = MakeSlider(questPanel.transform as RectTransform, "QuestProgress", new Color(1f, 0.85f, 0.3f, 1f));
        SetRect((RectTransform)qp.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 10), new Vector2(330, 12));
        qp.transform.SetParent(questPanel.transform, false);

        // Interaction prompt (bottom center)
        var prompt = MakeImage(rt, "InteractionPrompt", new Color(0.05f, 0.06f, 0.1f, 0.85f), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(520, 46));
        var pt = MakeText((RectTransform)prompt.transform, "PromptText", "", 15, new Color(0.9f, 0.95f, 1f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        pt.transform.SetParent(prompt.transform, false);
        prompt.gameObject.SetActive(true);

        // Dialogue panel (anchored bottom-center; 265px tall so 9 numbered options never overlap the body)
        var dlgPanel = MakeImage(rt, "DialoguePanel", new Color(0.05f, 0.06f, 0.1f, 0.92f), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(900, 265));
        var dlgTitle = MakeText((RectTransform)dlgPanel.transform, "DialogueSpeaker", "", 16, new Color(1f, 0.85f, 0.5f, 1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -18), new Vector2(860, 22), TextAnchor.MiddleLeft);
        dlgTitle.transform.SetParent(dlgPanel.transform, false);
        var dlgBody = MakeText((RectTransform)dlgPanel.transform, "DialogueBody", "", 14, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -52), new Vector2(860, 34), TextAnchor.UpperLeft);
        dlgBody.transform.SetParent(dlgPanel.transform, false);
        var dlgHint = MakeText((RectTransform)dlgPanel.transform, "DialogueHint", "Press 1-9 to choose, ESC to exit", 11, new Color(0.6f, 0.65f, 0.7f, 1f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 10), new Vector2(860, 16));
        dlgHint.transform.SetParent(dlgPanel.transform, false);

        var dlgOptions = new Text[9];
        for (int i = 0; i < 9; i++)
        {
            var opt = MakeText((RectTransform)dlgPanel.transform, "DialogueOption" + i, "", 11, Color.white, new Vector2(0, 0), new Vector2(0, 0), new Vector2(10, 28 + i * 15), new Vector2(860, 17), TextAnchor.MiddleLeft);
            opt.transform.SetParent(dlgPanel.transform, false);
            dlgOptions[i] = opt;
        }
        dlgPanel.gameObject.SetActive(false);

        // EventSystem
        var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        _ = es;

        // GameUI wiring
        var gameUI = canvas.gameObject.AddComponent<GameUI>();
        gameUI.healthBar = health;
        gameUI.healthText = healthTxt;
        gameUI.spiritBar = spirit;
        gameUI.rageBar = rage;
        gameUI.rageText = null;
        gameUI.berserkIndicator = berserk.gameObject;
        gameUI.shieldBar = shield;
        gameUI.shieldText = shieldTxt;
        gameUI.questPanel = questPanel.gameObject;
        gameUI.questTitle = qt;
        gameUI.questDescription = qd;
        gameUI.questObjectiveText = qObj;
        gameUI.questProgress = qp;
        gameUI.runeCooldownImages = runeImages;
        gameUI.runeCooldownTexts = ActualRuneTexts(rt);

        // Capture the dialogue text refs here; the DialogueSystem (created in
        // CreateSystems, which runs after CreateUI) is wired up in WireDialogueUI.
        uiDialogueSpeaker = dlgTitle.gameObject;
        uiDialogueBody = dlgBody.gameObject;
        uiDialogueHint = dlgHint.gameObject;
        uiDialogueOptions = new GameObject[dlgOptions.Length];
        for (int i = 0; i < dlgOptions.Length; i++)
            uiDialogueOptions[i] = dlgOptions[i].gameObject;

        // Interaction prompt references
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var inter = player.GetComponent<InteractionSystem>();
            var so2 = new SerializedObject(inter);
            so2.FindProperty("interactionPrompt").objectReferenceValue = prompt.gameObject;
            so2.FindProperty("promptText").objectReferenceValue = pt;
            so2.ApplyModifiedProperties();
        }

        return canvas.gameObject;
    }

    static void AddAsChild(RectTransform parent, GameObject go, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        SetRect(rt, aMin, aMax, pos, size);
    }

    static string RuneLabel(int i)
    {
        string[] labels = { "Q Storm", "W Frost", "E Wind", "R Spirit", "F Blood", "D Forb" };
        return i < labels.Length ? labels[i] : "";
    }

    static string RuneName(int i)
    {
        string[] names = { "Storm", "Frost", "Wind", "Spirit", "Blood", "Forbidden" };
        return i < names.Length ? names[i] : "";
    }

    /// <summary>
    /// Element colors from Magic_Runes_System.md (Storm electric blue, Frost
    /// pale ice, Wind light green, Spirit violet, Blood deep red, Forbidden
    /// dark purple). Alpha 0.9 so the base icon art reads through the overlay.
    /// </summary>
    static Color RuneOverlayColor(int i)
    {
        Color[] colors =
        {
            new Color(0f, 0.75f, 1f, 0.9f),
            new Color(0.75f, 0.92f, 1f, 0.9f),
            new Color(0.56f, 0.93f, 0.56f, 0.9f),
            new Color(0.66f, 0.33f, 0.97f, 0.9f),
            new Color(0.76f, 0.07f, 0.12f, 0.9f),
            new Color(0.55f, 0.25f, 0.85f, 0.9f),
        };
        return i < colors.Length ? colors[i] : new Color(0.3f, 0.55f, 1f, 0.9f);
    }

    /// <summary>
    /// Forces Assets/UI/Icons/Rune_*.png to import as Sprites so the HUD can
    /// reference them. Runs at the start of CreateUI, before any LoadAsset.
    /// </summary>
    static void EnsureRuneIconSprites()
    {
        for (int i = 0; i < 6; i++)
        {
            string path = "Assets/UI/Icons/Rune_" + RuneName(i) + ".png";
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) continue;
            if (imp.textureType != TextureImporterType.Sprite || imp.spriteImportMode != SpriteImportMode.Single)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spritePixelsPerUnit = 100;
                imp.SaveAndReimport();
                Debug.Log($"Rune icon import configured (Sprite) -> {path}");
            }
        }
    }

    static Text[] ActualRuneTexts(RectTransform rt)
    {
        var list = new List<Text>();
        for (int i = 0; i < 6; i++)
        {
            var found = rt.Find("RuneCooldownText" + i);
            if (found != null)
                list.Add(found.GetComponent<Text>());
            else
                list.Add(null);
        }
        return list.ToArray();
    }
}