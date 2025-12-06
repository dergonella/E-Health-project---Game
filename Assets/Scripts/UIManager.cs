using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Basic UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText; // For Level 0.1 timer display
    public GameObject winScreen;
    public GameObject loseScreen;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI loseScoreText;

    [Header("Level 0.1 Money Display")]
    public TextMeshProUGUI moneyEarnedText; // Shows "You earned X coins!" for Level 0.1

    [Header("Health & Focus UI")]
    public Slider healthBar;
    public Slider focusBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI focusText;

    [Header("Ability UI")]
    public GameObject shieldUI;
    public TextMeshProUGUI shieldCooldownText;
    public Image shieldIcon;
    public GameObject slowMotionUI;
    public TextMeshProUGUI slowMotionCooldownText;
    public Image slowMotionIcon;

    [Header("Status Effects")]
    public GameObject poisonIndicator;
    public GameObject stunIndicator;

    [Header("Back to Menu")]
    public Button backToMenuButton;

    private PlayerController player;
    private HealthSystem healthSystem;
    private AbilitySystem abilitySystem;

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
                if (slowMotionUI != null) slowMotionUI.SetActive(levelData.hasSlowMotion);
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

            // Subscribe to health events
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateHealthUI;
                healthSystem.OnFocusChanged += UpdateFocusUI;
                healthSystem.OnPoisonStatusChanged += UpdatePoisonIndicator;
                healthSystem.OnStunStatusChanged += UpdateStunIndicator;
            }

            // Subscribe to ability events
            if (abilitySystem != null)
            {
                abilitySystem.OnShieldStatusChanged += UpdateShieldStatus;
                abilitySystem.OnShieldCooldownChanged += UpdateShieldCooldown;
                abilitySystem.OnSlowMotionStatusChanged += UpdateSlowMotionStatus;
                abilitySystem.OnSlowMotionCooldownChanged += UpdateSlowMotionCooldown;
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

    /// <summary>
    /// Show Level 0.1 win screen with money earned display
    /// </summary>
    public void ShowLevel01WinScreen(int finalScore, int moneyEarned)
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        if (winScoreText != null)
        {
            winScoreText.text = $"LEVEL FINISHED!\n\nFinal Score: {finalScore}";
        }

        if (moneyEarnedText != null)
        {
            moneyEarnedText.text = $"You earned {moneyEarned} coins!";
            moneyEarnedText.gameObject.SetActive(true);
        }

        Debug.Log($"Level 0.1 Complete! Score: {finalScore}, Money Earned: {moneyEarned} coins");
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

        // Hide money text on lose screen
        if (moneyEarnedText != null)
        {
            moneyEarnedText.gameObject.SetActive(false);
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

    void UpdateSlowMotionStatus(bool isActive)
    {
        if (slowMotionIcon != null)
        {
            Color color = isActive ? Color.yellow : Color.white;
            slowMotionIcon.color = color;
        }
    }

    void UpdateSlowMotionCooldown(float current, float max)
    {
        if (slowMotionCooldownText != null)
        {
            if (current >= max)
            {
                slowMotionCooldownText.text = "Slow (E)\nREADY";
            }
            else
            {
                slowMotionCooldownText.text = $"Slow (E)\n{Mathf.CeilToInt(max - current)}s";
            }
        }
    }
}
