using UnityEngine;

/// <summary>
/// Combat FX - Static helpers for world-space combat feedback (floating damage numbers).
/// Created at runtime, so needs no scene wiring.
/// </summary>
public static class CombatFX
{
    static Font cachedFont;

    static Font GetFont()
    {
        if (cachedFont == null)
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return cachedFont;
    }

    public static void ShowDamage(Vector3 position, int amount, Color color)
    {
        SpawnText(position, amount.ToString(), color);
    }

    public static void ShowText(Vector3 position, string text, Color color)
    {
        SpawnText(position, text, color);
    }

    static void SpawnText(Vector3 position, string text, Color color)
    {
        var go = new GameObject("DamageNumber");
        go.transform.position = position + Vector3.up * 0.6f;

        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.font = GetFont();
        tm.fontSize = 96;
        tm.characterSize = 0.075f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;

        var fx = go.AddComponent<DamageNumberFX>();
        fx.lifeTime = 0.9f;
        fx.riseSpeed = 1.6f;
    }
}

/// <summary>
/// DamageNumberFX - rises, faces the camera, fades out, then destroys itself.
/// Secondary runtime-added class (resolved by type at runtime, not scene GUID).
/// </summary>
public class DamageNumberFX : MonoBehaviour
{
    public float lifeTime = 0.9f;
    public float riseSpeed = 1.6f;

    private float age;
    private TextMesh textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMesh>();
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;

        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = Mathf.Clamp01(1f - age / lifeTime);
            textMesh.color = c;
        }

        if (age >= lifeTime)
            Destroy(gameObject);
    }
}