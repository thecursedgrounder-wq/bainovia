#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Builds the CURRENT saved prototype scene to Windows without regenerating it,
/// so placement fixes (boss position, duplicate-wolf removal, ground snaps) are
/// preserved. BuildPrototypeScene() would otherwise recreate the flawed layout.
/// </summary>
public static class BuildCurrentScene
{
    const string ScenePath = "Assets/Scenes/BainoviaPrototype.unity";
    const string BuildDir = "Builds/Windows";
    const string BuildExe = "Bainovia.exe";

    public static void Run()
    {
        if (!File.Exists(ScenePath))
        {
            UnityEngine.Debug.LogError($"BUILD ABORTED: scene not found at {ScenePath}");
            EditorApplication.Exit(1);
            return;
        }

        EditorSceneManager.OpenScene(ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();

        if (!Directory.Exists(BuildDir))
            Directory.CreateDirectory(BuildDir);

        var report = BuildPipeline.BuildPlayer(
            new[] { ScenePath },
            Path.Combine(BuildDir, BuildExe),
            BuildTarget.StandaloneWindows64,
            BuildOptions.None);

        UnityEngine.Debug.Log("BUILD-CURRENT result: " + report.summary.result);
        UnityEngine.Debug.Log("BUILD-CURRENT total errors: " + report.summary.totalErrors);

        if (report.summary.result != BuildResult.Succeeded)
        {
            UnityEngine.Debug.LogError("BUILD-CURRENT FAILED");
            EditorApplication.Exit(1);
            return;
        }

        UnityEngine.Debug.Log("BUILD-CURRENT SUCCEEDED -> " + Path.GetFullPath(Path.Combine(BuildDir, BuildExe)));
        EditorApplication.Exit(0);
    }
}
#endif