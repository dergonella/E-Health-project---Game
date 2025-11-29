using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RunnerGameManager : MonoBehaviour
{
    [Header("Game State")]
    public bool isGameOver = false;
    public int score = 0;
    public float currentSpeed = 5f;

    [Header("Speed Settings")]
    public float startSpeed = 5f;
    public float maxSpeed = 15f;
    public float speedIncreaseRate = 0.1f; // Speed increase per second
    public float speedIncreasePerScore = 0.05f; // Additional speed per 100 points

    [Header("Score Settings")]
    public float scorePerSecond = 10f; // Base score earned per second

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI finalScoreText;
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button menuButton;

    private float gameTime = 0f;
    private float scoreTimer = 0f;

    void Start()
    {
        currentSpeed = startSpeed;
        isGameOver = false;
        score = 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);

        UpdateScoreUI();
    }

    void Update()
    {
        if (isGameOver)
            return;

        gameTime += Time.deltaTime;
        scoreTimer += Time.deltaTime;

        // Increase score over time
        if (scoreTimer >= 0.1f) // Update score every 0.1 seconds
        {
            int pointsToAdd = Mathf.FloorToInt(scorePerSecond * scoreTimer);
            score += pointsToAdd;
            scoreTimer = 0f;
            UpdateScoreUI();
        }

        // Gradually increase speed
        currentSpeed += speedIncreaseRate * Time.deltaTime;

        // Additional speed boost based on score
        float speedBonus = (score / 100f) * speedIncreasePerScore;
        currentSpeed = Mathf.Min(startSpeed + speedBonus, maxSpeed);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        // Stop all obstacles
        ObstacleSpawner spawner = FindObjectOfType<ObstacleSpawner>();
        if (spawner != null)
            spawner.StopSpawning();

        // Show game over UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = "Game Over!";

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + score.ToString();

        Time.timeScale = 0f; // Pause game
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }

    public int GetScore()
    {
        return score;
    }

    public float GetSpeed()
    {
        return currentSpeed;
    }
}
