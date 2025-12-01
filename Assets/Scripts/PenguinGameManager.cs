using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager for the penguin endless runner
/// Handles scoring, game state, and UI
/// </summary>
public class PenguinGameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("Game Settings")]
    [SerializeField] private float scoreRate = 10f; // Points per second

    private ObstacleSpawner obstacleSpawner;
    private float score = 0f;
    private float highScore = 0f;
    private bool isGameActive = false;
    private bool isGameOver = false;

    private const string HIGH_SCORE_KEY = "PenguinHighScore";

    void Awake()
    {
        obstacleSpawner = FindObjectOfType<ObstacleSpawner>();

        // Load high score
        highScore = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
    }

    void Start()
    {
        // Setup UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Setup buttons
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(ReturnToMenu);
        }

        // Start game
        StartGame();
    }

    void Update()
    {
        if (isGameActive && !isGameOver)
        {
            // Update score
            score += scoreRate * Time.deltaTime;
            UpdateScoreUI();
        }
    }

    void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        score = 0f;

        UpdateScoreUI();
        UpdateHighScoreUI();

        // Start spawning obstacles
        if (obstacleSpawner != null)
        {
            obstacleSpawner.StartSpawning();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        isGameActive = false;

        // Stop spawning
        if (obstacleSpawner != null)
        {
            obstacleSpawner.StopSpawning();
        }

        // Check for new high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetFloat(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        // Show game over screen
        ShowGameOverScreen();
    }

    void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Score: {Mathf.FloorToInt(score)}";
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = $"High Score: {Mathf.FloorToInt(highScore)}";
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Mathf.FloorToInt(score)}";
        }
    }

    void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {Mathf.FloorToInt(highScore)}";
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ReturnToMenu()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadMenu();
        }
        else
        {
            SceneManager.LoadScene("MenuScene");
        }
    }

    public float GetScore()
    {
        return score;
    }

    public bool IsGameActive()
    {
        return isGameActive && !isGameOver;
    }
}
