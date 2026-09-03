using UnityEngine;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Quest System - Manages quests, objectives, and rewards
/// </summary>
public class QuestSystem : MonoBehaviour
{
    [System.Serializable]
    public class Quest
    {
        public string questId;
        public string questName;
        public string description;
        public string factionId;
        public int requiredReputation;
        public List<QuestObjective> objectives;
        public List<QuestReward> rewards;
        public bool isMainQuest;
        public bool isCompleted;
        public bool isActive;
    }

    [System.Serializable]
    public class QuestObjective
    {
        public string objectiveId;
        public string description;
        public int targetCount;
        public int currentCount;
        public ObjectiveType type;
        public string targetId;

        public enum ObjectiveType
        {
            Kill,
            Collect,
            Talk,
            Explore,
            Defend,
            Escort
        }
    }

    [System.Serializable]
    public class QuestReward
    {
        public RewardType type;
        public int amount;
        public string itemId;

        public enum RewardType
        {
            Gold,
            Experience,
            Item,
            Reputation,
            SpiritEssence
        }
    }

    [Header("Quests")]
    public List<Quest> availableQuests;
    public List<Quest> activeQuests;
    public List<Quest> completedQuests;

    [Header("References")]
    public FactionSystem factionSystem;
    public BainoviaCharacterController player;

    private Dictionary<string, Quest> questDictionary = new Dictionary<string, Quest>();

    void Start()
    {
        InitializeQuests();
    }

    void InitializeQuests()
    {
        foreach (Quest quest in availableQuests)
        {
            questDictionary[quest.questId] = quest;
        }
    }

    public bool AcceptQuest(string questId)
    {
        if (!questDictionary.ContainsKey(questId))
            return false;

        Quest quest = questDictionary[questId];

        // Check if already active or completed
        if (quest.isActive || quest.isCompleted)
            return false;

        // Check faction reputation
        if (factionSystem != null)
        {
            int reputation = factionSystem.GetReputation(quest.factionId);
            if (reputation < quest.requiredReputation)
                return false;
        }

        // Activate quest
        quest.isActive = true;
        activeQuests.Add(quest);
        availableQuests.Remove(quest);

        Debug.Log($"Accepted quest: {quest.questName}");

        return true;
    }

