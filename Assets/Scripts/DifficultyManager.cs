using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Difficulty Progression")]
    [Tooltip("Time in seconds before difficulty increases")]
    public float difficultyIncreaseInterval = 30f;
    public float maxDifficultyLevel = 5f;
    public bool enableDifficultyScaling = true;

    [Header("Speed Scaling")]
    public float speedIncreasePerLevel = 0.3f;
    public float maxSpeedMultiplier = 1.8f;

    [Header("AI Behavior Scaling")]
    public float predictionAccuracyIncrease = 0.15f;
    public float alertRangeIncrease = 0.4f;

    [Header("Current State")]
    [SerializeField] private float currentDifficultyLevel = 1f;
    [SerializeField] private float gameTime = 0f;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float predictionMultiplier = 1f;
    [SerializeField] private float alertRangeMultiplier = 1f;

    private CobraAI[] allCobras;
    private bool gameActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeDifficulty();
    }

    void Update()
    {
        if (!gameActive || !enableDifficultyScaling) return;

        gameTime += Time.deltaTime;

        // Check if it's time to increase difficulty
        float targetLevel = 1f + (gameTime / difficultyIncreaseInterval);
        targetLevel = Mathf.Min(targetLevel, maxDifficultyLevel);

        if (targetLevel > currentDifficultyLevel)
        {
            currentDifficultyLevel = targetLevel;
            UpdateDifficulty();
        }
    }

    public void InitializeDifficulty()
    {
        currentDifficultyLevel = 1f;
        gameTime = 0f;
        speedMultiplier = 1f;
        predictionMultiplier = 1f;
        alertRangeMultiplier = 1f;
        gameActive = true;

        // Find all cobras
        allCobras = FindObjectsOfType<CobraAI>();
    }

    void UpdateDifficulty()
    {
        // Calculate new multipliers based on difficulty level
        float levelProgress = (currentDifficultyLevel - 1f) / (maxDifficultyLevel - 1f);

        speedMultiplier = 1f + (speedIncreasePerLevel * (currentDifficultyLevel - 1f));
        speedMultiplier = Mathf.Min(speedMultiplier, maxSpeedMultiplier);

        predictionMultiplier = 1f + (predictionAccuracyIncrease * levelProgress);
        alertRangeMultiplier = 1f + (alertRangeIncrease * levelProgress);

        // Apply to all cobras
        ApplyDifficultyToCobras();

        Debug.Log($"Difficulty increased to level {currentDifficultyLevel:F1}! Speed: x{speedMultiplier:F2}");
    }

    void ApplyDifficultyToCobras()
    {
        if (allCobras == null || allCobras.Length == 0)
        {
            allCobras = FindObjectsOfType<CobraAI>();
        }

        foreach (CobraAI cobra in allCobras)
        {
            if (cobra == null) continue;

            // Apply speed multiplier
            cobra.ApplyDifficultyScaling(speedMultiplier, predictionMultiplier, alertRangeMultiplier);
        }
    }

    public void StopDifficulty()
    {
        gameActive = false;
    }

    public void ResetDifficulty()
    {
        InitializeDifficulty();
    }

    // Getters for UI or other systems
    public float GetCurrentDifficultyLevel() => currentDifficultyLevel;
    public float GetSpeedMultiplier() => speedMultiplier;
    public float GetGameTime() => gameTime;
}
