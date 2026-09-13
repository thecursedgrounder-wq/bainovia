using UnityEngine;
using System.Collections.Generic;

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