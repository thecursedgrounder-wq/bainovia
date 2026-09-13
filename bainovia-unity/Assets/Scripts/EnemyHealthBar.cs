using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enemy Health Bar - World-space overhead HP bar for enemies.
/// Attached to enemies by the scene builder; builds its own canvas at runtime.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Settings")]
    public float heightAbove = 2.6f;
    public float width = 2f;
    public float height = 0.28f;

    [Header("References")]
    public Health health;

    private Transform barRect;
    private Image fill;

    void Start()
    {
        if (health == null) health = GetComponent<Health>();
        BuildBar();

        if (health != null)
        {
            health.onHealthChanged.AddListener(_ => Refresh());
            health.onDeath.AddListener(Hide);
        }
    }

    void BuildBar()
    {
        var canvasGo = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasScaler));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.SetParent(transform, false);
        canvasRect.localPosition = new Vector3(0f, heightAbove, 0f);

        float inverseScale = 1f;
        Vector3 lossy = transform.lossyScale;
        float maxScale = Mathf.Max(Mathf.Abs(lossy.x), Mathf.Max(Mathf.Abs(lossy.y), Mathf.Abs(lossy.z)));
        if (maxScale > 0.01f) inverseScale = 1f / maxScale;
        canvasRect.localScale = new Vector3(inverseScale, inverseScale, inverseScale);
        canvasRect.sizeDelta = new Vector2(width, height);

        var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        var bgRect = (RectTransform)bgGo.transform;
        bgRect.SetParent(canvasRect, false);
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgGo.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.08f, 0.92f);

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        var fillRect = (RectTransform)fillGo.transform;
        fillRect.SetParent(canvasRect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(1.5f, 1.5f);
        fillRect.offsetMax = new Vector2(-1.5f, -1.5f);
        fill = fillGo.GetComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.color = new Color(0.85f, 0.2f, 0.2f, 1f);

        barRect = canvasRect;

        Refresh();
    }

    void Refresh()
    {
        if (fill != null && health != null)
            fill.fillAmount = health.GetHealthPercent();
    }

    void Hide()
    {
        if (barRect != null)
            barRect.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (barRect != null && Camera.main != null)
            barRect.rotation = Camera.main.transform.rotation;
    }
}