using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Spirit Companion System - Collect spirits from enemies, each providing unique abilities
/// </summary>
public class SpiritCompanionSystem : MonoBehaviour
{
    [System.Serializable]
    public class SpiritCompanion
    {
        public string spiritId;
        public string spiritName;
        public string description;
        public SpiritType type;
        public Sprite portrait;
        public GameObject spiritModel;
        public int relationshipLevel;
        public int maxRelationshipLevel = 5;
        public bool isActive;
        public SpiritAbility ability;
    }

    [System.Serializable]
    public class SpiritAbility
    {
        public string abilityName;
        public string description;
        public float cooldown;
        public float duration;
        public float power;
        public GameObject vfx;
        public AudioClip sound;
    }

    public enum SpiritType
    {
        Offensive,
        Defensive,
        Support,
        Utility
    }

    [Header("Spirit Companions")]
    public List<SpiritCompanion> collectedSpirits = new List<SpiritCompanion>();
    public SpiritCompanion activeSpirit;

    [Header("Settings")]
    public int maxActiveSpirits = 3;
    public float spiritSummonDuration = 10f;
    public float relationshipGainMultiplier = 1f;

    [Header("References")]
    public BainoviaCharacterController player;
    public Transform spiritSpawnPoint;

    private Dictionary<string, SpiritCompanion> spiritDictionary = new Dictionary<string, SpiritCompanion>();
    private Dictionary<string, float> spiritCooldowns = new Dictionary<string, float>();
    private GameObject activeSpiritModel;

    void Start()
    {
        InitializeSpirits();
    }

    void InitializeSpirits()
    {
        foreach (SpiritCompanion spirit in collectedSpirits)
        {
            spiritDictionary[spirit.spiritId] = spirit;
            spiritCooldowns[spirit.spiritId] = 0f;
        }
    }

    void Update()
    {
        UpdateCooldowns();
        HandleSpiritInput();
    }

    void UpdateCooldowns()
    {
        foreach (var spiritId in spiritCooldowns.Keys.ToList())
        {
            if (spiritCooldowns[spiritId] > 0)
            {
                spiritCooldowns[spiritId] -= Time.deltaTime;
            }
        }
    }

    void HandleSpiritInput()
    {
        // Summon active spirit (1-3 keys)
        for (int i = 0; i < collectedSpirits.Count && i < maxActiveSpirits; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (collectedSpirits[i].isActive)
                {
                    SummonSpirit(collectedSpirits[i]);
                }
            }
        }

