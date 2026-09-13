using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Rune Magic System - Manages rune abilities, cooldowns, and combinations.
/// Casts are aimed at the mouse-cursor ground point; holding the backtick (`) key
/// locks onto the entity currently under the cursor (League-style target-only mode).
/// </summary>
public class RuneMagicSystem : MonoBehaviour
{
    [Header("Rune Settings")]
    public int maxSpiritEssence = 100;
    public int currentSpiritEssence;
    public float runeCooldown = 8f;
    public float spiritCooldown = 15f;

    [Header("Rune Types")]
    public RuneType[] availableRunes;

    [Header("VFX")]
    public GameObject[] runeVFX;
    public AudioClip[] runeSounds;

    [Header("References")]
    public BainoviaCharacterController characterController;
    public AudioSource audioSource;

    private Dictionary<RuneType, RuneData> runeDictionary = new Dictionary<RuneType, RuneData>();
    private Dictionary<RuneType, float> runeCooldowns = new Dictionary<RuneType, float>();
    private List<RuneType> activeCombo = new List<RuneType>();
    private float comboWindow = 3f;
    private float comboTimer;

    private int enemyMask = -1;
    private float essenceRegenAccumulator;

    [System.Serializable]
    public class RuneData
    {
        public RuneType type;
        public string name;
        public int spiritCost;
        public float cooldown;
        public float damage;
        public float range;
        public GameObject vfx;
        public AudioClip sound;
        public int level;
    }

    public enum RuneType
    {
        Storm,
        Frost,
        Wind,
        Spirit,
        Blood,
        Forbidden
    }

    void Start()
    {
        currentSpiritEssence = maxSpiritEssence;
        enemyMask = LayerMask.GetMask("Enemy");
        InitializeRunes();
    }

    void Update()
    {
        UpdateCooldowns();
        UpdateComboTimer();
        RegenerateSpiritEssence();
    }

    void InitializeRunes()
    {
        foreach (RuneType rune in availableRunes)
        {
            RuneData data = new RuneData
            {
                type = rune,
                name = rune.ToString(),
                spiritCost = GetSpiritCost(rune),
                cooldown = runeCooldown,
                damage = GetDamage(rune),
                range = GetRange(rune),
                level = 1
            };

            runeDictionary[rune] = data;
            runeCooldowns[rune] = 0f;
        }
    }

    void UpdateCooldowns()
    {
        foreach (var rune in runeCooldowns.Keys.ToList())
        {
            if (runeCooldowns[rune] > 0)
            {
                runeCooldowns[rune] -= Time.deltaTime;
            }
        }
    }

