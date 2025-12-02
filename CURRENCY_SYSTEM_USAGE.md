# Currency & Shop System - Usage Guide

## Overview
Three new scripts for managing money and shop purchases:
1. **CurrencyManager.cs** - Converts score to money, handles balance
2. **ShopItem.cs** - Data structure for shop items
3. **ShopManager.cs** - Manages shop inventory and purchases

---

## How to Use

### 1. Setup (Do Once)
In any menu scene, create two empty GameObjects:
- **CurrencyManager** - Add CurrencyManager.cs component
- **ShopManager** - Add ShopManager.cs component

These persist across all scenes automatically.

---

### 2. Award Money After Level Ends

When a level ends (win or lose), call this:

```csharp
// Example: In your existing GameManager or UIManager

public void OnLevelComplete(int finalScore, bool won)
{
    if (CurrencyManager.Instance != null)
    {
        // Award money based on score
        CurrencyManager.Instance.AwardMoney(finalScore, won);
    }
}
```

**For Dino Game specifically:**
```csharp
// In DinoGameManager.cs, modify GameOver():
public void GameOver()
{
    gameSpeed = 0f;
    enabled = false;

    player.gameObject.SetActive(false);
    spawner.gameObject.SetActive(false);
    gameOverText.gameObject.SetActive(true);
    retryButton.gameObject.SetActive(true);

    UpdateHiscore();

    // ADD THIS: Award money based on score
    if (CurrencyManager.Instance != null)
    {
        int finalScore = Mathf.FloorToInt(score);
        CurrencyManager.Instance.AwardMoney(finalScore, false);
    }
}
```

---

### 3. Display Money in UI

```csharp
// In any UI script:
using TMPro;

public TextMeshProUGUI moneyText;

void Update()
{
    if (CurrencyManager.Instance != null)
    {
        int money = CurrencyManager.Instance.GetMoney();
        moneyText.text = $"Coins: {money}";
    }
}
```

---

### 4. Shop Purchase Button

```csharp
// Attach to a shop item button:
public void OnPurchaseButtonClicked()
{
    string itemId = "skin_blue_dino"; // Or get from button data

    if (ShopManager.Instance.PurchaseItem(itemId))
    {
        Debug.Log("Purchase successful!");
        // Update UI, unlock item, etc.
    }
    else
    {
        Debug.Log("Not enough money or already owned!");
    }
}
```

---

### 5. Check If Player Can Afford

```csharp
// Before showing purchase button:
public void UpdatePurchaseButton(string itemId)
{
    ShopItem item = ShopManager.Instance.GetItemById(itemId);

    if (item != null)
    {
        bool canAfford = CurrencyManager.Instance.CanAfford(item.cost);
        bool alreadyOwned = ShopManager.Instance.IsItemPurchased(itemId);

        // Enable/disable button based on these
        purchaseButton.interactable = canAfford && !alreadyOwned;
    }
}
```

---

## Configuration

### Currency Settings (in CurrencyManager component):
- **Score To Money Ratio**: 100 (100 score points = 1 coin)
- **Level Complete Multiplier**: 1.5 (50% bonus for winning)

### Examples:
- Score 1000, Lost → 10 coins
- Score 1000, Won → 15 coins (with 1.5x bonus)
- Score 5000, Won → 75 coins

---

## Shop Items Setup

Edit ShopManager.InitializeShop() or create items in Unity Inspector:

```csharp
availableItems.Add(new ShopItem
{
    itemId = "upgrade_jump_boost",
    itemName = "Jump Boost",
    description = "Jump 20% higher",
    cost = 250,
    itemType = ShopItem.ShopItemType.Upgrade,
    isConsumable = false
});
```

---

## Testing Commands

```csharp
// Add 1000 coins for testing
CurrencyManager.Instance.AddMoney(1000);

// Reset all money
CurrencyManager.Instance.ResetAllMoney();

// Reset all purchases
ShopManager.Instance.ResetShopData();
```

---

## Integration with Existing Levels

For your main chase game (Levels 0-3), modify the GameManager:

```csharp
// In your main GameManager.cs GameOver() method:
public void GameOver(bool won)
{
    if (gameOver) return;
    gameOver = true;

    // Award money based on score
    if (CurrencyManager.Instance != null)
    {
        CurrencyManager.Instance.AwardMoney(currentScore, won);
    }

    // Show win/lose screen
    if (uiManager != null)
    {
        if (won)
            uiManager.ShowWinScreen(currentScore);
        else
            uiManager.ShowLoseScreen(currentScore);
    }
}
```

---

## Data Persistence

All data is saved automatically using PlayerPrefs:
- Total money balance
- Lifetime earnings/spending
- Purchased items
- Consumable quantities

Data persists between game sessions!
