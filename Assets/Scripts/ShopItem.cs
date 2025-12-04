using UnityEngine;

/// <summary>
/// Represents an item that can be purchased in the shop
/// </summary>
[System.Serializable]
public class ShopItem
{
    public string itemId;           // Unique identifier
    public string itemName;         // Display name
    public string description;      // Item description
    public int cost;                // Price in coins
    public Sprite icon;             // Item icon/image
    public ShopItemType itemType;   // Type of item
    public bool isPurchased;        // Has player bought this?
    public bool isEquipped;         // Is this item currently equipped?

    // Optional: for consumable items
    public int quantity;            // How many owned (for consumables)
    public bool isConsumable;       // Can be used multiple times

    public enum ShopItemType
    {
        Skin,           // Character skin
        PowerUp,        // Permanent power-up
        Consumable,     // Single-use item
        Upgrade,        // Ability upgrade
        Cosmetic        // Visual item
    }
}
