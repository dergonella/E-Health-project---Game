using UnityEngine;

/// <summary>
/// Main Level Setup Script for all persona levels.
/// Attach this to an empty GameObject in each level scene.
/// Configures player abilities, snakes, and win conditions based on LevelConfig.
/// </summary>
public class MainLevelSetup : MonoBehaviour
{
    [Header("Level Configuration")]
    [Tooltip("Assign a LevelConfig ScriptableObject or leave null to auto-detect from level number")]
    public LevelConfig levelConfig;

    [Header("Auto-Detection (if no LevelConfig assigned)")]
    [Tooltip("Level number (1, 2, or 3) - used if LevelConfig is null")]
    public int levelNumber = 1;

    [Header("Snake Prefabs")]
    [Tooltip("Fire snake prefab (shoots fire projectiles)")]
    public GameObject fireSnakePrefab;

    [Tooltip("Poison snake prefab (shoots poison projectiles)")]
    public GameObject poisonSnakePrefab;

    [Header("Spawn Points")]
    [Tooltip("Spawn points for snakes - assign transforms in scene")]
    public Transform[] snakeSpawnPoints;

    [Header("References (Auto-found if not set)")]
    public GameObject player;

    // Components found on player
    private PlayerController playerController;
    private HealthSystem healthSystem;
    private PlayerInventory inventory;
    private SlowMotionAbility slowMotion;

    void Start()
    {
        Debug.Log($"=== MAIN LEVEL SETUP: {GameState.SelectedPersona} Level {GameState.CurrentLevel} ===");

        // Get or create level config
        if (levelConfig == null)
        {
            levelConfig = CreateDefaultConfig();
        }

        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player == null)
        {
            Debug.LogError("No Player found in scene!");
            return;
        }

        // Get player components
        playerController = player.GetComponent<PlayerController>();
        healthSystem = player.GetComponent<HealthSystem>();
        inventory = player.GetComponent<PlayerInventory>();
        slowMotion = player.GetComponent<SlowMotionAbility>();

        // Setup everything
        SetupPlayer();
        SetupSnakes();
        SetupWinCondition();

