using UnityEngine;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Faction System - Manages faction reputation, relationships, and quests
/// </summary>
public class FactionSystem : MonoBehaviour
{
    [System.Serializable]
    public class Faction
    {
        public string factionId;
        public string factionName;
        public int reputation;
        public int maxReputation = 5;
        public int minReputation = -5;
        public List<string> alliedFactions;
        public List<string> enemyFactions;
        public List<string> availableQuests;
    }

    [Header("Factions")]
    public List<Faction> factions;

    [Header("References")]
    public BainoviaCharacterController player;

    private Dictionary<string, Faction> factionDictionary = new Dictionary<string, Faction>();

    void Start()
    {
        InitializeFactions();
    }

    void InitializeFactions()
    {
        foreach (Faction faction in factions)
        {
            factionDictionary[faction.factionId] = faction;
        }
    }

    public int GetReputation(string factionId)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return 0;

        return factionDictionary[factionId].reputation;
    }

    public void SetReputation(string factionId, int reputation)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return;

        Faction faction = factionDictionary[factionId];
        faction.reputation = Mathf.Clamp(reputation, faction.minReputation, faction.maxReputation);
    }

    public void AddReputation(string factionId, int amount)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return;

        Faction faction = factionDictionary[factionId];
        SetReputation(factionId, faction.reputation + amount);
    }

    public bool IsAllied(string factionId1, string factionId2)
    {
        if (!factionDictionary.ContainsKey(factionId1) || !factionDictionary.ContainsKey(factionId2))
            return false;

        Faction faction1 = factionDictionary[factionId1];
        return faction1.alliedFactions.Contains(factionId2);
    }

    public bool IsEnemy(string factionId1, string factionId2)
    {
        if (!factionDictionary.ContainsKey(factionId1) || !factionDictionary.ContainsKey(factionId2))
            return false;

        Faction faction1 = factionDictionary[factionId1];
        return faction1.enemyFactions.Contains(factionId2);
    }

    public List<string> GetAvailableQuests(string factionId)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return new List<string>();

        Faction faction = factionDictionary[factionId];
        List<string> availableQuests = new List<string>();

        foreach (string questId in faction.availableQuests)
        {
            // Check if player meets reputation requirement
            if (CanAcceptQuest(factionId, questId))
            {
                availableQuests.Add(questId);
            }
        }

        return availableQuests;
    }

    public bool CanAcceptQuest(string factionId, string questId)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return false;

        Faction faction = factionDictionary[factionId];
        int reputation = faction.reputation;

        // Quests require minimum reputation of 0
        return reputation >= 0;
    }

    public void CompleteQuest(string factionId, string questId, int reputationReward)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return;

        AddReputation(factionId, reputationReward);

        // Check for reputation changes with allied/enemy factions
        UpdateFactionRelationships(factionId);
    }

    void UpdateFactionRelationships(string factionId)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return;

        Faction faction = factionDictionary[factionId];

        // Allied factions gain small reputation
        foreach (string alliedId in faction.alliedFactions)
        {
            if (factionDictionary.ContainsKey(alliedId))
            {
                AddReputation(alliedId, 1);
            }
        }

        // Enemy factions lose reputation
        foreach (string enemyId in faction.enemyFactions)
        {
            if (factionDictionary.ContainsKey(enemyId))
            {
                AddReputation(enemyId, -1);
            }
        }
    }

    public float GetReputationPercent(string factionId)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return 0f;

        Faction faction = factionDictionary[factionId];
        float range = faction.maxReputation - faction.minReputation;
        float current = faction.reputation - faction.minReputation;
        return current / range;
    }

    public string GetReputationLevel(string factionId)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return "Unknown";

        Faction faction = factionDictionary[factionId];
        int reputation = faction.reputation;

        switch (reputation)
        {
            case -5: return "Exiled";
            case -4: return "Hated";
            case -3: return "Disliked";
            case -2: return "Suspicious";
            case -1: return "Wary";
            case 0: return "Neutral";
            case 1: return "Trusted";
            case 2: return "Respected";
            case 3: return "Honored";
            case 4: return "Revered";
            case 5: return "Legendary";
            default: return "Unknown";
        }
    }

    public float GetShopDiscount(string factionId)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return 0f;

        int reputation = factionDictionary[factionId].reputation;

        if (reputation <= 0) return 0f;
        return reputation * 0.1f; // 10% discount per reputation level
    }

    public List<Faction> GetAllFactions()
    {
        return new List<Faction>(factions);
    }

    public Faction GetFaction(string factionId)
    {
        if (!factionDictionary.ContainsKey(factionId))
            return null;

        return factionDictionary[factionId];
    }

    // Save/Load system
    public string SaveData()
    {
        FactionSaveData data = new FactionSaveData();
        foreach (Faction faction in factions)
        {
            data.factionReputations[faction.factionId] = faction.reputation;
        }
        // JsonUtility cannot serialize Dictionary; use System.Text.Json instead
        return JsonSerializer.Serialize(data);
    }

    public void LoadData(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData)) return;
        FactionSaveData data = JsonSerializer.Deserialize<FactionSaveData>(jsonData);
        if (data == null) return;
        foreach (var kvp in data.factionReputations)
        {
            SetReputation(kvp.Key, kvp.Value);
        }
    }

    [System.Serializable]
    private class FactionSaveData
    {
        public Dictionary<string, int> factionReputations = new Dictionary<string, int>();
    }
}
