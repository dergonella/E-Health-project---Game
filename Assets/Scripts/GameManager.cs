using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager for the E-Health chase levels (not the dino runner)
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    private int currentScore = 0;
    private bool gameOver = false;

    private UIManager uiManager;

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
        uiManager = FindObjectOfType<UIManager>();
        gameOver = false;
        currentScore = 0;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void UpdateScore(int score)
    {
        currentScore = score;

        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
        }

        // Check for win condition based on target score
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (gameOver) return;

        // Get target score from current level
        if (LevelManager.Instance != null)
        {
            LevelManager.LevelData currentLevel = LevelManager.Instance.GetCurrentLevelData();
            if (currentLevel != null && currentScore >= currentLevel.targetScore)
            {
                Debug.Log($"Level Complete! Reached target score of {currentLevel.targetScore}");

                // Notify TimedLevelManager if this is a timed level (Level 0.1)
                if (currentLevel.hasTimedChallenge)
                {
                    TimedLevelManager timedManager = Object.FindFirstObjectByType<TimedLevelManager>();
                    if (timedManager != null)
                    {
                        timedManager.OnLevelComplete();
                    }
                }

                GameOver(true); // Win!
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

    public void GameOver(bool won)
    {
        if (gameOver) return;

        gameOver = true;

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
}
