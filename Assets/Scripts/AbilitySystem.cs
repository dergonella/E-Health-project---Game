using UnityEngine;
using System;

/// <summary>
/// Manages player abilities: Shield (legacy system)
/// Note: New levels use PlayerInventory for shield instead
/// </summary>
public class AbilitySystem : MonoBehaviour
{
    [Header("Shield Ability")]
    public bool hasShield = false;
    public float shieldDuration = 5f;
    public float shieldCooldown = 5f;
    private float shieldTimer = 0f;
    private float shieldCooldownTimer = 0f;
    public bool isShieldActive { get; private set; }

    // Events
    public event Action<bool> OnShieldStatusChanged;
    public event Action<float, float> OnShieldCooldownChanged; // current, max

    private HealthSystem healthSystem;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    void Update()
    {
        HandleShield();
        HandleInput();
    }

    void HandleInput()
    {
        // Shield - Q key (legacy)
        if (hasShield && Input.GetKeyDown(KeyCode.Q))
        {
            ActivateShield();
        }
    }

    void HandleShield()
    {
        // Handle active shield
        if (isShieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }

        // Handle cooldown
        if (shieldCooldownTimer > 0f)
        {
            shieldCooldownTimer -= Time.deltaTime;
            OnShieldCooldownChanged?.Invoke(shieldCooldown - shieldCooldownTimer, shieldCooldown);
        }
    }

    public void ActivateShield()
    {
        // Check if already active or on cooldown
        if (isShieldActive || shieldCooldownTimer > 0f)
        {
            Debug.Log("Shield not ready!");
            return;
        }

        // Activate shield
        isShieldActive = true;
        shieldTimer = shieldDuration;
        OnShieldStatusChanged?.Invoke(true);
        Debug.Log("Shield activated!");
    }

    void DeactivateShield()
    {
        isShieldActive = false;
        shieldCooldownTimer = shieldCooldown;
        OnShieldStatusChanged?.Invoke(false);
        OnShieldCooldownChanged?.Invoke(0f, shieldCooldown);
        Debug.Log("Shield deactivated!");
    }

    public void ResetAbilities()
    {
        if (isShieldActive)
        {
            DeactivateShield();
        }

        shieldTimer = 0f;
        shieldCooldownTimer = 0f;
    }
}
