using UnityEngine;
using TMPro;

// Timed Level Manager for Level 0.1
/// <summary>
/// Timed Level Manager - Handles countdown timer for Level 0.1
/// Player must reach target score (2000) within time limit (30 seconds)
/// Displays countdown timer on screen
/// Converts excess points to money on completion
/// </summary>
public class TimedLevelManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 30f; // 30 seconds for Level 0.1
    [SerializeField] private bool enableTimer = true;

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
        if (!timerRunning || levelCompleted) return;

        // Check if game is over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            StopTimer();
            return;
        }

        // Countdown
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
        if (currentTime <= criticalThreshold)
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
            // Check if game is already over
            if (GameManager.Instance.IsGameOver())
            {
                Debug.Log("TimedLevelManager: Game already over, skipping time up logic");
                return;
            }

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
                // Player reached target and time expired - WIN!
                // Convert points to money and show win screen
                Debug.Log($"Time's up! Score: {score}/{targetScore} - YOU WIN!");

                if (timerText != null)
                {
                    timerText.text = "TIME'S UP!";
                    timerText.color = Color.green;
                }

                // Mark as completed and convert points
                levelCompleted = true;
                ConvertPointsToMoney();

                GameManager.Instance.GameOver(true); // Win!
            }
            else
            {
                // Player failed to reach target in time - LOSE!
                Debug.Log($"Time's up! Score: {score}/{targetScore} - Game Over (didn't reach 2000)");

                if (timerText != null)
                {
                    timerText.text = "TIME'S UP!";
                    timerText.color = criticalColor;
                }

                GameManager.Instance.GameOver(false); // Loss
            }
        }
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
