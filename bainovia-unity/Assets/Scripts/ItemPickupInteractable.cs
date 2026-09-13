using UnityEngine;

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
                // Notify the quest system that a collect objective item was picked up.
                QuestSystem questSystem = FindObjectOfType<QuestSystem>();
                if (questSystem != null && item != null)
                    questSystem.OnItemCollected(item.itemId);

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