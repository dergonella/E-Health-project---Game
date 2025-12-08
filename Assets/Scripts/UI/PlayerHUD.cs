using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Player HUD - Displays health bar, inventory counts, and ability cooldowns.
/// Attach to a Canvas in your scene.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("Health Bar")]
    public Slider healthSlider;
    public Image healthFillImage;
    public TextMeshProUGUI healthText;
    public Color healthyColor = new Color(0.2f, 0.8f, 0.2f);
    public Color damagedColor = new Color(0.8f, 0.8f, 0.2f);
    public Color criticalColor = new Color(0.8f, 0.2f, 0.2f);

    [Header("Inventory Display")]
    public TextMeshProUGUI medkitCountText;
    public TextMeshProUGUI shieldCountText;
    public Image shieldActiveIndicator;

    [Header("Status Effects")]
    public GameObject poisonIndicator;
    public GameObject stunIndicator;

    [Header("Bullet Slowdown UI")]
    [Tooltip("Container for all slowdown UI elements - hidden when ability not available")]
    public GameObject slowdownUIContainer;
    [Tooltip("Fill bar showing cooldown progress (fills up as cooldown completes)")]
    public Slider slowdownCooldownSlider;
    [Tooltip("Fill bar showing active duration (depletes while active)")]
    public Slider slowdownDurationSlider;
    [Tooltip("Text showing 'READY', cooldown time, or 'ACTIVE'")]
    public TextMeshProUGUI slowdownStatusText;
    [Tooltip("Icon that pulses/glows when ability is ready")]
    public Image slowdownIcon;
    [Tooltip("Full-screen overlay effect when slowdown is active")]
    public Image slowdownScreenOverlay;
    [Tooltip("Color of the icon when ready")]
    public Color slowdownReadyColor = new Color(0.2f, 0.8f, 1f);
    [Tooltip("Color of the icon when on cooldown")]
    public Color slowdownCooldownColor = new Color(0.5f, 0.5f, 0.5f);
    [Tooltip("Color of the icon when active")]
    public Color slowdownActiveColor = new Color(1f, 0.8f, 0.2f);

    [Header("Score Display")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI targetScoreText;

    // References
    private HealthSystem healthSystem;
    private PlayerInventory inventory;
    private PlayerController playerController;
    private BulletSlowdown bulletSlowdown;

    // Slowdown UI animation
    private float iconPulseTime = 0f;
    private bool wasSlowdownActive = false;

    void Start()
    {
        // Find player and get components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            healthSystem = player.GetComponent<HealthSystem>();
            inventory = player.GetComponent<PlayerInventory>();
            playerController = player.GetComponent<PlayerController>();
            bulletSlowdown = player.GetComponent<BulletSlowdown>();

            // Subscribe to health events
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateHealthBar;
                healthSystem.OnPoisonStatusChanged += UpdatePoisonStatus;
                healthSystem.OnStunStatusChanged += UpdateStunStatus;

                // Initialize health display
                UpdateHealthBar(healthSystem.currentHealth, healthSystem.maxHealth);
            }

            // Subscribe to inventory events
            if (inventory != null)
            {
                inventory.OnMedkitCountChanged += UpdateMedkitCount;
                inventory.OnShieldCountChanged += UpdateShieldCount;
                inventory.OnShieldActiveChanged += UpdateShieldActive;

                // Initialize inventory display
                UpdateMedkitCount(inventory.MedkitCount);
                UpdateShieldCount(inventory.ShieldCount);
                UpdateShieldActive(false);
            }

            // Subscribe to bullet slowdown events
            if (bulletSlowdown != null && bulletSlowdown.enabled)
            {
                bulletSlowdown.OnSlowdownActiveChanged += OnSlowdownActiveChanged;
                bulletSlowdown.OnDurationChanged += OnSlowdownDurationChanged;
                bulletSlowdown.OnCooldownChanged += OnSlowdownCooldownChanged;

                // Show slowdown UI
                if (slowdownUIContainer != null)
                {
                    slowdownUIContainer.SetActive(true);
                }
                InitializeSlowdownUI();
                Debug.Log("[PlayerHUD] Bullet Slowdown UI initialized");
            }
            else
            {
                // Hide slowdown UI if ability not available
                if (slowdownUIContainer != null)
                {
                    slowdownUIContainer.SetActive(false);
                }
            }
        }

        // Initialize status indicators
        if (poisonIndicator != null) poisonIndicator.SetActive(false);
        if (stunIndicator != null) stunIndicator.SetActive(false);

        // Initialize screen overlay (hidden)
        if (slowdownScreenOverlay != null)
        {
            slowdownScreenOverlay.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Update score from player controller
        if (playerController != null && scoreText != null)
        {
            scoreText.text = $"Score: {playerController.score}";
        }

        // Update slowdown UI animations
        UpdateSlowdownUIAnimations();
    }

    void UpdateHealthBar(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        // Update color based on health percentage
        if (healthFillImage != null)
        {
            float percent = current / max;
            if (percent > 0.5f)
                healthFillImage.color = healthyColor;
            else if (percent > 0.25f)
                healthFillImage.color = damagedColor;
            else
                healthFillImage.color = criticalColor;
        }
    }

    void UpdateMedkitCount(int count)
    {
        if (medkitCountText != null)
        {
            medkitCountText.text = $"Medkit (H): {count}";
        }
    }

    void UpdateShieldCount(int count)
    {
        if (shieldCountText != null)
        {
            shieldCountText.text = $"Shield (F): {count}";
        }
    }

    void UpdateShieldActive(bool active)
    {
        if (shieldActiveIndicator != null)
        {
            shieldActiveIndicator.gameObject.SetActive(active);
        }
    }

    void UpdatePoisonStatus(bool isPoisoned)
    {
        if (poisonIndicator != null)
        {
            poisonIndicator.SetActive(isPoisoned);
        }
    }

    void UpdateStunStatus(bool isStunned)
    {
        if (stunIndicator != null)
        {
            stunIndicator.SetActive(isStunned);
        }
    }

    public void SetTargetScore(int target)
    {
        if (targetScoreText != null)
        {
            targetScoreText.text = $"Target: {target}";
        }
    }

    // ===== BULLET SLOWDOWN UI METHODS =====

    void InitializeSlowdownUI()
    {
        // Set initial state - ready to use
        if (slowdownCooldownSlider != null)
        {
            slowdownCooldownSlider.maxValue = 1f;
            slowdownCooldownSlider.value = 1f; // Full = ready
        }

        if (slowdownDurationSlider != null)
        {
            slowdownDurationSlider.gameObject.SetActive(false); // Hidden until active
        }

        if (slowdownStatusText != null)
        {
            slowdownStatusText.text = "SLOWDOWN (O)\nREADY";
        }

        if (slowdownIcon != null)
        {
            slowdownIcon.color = slowdownReadyColor;
        }
    }

    void OnSlowdownActiveChanged(bool isActive)
    {
        wasSlowdownActive = isActive;

        if (isActive)
        {
            // Slowdown activated
            if (slowdownStatusText != null)
            {
                slowdownStatusText.text = "SLOWDOWN (O)\nACTIVE!";
            }

            if (slowdownIcon != null)
            {
                slowdownIcon.color = slowdownActiveColor;
            }

            // Show duration bar, hide cooldown bar
            if (slowdownDurationSlider != null)
            {
                slowdownDurationSlider.gameObject.SetActive(true);
                slowdownDurationSlider.maxValue = bulletSlowdown.duration;
                slowdownDurationSlider.value = bulletSlowdown.duration;
            }

            if (slowdownCooldownSlider != null)
            {
                slowdownCooldownSlider.gameObject.SetActive(false);
            }

            // Show screen overlay effect
            if (slowdownScreenOverlay != null)
            {
                slowdownScreenOverlay.gameObject.SetActive(true);
                // Cyan tint with low alpha
                slowdownScreenOverlay.color = new Color(0.2f, 0.8f, 1f, 0.15f);
            }
        }
        else
        {
            // Slowdown deactivated - now on cooldown
            if (slowdownStatusText != null)
            {
                slowdownStatusText.text = "SLOWDOWN (O)\nCOOLDOWN...";
            }

            if (slowdownIcon != null)
            {
                slowdownIcon.color = slowdownCooldownColor;
            }

            // Hide duration bar, show cooldown bar
            if (slowdownDurationSlider != null)
            {
                slowdownDurationSlider.gameObject.SetActive(false);
            }

            if (slowdownCooldownSlider != null)
            {
                slowdownCooldownSlider.gameObject.SetActive(true);
                slowdownCooldownSlider.maxValue = 1f;
                slowdownCooldownSlider.value = 0f; // Empty = on cooldown
            }

            // Hide screen overlay
            if (slowdownScreenOverlay != null)
            {
                slowdownScreenOverlay.gameObject.SetActive(false);
            }
        }
    }

    void OnSlowdownDurationChanged(float remaining, float max)
    {
        if (slowdownDurationSlider != null)
        {
            slowdownDurationSlider.value = remaining;
        }

        if (slowdownStatusText != null && bulletSlowdown != null && bulletSlowdown.IsActive)
        {
            slowdownStatusText.text = $"SLOWDOWN (O)\n{remaining:F1}s";
        }
    }

    void OnSlowdownCooldownChanged(float remaining, float max)
    {
        if (slowdownCooldownSlider != null)
        {
            // Progress fills up as cooldown completes (1 - remaining/max)
            float progress = 1f - (remaining / max);
            slowdownCooldownSlider.value = progress;
        }

        if (slowdownStatusText != null)
        {
            if (remaining <= 0f)
            {
                // Cooldown complete - ready again
                slowdownStatusText.text = "SLOWDOWN (O)\nREADY";
                if (slowdownIcon != null)
                {
                    slowdownIcon.color = slowdownReadyColor;
                }
            }
            else
            {
                slowdownStatusText.text = $"SLOWDOWN (O)\n{remaining:F0}s";
            }
        }
    }

    void UpdateSlowdownUIAnimations()
    {
        if (bulletSlowdown == null || !bulletSlowdown.enabled) return;

        // Pulse the icon when ready
        if (!bulletSlowdown.IsActive && !bulletSlowdown.IsOnCooldown && slowdownIcon != null)
        {
            iconPulseTime += Time.unscaledDeltaTime * 3f; // Use unscaled time
            float pulse = 0.8f + Mathf.Sin(iconPulseTime) * 0.2f;
            slowdownIcon.transform.localScale = Vector3.one * pulse;
        }
        else if (slowdownIcon != null)
        {
            // Reset scale when not pulsing
            slowdownIcon.transform.localScale = Vector3.one;
        }

        // Animate screen overlay when active
        if (bulletSlowdown.IsActive && slowdownScreenOverlay != null)
        {
            // Subtle breathing effect on the overlay
            float breathe = 0.12f + Mathf.Sin(Time.unscaledTime * 2f) * 0.05f;
            Color c = slowdownScreenOverlay.color;
            c.a = breathe;
            slowdownScreenOverlay.color = c;
        }
    }

    /// <summary>
    /// Call this to manually refresh slowdown UI (e.g., if component added at runtime)
    /// </summary>
    public void RefreshSlowdownUI()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            bulletSlowdown = player.GetComponent<BulletSlowdown>();

            if (bulletSlowdown != null && bulletSlowdown.enabled)
            {
                // Unsubscribe first to avoid duplicates
                bulletSlowdown.OnSlowdownActiveChanged -= OnSlowdownActiveChanged;
                bulletSlowdown.OnDurationChanged -= OnSlowdownDurationChanged;
                bulletSlowdown.OnCooldownChanged -= OnSlowdownCooldownChanged;

                // Subscribe
                bulletSlowdown.OnSlowdownActiveChanged += OnSlowdownActiveChanged;
                bulletSlowdown.OnDurationChanged += OnSlowdownDurationChanged;
                bulletSlowdown.OnCooldownChanged += OnSlowdownCooldownChanged;

                if (slowdownUIContainer != null)
                {
                    slowdownUIContainer.SetActive(true);
                }
                InitializeSlowdownUI();
            }
            else if (slowdownUIContainer != null)
            {
                slowdownUIContainer.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHealthBar;
            healthSystem.OnPoisonStatusChanged -= UpdatePoisonStatus;
            healthSystem.OnStunStatusChanged -= UpdateStunStatus;
        }

        if (inventory != null)
        {
            inventory.OnMedkitCountChanged -= UpdateMedkitCount;
            inventory.OnShieldCountChanged -= UpdateShieldCount;
            inventory.OnShieldActiveChanged -= UpdateShieldActive;
        }

        if (bulletSlowdown != null)
        {
            bulletSlowdown.OnSlowdownActiveChanged -= OnSlowdownActiveChanged;
            bulletSlowdown.OnDurationChanged -= OnSlowdownDurationChanged;
            bulletSlowdown.OnCooldownChanged -= OnSlowdownCooldownChanged;
        }
    }
}
