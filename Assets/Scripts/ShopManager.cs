using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the shop system - items, purchases, and inventory
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Items")]
    public List<ShopItem> availableItems = new List<ShopItem>();

    // Purchased items storage
    private HashSet<string> purchasedItemIds = new HashSet<string>();
    private Dictionary<string, int> consumableQuantities = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPurchaseData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize shop with default items (call this once at game start)
    /// </summary>
    public void InitializeShop()
    {
        // Example items - you'll set these up in Unity Editor
        availableItems = new List<ShopItem>
        {
            new ShopItem
            {
                itemId = "skin_blue_dino",
                itemName = "Blue Dino Skin",
                description = "A cool blue dinosaur skin",
                cost = 500,
                itemType = ShopItem.ShopItemType.Skin,
                isConsumable = false
            },
            new ShopItem
            {
                itemId = "powerup_double_jump",
                itemName = "Double Jump",
                description = "Unlock the ability to double jump",
                cost = 1000,
                itemType = ShopItem.ShopItemType.PowerUp,
                isConsumable = false
            },
            new ShopItem
            {
                itemId = "consumable_shield",
                itemName = "Shield Potion",
                description = "Protect from one hit",
                cost = 100,
                itemType = ShopItem.ShopItemType.Consumable,
                isConsumable = true
            }
        };

        LoadPurchaseData();
    }

    /// <summary>
    /// Attempt to purchase an item
    /// </summary>
    public bool PurchaseItem(string itemId)
    {
        ShopItem item = GetItemById(itemId);
        if (item == null)
        {
            Debug.LogError($"Item not found: {itemId}");
            return false;
        }

        // Check if already owned (for non-consumables)
        if (!item.isConsumable && IsItemPurchased(itemId))
        {
            Debug.Log($"Already owned: {item.itemName}");
            return false;
        }

        // Try to purchase
        if (CurrencyManager.Instance.TryPurchase(item.cost))
        {
            if (item.isConsumable)
            {
                // Add to consumable inventory
                AddConsumable(itemId, 1);
            }
            else
            {
                // Mark as purchased
                purchasedItemIds.Add(itemId);
                item.isPurchased = true;
            }

            SavePurchaseData();
            Debug.Log($"Purchased: {item.itemName}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Use a consumable item
    /// </summary>
    public bool UseConsumable(string itemId)
    {
        if (consumableQuantities.ContainsKey(itemId) && consumableQuantities[itemId] > 0)
        {
            consumableQuantities[itemId]--;
            SavePurchaseData();
            Debug.Log($"Used consumable: {itemId}, Remaining: {consumableQuantities[itemId]}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add consumable items
    /// </summary>
    public void AddConsumable(string itemId, int amount)
    {
        if (!consumableQuantities.ContainsKey(itemId))
        {
            consumableQuantities[itemId] = 0;
        }

        consumableQuantities[itemId] += amount;
        SavePurchaseData();
    }

    /// <summary>
    /// Check if item is purchased
    /// </summary>
    public bool IsItemPurchased(string itemId)
    {
        return purchasedItemIds.Contains(itemId);
    }

    /// <summary>
    /// Get consumable quantity
    /// </summary>
    public int GetConsumableQuantity(string itemId)
    {
        if (consumableQuantities.ContainsKey(itemId))
        {
            return consumableQuantities[itemId];
        }
        return 0;
    }

    /// <summary>
    /// Get item by ID
    /// </summary>
    public ShopItem GetItemById(string itemId)
    {
        return availableItems.Find(item => item.itemId == itemId);
    }

    /// <summary>
    /// Get all items of a specific type
    /// </summary>
    public List<ShopItem> GetItemsByType(ShopItem.ShopItemType type)
    {
        return availableItems.FindAll(item => item.itemType == type);
    }

    /// <summary>
    /// Save purchase data
    /// </summary>
    void SavePurchaseData()
    {
        // Save purchased items
        string purchasedIds = string.Join(",", purchasedItemIds);
        PlayerPrefs.SetString("PurchasedItems", purchasedIds);

        // Save consumable quantities
        foreach (var kvp in consumableQuantities)
        {
            PlayerPrefs.SetInt($"Consumable_{kvp.Key}", kvp.Value);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load purchase data
    /// </summary>
    void LoadPurchaseData()
    {
        // Load purchased items
        string purchasedIds = PlayerPrefs.GetString("PurchasedItems", "");
        if (!string.IsNullOrEmpty(purchasedIds))
        {
            string[] ids = purchasedIds.Split(',');
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    purchasedItemIds.Add(id);
                }
            }
        }

        // Load consumable quantities
        foreach (var item in availableItems)
        {
            if (item.isConsumable)
            {
                int qty = PlayerPrefs.GetInt($"Consumable_{item.itemId}", 0);
                if (qty > 0)
                {
                    consumableQuantities[item.itemId] = qty;
                }
            }
        }

        // Update item purchased status
        foreach (var item in availableItems)
        {
            item.isPurchased = purchasedItemIds.Contains(item.itemId);
        }
    }

    /// <summary>
    /// Reset all shop data (for testing)
    /// </summary>
    public void ResetShopData()
    {
        purchasedItemIds.Clear();
        consumableQuantities.Clear();
        PlayerPrefs.DeleteKey("PurchasedItems");
        SavePurchaseData();
        Debug.Log("Shop data reset!");
    }
}
