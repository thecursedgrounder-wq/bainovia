using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy Status - Applies crowd-control style effects (slow) to enemies and tints them.
/// Attached to enemies in the scene by the scene builder.
/// </summary>
public class EnemyStatus : MonoBehaviour
{
    [Header("Status")]
    public float slowTimer;
    public float slowFactor = 1f;

    [Header("References")]
    public EnemyAI ai;
    public NavMeshAgent navAgent;
    public MeshRenderer[] meshes;

    private Material[] tintMaterials;
    private Color[] baseColors;

    void Start()
    {
        if (ai == null) ai = GetComponent<EnemyAI>();
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (meshes == null || meshes.Length == 0)
            meshes = GetComponentsInChildren<MeshRenderer>(true);

        if (meshes != null && meshes.Length > 0)
        {
            tintMaterials = new Material[meshes.Length];
            baseColors = new Color[meshes.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                // Instance materials so tinting one enemy never bleeds into other
                // NPCs sharing the same imported GLB material.
                tintMaterials[i] = meshes[i].material;
                baseColors[i] = tintMaterials[i].color;
            }
        }
    }

    public void ApplySlow(float duration, float factor)
    {
        slowTimer = Mathf.Max(slowTimer, duration);
        slowFactor = Mathf.Min(slowFactor, factor);
    }

    void Update()
    {
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;

            // Apply the slow multiplier only while actively slowed, so the AI's
            // own chase/patrol speed writes are respected the rest of the time.
            if (navAgent != null && navAgent.enabled)
            {
                float baseSpeed = ai != null && ai.chaseSpeed > 0f ? ai.chaseSpeed : navAgent.speed;
                navAgent.speed = baseSpeed * slowFactor;
            }

            if (slowTimer <= 0)
            {
                slowFactor = 1f;
                if (navAgent != null && navAgent.enabled && ai != null)
                    navAgent.speed = ai.chaseSpeed;
            }
        }

        // Tint toward icy blue while slowed.
        if (tintMaterials != null)
        {
            for (int i = 0; i < tintMaterials.Length; i++)
            {
                if (tintMaterials[i] == null) continue;
                if (slowFactor < 0.999f)
                    tintMaterials[i].color = Color.Lerp(tintMaterials[i].color, new Color(0.55f, 0.75f, 1f, 1f), Time.deltaTime * 8f);
                else
                    tintMaterials[i].color = Color.Lerp(tintMaterials[i].color, baseColors[i], Time.deltaTime * 8f);
            }
        }
    }
}