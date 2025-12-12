using UnityEngine;
using System;

/// <summary>
/// Manages player's inventory items: Medkits (H) and Shields (F)
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Medkit Settings")]
    [Tooltip("Starting number of medkits")]
    public int startingMedkits = 3;
    [Tooltip("Health restored per medkit")]
    public float medkitHealAmount = 50f;

    [Header("Shield Settings")]
    [Tooltip("Starting number of shields")]
    public int startingShields = 3;
    [Tooltip("Duration of shield protection")]
    public float shieldDuration = 5f;

    // Current counts
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

        // Initialize inventory (base + bonus from market)
        MedkitCount = MarketData.GetTotalMedkits(startingMedkits);
        ShieldCount = MarketData.GetTotalShields(startingShields);
        IsShieldActive = false;

        Debug.Log($"[PlayerInventory] Loaded: {MedkitCount} medkits (base {startingMedkits} + {MarketData.BonusMedkits} bonus)");
        Debug.Log($"[PlayerInventory] Loaded: {ShieldCount} shields (base {startingShields} + {MarketData.BonusShields} bonus)");

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
    /// Use a medkit to restore health.
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

        MedkitCount--;
        healthSystem.Heal(medkitHealAmount);

        Debug.Log($"Used medkit! Healed {medkitHealAmount} HP. {MedkitCount} remaining.");
        OnMedkitCountChanged?.Invoke(MedkitCount);
    }

    /// <summary>
    /// Activate shield to block fire and poison.
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

        ShieldCount--;
        ActivateShield();

        Debug.Log($"Shield activated! {shieldDuration}s protection. {ShieldCount} remaining.");
        OnShieldCountChanged?.Invoke(ShieldCount);
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
    /// Add medkits to inventory (pickup).
    /// </summary>
    public void AddMedkit(int count = 1)
    {
        MedkitCount += count;
        OnMedkitCountChanged?.Invoke(MedkitCount);
    }

    /// <summary>
    /// Add shields to inventory (pickup).
    /// </summary>
    public void AddShield(int count = 1)
    {
        ShieldCount += count;
        OnShieldCountChanged?.Invoke(ShieldCount);
    }

    /// <summary>
    /// Reset inventory to starting values (includes market bonus).
    /// </summary>
    public void ResetInventory()
    {
        MedkitCount = MarketData.GetTotalMedkits(startingMedkits);
        ShieldCount = MarketData.GetTotalShields(startingShields);
        IsShieldActive = false;
        shieldTimer = 0f;

        OnMedkitCountChanged?.Invoke(MedkitCount);
        OnShieldCountChanged?.Invoke(ShieldCount);
        OnShieldActiveChanged?.Invoke(false);
    }
}
