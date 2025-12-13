using UnityEngine;
using System;

/// <summary>
/// Manages player health and focus
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 200f;  // Doubled from 100 for easy testing!
    public float currentHealth = 200f;

    [Header("Focus Settings")]
    public float maxFocus = 100f;
    public float currentFocus = 100f;
    public float focusRegenRate = 5f; // Focus per second

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 0.1f; // Reduced from 0.5 - shorter invulnerability for projectiles
    private float invulnerabilityTimer = 0f;
    public bool isInvulnerable { get; private set; }

    [Header("Poison Settings (DEADLY - only medkit cures!)")]
    public bool isPoisoned = false;
    [Tooltip("Base poison damage per second (stacks with each hit!)")]
    public float poisonDamagePerSecond = 8f;  // Much higher base damage
    [Tooltip("How much slower player moves while poisoned")]
    public float poisonSpeedReduction = 0.4f; // 40% slower
    [Tooltip("Current poison stacks - each hit adds 1 stack, damage = stacks * damagePerSecond")]
    public int poisonStacks = 0;
    private const int MAX_POISON_STACKS = 10; // Cap to prevent instant death

    public bool isStunned = false;
    private float stunTimer = 0f;

    // Shield reference (for blocking damage)
    private PlayerInventory inventory;

    // Events
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float, float> OnFocusChanged; // current, max
    public event Action OnDeath;
    public event Action<bool> OnPoisonStatusChanged;
    public event Action<bool> OnStunStatusChanged;

    void Start()
    {
        // Get inventory reference for shield checking
        inventory = GetComponent<PlayerInventory>();

        // Initialize health to max at start
        currentHealth = maxHealth;
        currentFocus = maxFocus;

        // Notify UI of initial values (fixes empty health bar at start)
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnFocusChanged?.Invoke(currentFocus, maxFocus);
    }

    void Update()
    {
        // Regenerate focus over time
        if (currentFocus < maxFocus)
        {
            RestoreFocus(focusRegenRate * Time.deltaTime);
        }

        // Handle invulnerability timer
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
            }
        }

        // Handle poison damage over time (DEADLY - continuous until medkit!)
        if (isPoisoned && poisonStacks > 0)
        {
            // Damage scales with stacks: more hits = faster death!
            // Each stack adds full damage per second
            float totalPoisonDamage = poisonStacks * poisonDamagePerSecond * Time.deltaTime;

            // Apply poison damage (bypass shield - poison DoT continues even with shield)
            TakeDamage(totalPoisonDamage, false, true);

            // NO timer expiration - only medkit cures poison!
        }

        // Handle stun timer
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                OnStunStatusChanged?.Invoke(false);
            }
        }
    }

    public void TakeDamage(float damage, bool triggerInvulnerability = true, bool bypassShield = false)
    {
        // Check if shield blocks this damage (unless bypassed - e.g., for poison DoT)
        if (!bypassShield && inventory != null && inventory.IsShieldActive)
        {
            Debug.Log($"[HealthSystem] Shield blocked {damage} damage!");
            return;
        }

        if (isInvulnerable && triggerInvulnerability) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (triggerInvulnerability)
        {
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ConsumeFocus(float amount)
    {
        currentFocus -= amount;
        currentFocus = Mathf.Max(0f, currentFocus);
        OnFocusChanged?.Invoke(currentFocus, maxFocus);
    }

    public void RestoreFocus(float amount)
    {
        currentFocus += amount;
        currentFocus = Mathf.Min(currentFocus, maxFocus);
        OnFocusChanged?.Invoke(currentFocus, maxFocus);
    }

    public bool HasFocus(float amount)
    {
        return currentFocus >= amount;
    }

    public void ApplyPoison()
    {
        // Shield blocks poison application
        if (inventory != null && inventory.IsShieldActive)
        {
            Debug.Log("[HealthSystem] Shield blocked poison application!");
            return;
        }

        // Add a poison stack (damage increases with each hit!)
        if (poisonStacks < MAX_POISON_STACKS)
        {
            poisonStacks++;
            Debug.Log($"[HealthSystem] POISON STACK ADDED! Now at {poisonStacks}/{MAX_POISON_STACKS} stacks. DPS: {poisonStacks * poisonDamagePerSecond}");
        }
        else
        {
            Debug.Log($"[HealthSystem] MAX POISON STACKS ({MAX_POISON_STACKS}) reached! Player is doomed without medkit!");
        }

        if (!isPoisoned)
        {
            isPoisoned = true;
            OnPoisonStatusChanged?.Invoke(true);
            Debug.Log("[HealthSystem] PLAYER POISONED! Only medkit can cure!");
        }
    }

    public void CurePoison()
    {
        if (isPoisoned || poisonStacks > 0)
        {
            int oldStacks = poisonStacks;
            isPoisoned = false;
            poisonStacks = 0;
            OnPoisonStatusChanged?.Invoke(false);
            Debug.Log($"[HealthSystem] POISON CURED! Cleared {oldStacks} stacks with medkit!");
        }
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        OnStunStatusChanged?.Invoke(true);
    }

    public float GetSpeedMultiplier()
    {
        if (isPoisoned)
        {
            return 1f - poisonSpeedReduction;
        }
        return 1f;
    }

    /// <summary>
    /// Returns health as percentage (0-1)
    /// </summary>
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// Check if player is alive
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0f;
    }

    void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("Player died!");

        // Trigger game over
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
        {
            GameManager.Instance.GameOver(false);
            Debug.Log("[HealthSystem] Game Over triggered - player health reached 0!");
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        currentFocus = maxFocus;
        isPoisoned = false;
        poisonStacks = 0;
        isStunned = false;
        isInvulnerable = false;
        stunTimer = 0f;
        invulnerabilityTimer = 0f;

        // Notify UI of reset values
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnFocusChanged?.Invoke(currentFocus, maxFocus);
        OnPoisonStatusChanged?.Invoke(false);
        OnStunStatusChanged?.Invoke(false);
    }
}
