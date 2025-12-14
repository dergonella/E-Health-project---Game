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

                GameManager.Instance.GameOver(true, true); // Win! (money already awarded)
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
    /// Convert points to money when player wins a timed level.
    /// Always awards money - no longer requires convertExcessPointsToMoney flag.
    /// </summary>
    private void ConvertPointsToMoney()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[TimedLevelManager] GameManager not found! Cannot award money.");
            return;
        }

        int finalScore = GameManager.Instance.GetScore();
        int targetScore = 2000; // Default

        // Try to get target score from LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.LevelData currentLevel = LevelManager.Instance.GetCurrentLevelData();
            if (currentLevel != null)
            {
                targetScore = currentLevel.targetScore;
            }
        }

        // Try to use PointsToMoneyConverter if available
        PointsToMoneyConverter converter = Object.FindFirstObjectByType<PointsToMoneyConverter>();
        if (converter != null)
        {
            converter.ConvertAndAwardMoney(finalScore, targetScore, true);
            Debug.Log($"[TimedLevelManager] Money awarded via PointsToMoneyConverter");
        }
        else
        {
            // Fallback: Calculate and add directly to MarketData
            // Using 10:1 ratio: targetScore/10 = base money, excess/10 = bonus
            int baseMoney = Mathf.FloorToInt((float)targetScore / 10f);
            int excessPoints = Mathf.Max(0, finalScore - targetScore);
            int bonusMoney = Mathf.FloorToInt((float)excessPoints / 10f);
            int totalMoney = baseMoney + bonusMoney;

            MarketData.AddMoney(totalMoney);
            Debug.Log($"[TimedLevelManager] Money awarded directly: {totalMoney} (base: {baseMoney}, bonus: {bonusMoney})");
            Debug.Log($"[TimedLevelManager] Player total money: {MarketData.Money}");
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
