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

        // For timed levels (Level 0.1): DON'T check win condition here!
        // Game ONLY ends when:
        // 1. Timer expires (TimedLevelManager handles win/loss)
        // 2. Player dies (snake collision)

        // For non-timed levels: Check win condition
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (gameOver) return;

        // Get current level data
        if (LevelManager.Instance != null)
        {
            LevelManager.LevelData currentLevel = LevelManager.Instance.GetCurrentLevelData();

            // IMPORTANT: For timed challenges (Level 0.1), NEVER end game based on score!
            // Timer handles everything
            if (currentLevel != null && currentLevel.hasTimedChallenge)
            {
                return; // Do nothing - let timer handle it
            }

            // For non-timed levels, end game when target score is reached
            if (currentLevel != null && currentScore >= currentLevel.targetScore)
            {
                Debug.Log($"Level Complete! Reached target score of {currentLevel.targetScore}");
                GameOver(true); // Win!
            }
        }
        else
        {
            // Fallback: If LevelManager doesn't exist
            // For Level 0.1 (timed challenge), NEVER end game based on score!
            // If TimedLevelManager exists in scene, it's a timed level - do nothing
            TimedLevelManager timedManager = Object.FindFirstObjectByType<TimedLevelManager>();
            if (timedManager != null)
            {
                // This is a timed level - don't end game based on score
                Debug.Log("Timed level detected (fallback) - timer will handle game end");
                return;
            }

            // For non-timed levels without LevelManager, use default target score
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
