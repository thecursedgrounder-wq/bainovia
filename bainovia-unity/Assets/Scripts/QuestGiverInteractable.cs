using UnityEngine;

/// <summary>
/// Quest Giver Interactable - For NPCs that give quests
/// </summary>
public class QuestGiverInteractable : NPCInteractable
{
    [Header("Quest Settings")]
    [SerializeField] private string questId;
    [SerializeField] private bool canRepeat = false;

    public override void Interact(BainoviaCharacterController player)
    {
        // Accept/complete is driven by dialogue options (givesQuest / auto-complete),
        // never auto-accepted here — otherwise "Another time" would be skipped.
        base.Interact(player);
    }

    public override string GetInteractionText()
    {
        if (questSystem != null && !string.IsNullOrEmpty(questId))
        {
            QuestSystem.Quest quest = questSystem.GetQuest(questId);

            if (quest != null)
            {
                if (!quest.isActive && !quest.isCompleted)
                {
                    return $"Accept Quest: {quest.questName}";
                }
                else if (quest.isActive)
                {
                    float progress = questSystem.GetQuestProgress(questId);
                    if (progress >= 1f)
                    {
                        return $"Complete Quest: {quest.questName}";
                    }
                    else
                    {
                        return $"Quest in Progress: {quest.questName}";
                    }
                }
            }
        }

        return base.GetInteractionText();
    }
}