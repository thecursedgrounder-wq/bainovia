using UnityEngine;

/// <summary>
/// Interactable - Base class for any object that can be interacted with
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected string interactionText = "Interact";
    [SerializeField] protected string alternateInteractionText = "";
    [SerializeField] protected bool requiresQuest = false;
    [SerializeField] protected string requiredQuestId;
    [SerializeField] protected bool requiresItem = false;
    [SerializeField] protected string requiredItemId;

    [Header("References")]
    [SerializeField] protected QuestSystem questSystem;
    [SerializeField] protected InventorySystem inventorySystem;

    protected virtual void Start()
    {
        if (questSystem == null)
            questSystem = FindObjectOfType<QuestSystem>();

        if (inventorySystem == null)
            inventorySystem = FindObjectOfType<InventorySystem>();
    }

    public virtual bool CanInteract()
    {
        // Check quest requirement
        if (requiresQuest && questSystem != null)
        {
            QuestSystem.Quest quest = questSystem.GetQuest(requiredQuestId);
            if (quest == null || !quest.isActive)
            {
                return false;
            }
        }

        // Check item requirement
        if (requiresItem && inventorySystem != null)
        {
            if (!inventorySystem.HasItem(requiredItemId))
            {
                return false;
            }
        }

        return true;
    }

    public abstract void Interact(BainoviaCharacterController player);

    public virtual void AlternateInteract(BainoviaCharacterController player)
    {
        // Default: no alternate interaction
    }

    public virtual string GetInteractionText()
    {
        return interactionText;
    }

    public virtual string GetAlternateInteractionText()
    {
        return alternateInteractionText;
    }

    public virtual bool HasAlternateInteraction()
    {
        return !string.IsNullOrEmpty(alternateInteractionText);
    }
}