    public void CompleteQuest(string questId)
    {
        if (!questDictionary.ContainsKey(questId))
            return;

        Quest quest = questDictionary[questId];

        if (!quest.isActive)
            return;

        // Check if all objectives are complete
        bool allObjectivesComplete = true;
        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective.currentCount < objective.targetCount)
            {
                allObjectivesComplete = false;
                break;
            }
        }

        if (!allObjectivesComplete)
        {
            Debug.Log($"Quest {quest.questName} has incomplete objectives");
            return;
        }

        // Complete quest
        quest.isCompleted = true;
        quest.isActive = false;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        // Give rewards
        GiveRewards(quest);

        // Update faction reputation
        if (factionSystem != null)
        {
            int reputationReward = GetReputationReward(quest);
            factionSystem.CompleteQuest(quest.factionId, questId, reputationReward);
        }

        Debug.Log($"Completed quest: {quest.questName}");
    }

    void GiveRewards(Quest quest)
    {
        foreach (QuestReward reward in quest.rewards)
        {
            switch (reward.type)
            {
                case QuestReward.RewardType.Gold:
                    // Add gold to player inventory
                    Debug.Log($"Reward: {reward.amount} gold");
                    break;
                case QuestReward.RewardType.Experience:
                    // Add experience to player
                    Debug.Log($"Reward: {reward.amount} experience");
                    break;
                case QuestReward.RewardType.Item:
                    // Add item to player inventory
                    Debug.Log($"Reward: Item {reward.itemId}");
                    break;
                case QuestReward.RewardType.Reputation:
                    // Reputation is handled by faction system
                    break;
                case QuestReward.RewardType.SpiritEssence:
                    if (player != null)
                    {
                        player.AddSpiritEssence(reward.amount);
                    }
                    break;
            }
        }
    }

    public void UpdateObjective(string questId, string objectiveId, int amount)
    {
        if (!questDictionary.ContainsKey(questId))
            return;

        Quest quest = questDictionary[questId];

        if (!quest.isActive)
            return;

        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective.objectiveId == objectiveId)
            {
                objective.currentCount = Mathf.Min(objective.currentCount + amount, objective.targetCount);
                Debug.Log($"Updated objective {objectiveId}: {objective.currentCount}/{objective.targetCount}");

                // Check if quest is complete
                CheckQuestCompletion(quest);
                break;
            }
        }
    }

    void CheckQuestCompletion(Quest quest)
    {
        bool allComplete = true;
        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective.currentCount < objective.targetCount)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            Debug.Log($"Quest {quest.questName} is ready to complete!");
        }
    }

    public void AbandonQuest(string questId)
    {
        if (!questDictionary.ContainsKey(questId))
            return;

        Quest quest = questDictionary[questId];

        if (!quest.isActive)
            return;

        quest.isActive = false;
        activeQuests.Remove(quest);
        availableQuests.Add(quest);

        // Reset objectives
        foreach (QuestObjective objective in quest.objectives)
        {
            objective.currentCount = 0;
        }

        Debug.Log($"Abandoned quest: {quest.questName}");
    }

    public List<Quest> GetAvailableQuests()
    {
        List<Quest> available = new List<Quest>();

        foreach (Quest quest in availableQuests)
        {
            // Check faction reputation
            if (factionSystem != null)
            {
                int reputation = factionSystem.GetReputation(quest.factionId);
                if (reputation >= quest.requiredReputation)
                {
                    available.Add(quest);
                }
            }
            else
            {
                available.Add(quest);
            }
        }

        return available;
    }

    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests);
    }

    public List<Quest> GetCompletedQuests()
    {
        return new List<Quest>(completedQuests);
    }

    public Quest GetQuest(string questId)
    {
        if (!questDictionary.ContainsKey(questId))
            return null;

        return questDictionary[questId];
    }

    public float GetQuestProgress(string questId)
    {
        if (!questDictionary.ContainsKey(questId))
            return 0f;

        Quest quest = questDictionary[questId];

        if (quest.objectives.Count == 0)
            return 1f;

        int totalObjectives = quest.objectives.Count;
        int completedObjectives = 0;

        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective.currentCount >= objective.targetCount)
            {
                completedObjectives++;
            }
        }

        return (float)completedObjectives / totalObjectives;
    }

    private int GetReputationReward(Quest quest)
    {
        int baseReward = 10;
        if (quest.isMainQuest) baseReward = 20;
        return baseReward;
    }

    // Save/Load system
    public string SaveData()
    {
        QuestSaveData data = new QuestSaveData();
        foreach (Quest quest in questDictionary.Values)
        {
            data.questStates[quest.questId] = new QuestState
            {
                isActive = quest.isActive,
                isCompleted = quest.isCompleted,
                objectiveProgress = new Dictionary<string, int>()
            };

            foreach (QuestObjective objective in quest.objectives)
            {
                data.questStates[quest.questId].objectiveProgress[objective.objectiveId] = objective.currentCount;
            }
        }
        // JsonUtility cannot serialize Dictionary; use System.Text.Json instead
        return JsonSerializer.Serialize(data);
    }

    public void LoadData(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData)) return;
        QuestSaveData data = JsonSerializer.Deserialize<QuestSaveData>(jsonData);
        if (data == null) return;
        foreach (var kvp in data.questStates)
        {
            if (questDictionary.ContainsKey(kvp.Key))
            {
                Quest quest = questDictionary[kvp.Key];
                quest.isActive = kvp.Value.isActive;
                quest.isCompleted = kvp.Value.isCompleted;

                foreach (var objKvp in kvp.Value.objectiveProgress)
                {
                    foreach (QuestObjective objective in quest.objectives)
                    {
                        if (objective.objectiveId == objKvp.Key)
                        {
                            objective.currentCount = objKvp.Value;
                        }
                    }
                }

                // Update lists
                if (quest.isActive && !activeQuests.Contains(quest))
                {
                    activeQuests.Add(quest);
                    availableQuests.Remove(quest);
                }

                if (quest.isCompleted && !completedQuests.Contains(quest))
                {
                    completedQuests.Add(quest);
                    activeQuests.Remove(quest);
                }
            }
        }
    }

    [System.Serializable]
    private class QuestSaveData
    {
        public Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();
    }

    [System.Serializable]
    private class QuestState
    {
        public bool isActive;
        public bool isCompleted;
        public Dictionary<string, int> objectiveProgress = new Dictionary<string, int>();
    }
}
