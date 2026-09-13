#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;
using System.Linq;

public static class SceneValidator
{
    const string ScenePath = "Assets/Scenes/BainoviaPrototype.unity";

    [MenuItem("Bainovia/Validate Scene")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var log = new StringBuilder();
        log.AppendLine("=== SCENE VALIDATION ===");

        var all = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject);
        var gos = all.ToArray();

        // 1) Missing scripts
        int missing = 0;
        foreach (var go in gos)
        {
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) { missing++; log.AppendLine($"MISSING SCRIPT on {go.name}"); break; }
            }
        }
        log.AppendLine($"missing MonoBehaviour scripts: {missing}");

        // 2) Objects that appear to be pure gameplay/spawn logic with no renderer/audio (sanity)
        int empty = 0;
        foreach (var go in gos)
        {
            if (go.transform.childCount == 0 && go.GetComponent<Renderer>() == null &&
                go.GetComponents<Component>().All(c => c == null || c is Transform || c is MonoBehaviour))
            {
                empty++;
                log.AppendLine($"EMPTY GO (logic-only): {go.name} @ {go.transform.position}");
            }
        }
        log.AppendLine($"empty logic-only objects: {empty}");

        // 3) Missing mesh references on renderers
        int noMesh = 0;
        foreach (var go in gos)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh == null)
            {
                noMesh++;
                log.AppendLine($"NO MESH on {go.name}");
            }
        }
        log.AppendLine($"renderers without mesh: {noMesh}");

        // 4) Duplicate names that could break Find() logic
        var dup = gos.GroupBy(g => g.name).Where(g => g.Count() > 1).Select(g => $"{g.Key} x{g.Count()}");
        log.AppendLine($"duplicate-named objects: {(dup.Count() == 0 ? "none" : string.Join(", ", dup))}");

        // 5) Script component counts per type (see which gameplay systems are wired)
        var counts = new System.Collections.Generic.Dictionary<string, int>();
        foreach (var go in gos)
        {
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                var t = mb.GetType().Name;
                counts[t] = counts.ContainsKey(t) ? counts[t] + 1 : 1;
            }
        }
        log.AppendLine("=== SCRIPT COMPONENT COUNTS ===");
        foreach (var kv in counts.OrderByDescending(k => k.Value))
            log.AppendLine($"  {kv.Key}: {kv.Value}");

        UnityEngine.Debug.Log(log.ToString());
    }
}
#endif