using UnityEngine;

/// <summary>
/// Manages currency (money) system - converts scores to money and handles shop purchases
/// This is a persistent manager that carries data across scenes
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Currency Settings")]
    [Tooltip("How many score points = 1 coin (e.g., 100 score = 1 coin)")]
    public int scoreToMoneyRatio = 100;

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
    /// </summary>
    public void AwardMoney(int score, bool isLevelComplete = false)
    {
        int moneyEarned = ConvertScoreToMoney(score, isLevelComplete);
        totalMoney += moneyEarned;
        lifetimeMoneyEarned += moneyEarned;

        Debug.Log($"Money awarded: {moneyEarned} coins (Total: {totalMoney})");

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
    /// </summary>
    public void AddMoney(int amount)
    {
        totalMoney += amount;
        lifetimeMoneyEarned += amount;
        SaveMoneyData();
    }

    /// <summary>
    /// Get current money balance
    /// </summary>
    public int GetMoney()
    {
        return totalMoney;
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
    /// </summary>
    void LoadMoneyData()
    {
        totalMoney = PlayerPrefs.GetInt("TotalMoney", 0);
        lifetimeMoneyEarned = PlayerPrefs.GetInt("LifetimeEarned", 0);
        lifetimeMoneySpent = PlayerPrefs.GetInt("LifetimeSpent", 0);

        Debug.Log($"Money loaded: {totalMoney} coins");
    }

    /// <summary>
    /// Reset all money data (for testing)
    /// </summary>
    public void ResetAllMoney()
    {
        totalMoney = 0;
        lifetimeMoneyEarned = 0;
        lifetimeMoneySpent = 0;
        SaveMoneyData();
        Debug.Log("All money data reset!");
    }
}
