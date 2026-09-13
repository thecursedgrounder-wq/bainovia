using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Headless rendering probe for the dream-loop workflow. Opens the prototype
/// scene, enters play mode WITHOUT -nographics so a frame actually renders,
/// hides the HUD for a clean art shot, warms up a few frames for animators /
/// navmesh, captures a 1366x768 PNG and quits the editor.
/// </summary>
public static class BainoviaScreenshot
{
    const string ScenePath = "Assets/Scenes/BainoviaPrototype.unity";
    const int WarmupFrames = 60;
    const int ShotWidth = 1366;
    const int ShotHeight = 768;

    static string outputPath;
    static int frame;

    public static void CaptureBaseline()
    {
        CaptureTo("C:/Users/thecu/OneDrive/Documents/Default Project/.dream-loop/shots/baseline.png");
    }

    public static void CapturePass1()
    {
        CaptureTo("C:/Users/thecu/OneDrive/Documents/Default Project/.dream-loop/shots/pass1.png");
    }

    /// <summary>
    /// Player-eye probe: stages the shot from the spawn position with the
    /// gameplay follow-cam pose, so we verify what the actual game view shows
    /// (not just the art-director cinematic).
    /// </summary>
    public static void CaptureGameplay()
    {
        gameplayShot = true;
        CaptureTo("C:/Users/thecu/OneDrive/Documents/Default Project/.dream-loop/shots/gameview.png");
    }

    static bool gameplayShot;

    static void CaptureTo(string path)
    {
        outputPath = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        if (File.Exists(outputPath)) File.Delete(outputPath);

        // Keep state alive across the play-mode transition: batch + editor
        // otherwise reloads the domain and clears our editor update hook.
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;

        EditorSceneManager.OpenScene(ScenePath);
        frame = 0;
        Application.targetFrameRate = 60;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
        Debug.Log("DREAMSHOT entered play mode, awaiting frames...");
    }

    static void Tick()
    {
        frame++;
        try
        {
            if (frame == 3)
            {
                Screen.SetResolution(ShotWidth, ShotHeight, false);
                var hud = GameObject.Find("HUD");
                if (hud != null) hud.SetActive(false);

                // Deterministic stage: pin the player at spawn and park every
                // camera-facing script so frames are reproducible across rounds.
                var player = GameObject.Find("Player");
                if (player != null)
                {
                    var agent = player.GetComponent<NavMeshAgent>();
                    if (agent != null) agent.enabled = false;
                    player.transform.position = new Vector3(0f, 1f, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                foreach (var c in Object.FindObjectsByType<CameraController>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
                    c.enabled = false;
                var cam = Object.FindObjectOfType<Camera>();
                if (cam != null)
                {
                    if (gameplayShot)
                    {
                        // Gameplay-eye: look down the beaten path toward camp.
                        cam.transform.position = new Vector3(0f, 1.7f, -1.4f);
                        cam.transform.LookAt(new Vector3(0f, 1f, 9f), Vector3.up);
                    }
                    else
                    {
                        cam.transform.position = new Vector3(5.5f, 7f, -16f);
                        cam.transform.LookAt(new Vector3(0f, 4f, 8f), Vector3.up);
                    }
                }
            }

            if (frame >= WarmupFrames)
            {
                RenderAndSave();
                Debug.Log("DREAMSHOT captured -> " + outputPath + " (frame " + frame + ")");
                EditorApplication.update -= Tick;
                EditorApplication.ExitPlaymode();
                EditorApplication.Exit(0);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            EditorApplication.update -= Tick;
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Batch-friendly capture: screen capture APIs need a GameView that does not
    /// exist in batch mode, so render the main camera into a RenderTexture and
    /// read the pixels back manually. Works headless as long as a GPU device is
    /// present (invoke WITHOUT -nographics).
    /// </summary>
    static void RenderAndSave()
    {
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        foreach (var c in cams)
            Debug.Log($"DREAMSHOT cam '{c.name}' en={c.enabled} clear={(int)c.clearFlags} bg={c.backgroundColor} pos={c.transform.position} rot={c.transform.eulerAngles} solid={c.depth}");

        var cam = cams.Length > 0 ? cams[0] : null;
        if (cam == null) { Debug.LogError("DREAMSHOT no camera found"); return; }

        // Belt & braces: never let a leftover default skybox paint the frame.
        var prevSky = RenderSettings.skybox;
        RenderSettings.skybox = null;
        cam.clearFlags = CameraClearFlags.SolidColor;
        var prevBg = cam.backgroundColor;
        cam.backgroundColor = new Color(0.045f, 0.07f, 0.11f);

        var rt = new RenderTexture(ShotWidth, ShotHeight, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(ShotWidth, ShotHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, ShotWidth, ShotHeight), 0, 0);
        tex.Apply(false);
        var px = tex.GetPixel(ShotWidth / 2, ShotHeight / 2);
        Debug.Log($"DREAMSHOT centerpx={px}");
        var py = tex.GetPixel(ShotWidth / 2, 40);
        Debug.Log($"DREAMSHOT skyrowpx={py}");
        RenderTexture.active = previous;
        cam.targetTexture = null;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = prevBg;
        RenderSettings.skybox = prevSky;
        var bytes = tex.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(rt);
    }
}