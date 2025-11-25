using UnityEngine;
using System;

/// <summary>
/// Manages player abilities: Shield and Slow Motion
/// </summary>
public class AbilitySystem : MonoBehaviour
{
    [Header("Shield Ability")]
    public bool hasShield = false;
    public float shieldDuration = 5f;  // Increased from 3 - longer shield for testing
    public float shieldCooldown = 5f;  // Reduced from 15 - quick cooldown for testing
    public float shieldFocusCost = 30f;  // Not used anymore
    private float shieldTimer = 0f;
    private float shieldCooldownTimer = 0f;
    public bool isShieldActive { get; private set; }

    [Header("Slow Motion Ability")]
    public bool hasSlowMotion = false;
    public float slowMotionTimeScale = 0.2f;  // Even slower for testing
    public float slowMotionDuration = 6f;  // Increased from 4 - longer duration for testing
    public float slowMotionCooldown = 8f;  // Reduced from 20 - quick cooldown for testing
    public float slowMotionFocusCost = 40f;  // Not used anymore
    private float slowMotionTimer = 0f;
    private float slowMotionCooldownTimer = 0f;
    public bool isSlowMotionActive { get; private set; }

    // Events
    public event Action<bool> OnShieldStatusChanged;
    public event Action<float, float> OnShieldCooldownChanged; // current, max
    public event Action<bool> OnSlowMotionStatusChanged;
    public event Action<float, float> OnSlowMotionCooldownChanged; // current, max

    private HealthSystem healthSystem;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    void Update()
    {
        HandleShield();
        HandleSlowMotion();
        HandleInput();
    }

    void HandleInput()
    {
        // Shield - Q key
        if (hasShield && Input.GetKeyDown(KeyCode.Q))
        {
            ActivateShield();
        }

        // Slow Motion - E key
        if (hasSlowMotion && Input.GetKeyDown(KeyCode.E))
        {
            ActivateSlowMotion();
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

    void HandleSlowMotion()
    {
        // Handle active slow motion
        if (isSlowMotionActive)
        {
            slowMotionTimer -= Time.unscaledDeltaTime; // Use unscaled time
            if (slowMotionTimer <= 0f)
            {
                DeactivateSlowMotion();
            }
        }

        // Handle cooldown
        if (slowMotionCooldownTimer > 0f)
        {
            slowMotionCooldownTimer -= Time.deltaTime;
            OnSlowMotionCooldownChanged?.Invoke(slowMotionCooldown - slowMotionCooldownTimer, slowMotionCooldown);
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

        // NO FOCUS COST - abilities are free now!

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

    public void ActivateSlowMotion()
    {
        // Check if already active or on cooldown
        if (isSlowMotionActive || slowMotionCooldownTimer > 0f)
        {
            Debug.Log("Slow motion not ready!");
            return;
        }

        // NO FOCUS COST - abilities are free now!

        // Activate slow motion
        isSlowMotionActive = true;
        slowMotionTimer = slowMotionDuration;
        Time.timeScale = slowMotionTimeScale;
        OnSlowMotionStatusChanged?.Invoke(true);
        Debug.Log("Slow motion activated!");
    }

    void DeactivateSlowMotion()
    {
        isSlowMotionActive = false;
        slowMotionCooldownTimer = slowMotionCooldown;
        Time.timeScale = 1f;
        OnSlowMotionStatusChanged?.Invoke(false);
        OnSlowMotionCooldownChanged?.Invoke(0f, slowMotionCooldown);
        Debug.Log("Slow motion deactivated!");
    }

    public void ResetAbilities()
    {
        if (isShieldActive)
        {
            DeactivateShield();
        }
        if (isSlowMotionActive)
        {
            DeactivateSlowMotion();
        }

        shieldTimer = 0f;
        shieldCooldownTimer = 0f;
        slowMotionTimer = 0f;
        slowMotionCooldownTimer = 0f;
    }

    void OnDestroy()
    {
        // Make sure to reset time scale when destroyed
        Time.timeScale = 1f;
    }
}
