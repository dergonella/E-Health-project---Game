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

    [Header("Status Effects")]
    public bool isPoisoned = false;
    public float poisonDamagePerSecond = 2f;  // Reduced from 3 for easier testing
    public float poisonDuration = 6f;  // Reduced from 8 for easier testing
    public float poisonSpeedReduction = 0.3f; // Changed from 0.4 - only 30% slower for easier testing
    private float poisonTimer = 0f;

    public bool isStunned = false;
    private float stunTimer = 0f;

    // Events
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float, float> OnFocusChanged; // current, max
    public event Action OnDeath;
    public event Action<bool> OnPoisonStatusChanged;
    public event Action<bool> OnStunStatusChanged;

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

        // Handle poison damage over time
        if (isPoisoned)
        {
            poisonTimer -= Time.deltaTime;

            // Apply poison damage
            TakeDamage(poisonDamagePerSecond * Time.deltaTime, false);

            // Check if poison expired
            if (poisonTimer <= 0f)
            {
                CurePoison();
            }
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

    public void TakeDamage(float damage, bool triggerInvulnerability = true)
    {
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
        if (!isPoisoned)
        {
            isPoisoned = true;
            poisonTimer = poisonDuration;
            OnPoisonStatusChanged?.Invoke(true);
            Debug.Log("Player poisoned!");
        }
        else
        {
            // Refresh poison duration
            poisonTimer = poisonDuration;
        }
    }

    public void CurePoison()
    {
        if (isPoisoned)
        {
            isPoisoned = false;
            poisonTimer = 0f;
            OnPoisonStatusChanged?.Invoke(false);
            Debug.Log("Poison cured!");
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

    void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("Player died!");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        currentFocus = maxFocus;
        isPoisoned = false;
        isStunned = false;
        isInvulnerable = false;
        poisonTimer = 0f;
        stunTimer = 0f;
        invulnerabilityTimer = 0f;

        // Notify UI of reset values
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnFocusChanged?.Invoke(currentFocus, maxFocus);
        OnPoisonStatusChanged?.Invoke(false);
        OnStunStatusChanged?.Invoke(false);
    }
}
