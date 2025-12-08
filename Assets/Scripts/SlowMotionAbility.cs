using UnityEngine;
using System;

/// <summary>
/// Slow Motion Ability - Press Q to slow down time.
/// Available from Level 2 onwards.
/// Duration: 5 seconds, Cooldown: 15 seconds
///
/// BULLET TIME MODE: Player moves at normal speed while enemies are slowed!
/// </summary>
public class SlowMotionAbility : MonoBehaviour
{
    [Header("Slow Motion Settings")]
    [Tooltip("How slow enemies become (0.3 = 30% speed)")]
    public float slowTimeScale = 0.3f;

    [Tooltip("Duration of slow motion effect (real seconds)")]
    public float duration = 5f;

    [Tooltip("Cooldown before ability can be used again")]
    public float cooldown = 15f;

    [Header("Audio (Optional)")]
    public AudioClip activateSound;
    public AudioClip deactivateSound;

    // State tracking
    public bool IsActive { get; private set; }
    public bool IsOnCooldown { get; private set; }
    public float RemainingDuration { get; private set; }
    public float RemainingCooldown { get; private set; }

    // Store original time scale
    private float originalTimeScale = 1f;
    private float originalFixedDeltaTime;

    // Player speed compensation - need to boost speed AND acceleration
    private PlayerController playerController;
    private float originalPlayerSpeed;
    private float originalPlayerAcceleration;
    private float originalPlayerDeceleration;

    // Events for UI
    public event Action<bool> OnSlowMotionActiveChanged;
    public event Action<float, float> OnDurationChanged; // remaining, max
    public event Action<float, float> OnCooldownChanged; // remaining, max

    private AudioSource audioSource;

    void Start()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
        audioSource = GetComponent<AudioSource>();

