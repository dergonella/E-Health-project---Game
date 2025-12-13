using UnityEngine;

/// <summary>
/// Stores player's money and consumable inventory that persists between sessions.
/// Uses PlayerPrefs for permanent storage.
/// Classic game style: items are consumed when used and don't replenish.
/// </summary>
public static class MarketData
{
    // PlayerPrefs keys
    private const string MONEY_KEY = "PlayerMoney";
    private const string MEDKITS_KEY = "Medkits";
    private const string SHIELDS_KEY = "Shields";
    private const string SLOWMO_KEY = "SlowMotion";
    private const string INITIALIZED_KEY = "InventoryInitialized";

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
    /// Current medkits in inventory (consumable - when used, they're gone)
    /// </summary>
    public static int Medkits
    {
        get => PlayerPrefs.GetInt(MEDKITS_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(MEDKITS_KEY, Mathf.Max(0, value));
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Current shields in inventory (consumable - when used, they're gone)
    /// </summary>
    public static int Shields
    {
        get => PlayerPrefs.GetInt(SHIELDS_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(SHIELDS_KEY, Mathf.Max(0, value));
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Current slow motion in inventory (consumable - when used, they're gone)
    /// </summary>
    public static int SlowMotion
    {
        get => PlayerPrefs.GetInt(SLOWMO_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(SLOWMO_KEY, Mathf.Max(0, value));
            PlayerPrefs.Save();
        }
    }

    // Legacy properties for backward compatibility (redirect to new system)
    public static int BonusMedkits
    {
        get => Medkits;
        set => Medkits = value;
    }

    public static int BonusShields
    {
        get => Shields;
        set => Shields = value;
    }

    public static int BonusSlowMotion
    {
        get => SlowMotion;
        set => SlowMotion = value;
    }

    /// <summary>
    /// Initialize inventory with starting values (only runs once per new game)
    /// Uses BASE constants if no parameters provided
    /// </summary>
    public static void InitializeIfNeeded(int startMedkits = -1, int startShields = -1, int startSlowMotion = -1)
    {
        // Use base constants if not specified
        if (startMedkits < 0) startMedkits = BASE_MEDKITS;
        if (startShields < 0) startShields = BASE_SHIELDS;
        if (startSlowMotion < 0) startSlowMotion = BASE_SLOWMOTION;

        if (!PlayerPrefs.HasKey(INITIALIZED_KEY))
        {
            Medkits = startMedkits;
            Shields = startShields;
            SlowMotion = startSlowMotion;
            PlayerPrefs.SetInt(INITIALIZED_KEY, 1);
            PlayerPrefs.Save();
            Debug.Log($"[MarketData] Inventory initialized: {startMedkits} medkits, {startShields} shields, {startSlowMotion} slow motion");
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
            Medkits++;
            Debug.Log($"[MarketData] Bought medkit! Total: {Medkits}");
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
            Shields++;
            Debug.Log($"[MarketData] Bought shield! Total: {Shields}");
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
            SlowMotion++;
            Debug.Log($"[MarketData] Bought slow motion! Total: {SlowMotion}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Use a medkit. Returns true if successful.
    /// </summary>
    public static bool UseMedkit()
    {
        if (Medkits > 0)
        {
            Medkits--;
            Debug.Log($"[MarketData] Used medkit! Remaining: {Medkits}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Use a shield. Returns true if successful.
    /// </summary>
    public static bool UseShield()
    {
        if (Shields > 0)
        {
            Shields--;
            Debug.Log($"[MarketData] Used SHIELD! Remaining: {Shields}");
            Debug.Log($"[MarketData] Stack trace: {System.Environment.StackTrace}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Use slow motion. Returns true if successful.
    /// </summary>
    public static bool UseSlowMotion()
    {
        if (SlowMotion > 0)
        {
            SlowMotion--;
            Debug.Log($"[MarketData] Used SLOW MOTION! Remaining: {SlowMotion}");
            Debug.Log($"[MarketData] Stack trace: {System.Environment.StackTrace}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get total medkits (now just returns current count - no base amount added)
    /// </summary>
    public static int GetTotalMedkits(int baseAmount = 0)
    {
        // baseAmount is ignored - we use pure consumable system now
        return Medkits;
    }

    /// <summary>
    /// Get total shields (now just returns current count - no base amount added)
    /// </summary>
    public static int GetTotalShields(int baseAmount = 0)
    {
        // baseAmount is ignored - we use pure consumable system now
        return Shields;
    }

    /// <summary>
    /// Get total slow motion (now just returns current count - no base amount added)
    /// </summary>
    public static int GetTotalSlowMotion(int baseAmount = 0)
    {
        // baseAmount is ignored - we use pure consumable system now
        return SlowMotion;
    }

    // Base starting values for all items
    public const int BASE_MEDKITS = 3;
    public const int BASE_SHIELDS = 3;
    public const int BASE_SLOWMOTION = 3;

    /// <summary>
    /// DEVELOPER ONLY: Reset all market data to base values (3 of each item, 0 money)
    /// </summary>
    public static void ResetAllData()
    {
        // Reset money to 0
        PlayerPrefs.SetInt(MONEY_KEY, 0);

        // Reset items to BASE values (3 each)
        PlayerPrefs.SetInt(MEDKITS_KEY, BASE_MEDKITS);
        PlayerPrefs.SetInt(SHIELDS_KEY, BASE_SHIELDS);
        PlayerPrefs.SetInt(SLOWMO_KEY, BASE_SLOWMOTION);

        // Mark as initialized so InitializeIfNeeded doesn't overwrite
        PlayerPrefs.SetInt(INITIALIZED_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log($"[MarketData] All data reset to base values! Medkits: {BASE_MEDKITS}, Shields: {BASE_SHIELDS}, SlowMotion: {BASE_SLOWMOTION}, Money: 0");
    }

    /// <summary>
    /// DEVELOPER ONLY: Give money for testing
    /// </summary>
    public static void DevAddMoney(int amount)
    {
        Money += amount;
        Debug.Log($"[MarketData] DEV: Added {amount} money. Total: {Money}");
    }

    /// <summary>
    /// DEVELOPER ONLY: Give items for testing
    /// </summary>
    public static void DevAddItems(int medkits, int shields, int slowMotion)
    {
        Medkits += medkits;
        Shields += shields;
        SlowMotion += slowMotion;
        Debug.Log($"[MarketData] DEV: Added items. Medkits: {Medkits}, Shields: {Shields}, SlowMotion: {SlowMotion}");
    }
}
