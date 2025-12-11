using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Basic UI References")]
    public TextMeshProUGUI scoreText;
    public GameObject winScreen;
    public GameObject loseScreen;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI loseScoreText;

    [Header("Health & Focus UI")]
    public Slider healthBar;
    public Slider focusBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI focusText;

    [Header("Ability UI")]
    public GameObject shieldUI;
    public TextMeshProUGUI shieldCooldownText;
    public Image shieldIcon;

    [Header("Status Effects")]
    public GameObject poisonIndicator;
    public GameObject stunIndicator;

    [Header("Inventory UI (New System)")]
    public TextMeshProUGUI medkitCountText;
    public TextMeshProUGUI shieldCountText;
    public Slider shieldActiveBar;
    public TextMeshProUGUI shieldStatusText;

    [Header("Slow Motion UI")]
    public TextMeshProUGUI slowMotionCountText;

    [Header("Back to Menu")]
    public Button backToMenuButton;

    private PlayerController player;
    private HealthSystem healthSystem;
    private AbilitySystem abilitySystem;
    private PlayerInventory playerInventory;
    private BulletSlowdown bulletSlowdown;

    void Start()
    {
        HideGameOverScreens();
        InitializeUI();
        SetupPlayer();
        SetupBackToMenuButton();
    }

    void InitializeUI()
    {
        // Make sure score text is visible
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
        }

        // Hide status indicators initially
        if (poisonIndicator != null)
            poisonIndicator.SetActive(false);
        if (stunIndicator != null)
            stunIndicator.SetActive(false);

        // Get level data to show/hide appropriate UI
        if (LevelManager.Instance != null)
        {
            var levelData = LevelManager.Instance.GetCurrentLevelData();
            if (levelData != null)
            {
                // Show/hide health/focus bars based on level
                bool hasHealth = levelData.hasHealthSystem;
                if (healthBar != null) healthBar.gameObject.SetActive(hasHealth);
                if (healthText != null) healthText.gameObject.SetActive(hasHealth);
                if (focusBar != null) focusBar.gameObject.SetActive(hasHealth);
                if (focusText != null) focusText.gameObject.SetActive(hasHealth);

                // Show/hide ability UI based on level
                if (shieldUI != null) shieldUI.SetActive(levelData.hasShield);
            }
        }
    }

    void SetupPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerController>();
            healthSystem = playerObj.GetComponent<HealthSystem>();
            abilitySystem = playerObj.GetComponent<AbilitySystem>();
            playerInventory = playerObj.GetComponent<PlayerInventory>();

            // Subscribe to health events
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateHealthUI;
                healthSystem.OnFocusChanged += UpdateFocusUI;
                healthSystem.OnPoisonStatusChanged += UpdatePoisonIndicator;
                healthSystem.OnStunStatusChanged += UpdateStunIndicator;
            }

            // Subscribe to ability events (legacy system)
            if (abilitySystem != null)
            {
                abilitySystem.OnShieldStatusChanged += UpdateShieldStatus;
                abilitySystem.OnShieldCooldownChanged += UpdateShieldCooldown;
            }

            // Subscribe to inventory events (new system)
            if (playerInventory != null)
            {
                playerInventory.OnMedkitCountChanged += UpdateMedkitUI;
                playerInventory.OnShieldCountChanged += UpdateShieldCountUI;
                playerInventory.OnShieldActiveChanged += UpdateShieldActiveUI;
                playerInventory.OnShieldTimerChanged += UpdateShieldTimerUI;

                // Initialize UI with current values
                UpdateMedkitUI(playerInventory.MedkitCount);
                UpdateShieldCountUI(playerInventory.ShieldCount);
                UpdateShieldActiveUI(false);
            }

            // Subscribe to slow motion events
            bulletSlowdown = playerObj.GetComponent<BulletSlowdown>();
            if (bulletSlowdown != null)
            {
                bulletSlowdown.OnCountChanged += UpdateSlowMotionUI;

                // Initialize UI with current value
                UpdateSlowMotionUI(bulletSlowdown.SlowMotionCount);
            }
        }
    }

    void SetupBackToMenuButton()
    {
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }
    }

    void OnBackToMenuClicked()
    {
        // Reset time scale in case slow motion is active
        Time.timeScale = 1f;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadMenu();
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
            // Make sure it's visible when updating
            scoreText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ScoreText is not assigned in UIManager!");
        }
    }

    public void ShowWinScreen(int finalScore)
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
        if (winScoreText != null)
        {
            winScoreText.text = $"Total Score: {finalScore}";
        }
    }

    public void ShowLoseScreen(int finalScore)
    {
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
        }
        if (loseScoreText != null)
        {
            loseScoreText.text = $"Total Score: {finalScore}";
        }
    }

    public void HideGameOverScreens()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
        if (loseScreen != null)
        {
            loseScreen.SetActive(false);
        }
    }

    // Health & Focus UI updates
    void UpdateHealthUI(float current, float max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }

    void UpdateFocusUI(float current, float max)
    {
        if (focusBar != null)
        {
            focusBar.maxValue = max;
            focusBar.value = current;
        }
        if (focusText != null)
        {
            focusText.text = $"Focus: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }

    // Status effect indicators
    void UpdatePoisonIndicator(bool isPoisoned)
    {
        if (poisonIndicator != null)
        {
            poisonIndicator.SetActive(isPoisoned);
        }
    }

    void UpdateStunIndicator(bool isStunned)
    {
        if (stunIndicator != null)
        {
            stunIndicator.SetActive(isStunned);
        }
    }

    // Ability UI updates
    void UpdateShieldStatus(bool isActive)
    {
        if (shieldIcon != null)
        {
            Color color = isActive ? Color.cyan : Color.white;
            shieldIcon.color = color;
        }
    }

    void UpdateShieldCooldown(float current, float max)
    {
        if (shieldCooldownText != null)
        {
            if (current >= max)
            {
                shieldCooldownText.text = "Shield (Q)\nREADY";
            }
            else
            {
                shieldCooldownText.text = $"Shield (Q)\n{Mathf.CeilToInt(max - current)}s";
            }
        }
    }

    // ===== NEW INVENTORY UI METHODS =====

    void UpdateMedkitUI(int count)
    {
        if (medkitCountText != null)
        {
            medkitCountText.text = $"Medkit (H): {count}";
        }
    }

    void UpdateShieldCountUI(int count)
    {
        if (shieldCountText != null)
        {
            shieldCountText.text = $"Shield (F): {count}";
        }
    }

    void UpdateShieldActiveUI(bool isActive)
    {
        if (shieldActiveBar != null)
        {
            shieldActiveBar.gameObject.SetActive(isActive);
        }

        if (shieldStatusText != null)
        {
            if (isActive)
            {
                shieldStatusText.text = "SHIELD ACTIVE";
                shieldStatusText.color = Color.cyan;
            }
            else
            {
                shieldStatusText.text = "Shield Ready";
                shieldStatusText.color = Color.white;
            }
        }
    }

    void UpdateShieldTimerUI(float remaining, float max)
    {
        if (shieldActiveBar != null)
        {
            shieldActiveBar.maxValue = max;
            shieldActiveBar.value = remaining;
        }

        if (shieldStatusText != null && remaining > 0)
        {
            shieldStatusText.text = $"SHIELD: {remaining:F1}s";
        }
    }

    // ===== SLOW MOTION UI =====

    void UpdateSlowMotionUI(int count)
    {
        if (slowMotionCountText != null)
        {
            slowMotionCountText.text = $"Slow Motion (O): {count}";
        }
    }
}