        Debug.Log($"=== LEVEL SETUP COMPLETE ===");
    }

    LevelConfig CreateDefaultConfig()
    {
        // Use GameState level if available, otherwise use inspector value
        int level = GameState.CurrentLevel > 0 ? GameState.CurrentLevel : levelNumber;

        Debug.Log($"Creating default config for Level {level}");

        switch (level)
        {
            case 1:
                return LevelConfig.CreateLevel1Default();
            case 2:
                return LevelConfig.CreateLevel2Default();
            case 3:
                return LevelConfig.CreateLevel3Default();
            default:
                return LevelConfig.CreateLevel1Default();
        }
    }

    void SetupPlayer()
    {
        Debug.Log("Setting up player...");

        // Ensure required components exist
        if (healthSystem == null)
        {
            healthSystem = player.AddComponent<HealthSystem>();
            Debug.Log("  + Added HealthSystem");
        }

        // Reset health to full
        healthSystem.ResetHealth();

        // Setup inventory
        if (levelConfig.enableMedkit || levelConfig.enableShield)
        {
            if (inventory == null)
            {
                inventory = player.AddComponent<PlayerInventory>();
                Debug.Log("  + Added PlayerInventory");
            }

            inventory.startingMedkits = levelConfig.startingMedkits;
            inventory.startingShields = levelConfig.startingShields;
            inventory.ResetInventory();

            Debug.Log($"  Inventory: {inventory.MedkitCount} medkits, {inventory.ShieldCount} shields");
        }
        else if (inventory != null)
        {
            inventory.enabled = false;
        }

        // Setup slow motion (Level 2+)
        if (levelConfig.enableSlowMotion)
        {
            if (slowMotion == null)
            {
                slowMotion = player.AddComponent<SlowMotionAbility>();
                Debug.Log("  + Added SlowMotionAbility");
            }
            slowMotion.enabled = true;
            Debug.Log("  Slow Motion: ENABLED (Q key)");
        }
        else if (slowMotion != null)
        {
            slowMotion.enabled = false;
            Debug.Log("  Slow Motion: DISABLED");
        }

        // Enable wall collision for maze
        if (playerController != null)
        {
            playerController.useWallCollision = true;
        }

        Debug.Log($"  Player HP: {healthSystem.currentHealth}/{healthSystem.maxHealth}");
    }

    void SetupSnakes()
    {
        Debug.Log("Setting up snakes...");

        // Find existing snakes in scene
        CobraAI[] existingSnakes = FindObjectsByType<CobraAI>(FindObjectsSortMode.None);

        int fireIndex = 0;
        int poisonIndex = 0;

        // Configure existing snakes
        foreach (CobraAI snake in existingSnakes)
        {
            // Determine snake type based on config
            bool isPoison = false;

            if (fireIndex < levelConfig.fireSnakeCount)
            {
                isPoison = false;
                fireIndex++;
            }
            else if (poisonIndex < levelConfig.poisonSnakeCount)
            {
                isPoison = true;
                poisonIndex++;
            }
            else
            {
                // Extra snakes - default to fire
                isPoison = false;
            }

            ConfigureSnake(snake, isPoison);
        }

        // Spawn additional snakes if needed
        int totalNeeded = levelConfig.TotalSnakes;
        int totalExisting = existingSnakes.Length;

        if (totalExisting < totalNeeded && snakeSpawnPoints != null && snakeSpawnPoints.Length > 0)
        {
            Debug.Log($"  Need {totalNeeded - totalExisting} more snakes");

            for (int i = totalExisting; i < totalNeeded; i++)
            {
                bool isPoison = (i >= levelConfig.fireSnakeCount);
                SpawnSnake(isPoison, i);
            }
        }

        Debug.Log($"  Total snakes: {levelConfig.fireSnakeCount} fire, {levelConfig.poisonSnakeCount} poison");
    }

    void ConfigureSnake(CobraAI snake, bool isPoison)
    {
        // Enable projectiles
        snake.canShootProjectiles = levelConfig.snakesCanShoot;
        snake.fireRate = 1f / levelConfig.snakeShootInterval; // Convert interval to rate

        // Set projectile type
        snake.projectileType = isPoison ? Projectile.ProjectileType.Poison : Projectile.ProjectileType.Fire;

        // Enable wall avoidance for maze
        snake.useWallAvoidance = true;

        // Set speed
        snake.speed = levelConfig.snakeSpeed;

        // NOT instant kill mode (we have HP system now)
        snake.isInstantKillMode = false;

        string snakeType = isPoison ? "Poison" : "Fire";
        Debug.Log($"  - {snake.gameObject.name}: {snakeType} snake, shoot={snake.canShootProjectiles}");
    }

    void SpawnSnake(bool isPoison, int index)
    {
        GameObject prefab = isPoison ? poisonSnakePrefab : fireSnakePrefab;

        if (prefab == null)
        {
            Debug.LogWarning($"  ! No {(isPoison ? "poison" : "fire")} snake prefab assigned");
            return;
        }

        // Get spawn point
        int spawnIndex = index % snakeSpawnPoints.Length;
        Vector3 spawnPos = snakeSpawnPoints[spawnIndex].position;

        GameObject snakeObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        CobraAI snake = snakeObj.GetComponent<CobraAI>();

        if (snake != null)
        {
            ConfigureSnake(snake, isPoison);
        }

        Debug.Log($"  + Spawned {(isPoison ? "poison" : "fire")} snake at {spawnPos}");
    }

    void SetupWinCondition()
    {
        Debug.Log($"Setting up win condition: Score {levelConfig.targetScore}");

        // Update GameManager target score if it exists
        if (GameManager.Instance != null)
        {
            // GameManager should read from LevelConfig
            Debug.Log($"  Target score: {levelConfig.targetScore}");
        }
    }

    /// <summary>
    /// Check if player has won (reached target score)
    /// </summary>
    public bool CheckWinCondition(int currentScore)
    {
        return currentScore >= levelConfig.targetScore;
    }

    /// <summary>
    /// Called when player wins the level
    /// </summary>
    public void OnLevelComplete()
    {
        Debug.Log($"Level {GameState.CurrentLevel} COMPLETE!");

        // Unlock next level
        GameState.UnlockNextLevel();

        // Load next level or return to menu
        if (GameState.CurrentLevel < 3)
        {
            GameState.CurrentLevel++;
            string nextScene = GameState.GetCurrentSceneName();
            Debug.Log($"Loading next level: {nextScene}");
            // UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("All levels complete for this persona!");
            // Return to level select or show completion screen
        }
    }
}
