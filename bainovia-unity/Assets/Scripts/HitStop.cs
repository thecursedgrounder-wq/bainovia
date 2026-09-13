using System.Collections;
using UnityEngine;

/// <summary>
/// HitStop - tiny timescale-freeze utility inspired by the hit-stop / camera-shake
/// feel of open-source action RPG combat scripts (Kenji966/Rpg-Action-Character-
/// Controller-Combat-System). Briefly freezes all gameplay while leaving UI timers
/// (which use unscaled time) unaffected.
/// </summary>
public static class HitStop
{
    private static MonoBehaviour host;
    private static float restoredTimeScale = 1f;
    private static bool frozen;

    /// <summary>Freeze the game for <paramref name="duration"/> seconds (realtime).</summary>
    public static void Pause(float duration)
    {
        if (frozen || duration <= 0f)
            return;

        if (host == null)
            host = CreateHost();

        restoredTimeScale = Time.timeScale;
        frozen = true;
        host.StartCoroutine(FreezeRoutine(duration));
    }

    static MonoBehaviour CreateHost()
    {
        GameObject go = new GameObject("HitStopRunner");
        Object.DontDestroyOnLoad(go);
        return go.AddComponent<HitStopHost>();
    }

    static IEnumerator FreezeRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = restoredTimeScale;
        frozen = false;
    }

    /// <summary>
    /// Host makes it possible to run a coroutine from a static context.
    /// </summary>
    class HitStopHost : MonoBehaviour
    {
    }
}