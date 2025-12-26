using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class DinoGameManager : MonoBehaviour
{
    public static DinoGameManager Instance { get; private set; }

    public float initialGameSpeed = 6.25f;    // +25% (was 5)
    public float gameSpeedIncrease = 0.125f;  // +25% (was 0.1)
    public float gameSpeed { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button retryButton;

    [Header("Menu Button")]
    [SerializeField] private Button menuButton;
    [Tooltip("Scene name to load when menu button is clicked")]
    public string menuSceneName = "MainMenu";

    private DinoPlayer player;
    private DinoSpawner spawner;

    private float score;
    public float Score => score;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        player = FindObjectOfType<DinoPlayer>();
        spawner = FindObjectOfType<DinoSpawner>();

        // Setup menu button
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(GoToMenu);
        }

        NewGame();
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void NewGame()
    {
        DinoObstacle[] obstacles = FindObjectsOfType<DinoObstacle>();

        foreach (var obstacle in obstacles) {
            Destroy(obstacle.gameObject);
        }

        score = 0f;
        gameSpeed = initialGameSpeed;
        enabled = true;

        player.gameObject.SetActive(true);
        spawner.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        // Hide menu button during gameplay
        if (menuButton != null)
            menuButton.gameObject.SetActive(false);

        UpdateHiscore();
    }

    public void GameOver()
    {
        gameSpeed = 0f;
        enabled = false;

        player.gameObject.SetActive(false);
        spawner.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);

        // Show menu button on game over
        if (menuButton != null)
            menuButton.gameObject.SetActive(true);

        UpdateHiscore();

        // Award money based on score (Dino game rewards survival time)
        AwardMoney();
    }

    /// <summary>
    /// Award money based on Dino game score.
    /// Score is based on survival time, so we convert directly.
    /// </summary>
    private void AwardMoney()
    {
        // Dino game: score = survival time * speed
        // Convert: every 10 score = 1 money (10:1 ratio)
        int earnedMoney = Mathf.FloorToInt(score / 10f);

        if (earnedMoney > 0)
        {
            MarketData.AddMoney(earnedMoney);
            Debug.Log($"[DinoGameManager] Awarded {earnedMoney} money for score {Mathf.FloorToInt(score)}");
            Debug.Log($"[DinoGameManager] Player total money: {MarketData.Money}");
        }
    }

    private void Update()
    {
        gameSpeed += gameSpeedIncrease * Time.deltaTime;
        score += gameSpeed * Time.deltaTime;
        scoreText.text = Mathf.FloorToInt(score).ToString("D5");
    }

    private void UpdateHiscore()
    {
        float hiscore = PlayerPrefs.GetFloat("hiscore", 0);

        if (score > hiscore)
        {
            hiscore = score;
            PlayerPrefs.SetFloat("hiscore", hiscore);
        }

        hiscoreText.text = Mathf.FloorToInt(hiscore).ToString("D5");
    }

}
