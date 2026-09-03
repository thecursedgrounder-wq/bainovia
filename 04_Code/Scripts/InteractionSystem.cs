using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Interaction System - Handles NPC and object interactions with raycasting
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode alternateInteractKey = KeyCode.Q;

    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private UnityEngine.UI.Text promptText;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BainoviaCharacterController player;

    private Interactable currentTarget;
    private bool canInteract;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (player == null)
            player = FindObjectOfType<BainoviaCharacterController>();
    }

    void Update()
    {
        CheckForInteractables();
        HandleInteractionInput();
        UpdatePrompt();
    }

    void CheckForInteractables()
    {
        if (playerCamera == null)
        {
            currentTarget = null;
            canInteract = false;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableMask, QueryTriggerInteraction.Ignore);

        if (hitSomething)
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null && interactable.CanInteract())
            {
                currentTarget = interactable;
                canInteract = true;
                return;
            }
        }

        currentTarget = null;
        canInteract = false;
    }

    void HandleInteractionInput()
    {
        if (canInteract && currentTarget != null)
        {
            // Primary interaction (E key)
            if (Input.GetKeyDown(interactKey))
            {
                currentTarget.Interact(player);
            }

            // Alternate interaction (Q key)
            if (Input.GetKeyDown(alternateInteractKey))
            {
                currentTarget.AlternateInteract(player);
            }
        }
    }

    void UpdatePrompt()
    {
        if (interactionPrompt == null)
            return;

        if (canInteract && currentTarget != null)
        {
            interactionPrompt.SetActive(true);

            if (promptText != null)
            {
                string prompt = $"[{interactKey}] {currentTarget.GetInteractionText()}";
                
                if (currentTarget.HasAlternateInteraction())
                {
                    prompt += $"  [{alternateInteractKey}] {currentTarget.GetAlternateInteractionText()}";
                }

                promptText.text = prompt;
            }
        }
        else
        {
            interactionPrompt.SetActive(false);
        }
    }

    public Interactable GetCurrentTarget()
    {
        return currentTarget;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void SetInteractDistance(float distance)
    {
        interactDistance = distance;
    }

    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance);
        }
    }
}

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

/// <summary>
/// NPC Interactable - For talking to NPCs
/// </summary>
public class NPCInteractable : Interactable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName;
    [SerializeField] private string startDialogueNodeId;
    [SerializeField] private DialogueSystem dialogueSystem;

    protected override void Start()
    {
        base.Start();
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && dialogueSystem != null && !dialogueSystem.IsDialogueActive();
    }

    public override void Interact(BainoviaCharacterController player)
    {
        if (dialogueSystem != null && !string.IsNullOrEmpty(startDialogueNodeId))
        {
            dialogueSystem.StartDialogue(startDialogueNodeId);
        }
    }

    public override string GetInteractionText()
    {
        return $"Talk to {npcName}";
    }
}

/// <summary>
/// Item Pickup Interactable - For picking up items
/// </summary>
public class ItemPickupInteractable : Interactable
{
    [Header("Item Settings")]
    [SerializeField] private InventorySystem.Item item;
    [SerializeField] private bool autoPickup = false;
    [SerializeField] private GameObject pickupVFX;

    public override bool CanInteract()
    {
        return base.CanInteract() && inventorySystem != null;
    }

    public override void Interact(BainoviaCharacterController player)
    {
        if (inventorySystem != null && item != null)
        {
            bool added = inventorySystem.AddItem(item);

            if (added)
            {
                if (pickupVFX != null)
                {
                    Instantiate(pickupVFX, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }

    public override string GetInteractionText()
    {
        return item != null ? $"Pick up {item.itemName}" : "Pick up";
    }

    void OnTriggerEnter(Collider other)
    {
        if (autoPickup && other.CompareTag("Player"))
        {
            BainoviaCharacterController playerController = other.GetComponent<BainoviaCharacterController>();
            if (playerController != null)
            {
                Interact(playerController);
            }
        }
    }
}

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
        if (questSystem != null && !string.IsNullOrEmpty(questId))
        {
            QuestSystem.Quest quest = questSystem.GetQuest(questId);

            if (quest != null && !quest.isCompleted)
            {
                if (!quest.isActive)
                {
                    questSystem.AcceptQuest(questId);
                }
                else
                {
                    // Check if quest can be completed
                    float progress = questSystem.GetQuestProgress(questId);
                    if (progress >= 1f)
                    {
                        questSystem.CompleteQuest(questId);
                    }
                }
            }
            else if (canRepeat)
            {
                // Reset quest for repeat
                questSystem.AbandonQuest(questId);
                questSystem.AcceptQuest(questId);
            }
        }

        // Also start dialogue
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

/// <summary>
/// Crafting Station Interactable - For crafting items
/// </summary>
public class CraftingStationInteractable : Interactable
{
    [Header("Crafting Settings")]
    [SerializeField] private string stationName;
    [SerializeField] private List<InventorySystem.Item> craftableItems;

    public override void Interact(BainoviaCharacterController player)
    {
        // Open crafting UI
        Debug.Log($"Opened {stationName} crafting station");
    }

    public override string GetInteractionText()
    {
        return $"Craft at {stationName}";
    }

    public List<InventorySystem.Item> GetCraftableItems()
    {
        return craftableItems;
    }
}
