using UnityEngine;
using TMPro;

// Timed Level Manager for Level 0.1
/// <summary>
/// Timed Level Manager - DEPRECATED - Timer logic now handled by GameManager
/// This script is kept for backward compatibility
/// You can disable this component - GameManager handles all Level 0.1 timer logic
/// </summary>
public class TimedLevelManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 30f; // 30 seconds for Level 0.1
    [SerializeField] private bool enableTimer = false; // DISABLED - GameManager handles timer now

    [Header("UI Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float warningThreshold = 10f; // Yellow at 10 seconds
    [SerializeField] private float criticalThreshold = 5f; // Red at 5 seconds

    private float currentTime;
    private bool timerRunning = false;
    private bool levelCompleted = false;

    void Start()
    {
        if (enableTimer)
        {
            StartTimer();
        }
    }

    void Update()
    {
        // REMOVED levelCompleted check - we want timer to keep running even after reaching target score!
        if (!timerRunning) return;

        // Check if game is over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            StopTimer();
            return;
        }

        // Countdown - keep counting down even if levelCompleted is true!
        currentTime -= Time.deltaTime;

        // Update UI
        UpdateTimerDisplay();

        // Check for time up
        if (currentTime <= 0f)
        {
            TimeUp();
        }
    }

    private void StartTimer()
    {
        currentTime = timeLimit;
        timerRunning = true;
        levelCompleted = false;

        Debug.Log($"TimedLevelManager: Timer started - {timeLimit} seconds");

        if (timerText == null)
        {
            // Try to find timer text if not assigned
            GameObject timerObj = GameObject.Find("TimerText");
            if (timerObj != null)
            {
                timerText = timerObj.GetComponent<TextMeshProUGUI>();
            }

            if (timerText == null)
            {
                Debug.LogWarning("TimedLevelManager: Timer Text not assigned and couldn't be found!");
            }
        }
    }

    private void StopTimer()
    {
        timerRunning = false;
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        // Format time as MM:SS
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        int milliseconds = Mathf.FloorToInt((currentTime * 100f) % 100f);

        timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";

        // Change color based on remaining time
        // BUT if levelCompleted is true (reached 2000 points), keep timer GREEN!
        if (levelCompleted)
        {
            // Target score reached - timer stays green even as it counts down
            timerText.color = Color.green;
        }
        else if (currentTime <= criticalThreshold)
        {
            timerText.color = criticalColor;
        }
        else if (currentTime <= warningThreshold)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    private void TimeUp()
    {
        StopTimer();

        Debug.Log("TimedLevelManager: Time's up!");

        // Check if player reached target score
        if (GameManager.Instance != null)
        {
            int score = GameManager.Instance.GetScore();
            int targetScore = 2000;

            if (LevelManager.Instance != null)
            {
                var levelData = LevelManager.Instance.GetCurrentLevelData();
                if (levelData != null)
                {
                    targetScore = levelData.targetScore;
                }
            }

            if (score >= targetScore)
            {
                // Player reached target and time ran out - WIN!
                // Convert all excess points to money!
                Debug.Log($"Time's up! Final score: {score}/{targetScore} - YOU WIN!");

                if (timerText != null)
                {
                    timerText.text = "TIME'S UP!";
                    timerText.color = Color.green; // Green because they won
                }

                // Convert points to money BEFORE ending game
                ConvertPointsToMoney();

                // Now trigger win
                GameManager.Instance.GameOver(true); // Win!
            }
            else
            {
                // Player failed to reach target in time - LOSE!
                Debug.Log($"Time's up! Score: {score}/{targetScore} - Game Over");

                if (timerText != null)
                {
                    timerText.text = "TIME'S UP!";
                    timerText.color = criticalColor; // Red because they lost
                }

                GameManager.Instance.GameOver(false); // Loss
            }
        }
    }

    /// <summary>
    /// Called when target score is reached (player reached 2000 points)
    /// But DON'T stop the game - let player keep collecting until timer expires!
    /// </summary>
    public void OnLevelComplete()
    {
        if (levelCompleted) return;

        levelCompleted = true;
        // DON'T set levelCompleted flag yet - that would stop the timer in Update()!
        // We want timer to KEEP RUNNING so player can collect more points!

        Debug.Log($"TimedLevelManager: Target score reached! Timer is GREEN but still counting down!");

        // Change timer color to green to show target was reached
        if (timerText != null)
        {
            timerText.color = Color.green;
        }

        // DON'T convert points yet - wait for timer to expire
        // DON'T call GameOver() yet - let timer run out first!
    }

    /// <summary>
    /// Convert excess points above target score to money (for Level 0.1).
    /// </summary>
    private void ConvertPointsToMoney()
    {
        // Check if current level has points-to-money conversion enabled
        if (LevelManager.Instance != null && GameManager.Instance != null)
        {
            LevelManager.LevelData currentLevel = LevelManager.Instance.GetCurrentLevelData();
            if (currentLevel != null && currentLevel.convertExcessPointsToMoney)
            {
                // Get PointsToMoneyConverter component (should be on same GameObject or in scene)
                PointsToMoneyConverter converter = Object.FindFirstObjectByType<PointsToMoneyConverter>();

                if (converter != null)
                {
                    int finalScore = GameManager.Instance.GetScore();
                    int targetScore = currentLevel.targetScore;

                    // Convert and award money
                    converter.ConvertAndAwardMoney(finalScore, targetScore, true);
                }
                else
                {
                    Debug.LogWarning("TimedLevelManager: PointsToMoneyConverter not found in scene! Cannot convert points to money.");
                }
            }
        }
    }

    /// <summary>
    /// Get remaining time in seconds
    /// </summary>
    public float GetRemainingTime()
    {
        return currentTime;
    }

    /// <summary>
    /// Check if timer is still running
    /// </summary>
    public bool IsTimerRunning()
    {
        return timerRunning;
    }
}