    void UpdateComboTimer()
    {
        if (activeCombo.Count > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                activeCombo.Clear();
            }
        }
    }

    void RegenerateSpiritEssence()
    {
        if (currentSpiritEssence >= maxSpiritEssence)
        {
            essenceRegenAccumulator = 0f;
            return;
        }

        essenceRegenAccumulator += Time.deltaTime;
        if (essenceRegenAccumulator >= 1f)
        {
            essenceRegenAccumulator -= 1f;
            currentSpiritEssence = Mathf.Min(currentSpiritEssence + 1, maxSpiritEssence);
        }
    }

    // ---------------------------------------------------------------- casting

    /// <summary>
    /// Legacy signature: casts toward the ground point under the cursor.
    /// </summary>
    public bool CastRune(RuneType runeType)
    {
        return CastRune(runeType, false);
    }

    /// <summary>
    /// Casts a rune toward the mouse-cursor ground point.
    /// If <paramref name="targetOnly"/> is true (backtick held), the cast only fires
    /// when an enemy is directly under the cursor and locks onto it.
    /// </summary>
    public bool CastRune(RuneType runeType, bool targetOnly)
    {
        if (!runeDictionary.ContainsKey(runeType))
            return false;

        RuneData rune = runeDictionary[runeType];

        if (runeCooldowns[runeType] > 0)
            return false;

        if (currentSpiritEssence < rune.spiritCost)
            return false;

        Vector3 aimPoint = GetMouseWorldPoint();
        Health locked = GetEnemyUnderCursor();
        Vector3 center = aimPoint;

        if (rune.type == RuneType.Spirit)
            center = transform.position;

        if (targetOnly)
        {
            if (locked == null || !WithinCastRange(locked.transform.position, rune.range))
            {
                Debug.Log("Target-only mode: no valid entity under cursor.");
                return false;
            }
            center = locked.transform.position;
        }
        else if (locked != null && WithinCastRange(locked.transform.position, rune.range))
        {
            // Snap to the entity directly under the cursor when in reach.
            center = locked.transform.position;
        }

        currentSpiritEssence -= rune.spiritCost;
        runeCooldowns[runeType] = rune.cooldown;

        activeCombo.Add(runeType);
        comboTimer = comboWindow;

        CheckCombo();

        ExecuteRuneEffect(rune, center);

        if (characterController != null)
            characterController.AddRage(10f);

        return true;
    }

    bool WithinCastRange(Vector3 targetPos, float runeRange)
    {
        return Vector3.SqrMagnitude(targetPos - transform.position) <= runeRange * runeRange;
    }

    Vector3 GetMouseWorldPoint()
    {
        if (Camera.main == null)
            return transform.position + transform.forward * 5f;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return transform.position + transform.forward * 5f;
    }

    Health GetEnemyUnderCursor()
    {
        if (Camera.main == null)
            return null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, enemyMask))
        {
            Health h = hit.collider.GetComponent<Health>();
            if (h != null && !h.IsDead())
                return h;
        }
        return null;
    }

    // ---------------------------------------------------------------- effects

    void ExecuteRuneEffect(RuneData rune, Vector3 center)
    {
        if (rune.sound != null && audioSource != null)
            audioSource.PlayOneShot(rune.sound);

        switch (rune.type)
        {
            case RuneType.Storm:
                SpawnBurst(center, new Color(0.7f, 0.8f, 1f, 1f), 2.5f);
                DamageAround(center, rune.range, rune.damage);
                break;

            case RuneType.Frost:
                SpawnBurst(center, new Color(0.6f, 0.85f, 1f, 1f), 2f);
                foreach (Collider c in OverlapEnemies(center, rune.range))
                {
                    Health h = c.GetComponent<Health>();
                    if (h == null) continue;

                    h.TakeDamage(Mathf.RoundToInt(rune.damage));

                    EnemyStatus status = c.GetComponent<EnemyStatus>();
                    if (status == null)
                        status = c.gameObject.AddComponent<EnemyStatus>();
                    status.ApplySlow(3.5f, 0.4f);
                }
                break;

            case RuneType.Wind:
                SpawnBurst(center, new Color(0.9f, 0.95f, 1f, 1f), 2f);
                foreach (Collider c in OverlapEnemies(center, rune.range))
                {
                    Health h = c.GetComponent<Health>();
                    if (h == null) continue;

                    h.TakeDamage(Mathf.RoundToInt(rune.damage * 0.6f));

                    NavMeshAgent agent = c.GetComponent<NavMeshAgent>();
                    if (agent != null)
                    {
                        Vector3 dir = (c.transform.position - center).normalized;
                        dir.y = 0f;
                        if (dir.sqrMagnitude > 0.01f)
                            agent.Move(dir * 5f);
                    }
                }
                break;

            case RuneType.Spirit:
                if (characterController != null)
                {
                    characterController.ApplySpiritShield(40);
                    characterController.Heal(10);
                }
                SpawnBurst(transform.position, new Color(0.5f, 0.8f, 1f, 1f), 2f);
                break;

            case RuneType.Blood:
                int hits = DamageAround(center, rune.range, rune.damage);
                if (characterController != null && hits > 0)
                {
                    int heal = Mathf.Max(1, Mathf.RoundToInt(rune.damage * 0.4f * Mathf.Min(hits, 3)));
                    characterController.Heal(heal);
                }
                SpawnBurst(center, new Color(0.8f, 0.2f, 0.3f, 1f), 2.5f);
                break;

            case RuneType.Forbidden:
                SpawnBurst(center, new Color(1f, 0.3f, 0.7f, 1f), 4f);
                DamageAround(center, rune.range, rune.damage);
                if (characterController != null)
                    characterController.TakeDamage(15);
                break;
        }

        Debug.Log($"Cast {rune.name} rune at {center}");
    }

    Collider[] OverlapEnemies(Vector3 center, float radius)
    {
        return Physics.OverlapSphere(center, radius, enemyMask);
    }

    int DamageAround(Vector3 center, float radius, float damage)
    {
        int hits = 0;
        foreach (Collider c in OverlapEnemies(center, radius))
        {
            Health h = c.GetComponent<Health>();
            if (h == null) continue;

            h.TakeDamage(Mathf.RoundToInt(damage));
            hits++;
        }
        return hits;
    }

    void SpawnBurst(Vector3 pos, Color color, float radius)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "RuneBurst";
        go.transform.position = pos + Vector3.up * 0.5f;

        var mr = go.GetComponent<MeshRenderer>();
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var mat = new Material(shader);
        mat.color = color;
        mr.sharedMaterial = mat;

        Object.Destroy(go.GetComponent<Collider>());

        var burst = go.AddComponent<RuneBurstFX>();
        burst.radius = radius;
        burst.life = 0.45f;
    }

    // ---------------------------------------------------------------- combos

    void CheckCombo()
    {
        if (activeCombo.Count >= 2)
        {
            if (activeCombo.Contains(RuneType.Storm) && activeCombo.Contains(RuneType.Frost))
            {
                ExecuteCombo("StormFrost");
                activeCombo.Clear();
            }
            else if (activeCombo.Contains(RuneType.Wind) && activeCombo.Contains(RuneType.Storm))
            {
                ExecuteCombo("WindStorm");
                activeCombo.Clear();
            }
            else if (activeCombo.Contains(RuneType.Spirit) && activeCombo.Contains(RuneType.Blood))
            {
                ExecuteCombo("SpiritBlood");
                activeCombo.Clear();
            }
        }
    }

    void ExecuteCombo(string comboName)
    {
        Debug.Log($"Combo executed: {comboName}");

        switch (comboName)
        {
            case "StormFrost":
                SpawnBurst(transform.position, new Color(0.7f, 0.95f, 1f, 1f), 4f);
                DamageAround(transform.position, 6f, 200f);
                break;
            case "WindStorm":
                SpawnBurst(transform.position, new Color(0.9f, 0.85f, 1f, 1f), 5f);
                DamageAround(transform.position, 7f, 250f);
                break;
            case "SpiritBlood":
                if (characterController != null)
                    characterController.Heal(50);
                break;
        }
    }

    void DealComboDamage(float damage)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 10f, enemyMask);
        foreach (Collider enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(Mathf.RoundToInt(damage));
            }
        }
    }

    // ---------------------------------------------------------------- upgrades + getters

    public void UpgradeRune(RuneType runeType)
    {
        if (!runeDictionary.ContainsKey(runeType))
            return;

        RuneData rune = runeDictionary[runeType];
        rune.level++;
        rune.damage *= 1.2f;
        rune.cooldown *= 0.9f;

        Debug.Log($"Upgraded {rune.name} to level {rune.level}");
    }

    public bool CanCastRune(RuneType runeType)
    {
        if (!runeDictionary.ContainsKey(runeType))
            return false;

        RuneData rune = runeDictionary[runeType];
        return runeCooldowns[runeType] <= 0 && currentSpiritEssence >= rune.spiritCost;
    }

    public float GetRuneCooldown(RuneType runeType)
    {
        if (!runeCooldowns.ContainsKey(runeType))
            return 0f;

        return runeCooldowns[runeType];
    }

    public float GetRuneCooldownMax(RuneType runeType)
    {
        if (!runeDictionary.ContainsKey(runeType))
            return 1f;

        return Mathf.Max(runeDictionary[runeType].cooldown, 1f);
    }

    public int GetSpiritCost(RuneType runeType)
    {
        switch (runeType)
        {
            case RuneType.Storm: return 20;
            case RuneType.Frost: return 25;
            case RuneType.Wind: return 15;
            case RuneType.Spirit: return 30;
            case RuneType.Blood: return 35;
            case RuneType.Forbidden: return 50;
            default: return 20;
        }
    }

    public float GetDamage(RuneType runeType)
    {
        switch (runeType)
        {
            case RuneType.Storm: return 50f;
            case RuneType.Frost: return 40f;
            case RuneType.Wind: return 30f;
            case RuneType.Spirit: return 35f;
            case RuneType.Blood: return 60f;
            case RuneType.Forbidden: return 100f;
            default: return 50f;
        }
    }

    public float GetRange(RuneType runeType)
    {
        switch (runeType)
        {
            case RuneType.Storm: return 5f;
            case RuneType.Frost: return 4f;
            case RuneType.Wind: return 6f;
            case RuneType.Spirit: return 3f;
            case RuneType.Blood: return 2f;
            case RuneType.Forbidden: return 10f;
            default: return 5f;
        }
    }

    public void AddSpiritEssence(int amount)
    {
        currentSpiritEssence = Mathf.Min(currentSpiritEssence + amount, maxSpiritEssence);
    }

    public void SetSpiritEssence(int amount)
    {
        currentSpiritEssence = Mathf.Clamp(amount, 0, maxSpiritEssence);
    }

    public int GetSpiritEssenceValue()
    {
        return currentSpiritEssence;
    }

    public float GetSpiritEssencePercent()
    {
        return (float)currentSpiritEssence / maxSpiritEssence;
    }

    public List<RuneType> GetAvailableRunes()
    {
        var result = new List<RuneType>();
        foreach (RuneType rune in availableRunes)
        {
            if (runeDictionary.ContainsKey(rune))
                result.Add(rune);
        }
        return result;
    }

    // Save/Load system
    public string SaveData()
    {
        RuneSaveData data = new RuneSaveData();
        data.currentSpiritEssence = currentSpiritEssence;
        data.runeLevels = new Dictionary<string, int>();
        foreach (var pair in runeDictionary)
        {
            data.runeLevels[pair.Key.ToString()] = pair.Value.level;
        }
        return JsonConvert.SerializeObject(data);
    }

    public void LoadData(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
            return;

        RuneSaveData data = JsonConvert.DeserializeObject<RuneSaveData>(jsonData);
        if (data == null)
            return;

        currentSpiritEssence = Mathf.Clamp(data.currentSpiritEssence, 0, maxSpiritEssence);

        if (data.runeLevels == null)
            return;

        foreach (var pair in data.runeLevels)
        {
            RuneType runeType;
            if (System.Enum.TryParse(pair.Key, out runeType) && runeDictionary.ContainsKey(runeType))
            {
                runeDictionary[runeType].level = pair.Value;
            }
        }
    }

    [System.Serializable]
    private class RuneSaveData
    {
        public int currentSpiritEssence;
        public Dictionary<string, int> runeLevels;
    }
}

/// <summary>
/// RuneBurstFX - procedural AOE burst that shrinks and fades out.
/// Secondary runtime-added class (resolved by type at runtime, not scene GUID).
/// </summary>
public class RuneBurstFX : MonoBehaviour
{
    public float radius = 2f;
    public float life = 0.45f;

    private float age;
    private Material mat;

    void Start()
    {
        var mr = GetComponent<MeshRenderer>();
        if (mr != null)
            mat = mr.sharedMaterial;
        transform.localScale = Vector3.one * (radius * 2f);
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = age / life;

        transform.localScale = Vector3.one * (radius * 2f) * (1f - t);

        if (mat != null)
        {
            Color c = mat.color;
            c.a = 1f - t;
            mat.color = c;
        }

        if (age >= life)
            Destroy(gameObject);
    }
}