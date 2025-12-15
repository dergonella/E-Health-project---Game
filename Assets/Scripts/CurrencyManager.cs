using UnityEngine;

/// <summary>
/// Manages currency (money) system - converts scores to money and handles shop purchases
/// This is a persistent manager that carries data across scenes
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Currency Settings")]
    [Tooltip("How many score points = 1 coin (e.g., 10 score = 1 coin)")]
    public int scoreToMoneyRatio = 10;

    [Tooltip("Bonus multiplier for completing levels (e.g., 1.5x money for winning)")]
    public float levelCompleteMultiplier = 1.5f;

    // Player's total money
    private int totalMoney = 0;

    // Stats
    private int lifetimeMoneyEarned = 0;
    private int lifetimeMoneySpent = 0;

    void Awake()
    {
        // Singleton pattern - persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMoneyData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Convert score to money using the ratio
    /// </summary>
    public int ConvertScoreToMoney(int score, bool isLevelComplete = false)
    {
        float money = (float)score / scoreToMoneyRatio;

        // Apply bonus if level was completed
        if (isLevelComplete)
        {
            money *= levelCompleteMultiplier;
        }

        return Mathf.FloorToInt(money);
    }

    /// <summary>
    /// Award money to the player (after level ends)
    /// Also syncs with MarketData for unified money system
    /// </summary>
    public void AwardMoney(int score, bool isLevelComplete = false)
    {
        int moneyEarned = ConvertScoreToMoney(score, isLevelComplete);
        totalMoney += moneyEarned;
        lifetimeMoneyEarned += moneyEarned;

        // SYNC with MarketData so Market screen shows correct money
        MarketData.AddMoney(moneyEarned);

        Debug.Log($"[CurrencyManager] Money awarded: {moneyEarned} coins (Total: {totalMoney}, MarketData: {MarketData.Money})");

        SaveMoneyData();
    }

    /// <summary>
    /// Try to purchase an item - returns true if successful
    /// </summary>
    public bool TryPurchase(int cost)
    {
        if (totalMoney >= cost)
        {
            totalMoney -= cost;
            lifetimeMoneySpent += cost;

            Debug.Log($"Purchase successful! Cost: {cost}, Remaining: {totalMoney}");

            SaveMoneyData();
            return true;
        }
        else
        {
            Debug.Log($"Not enough money! Need: {cost}, Have: {totalMoney}");
            return false;
        }
    }

    /// <summary>
    /// Add money directly (for testing or special rewards)
    /// Also syncs with MarketData for unified money system
    /// </summary>
    public void AddMoney(int amount)
    {
        totalMoney += amount;
        lifetimeMoneyEarned += amount;

        // SYNC with MarketData so Market screen shows correct money
        MarketData.AddMoney(amount);

        SaveMoneyData();
        Debug.Log($"[CurrencyManager] Added {amount} money. Total: {totalMoney}, MarketData: {MarketData.Money}");
    }

    /// <summary>
    /// Get current money balance (synced with MarketData)
    /// </summary>
    public int GetMoney()
    {
        // Return from MarketData to ensure sync
        return MarketData.Money;
    }

    /// <summary>
    /// Check if player can afford something
    /// </summary>
    public bool CanAfford(int cost)
    {
        return totalMoney >= cost;
    }

    /// <summary>
    /// Get lifetime earnings
    /// </summary>
    public int GetLifetimeEarned()
    {
        return lifetimeMoneyEarned;
    }

    /// <summary>
    /// Get lifetime spending
    /// </summary>
    public int GetLifetimeSpent()
    {
        return lifetimeMoneySpent;
    }

    /// <summary>
    /// Save money data to PlayerPrefs
    /// </summary>
    void SaveMoneyData()
    {
        PlayerPrefs.SetInt("TotalMoney", totalMoney);
        PlayerPrefs.SetInt("LifetimeEarned", lifetimeMoneyEarned);
        PlayerPrefs.SetInt("LifetimeSpent", lifetimeMoneySpent);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load money data from PlayerPrefs
    /// Syncs with MarketData to ensure unified money system
    /// </summary>
    void LoadMoneyData()
    {
        // Initialize MarketData if needed (sets starting money to 300)
        MarketData.InitializeIfNeeded();

        // Use MarketData as the single source of truth
        totalMoney = MarketData.Money;
        lifetimeMoneyEarned = PlayerPrefs.GetInt("LifetimeEarned", 0);
        lifetimeMoneySpent = PlayerPrefs.GetInt("LifetimeSpent", 0);

        Debug.Log($"[CurrencyManager] Money loaded: {totalMoney} coins (synced with MarketData)");
    }

    /// <summary>
    /// Reset all money data (for testing) - resets to starting value 300
    /// </summary>
    public void ResetAllMoney()
    {
        // Reset MarketData (which sets money to 300)
        MarketData.ResetAllData();

        // Sync local values
        totalMoney = MarketData.Money;
        lifetimeMoneyEarned = 0;
        lifetimeMoneySpent = 0;

        PlayerPrefs.SetInt("LifetimeEarned", 0);
        PlayerPrefs.SetInt("LifetimeSpent", 0);
        PlayerPrefs.Save();

        Debug.Log($"[CurrencyManager] All data reset! Money: {totalMoney}");
    }
}
