using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Rune Magic System - Manages rune abilities, cooldowns, and combinations
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
        if (currentSpiritEssence < maxSpiritEssence)
        {
            currentSpiritEssence = Mathf.Min(currentSpiritEssence + 1, maxSpiritEssence);
        }
    }

    public bool CastRune(RuneType runeType)
    {
        if (!runeDictionary.ContainsKey(runeType))
            return false;

        RuneData rune = runeDictionary[runeType];

        // Check cooldown
        if (runeCooldowns[runeType] > 0)
            return false;

        // Check spirit essence
        if (currentSpiritEssence < rune.spiritCost)
            return false;

        // Cast rune
        currentSpiritEssence -= rune.spiritCost;
        runeCooldowns[runeType] = rune.cooldown;

        // Add to combo
        activeCombo.Add(runeType);
        comboTimer = comboWindow;

        // Check for combo
        CheckCombo();

        // Execute rune effect
        ExecuteRuneEffect(rune);

        return true;
    }

    void ExecuteRuneEffect(RuneData rune)
    {
        // Spawn VFX
        if (rune.vfx != null)
        {
            Instantiate(rune.vfx, transform.position, Quaternion.identity);
        }

        // Play sound
        if (rune.sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(rune.sound);
        }

        // Deal damage
        DealRuneDamage(rune);

        Debug.Log($"Cast {rune.name} rune");
    }

    void DealRuneDamage(RuneData rune)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, rune.range, LayerMask.GetMask("Enemy"));
        foreach (Collider enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(Mathf.RoundToInt(rune.damage));
            }
        }
    }

    void CheckCombo()
    {
        if (activeCombo.Count >= 2)
        {
            // Check for specific combos
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
                // Chain lightning that freezes
                DealComboDamage(200f);
                break;
            case "WindStorm":
                // Lightning tornado
                DealComboDamage(250f);
                break;
            case "SpiritBlood":
                // Spirit drain heals player
                characterController?.Heal(50);
                break;
        }
    }

    void DealComboDamage(float damage)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Enemy"));
        foreach (Collider enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(Mathf.RoundToInt(damage));
            }
        }
    }

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

    public float GetSpiritEssencePercent()
    {
        return (float)currentSpiritEssence / maxSpiritEssence;
    }

    public List<RuneType> GetAvailableRunes()
    {
        return new List<RuneType>(runeDictionary.Keys);
    }
}