        // Dismiss spirit (X key)
        if (Input.GetKeyDown(KeyCode.X))
        {
            DismissSpirit();
        }
    }

    public bool CollectSpirit(string spiritId, string spiritName, SpiritType type, SpiritAbility ability)
    {
        if (spiritDictionary.ContainsKey(spiritId))
            return false;

        SpiritCompanion newSpirit = new SpiritCompanion
        {
            spiritId = spiritId,
            spiritName = spiritName,
            description = $"A {type.ToString().ToLower()} spirit",
            type = type,
            relationshipLevel = 1,
            isActive = true,
            ability = ability
        };

        collectedSpirits.Add(newSpirit);
        spiritDictionary[spiritId] = newSpirit;
        spiritCooldowns[spiritId] = 0f;

        Debug.Log($"Collected spirit: {spiritName}");
        return true;
    }

    public bool SummonSpirit(SpiritCompanion spirit)
    {
        if (spirit == null)
            return false;

        if (spiritCooldowns[spirit.spiritId] > 0)
        {
            Debug.Log($"{spirit.spiritName} is on cooldown");
            return false;
        }

        if (activeSpiritModel != null)
        {
            DismissSpirit();
        }

        activeSpirit = spirit;
        spiritCooldowns[spirit.spiritId] = spirit.ability.cooldown;

        // Spawn spirit model
        if (spirit.spiritModel != null && spiritSpawnPoint != null)
        {
            activeSpiritModel = Instantiate(spirit.spiritModel, spiritSpawnPoint.position, Quaternion.identity);
        }

        // Play sound
        if (spirit.ability.sound != null)
        {
            AudioSource.PlayClipAtPoint(spirit.ability.sound, Camera.main.transform.position);
        }

        // Apply spirit ability
        ApplySpiritAbility(spirit);

        Debug.Log($"Summoned spirit: {spirit.spiritName}");

        // Auto-dismiss after duration
        Invoke(nameof(DismissSpirit), spiritSummonDuration);

        return true;
    }

    void ApplySpiritAbility(SpiritCompanion spirit)
    {
        switch (spirit.type)
        {
            case SpiritType.Offensive:
                // Deal damage to nearby enemies
                DealSpiritDamage(spirit.ability.power);
                break;

            case SpiritType.Defensive:
                // Apply shield to player
                ApplySpiritShield(spirit.ability.power);
                break;

            case SpiritType.Support:
                // Heal player
                player?.Heal(Mathf.RoundToInt(spirit.ability.power));
                break;

            case SpiritType.Utility:
                // Reveal nearby secrets or increase movement speed
                player?.AddSpiritEssence(Mathf.RoundToInt(spirit.ability.power));
                break;
        }

        // Spawn VFX
        if (spirit.ability.vfx != null && spiritSpawnPoint != null)
        {
            Instantiate(spirit.ability.vfx, spiritSpawnPoint.position, Quaternion.identity);
        }
    }

    void DealSpiritDamage(float power)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Enemy"));
        foreach (Collider enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(Mathf.RoundToInt(power));
            }
        }
    }

    void ApplySpiritShield(float power)
    {
        // This would need to be implemented in the player controller
        // For now, just heal as a temporary measure
        player?.Heal(Mathf.RoundToInt(power));
    }

    public void DismissSpirit()
    {
        if (activeSpiritModel != null)
        {
            Destroy(activeSpiritModel);
            activeSpiritModel = null;
        }

        activeSpirit = null;
        CancelInvoke(nameof(DismissSpirit));

        Debug.Log("Spirit dismissed");
    }

    public void IncreaseRelationship(string spiritId, int amount)
    {
        if (!spiritDictionary.ContainsKey(spiritId))
            return;

        SpiritCompanion spirit = spiritDictionary[spiritId];
        int actualAmount = Mathf.RoundToInt(amount * relationshipGainMultiplier);
        spirit.relationshipLevel = Mathf.Min(spirit.relationshipLevel + actualAmount, spirit.maxRelationshipLevel);

        // Increase ability power based on relationship
        spirit.ability.power *= 1.1f;

        Debug.Log($"{spirit.spiritName} relationship increased to {spirit.relationshipLevel}");
    }

    public bool SetActiveSpirit(string spiritId, bool isActive)
    {
        if (!spiritDictionary.ContainsKey(spiritId))
            return false;

        SpiritCompanion spirit = spiritDictionary[spiritId];
        spirit.isActive = isActive;

        Debug.Log($"{spirit.spiritName} set to {(isActive ? "active" : "inactive")}");
        return true;
    }

    public SpiritCompanion GetSpirit(string spiritId)
    {
        if (!spiritDictionary.ContainsKey(spiritId))
            return null;

        return spiritDictionary[spiritId];
    }

    public List<SpiritCompanion> GetActiveSpirits()
    {
        List<SpiritCompanion> active = new List<SpiritCompanion>();
        foreach (SpiritCompanion spirit in collectedSpirits)
        {
            if (spirit.isActive)
            {
                active.Add(spirit);
            }
        }
        return active;
    }

    public List<SpiritCompanion> GetSpiritsByType(SpiritType type)
    {
        List<SpiritCompanion> spirits = new List<SpiritCompanion>();
        foreach (SpiritCompanion spirit in collectedSpirits)
        {
            if (spirit.type == type)
            {
                spirits.Add(spirit);
            }
        }
        return spirits;
    }

    public float GetSpiritCooldown(string spiritId)
    {
        if (!spiritCooldowns.ContainsKey(spiritId))
            return 0f;

        return spiritCooldowns[spiritId];
    }

    public int GetCollectedCount()
    {
        return collectedSpirits.Count;
    }

    // Spirit dialogue - spirits have personalities and can talk
    public string GetSpiritDialogue(string spiritId)
    {
        if (!spiritDictionary.ContainsKey(spiritId))
            return "";

        SpiritCompanion spirit = spiritDictionary[spiritId];

        // Return dialogue based on relationship level
        switch (spirit.relationshipLevel)
        {
            case 1:
                return $"I am {spirit.spiritName}. I serve you... for now.";
            case 2:
                return $"You have potential, mortal.";
            case 3:
                return $"I begin to trust your judgment.";
            case 4:
                return $"We fight as one now.";
            case 5:
                return $"I am honored to be your companion.";
            default:
                return "";
        }
    }

    // Save/Load system
    public string SaveData()
    {
        SpiritSaveData data = new SpiritSaveData();
        foreach (SpiritCompanion spirit in collectedSpirits)
        {
            data.spiritStates[spirit.spiritId] = new SpiritState
            {
                relationshipLevel = spirit.relationshipLevel,
                isActive = spirit.isActive
            };
        }
        // JsonUtility cannot serialize Dictionary; use System.Text.Json instead
        return JsonSerializer.Serialize(data);
    }

    public void LoadData(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData)) return;
        SpiritSaveData data = JsonSerializer.Deserialize<SpiritSaveData>(jsonData);
        if (data == null) return;
        foreach (var kvp in data.spiritStates)
        {
            if (spiritDictionary.ContainsKey(kvp.Key))
            {
                SpiritCompanion spirit = spiritDictionary[kvp.Key];
                spirit.relationshipLevel = kvp.Value.relationshipLevel;
                spirit.isActive = kvp.Value.isActive;
            }
        }
    }

    [System.Serializable]
    private class SpiritSaveData
    {
        public Dictionary<string, SpiritState> spiritStates = new Dictionary<string, SpiritState>();
    }

    [System.Serializable]
    private class SpiritState
    {
        public int relationshipLevel;
        public bool isActive;
    }
}
