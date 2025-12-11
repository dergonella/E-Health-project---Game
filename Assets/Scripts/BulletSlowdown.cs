using UnityEngine;
using System;

/// <summary>
/// Bullet Slowdown Ability - Press O to slow down enemies and their projectiles.
/// Available in Level 2 and Level 3.
/// The player moves at NORMAL speed while everything else is slowed!
///
/// IMPORTANT: Add this component directly to the Player in Level 2 and Level 3 scenes,
/// or ensure MainLevelSetup is configured correctly.
/// </summary>
public class BulletSlowdown : MonoBehaviour
{
    [Header("Inventory")]
    [Tooltip("Starting number of slow motion uses")]
    public int startingSlowMotion = 3;

    [Header("Slowdown Settings")]
    [Tooltip("How slow enemies/projectiles become (0.1 = 10% speed, 90% slowdown)")]
    public float slowTimeScale = 0.1f;

    [Tooltip("Duration of slowdown effect (real seconds)")]
    public float duration = 5f;

    [Tooltip("Cooldown before ability can be used again")]
    public float cooldown = 15f;

    [Header("Audio (Optional)")]
    public AudioClip activateSound;
    public AudioClip deactivateSound;

    // Inventory
    public int SlowMotionCount { get; private set; }

    // State tracking
    public bool IsActive { get; private set; }
    public bool IsOnCooldown { get; private set; }
    public float RemainingDuration { get; private set; }
    public float RemainingCooldown { get; private set; }

    // Event for UI (inventory count)
    public event Action<int> OnCountChanged;

    // Store original values
    private float originalTimeScale = 1f;
    private float originalFixedDeltaTime;

    // Player speed compensation
    private PlayerController playerController;
    private Rigidbody2D playerRb;
    private float originalPlayerSpeed;
    private float originalPlayerAcceleration;
    private float originalPlayerDeceleration;

    // Events for UI
    public event Action<bool> OnSlowdownActiveChanged;
    public event Action<float, float> OnDurationChanged; // remaining, max
    public event Action<float, float> OnCooldownChanged; // remaining, max

    private AudioSource audioSource;
    private bool isInitialized = false;

    void Awake()
    {
        Debug.Log("[BulletSlowdown] Component AWAKE on " + gameObject.name);
    }

    void Start()
    {
        Debug.Log("[BulletSlowdown] Component START - Initializing...");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get player controller for speed compensation
        playerController = GetComponent<PlayerController>();
        playerRb = GetComponent<Rigidbody2D>();

        if (playerController != null)
        {
            originalPlayerSpeed = playerController.speed;
            originalPlayerAcceleration = playerController.acceleration;
            originalPlayerDeceleration = playerController.deceleration;
            Debug.Log($"[BulletSlowdown] Found PlayerController - Speed: {originalPlayerSpeed}");
        }
        else
        {
            Debug.LogWarning("[BulletSlowdown] No PlayerController found on this GameObject!");
        }

        // Store original fixed delta time for physics compensation
        originalFixedDeltaTime = Time.fixedDeltaTime;

        // Initialize inventory
        SlowMotionCount = startingSlowMotion;
        OnCountChanged?.Invoke(SlowMotionCount);

        // Ensure we start in normal state
        IsActive = false;
        IsOnCooldown = false;
        isInitialized = true;

        Debug.Log($"[BulletSlowdown] READY! {SlowMotionCount} slow motions available. Press O to activate.");
    }

    void Update()
    {
        // Handle input (O key)
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("[BulletSlowdown] O key pressed!");
            TryActivate();
        }

        // Handle active slowdown - use unscaledDeltaTime since we're modifying timeScale
        if (IsActive)
        {
            RemainingDuration -= Time.unscaledDeltaTime;
            OnDurationChanged?.Invoke(RemainingDuration, duration);

            if (RemainingDuration <= 0f)
            {
                Deactivate();
            }
        }

