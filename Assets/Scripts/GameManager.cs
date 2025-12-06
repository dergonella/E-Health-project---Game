using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager for the E-Health chase levels (not the dino runner)
/// LEVEL 0.1 and 0.2 SPECIAL BEHAVIOR:
/// - Game only ends when timer reaches 0 (not when reaching target score)
/// - If score >= target score when timer ends: WIN + convert points to money
/// - If score < target score when timer ends: LOSE
/// - If snake touches player anytime: LOSE immediately
/// - Level 0.2 adds: Growing snakes in maze environment
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level 0.1 Settings")]
    public float timeLeft = 30f;
    public int targetScore = 2000;

    [Header("Game State")]
    private int currentScore = 0;
    private bool gameOver = false;
    private bool targetScoreReached = false;

    private UIManager uiManager;
    private bool isLevel01 = false;
    private bool isLevel02 = false;

    void Awake()
    {
        // Singleton pattern - only one GameManager per scene
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        uiManager = Object.FindFirstObjectByType<UIManager>();
        gameOver = false;
        currentScore = 0;
        targetScoreReached = false;

        // Check if this is Level 0.1 or Level 0.2
        string loadedSceneName = SceneManager.GetActiveScene().name;
        isLevel01 = (loadedSceneName == "Level0.1" || loadedSceneName == "Level0_1_TimedChallenge");
        isLevel02 = (loadedSceneName == "Level0.2" || loadedSceneName == "Level0_2_GrowingSnakes");

        // Get target score from LevelManager if available
        if (LevelManager.Instance != null)
        {
            LevelManager.LevelData currentLevel = LevelManager.Instance.GetCurrentLevelData();
            if (currentLevel != null)
            {
                targetScore = currentLevel.targetScore;

                // For Level 0.1 and 0.2, use time limit from level data
                if ((isLevel01 || isLevel02) && currentLevel.hasTimedChallenge)
                {
                    timeLeft = currentLevel.timeLimitSeconds;
                }
            }
        }

        Debug.Log($"GameManager Start: isLevel01={isLevel01}, isLevel02={isLevel02}, targetScore={targetScore}, timeLeft={timeLeft}");
    }

    void Update()
    {
        // LEVEL 0.1 and 0.2: Timer-based gameplay
        if ((isLevel01 || isLevel02) && !gameOver)
        {
            // Timer counts down
            timeLeft -= Time.deltaTime;

            // Update timer UI if available
            UpdateTimerUI();

            // When timer reaches 0, end the game
            if (timeLeft <= 0f)
            {
                EndGame();
            }
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Add score when player collects shards
    /// </summary>
    public void AddScore(int amount)
    {
        if (gameOver) return;

        currentScore += amount;

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore);
        }

        // For Level 0.1 and 0.2: Check if target score reached (but don't end game!)
        if (isLevel01 || isLevel02)
        {
            CheckTargetScoreReached();
        }
        else
        {
            // For normal levels: Check win condition
            CheckWinCondition();
        }
    }

    /// <summary>
    /// Update score (backward compatibility)
    /// </summary>
    public void UpdateScore(int score)
    {
        if (gameOver) return;

        currentScore = score;

        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
        }

        // For Level 0.1 and 0.2: Check if target score reached (but don't end game!)
        if (isLevel01 || isLevel02)
        {
            CheckTargetScoreReached();
        }
        else
        {
            // For normal levels: Check win condition
            CheckWinCondition();
        }
    }

    /// <summary>
    /// LEVEL 0.1: Check if target score reached (but DON'T end game - just turn timer green)
    /// </summary>
    private void CheckTargetScoreReached()
    {
        if (gameOver) return;
        if (targetScoreReached) return; // Already notified

        if (currentScore >= targetScore)
        {
            targetScoreReached = true;
            Debug.Log($"Level 0.1: Target score {targetScore} reached! Timer turns GREEN. Keep collecting for bonus money!");

            // Notify TimedLevelManager to turn timer green (if it exists)
            TimedLevelManager timedManager = Object.FindFirstObjectByType<TimedLevelManager>();
            if (timedManager != null)
            {
                timedManager.OnLevelComplete();
            }

            // DON'T call GameOver() - let timer run out!
        }
    }

    /// <summary>
    /// Check win condition for normal levels (NOT Level 0.1)
    /// </summary>
    private void CheckWinCondition()
    {
        if (gameOver) return;

        // Get target score from current level
        if (LevelManager.Instance != null)
        {
            LevelManager.LevelData currentLevel = LevelManager.Instance.GetCurrentLevelData();
            if (currentLevel != null && currentScore >= currentLevel.targetScore)
            {
                // Check if this is Level 0.2 (should also continue after target score)
                string loadedSceneName = SceneManager.GetActiveScene().name;
                bool isLevel02 = currentLevel.sceneName == "Level0_2_GrowingSnakes" || loadedSceneName == "Level0.2";

                if (isLevel02)
                {
                    // For Level 0.2, don't end game at target score
                    if (!targetScoreReached)
                    {
                        targetScoreReached = true;
                        Debug.Log($"Target score reached! Keep collecting for bonus points!");
                    }
                }
                else
                {
                    // For normal levels (Level 0, etc.), end immediately at target score
                    Debug.Log($"Level Complete! Reached target score of {currentLevel.targetScore}");
                    GameOver(true); // Win!
                }
            }
        }
        else
        {
            // Fallback: If LevelManager doesn't exist, use default target score
            Debug.LogWarning("LevelManager.Instance is null! Using fallback win condition.");
            if (currentScore >= 2000)
            {
                Debug.Log($"Level Complete! Reached fallback target score of 2000");
                GameOver(true); // Win!
            }
        }
    }

    /// <summary>
    /// LEVEL 0.1 and 0.2: End game when timer reaches 0
    /// </summary>
    void EndGame()
    {
        if (gameOver) return;

        gameOver = true;

        Debug.Log($"Level 0.1: Time's up! Final score: {currentScore}/{targetScore}");

        if (currentScore >= targetScore)
        {
            // WIN: Score >= 2000 when time ran out
            Debug.Log("LEVEL 0.1 COMPLETE! Converting points to money...");

            // Calculate money earned BEFORE converting
            int moneyEarned = CalculateMoneyEarned();

            // Convert points to money
            ConvertPointsToMoney();

            // Show Level 0.1 special win screen with money earned
            ShowLevel01WinScreen(moneyEarned);
        }
        else
        {
            // LOSE: Score < 2000 when time ran out
            Debug.Log("TIME UP → YOU LOST (Score below 2000)");

            // Show lose screen
            GameOver(false);
        }
    }

    /// <summary>
    /// Calculate how much money will be earned from points (Level 0.1)
    /// </summary>
    private int CalculateMoneyEarned()
    {
        // Use PointsToMoneyConverter to calculate
        PointsToMoneyConverter converter = Object.FindFirstObjectByType<PointsToMoneyConverter>();

        if (converter != null)
        {
            return converter.CalculatePotentialMoney(currentScore, targetScore);
        }
        else
        {
            // Fallback calculation: 100 points = 1 coin
            return Mathf.FloorToInt((float)currentScore / 100);
        }
    }

    /// <summary>
    /// Show Level 0.1 win screen with money display
    /// </summary>
    private void ShowLevel01WinScreen(int moneyEarned)
    {
        if (uiManager != null)
        {
            uiManager.ShowLevel01WinScreen(currentScore, moneyEarned);
        }
        else
        {
            Debug.LogWarning("UIManager not found! Cannot show Level 0.1 win screen.");
        }
    }

    /// <summary>
    /// Update timer UI display
    /// </summary>
    private void UpdateTimerUI()
    {
        if (uiManager != null && uiManager.timerText != null)
        {
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            int milliseconds = Mathf.FloorToInt((timeLeft * 100f) % 100f);

            uiManager.timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";

            // Change color based on state
            if (targetScoreReached)
            {
                // Target reached - timer is GREEN
                uiManager.timerText.color = Color.green;
            }
            else if (timeLeft <= 5f)
            {
                // Critical - RED
                uiManager.timerText.color = Color.red;
            }
            else if (timeLeft <= 10f)
            {
                // Warning - YELLOW
                uiManager.timerText.color = Color.yellow;
            }
            else
            {
                // Normal - WHITE
                uiManager.timerText.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Convert points to money (Level 0.1 only)
    /// </summary>
    private void ConvertPointsToMoney()
    {
        // Check if current level has points-to-money conversion enabled
        if (LevelManager.Instance != null)
        {
            LevelManager.LevelData currentLevel = LevelManager.Instance.GetCurrentLevelData();
            if (currentLevel != null && currentLevel.convertExcessPointsToMoney)
            {
                // Get PointsToMoneyConverter component
                PointsToMoneyConverter converter = Object.FindFirstObjectByType<PointsToMoneyConverter>();

                if (converter != null)
                {
                    // Convert and award money
                    converter.ConvertAndAwardMoney(currentScore, targetScore, true);
                }
                else
                {
                    Debug.LogWarning("GameManager: PointsToMoneyConverter not found! Cannot convert points to money.");
                }
            }
        }
    }

    public void GameOver(bool won)
    {
        if (gameOver) return;

        gameOver = true;

        // Debug: Print stack trace to see WHO called GameOver
        Debug.Log($"GameOver called! Won = {won}, Score = {currentScore}");
        Debug.Log($"Stack trace: {System.Environment.StackTrace}");

        if (uiManager != null)
        {
            if (won)
            {
                uiManager.ShowWinScreen(currentScore);
            }
            else
            {
                uiManager.ShowLoseScreen(currentScore);
            }
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        // Get current level index and load next one
        if (LevelManager.Instance != null)
        {
            var currentLevelData = LevelManager.Instance.GetCurrentLevelData();
            if (currentLevelData != null)
            {
                // Find current level index
                for (int i = 0; i < LevelManager.Instance.levels.Length; i++)
                {
                    if (LevelManager.Instance.levels[i] == currentLevelData)
                    {
                        // Load next level if it exists
                        if (i + 1 < LevelManager.Instance.levels.Length)
                        {
                            LevelManager.Instance.LoadLevel(i + 1);
                            return;
                        }
                        else
                        {
                            // No more levels, go to menu
                            LevelManager.Instance.LoadMenu();
                            return;
                        }
                    }
                }
            }
        }

        // Fallback: just reload menu
        LevelManager.Instance?.LoadMenu();
    }

    public int GetScore()
    {
        return currentScore;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }
}
