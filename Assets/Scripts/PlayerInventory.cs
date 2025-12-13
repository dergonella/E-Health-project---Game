using UnityEngine;
using System;

/// <summary>
/// Manages player's inventory items: Medkits (H) and Shields (F)
/// Classic game style: items are consumable and persist across levels.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Medkit Settings")]
    [Tooltip("Starting number of medkits (only for new game)")]
    public int startingMedkits = 3;
    [Tooltip("Health restored per medkit")]
    public float medkitHealAmount = 50f;

    [Header("Shield Settings")]
    [Tooltip("Starting number of shields (only for new game)")]
    public int startingShields = 3;
    [Tooltip("Duration of shield protection")]
    public float shieldDuration = 5f;

    // Current counts (synced with MarketData)
    public int MedkitCount { get; private set; }
    public int ShieldCount { get; private set; }

    // Shield state
    public bool IsShieldActive { get; private set; }
    private float shieldTimer = 0f;

    // References
    private HealthSystem healthSystem;

    // Events for UI updates
    public event Action<int> OnMedkitCountChanged;
    public event Action<int> OnShieldCountChanged;
    public event Action<bool> OnShieldActiveChanged;
    public event Action<float, float> OnShieldTimerChanged; // current, max

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();

        // Initialize inventory if this is a new game
        MarketData.InitializeIfNeeded(startingMedkits, startingShields, 3);

        // Load current inventory from persistent storage
        MedkitCount = MarketData.Medkits;
        ShieldCount = MarketData.Shields;
        IsShieldActive = false;

        Debug.Log($"[PlayerInventory] Loaded from save: {MedkitCount} medkits, {ShieldCount} shields");

        // Notify UI of initial values
        OnMedkitCountChanged?.Invoke(MedkitCount);
        OnShieldCountChanged?.Invoke(ShieldCount);
    }

    void Update()
    {
        // Handle input
        if (Input.GetKeyDown(KeyCode.H))
        {
            UseMedkit();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            UseShield();
        }

        // Handle shield timer
        if (IsShieldActive)
        {
            shieldTimer -= Time.deltaTime;
            OnShieldTimerChanged?.Invoke(shieldTimer, shieldDuration);

            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }
    }

    /// <summary>
    /// Use a medkit to restore health. Permanently consumes the item.
    /// </summary>
    public void UseMedkit()
    {
        if (MedkitCount <= 0)
        {
            Debug.Log("No medkits available!");
            return;
        }

        if (healthSystem == null)
        {
            Debug.LogWarning("No HealthSystem found - cannot use medkit");
            return;
        }

        // Don't waste medkit if at full health
        if (healthSystem.currentHealth >= healthSystem.maxHealth)
        {
            Debug.Log("Already at full health!");
            return;
        }

        // Use medkit and save to persistent storage
        if (MarketData.UseMedkit())
        {
            MedkitCount = MarketData.Medkits; // Sync with saved value
            healthSystem.Heal(medkitHealAmount);

            Debug.Log($"Used medkit! Healed {medkitHealAmount} HP. {MedkitCount} remaining (saved).");
            OnMedkitCountChanged?.Invoke(MedkitCount);
        }
    }

    /// <summary>
    /// Activate shield to block fire and poison. Permanently consumes the item.
    /// </summary>
    public void UseShield()
    {
        if (ShieldCount <= 0)
        {
            Debug.Log("No shields available!");
            return;
        }

        if (IsShieldActive)
        {
            Debug.Log("Shield already active!");
            return;
        }

        // Use shield and save to persistent storage
        if (MarketData.UseShield())
        {
            ShieldCount = MarketData.Shields; // Sync with saved value
            ActivateShield();

            Debug.Log($"Shield activated! {shieldDuration}s protection. {ShieldCount} remaining (saved).");
            OnShieldCountChanged?.Invoke(ShieldCount);
        }
    }

    void ActivateShield()
    {
        IsShieldActive = true;
        shieldTimer = shieldDuration;
        OnShieldActiveChanged?.Invoke(true);
        OnShieldTimerChanged?.Invoke(shieldTimer, shieldDuration);

        // Cure poison when shield activates
        if (healthSystem != null && healthSystem.isPoisoned)
        {
            healthSystem.CurePoison();
            Debug.Log("Shield cured existing poison!");
        }
    }

    void DeactivateShield()
    {
        IsShieldActive = false;
        shieldTimer = 0f;
        OnShieldActiveChanged?.Invoke(false);
        Debug.Log("Shield expired!");
    }

    /// <summary>
    /// Check if shield blocks incoming projectile damage.
    /// Shield blocks fire and poison projectiles.
    /// </summary>
    public bool BlocksProjectile()
    {
        return IsShieldActive;
    }

    /// <summary>
    /// Add medkits to inventory (pickup). Saves to persistent storage.
    /// </summary>
    public void AddMedkit(int count = 1)
    {
        MarketData.Medkits += count;
        MedkitCount = MarketData.Medkits;
        OnMedkitCountChanged?.Invoke(MedkitCount);
        Debug.Log($"[PlayerInventory] Picked up {count} medkit(s)! Total: {MedkitCount}");
    }

    /// <summary>
    /// Add shields to inventory (pickup). Saves to persistent storage.
    /// </summary>
    public void AddShield(int count = 1)
    {
        MarketData.Shields += count;
        ShieldCount = MarketData.Shields;
        OnShieldCountChanged?.Invoke(ShieldCount);
        Debug.Log($"[PlayerInventory] Picked up {count} shield(s)! Total: {ShieldCount}");
    }

    /// <summary>
    /// Refresh inventory from saved data (call when returning from market).
    /// Does NOT reset - just syncs with current saved values.
    /// </summary>
    public void RefreshInventory()
    {
        MedkitCount = MarketData.Medkits;
        ShieldCount = MarketData.Shields;
        IsShieldActive = false;
        shieldTimer = 0f;

        OnMedkitCountChanged?.Invoke(MedkitCount);
        OnShieldCountChanged?.Invoke(ShieldCount);
        OnShieldActiveChanged?.Invoke(false);

        Debug.Log($"[PlayerInventory] Refreshed: {MedkitCount} medkits, {ShieldCount} shields");
    }

    /// <summary>
    /// Reset inventory to starting values (for new game only).
    /// </summary>
    public void ResetInventory()
    {
        MarketData.Medkits = startingMedkits;
        MarketData.Shields = startingShields;

        MedkitCount = MarketData.Medkits;
        ShieldCount = MarketData.Shields;
        IsShieldActive = false;
        shieldTimer = 0f;

        OnMedkitCountChanged?.Invoke(MedkitCount);
        OnShieldCountChanged?.Invoke(ShieldCount);
        OnShieldActiveChanged?.Invoke(false);

        Debug.Log($"[PlayerInventory] Reset to starting values: {MedkitCount} medkits, {ShieldCount} shields");
    }
}
