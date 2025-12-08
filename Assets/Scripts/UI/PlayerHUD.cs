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

    [Header("Slow Motion Display")]
    public GameObject slowMotionPanel;
    public Slider slowMotionCooldownSlider;
    public TextMeshProUGUI slowMotionText;
    public Image slowMotionActiveIndicator;

    [Header("Status Effects")]
    public GameObject poisonIndicator;
    public GameObject stunIndicator;

    [Header("Score Display")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI targetScoreText;

    // References
    private HealthSystem healthSystem;
    private PlayerInventory inventory;
    private SlowMotionAbility slowMotion;
    private PlayerController playerController;

    void Start()
    {
        // Find player and get components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            healthSystem = player.GetComponent<HealthSystem>();
            inventory = player.GetComponent<PlayerInventory>();
            slowMotion = player.GetComponent<SlowMotionAbility>();
            playerController = player.GetComponent<PlayerController>();

            // Subscribe to events
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateHealthBar;
                healthSystem.OnPoisonStatusChanged += UpdatePoisonStatus;
                healthSystem.OnStunStatusChanged += UpdateStunStatus;

                // Initialize health display
                UpdateHealthBar(healthSystem.currentHealth, healthSystem.maxHealth);
            }

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

            if (slowMotion != null)
            {
                slowMotion.OnSlowMotionActiveChanged += UpdateSlowMotionActive;
                slowMotion.OnCooldownChanged += UpdateSlowMotionCooldown;

                if (slowMotionPanel != null)
                    slowMotionPanel.SetActive(true);
            }
            else
            {
                // Hide slow motion UI if ability not available
                if (slowMotionPanel != null)
                    slowMotionPanel.SetActive(false);
            }
        }

        // Initialize status indicators
        if (poisonIndicator != null) poisonIndicator.SetActive(false);
        if (stunIndicator != null) stunIndicator.SetActive(false);
    }

    void Update()
    {
        // Update score from player controller
        if (playerController != null && scoreText != null)
        {
            scoreText.text = $"Score: {playerController.score}";
        }
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

    void UpdateSlowMotionActive(bool active)
    {
        if (slowMotionActiveIndicator != null)
        {
            slowMotionActiveIndicator.gameObject.SetActive(active);
        }

        if (slowMotionText != null)
        {
            slowMotionText.text = active ? "SLOW MOTION" : "Slow Motion (Q)";
        }
    }

    void UpdateSlowMotionCooldown(float remaining, float max)
    {
        if (slowMotionCooldownSlider != null)
        {
            slowMotionCooldownSlider.maxValue = max;
            slowMotionCooldownSlider.value = max - remaining;
        }

        if (slowMotionText != null && remaining > 0)
        {
            slowMotionText.text = $"Cooldown: {remaining:F1}s";
        }
        else if (slowMotionText != null)
        {
            slowMotionText.text = "Slow Motion (Q): Ready";
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

        if (slowMotion != null)
        {
            slowMotion.OnSlowMotionActiveChanged -= UpdateSlowMotionActive;
            slowMotion.OnCooldownChanged -= UpdateSlowMotionCooldown;
        }
    }
}