        // Get player controller for speed compensation
        playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            originalPlayerSpeed = playerController.speed;
            originalPlayerAcceleration = playerController.acceleration;
            originalPlayerDeceleration = playerController.deceleration;
        }

        // Ensure we start in normal time
        IsActive = false;
        IsOnCooldown = false;
    }

    void Update()
    {
        // Handle input (Q key)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("[SlowMotionAbility] Q key pressed! Attempting to activate...");
            TryActivate();
        }

        // Handle active slow motion
        if (IsActive)
        {
            // Use unscaled delta time since we're modifying timeScale
            RemainingDuration -= Time.unscaledDeltaTime;
            OnDurationChanged?.Invoke(RemainingDuration, duration);

            if (RemainingDuration <= 0f)
            {
                Deactivate();
            }
        }

        // Handle cooldown
        if (IsOnCooldown && !IsActive)
        {
            // Use unscaled delta time for consistent cooldown
            RemainingCooldown -= Time.unscaledDeltaTime;
            OnCooldownChanged?.Invoke(RemainingCooldown, cooldown);

            if (RemainingCooldown <= 0f)
            {
                IsOnCooldown = false;
                RemainingCooldown = 0f;
                Debug.Log("Slow Motion ready!");
            }
        }
    }

    /// <summary>
    /// Try to activate slow motion.
    /// </summary>
    public void TryActivate()
    {
        if (IsActive)
        {
            Debug.Log("Slow motion already active!");
            return;
        }

        if (IsOnCooldown)
        {
            Debug.Log($"Slow motion on cooldown: {RemainingCooldown:F1}s remaining");
            return;
        }

        Activate();
    }

    void Activate()
    {
        IsActive = true;
        RemainingDuration = duration;

        // Store current time scale
        originalTimeScale = Time.timeScale;

        // Store original player values BEFORE changing them
        if (playerController != null)
        {
            originalPlayerSpeed = playerController.speed;
            originalPlayerAcceleration = playerController.acceleration;
            originalPlayerDeceleration = playerController.deceleration;
        }

        // Apply slow motion to the world
        Time.timeScale = slowTimeScale;
        // DON'T change fixedDeltaTime - keep physics running at normal speed for player
        // Time.fixedDeltaTime = originalFixedDeltaTime * slowTimeScale;

        // BULLET TIME: Compensate player speed, acceleration, and deceleration
        // When timeScale is 0.3, player needs 1/0.3 = 3.33x everything to feel normal
        if (playerController != null)
        {
            float compensation = 1f / slowTimeScale;
            playerController.speed = originalPlayerSpeed * compensation;
            playerController.acceleration = originalPlayerAcceleration * compensation;
            playerController.deceleration = originalPlayerDeceleration * compensation;
            Debug.Log($"[SlowMotionAbility] Player boosted by {compensation}x - Speed: {playerController.speed}, Accel: {playerController.acceleration}");
        }

        Debug.Log($"[SlowMotionAbility] BULLET TIME ACTIVATED! World slowed to {slowTimeScale * 100}%");
        OnSlowMotionActiveChanged?.Invoke(true);
        OnDurationChanged?.Invoke(RemainingDuration, duration);

        // Play sound
        if (audioSource != null && activateSound != null)
        {
            audioSource.PlayOneShot(activateSound);
        }
    }

    void Deactivate()
    {
        IsActive = false;
        RemainingDuration = 0f;

        // Restore normal time
        Time.timeScale = originalTimeScale;
        // Time.fixedDeltaTime = originalFixedDeltaTime; // Not needed since we didn't change it

        // Restore player's original speed, acceleration, and deceleration
        if (playerController != null)
        {
            playerController.speed = originalPlayerSpeed;
            playerController.acceleration = originalPlayerAcceleration;
            playerController.deceleration = originalPlayerDeceleration;
            Debug.Log($"[SlowMotionAbility] Player restored - Speed: {originalPlayerSpeed}, Accel: {originalPlayerAcceleration}");
        }

        // Start cooldown
        IsOnCooldown = true;
        RemainingCooldown = cooldown;

        Debug.Log("[SlowMotionAbility] Bullet time ended. Starting cooldown.");
        OnSlowMotionActiveChanged?.Invoke(false);
        OnCooldownChanged?.Invoke(RemainingCooldown, cooldown);

        // Play sound
        if (audioSource != null && deactivateSound != null)
        {
            audioSource.PlayOneShot(deactivateSound);
        }
    }

    /// <summary>
    /// Force deactivate slow motion (e.g., on death).
    /// </summary>
    public void ForceDeactivate()
    {
        if (IsActive)
        {
            Deactivate();
        }
    }

    /// <summary>
    /// Reset ability state (for level restart).
    /// </summary>
    public void ResetAbility()
    {
        if (IsActive)
        {
            Time.timeScale = originalTimeScale;

            // Restore player values
            if (playerController != null)
            {
                playerController.speed = originalPlayerSpeed;
                playerController.acceleration = originalPlayerAcceleration;
                playerController.deceleration = originalPlayerDeceleration;
            }
        }

        IsActive = false;
        IsOnCooldown = false;
        RemainingDuration = 0f;
        RemainingCooldown = 0f;

        OnSlowMotionActiveChanged?.Invoke(false);
    }

    void OnDisable()
    {
        // Ensure time scale and player values are reset when component is disabled
        if (IsActive)
        {
            Time.timeScale = originalTimeScale;

            if (playerController != null)
            {
                playerController.speed = originalPlayerSpeed;
                playerController.acceleration = originalPlayerAcceleration;
                playerController.deceleration = originalPlayerDeceleration;
            }
        }
    }

    void OnDestroy()
    {
        // Ensure time scale and player values are reset when object is destroyed
        if (IsActive)
        {
            Time.timeScale = 1f;

            if (playerController != null)
            {
                playerController.speed = originalPlayerSpeed;
                playerController.acceleration = originalPlayerAcceleration;
                playerController.deceleration = originalPlayerDeceleration;
            }
        }
    }

    /// <summary>
    /// Get cooldown progress (0-1 for UI)
    /// </summary>
    public float GetCooldownProgress()
    {
        if (!IsOnCooldown) return 1f;
        return 1f - (RemainingCooldown / cooldown);
    }
}