        // Handle cooldown - use unscaledDeltaTime for consistent cooldown
        if (IsOnCooldown && !IsActive)
        {
            RemainingCooldown -= Time.unscaledDeltaTime;
            OnCooldownChanged?.Invoke(RemainingCooldown, cooldown);

            if (RemainingCooldown <= 0f)
            {
                IsOnCooldown = false;
                RemainingCooldown = 0f;
                Debug.Log("[BulletSlowdown] Ready to use again!");
            }
        }
    }

    /// <summary>
    /// Try to activate bullet slowdown.
    /// </summary>
    public void TryActivate()
    {
        if (SlowMotionCount <= 0)
        {
            Debug.Log("[BulletSlowdown] No slow motion available!");
            return;
        }

        if (IsActive)
        {
            Debug.Log("[BulletSlowdown] Already active!");
            return;
        }

        if (IsOnCooldown)
        {
            Debug.Log($"[BulletSlowdown] On cooldown: {RemainingCooldown:F1}s remaining");
            return;
        }

        // Use one from inventory
        SlowMotionCount--;
        OnCountChanged?.Invoke(SlowMotionCount);

        Activate();
    }

    /// <summary>
    /// Add slow motion to inventory (from market/pickup).
    /// </summary>
    public void AddSlowMotion(int count = 1)
    {
        SlowMotionCount += count;
        OnCountChanged?.Invoke(SlowMotionCount);
        Debug.Log($"[BulletSlowdown] +{count}! Total: {SlowMotionCount}");
    }

    void Activate()
    {
        IsActive = true;
        RemainingDuration = duration;

        // Store current time scale
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

        // Store original player values BEFORE changing them
        if (playerController != null)
        {
            originalPlayerSpeed = playerController.speed;
            originalPlayerAcceleration = playerController.acceleration;
            originalPlayerDeceleration = playerController.deceleration;
        }

        // Apply slow motion to the world (enemies, projectiles, etc.)
        Time.timeScale = slowTimeScale;

        // CRITICAL: Also adjust fixedDeltaTime to maintain physics update rate
        // This ensures physics-based movement still works correctly
        Time.fixedDeltaTime = originalFixedDeltaTime * slowTimeScale;

        // Compensate player speed so they move at normal speed
        // When timeScale is 0.1, player needs 1/0.1 = 10x speed to feel normal
        if (playerController != null)
        {
            float compensation = 1f / slowTimeScale;
            playerController.speed = originalPlayerSpeed * compensation;
            playerController.acceleration = originalPlayerAcceleration * compensation;
            playerController.deceleration = originalPlayerDeceleration * compensation;
            Debug.Log($"[BulletSlowdown] Player speed boosted from {originalPlayerSpeed} to {playerController.speed} ({compensation}x)");
        }

        Debug.Log($"[BulletSlowdown] ====== ACTIVATED! ======");
        Debug.Log($"[BulletSlowdown] World slowed to {slowTimeScale * 100}% speed");
        Debug.Log($"[BulletSlowdown] TimeScale: {Time.timeScale}, FixedDeltaTime: {Time.fixedDeltaTime}");

        OnSlowdownActiveChanged?.Invoke(true);
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
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // Restore player's original speed
        if (playerController != null)
        {
            playerController.speed = originalPlayerSpeed;
            playerController.acceleration = originalPlayerAcceleration;
            playerController.deceleration = originalPlayerDeceleration;
            Debug.Log($"[BulletSlowdown] Player speed restored to {originalPlayerSpeed}");
        }

        // Start cooldown
        IsOnCooldown = true;
        RemainingCooldown = cooldown;

        Debug.Log("[BulletSlowdown] ====== DEACTIVATED! ======");
        Debug.Log($"[BulletSlowdown] TimeScale restored to {Time.timeScale}");
        Debug.Log($"[BulletSlowdown] Cooldown: {cooldown}s");

        OnSlowdownActiveChanged?.Invoke(false);
        OnCooldownChanged?.Invoke(RemainingCooldown, cooldown);

        // Play sound
        if (audioSource != null && deactivateSound != null)
        {
            audioSource.PlayOneShot(deactivateSound);
        }
    }

    /// <summary>
    /// Force deactivate (e.g., on death).
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
            Time.fixedDeltaTime = originalFixedDeltaTime;

            if (playerController != null)
            {
                playerController.speed = originalPlayerSpeed;
                playerController.acceleration = originalPlayerAcceleration;
                playerController.deceleration = originalPlayerDeceleration;
            }
        }

        // Reset inventory
        SlowMotionCount = startingSlowMotion;
        OnCountChanged?.Invoke(SlowMotionCount);

        IsActive = false;
        IsOnCooldown = false;
        RemainingDuration = 0f;
        RemainingCooldown = 0f;

        OnSlowdownActiveChanged?.Invoke(false);
    }

    void OnDisable()
    {
        // Ensure time scale and player values are reset
        if (IsActive)
        {
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;

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
        // Ensure time scale is reset when destroyed
        if (IsActive)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f; // Default Unity fixed delta time

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
