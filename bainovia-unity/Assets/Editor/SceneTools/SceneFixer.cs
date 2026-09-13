#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public static class SceneFixer
{
    const string ScenePath = "Assets/Scenes/BainoviaPrototype.unity";

    [MenuItem("Bainovia/Fix Scene Placement")]
    public static void ApplyPlacementFixes()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var roots = scene.GetRootGameObjects();
        var log = new System.Text.StringBuilder();
        log.AppendLine("=== SCENE FIXER REPORT ===");

        // 1) Remove duplicate Thunder Wolf (keep the one at x=-10, remove any with x > -9)
        int removed = 0;
        foreach (var go in roots.Where(o => o.name == "Thunder Wolf"))
        {
            if (go.transform.position.x > -9f)
            {
                var pos = go.transform.position;
                Object.DestroyImmediate(go);
                removed++;
                log.AppendLine($"REMOVE duplicate Thunder Wolf at ({pos.x:F2},{pos.y:F2},{pos.z:F2})");
            }
        }
        log.AppendLine($"duplicate wolves removed: {removed}");

        Move(roots, "Frost Brute", new Vector3(4.5f, 0f, 9.5f), log);
        Move(roots, "Alpha Loup-Tonnerre", new Vector3(24f, 0f, -32f), log);

        // 2) Report object counts + positions for verification
        var allRoots = scene.GetRootGameObjects().Where(o => o != null).ToArray();
        log.AppendLine($"=== ROOT COUNT: {allRoots.Length} ===");
        foreach (var go in allRoots.OrderBy(o => o.name))
        {
            log.AppendLine($"ROOT {go.name} @ ({go.transform.position.x:F1},{go.transform.position.y:F1},{go.transform.position.z:F1})");
        }

        // 3) Sinking/floating check: for each prop-like root, compute lowest child mesh point vs ground (y=0)
        log.AppendLine("=== HEIGHT CHECK (lowest mesh world-Y of each root) ===");
        var snapList = new HashSet<string> { "Barrel1", "Barrel2", "BrokenColumn1", "BrokenColumn2" };
        foreach (var go in allRoots.Where(o => o != null).OrderBy(o => o.name))
        {
            float minY = float.PositiveInfinity;
            foreach (var mf in go.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null || mf.transform.lossyScale.y == 0) continue;
                var b = mf.sharedMesh.bounds;
                var corners = new[] {
                    new Vector3(b.min.x, b.min.y, b.min.z), new Vector3(b.min.x, b.min.y, b.max.z),
                    new Vector3(b.min.x, b.max.y, b.min.z), new Vector3(b.min.x, b.max.y, b.max.z),
                    new Vector3(b.max.x, b.min.y, b.min.z), new Vector3(b.max.x, b.min.y, b.max.z),
                    new Vector3(b.max.x, b.max.y, b.min.z), new Vector3(b.max.x, b.max.y, b.max.z),
                };
                foreach (var c in corners)
                {
                    var w = mf.transform.TransformPoint(c);
                    if (w.y < minY) minY = w.y;
                }
            }
            if (minY != float.PositiveInfinity)
            {
                var flag = minY < -0.05f ? "SUNK" : (minY > 0.3f ? "FLOATING" : "ok");
                if (flag != "ok")
                    log.AppendLine($"HEIGHT {go.name}: minY={minY:F3} -> {flag}");
                if (flag == "SUNK" && snapList.Contains(go.name))
                {
                    var p = go.transform.position;
                    var ny = p.y - minY;
                    go.transform.position = new Vector3(p.x, ny, p.z);
                    log.AppendLine($"SNAP {go.name}: raised y {p.y:F3} -> {ny:F3}");
                }
            }
        }

        EditorSceneManager.SaveScene(scene);
        UnityEngine.Debug.Log(log.ToString());
    }

    static void Move(GameObject[] roots, string name, Vector3 target, System.Text.StringBuilder log)
    {
        var found = roots.Where(o => o != null && o.name == name).ToList();
        if (found.Count == 0)
        {
            log.AppendLine($"!! {name}: NOT FOUND");
            return;
        }
        foreach (var go in found)
        {
            var pos = go.transform.position;
            go.transform.position = target;
            log.AppendLine($"MOVE {name}: ({pos.x:F2},{pos.y:F2},{pos.z:F2}) -> ({target.x:F2},{target.y:F2},{target.z:F2})");
        }
    }
}
#endif