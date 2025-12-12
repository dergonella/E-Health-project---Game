using UnityEngine;

/// <summary>
/// Stores player's money and bonus inventory that persists between sessions.
/// Uses PlayerPrefs for permanent storage.
/// </summary>
public static class MarketData
{
    // PlayerPrefs keys
    private const string MONEY_KEY = "PlayerMoney";
    private const string BONUS_MEDKITS_KEY = "BonusMedkits";
    private const string BONUS_SHIELDS_KEY = "BonusShields";
    private const string BONUS_SLOWMO_KEY = "BonusSlowMotion";

    /// <summary>
    /// Player's current money
    /// </summary>
    public static int Money
    {
        get => PlayerPrefs.GetInt(MONEY_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(MONEY_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Bonus medkits from market (added to starting amount)
    /// </summary>
    public static int BonusMedkits
    {
        get => PlayerPrefs.GetInt(BONUS_MEDKITS_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(BONUS_MEDKITS_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Bonus shields from market (added to starting amount)
    /// </summary>
    public static int BonusShields
    {
        get => PlayerPrefs.GetInt(BONUS_SHIELDS_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(BONUS_SHIELDS_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Bonus slow motion from market (added to starting amount)
    /// </summary>
    public static int BonusSlowMotion
    {
        get => PlayerPrefs.GetInt(BONUS_SLOWMO_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(BONUS_SLOWMO_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Add money to player's wallet
    /// </summary>
    public static void AddMoney(int amount)
    {
        Money += amount;
    }

    /// <summary>
    /// Try to spend money. Returns true if successful.
    /// </summary>
    public static bool SpendMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Buy a medkit. Returns true if successful.
    /// </summary>
    public static bool BuyMedkit(int price)
    {
        if (SpendMoney(price))
        {
            BonusMedkits++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Buy a shield. Returns true if successful.
    /// </summary>
    public static bool BuyShield(int price)
    {
        if (SpendMoney(price))
        {
            BonusShields++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Buy slow motion. Returns true if successful.
    /// </summary>
    public static bool BuySlowMotion(int price)
    {
        if (SpendMoney(price))
        {
            BonusSlowMotion++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get total medkits (base + bonus)
    /// </summary>
    public static int GetTotalMedkits(int baseAmount)
    {
        return baseAmount + BonusMedkits;
    }

    /// <summary>
    /// Get total shields (base + bonus)
    /// </summary>
    public static int GetTotalShields(int baseAmount)
    {
        return baseAmount + BonusShields;
    }

    /// <summary>
    /// Get total slow motion (base + bonus)
    /// </summary>
    public static int GetTotalSlowMotion(int baseAmount)
    {
        return baseAmount + BonusSlowMotion;
    }

    /// <summary>
    /// DEVELOPER ONLY: Reset all market data (money and bonus items)
    /// </summary>
    public static void ResetAllData()
    {
        PlayerPrefs.DeleteKey(MONEY_KEY);
        PlayerPrefs.DeleteKey(BONUS_MEDKITS_KEY);
        PlayerPrefs.DeleteKey(BONUS_SHIELDS_KEY);
        PlayerPrefs.DeleteKey(BONUS_SLOWMO_KEY);
        PlayerPrefs.Save();
        Debug.Log("[MarketData] All data reset!");
    }

    /// <summary>
    /// DEVELOPER ONLY: Give money for testing
    /// </summary>
    public static void DevAddMoney(int amount)
    {
        Money += amount;
        Debug.Log($"[MarketData] DEV: Added {amount} money. Total: {Money}");
    }
}
