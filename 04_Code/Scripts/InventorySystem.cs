using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Inventory System - Manages items, equipment, runes, and consumables
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [System.Serializable]
    public class Item
    {
        public string itemId;
        public string itemName;
        public string description;
        public ItemType type;
        public Sprite icon;
        public int maxStack;
        public int currentStack;
        public int value;
        public bool isEquipped;
        public GameObject worldModel;
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Rune,
        Consumable,
        Material,
        QuestItem,
        Accessory
    }

    [Header("Inventory Settings")]
    public int maxSlots = 30;
    public int gold = 0;

    [Header("Equipment Slots")]
    public Item primaryWeapon;
    public Item secondaryWeapon;
    public Item armor;
    public Item accessory1;
    public Item accessory2;

    [Header("Inventory")]
    public List<Item> inventory = new List<Item>();

    [Header("References")]
    public BainoviaCharacterController player;
    public RuneMagicSystem runeSystem;

    private Dictionary<string, Item> itemDictionary = new Dictionary<string, Item>();

    void Start()
    {
        InitializeInventory();
    }

    void InitializeInventory()
    {
        foreach (Item item in inventory)
        {
            itemDictionary[item.itemId] = item;
        }
    }

    public bool AddItem(Item item)
    {
        // Check if item already exists and can stack
        if (item.maxStack > 1)
        {
            foreach (Item invItem in inventory)
            {
                if (invItem.itemId == item.itemId && invItem.currentStack < invItem.maxStack)
                {
                    int spaceAvailable = invItem.maxStack - invItem.currentStack;
                    int amountToAdd = Mathf.Min(item.currentStack, spaceAvailable);
                    invItem.currentStack += amountToAdd;
                    item.currentStack -= amountToAdd;

                    if (item.currentStack <= 0)
                    {
                        Debug.Log($"Added to stack: {item.itemName}");
                        return true;
                    }
                }
            }
        }

        // Add as new item if inventory not full
        if (inventory.Count < maxSlots)
        {
            Item newItem = new Item
            {
                itemId = item.itemId,
                itemName = item.itemName,
                description = item.description,
                type = item.type,
                icon = item.icon,
                maxStack = item.maxStack,
                currentStack = item.currentStack,
                value = item.value,
                isEquipped = false,
                worldModel = item.worldModel
            };

            inventory.Add(newItem);
            itemDictionary[item.itemId] = newItem;

            Debug.Log($"Added item: {item.itemName}");
            return true;
        }

        Debug.Log("Inventory full!");
        return false;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (!itemDictionary.ContainsKey(itemId))
            return false;

        Item item = itemDictionary[itemId];

        if (item.currentStack >= amount)
        {
            item.currentStack -= amount;

            if (item.currentStack <= 0)
            {
                if (item.isEquipped)
                    UnequipItem(itemId);

                inventory.Remove(item);
                itemDictionary.Remove(itemId);
            }

            Debug.Log($"Removed {amount} of {item.itemName}");
            return true;
        }

        return false;
    }

    public bool EquipItem(string itemId)
    {
        if (!itemDictionary.ContainsKey(itemId))
            return false;

        Item item = itemDictionary[itemId];

        if (item.isEquipped)
            return false;

        switch (item.type)
        {
            case ItemType.Weapon:
                if (primaryWeapon == null)
                {
                    primaryWeapon = item;
                    item.isEquipped = true;
                    ApplyWeaponStats(item);
                }
                else if (secondaryWeapon == null)
                {
                    secondaryWeapon = item;
                    item.isEquipped = true;
                    ApplyWeaponStats(item);
                }
                else
                {
                    UnequipItem(primaryWeapon.itemId);
                    primaryWeapon = item;
                    item.isEquipped = true;
                    ApplyWeaponStats(item);
                }
                break;

            case ItemType.Armor:
                if (armor != null)
                    UnequipItem(armor.itemId);

                armor = item;
                item.isEquipped = true;
                ApplyArmorStats(item);
                break;

            case ItemType.Accessory:
                if (accessory1 == null)
                {
                    accessory1 = item;
                    item.isEquipped = true;
                }
                else if (accessory2 == null)
                {
                    accessory2 = item;
                    item.isEquipped = true;
                }
                else
                {
                    UnequipItem(accessory1.itemId);
                    accessory1 = item;
                    item.isEquipped = true;
                }
                break;

            case ItemType.Rune:
                if (runeSystem != null)
                {
                    // Add rune to rune system
                    Debug.Log($"Rune equipped: {item.itemName}");
                }
                break;

            default:
                Debug.Log($"Cannot equip item type: {item.type}");
                return false;
        }

        Debug.Log($"Equipped: {item.itemName}");
        return true;
    }

    public bool UnequipItem(string itemId)
    {
        if (!itemDictionary.ContainsKey(itemId))
            return false;

        Item item = itemDictionary[itemId];

        if (!item.isEquipped)
            return false;

        item.isEquipped = false;

        if (primaryWeapon == item)
        {
            RemoveWeaponStats(item);
            primaryWeapon = null;
        }
        else if (secondaryWeapon == item)
        {
            RemoveWeaponStats(item);
            secondaryWeapon = null;
        }
        else if (armor == item)
        {
            RemoveArmorStats(item);
            armor = null;
        }
        else if (accessory1 == item)
        {
            accessory1 = null;
        }
        else if (accessory2 == item)
        {
            accessory2 = null;
        }

        Debug.Log($"Unequipped: {item.itemName}");
        return true;
    }

    void ApplyWeaponStats(Item weapon)
    {
        if (player == null) return;

        // Apply weapon damage bonus
        player.lightAttackDamage += weapon.value;
        player.heavyAttackDamage += Mathf.RoundToInt(weapon.value * 1.5f);
    }

    void RemoveWeaponStats(Item weapon)
    {
        if (player == null) return;

        player.lightAttackDamage -= weapon.value;
        player.heavyAttackDamage -= Mathf.RoundToInt(weapon.value * 1.5f);
    }

    void ApplyArmorStats(Item armorItem)
    {
        if (player == null) return;

        // Apply armor health bonus
        player.maxHealth += armorItem.value;
        player.currentHealth += armorItem.value;
    }

    void RemoveArmorStats(Item armorItem)
    {
        if (player == null) return;

        player.maxHealth -= armorItem.value;
        player.currentHealth = Mathf.Min(player.currentHealth, player.maxHealth);
    }

    public bool UseConsumable(string itemId)
    {
        if (!itemDictionary.ContainsKey(itemId))
            return false;

        Item item = itemDictionary[itemId];

        if (item.type != ItemType.Consumable)
            return false;

        // Apply consumable effect based on item name/type
        if (item.itemName.ToLower().Contains("health"))
        {
            player?.Heal(item.value);
        }
        else if (item.itemName.ToLower().Contains("spirit"))
        {
            player?.AddSpiritEssence(item.value);
        }

        RemoveItem(itemId, 1);
        Debug.Log($"Used consumable: {item.itemName}");
        return true;
    }

    public bool HasItem(string itemId)
    {
        return itemDictionary.ContainsKey(itemId);
    }

    public int GetItemCount(string itemId)
    {
        if (!itemDictionary.ContainsKey(itemId))
            return 0;

        return itemDictionary[itemId].currentStack;
    }

    public Item GetItem(string itemId)
    {
        if (!itemDictionary.ContainsKey(itemId))
            return null;

        return itemDictionary[itemId];
    }

    public List<Item> GetItemsByType(ItemType type)
    {
        List<Item> items = new List<Item>();
        foreach (Item item in inventory)
        {
            if (item.type == type)
            {
                items.Add(item);
            }
        }
        return items;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"Gold: {gold}");
    }

    public bool RemoveGold(int amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;
        Debug.Log($"Gold: {gold}");
        return true;
    }

    public int GetGold()
    {
        return gold;
    }

    public int GetInventoryCount()
    {
        return inventory.Count;
    }

    public int GetEmptySlots()
    {
        return maxSlots - inventory.Count;
    }

    // Save/Load system
    public string SaveData()
    {
        InventorySaveData data = new InventorySaveData();
        data.gold = gold;
        data.maxSlots = maxSlots;

        foreach (Item item in inventory)
        {
            data.itemStates[item.itemId] = new ItemState
            {
                currentStack = item.currentStack,
                isEquipped = item.isEquipped
            };
        }

        return JsonUtility.ToJson(data);
    }

    public void LoadData(string jsonData)
    {
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(jsonData);
        gold = data.gold;
        maxSlots = data.maxSlots;

        foreach (var kvp in data.itemStates)
        {
            if (itemDictionary.ContainsKey(kvp.Key))
            {
                Item item = itemDictionary[kvp.Key];
                item.currentStack = kvp.Value.currentStack;

                if (kvp.Value.isEquipped && !item.isEquipped)
                {
                    EquipItem(kvp.Key);
                }
            }
        }
    }

    [System.Serializable]
    private class InventorySaveData
    {
        public int gold;
        public int maxSlots;
        public Dictionary<string, ItemState> itemStates = new Dictionary<string, ItemState>();
    }

    [System.Serializable]
    private class ItemState
    {
        public int currentStack;
        public bool isEquipped;
    }
}
