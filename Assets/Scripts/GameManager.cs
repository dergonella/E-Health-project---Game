using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int winScore = 500;  // Reduced from 2000 for quick testing - only 5 shards needed!
    public int totalShards = 10;  // Reduced back to 10 for faster testing

    [Header("References")]
    public UIManager uiManager;
    public GameObject playerPrefab;
    public GameObject cobraChasePrefab;
    public GameObject cobraAttackPrefab;
    public GameObject cobraRandomPrefab;
    public GameObject shardPrefab;
    public GameObject painkillerPrefab;

    [Header("Painkiller Settings")]
    public int totalPainkillers = 2;  // Always spawn 2 painkillers for testing

    [Header("Mine Settings")]
    public GameObject minePrefab;
    public int totalMines = 5;  // Always spawn 5 mines randomly
    public bool spawnMines = false;  // Set to true for levels with mines (Level 3)

    [Header("Spawn Positions - Four Corners Strategy")]
    public Vector3 playerSpawnPosition = new Vector3(-3f, -2f, 0f);  // Bottom-left corner
    public Vector3[] cobraSpawnPositions = new Vector3[]
    {
        new Vector3(3f, 2f, 0f),     // Top-right corner - Attack cobra
        new Vector3(-3f, 2f, 0f),    // Top-left corner - Ambusher/Chase
        new Vector3(3f, -2f, 0f)     // Bottom-right corner - Random
    };

    // Game state
    public enum GameState { Playing, Won, Lost }
    public GameState currentState = GameState.Playing;

    private GameObject player;
    private GameObject[] cobras;
    private GameObject[] shards;
    private GameObject[] painkillers;
    private GameObject[] mines;

    void Awake()
    {
        // Singleton pattern
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
        InitializeGame();
    }

    void Update()
    {
        // Handle restart input
        if (currentState != GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetGame();
            }
        }

        // Check win condition
        if (currentState == GameState.Playing && player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null && playerController.score >= winScore)
            {
                GameOver(true);
            }
        }

        // ESC to quit (removed Editor code as per code review)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void InitializeGame()
    {
        // Reset time scale in case slow motion was active
        Time.timeScale = 1f;

        currentState = GameState.Playing;

        // Get level data from LevelManager
        bool cobraInstantKill = false;
        if (LevelManager.Instance != null)
        {
            var levelData = LevelManager.Instance.GetCurrentLevelData();
            if (levelData != null)
            {
                winScore = levelData.targetScore;
                spawnMines = levelData.hasMines;  // Set mines based on level data
                cobraInstantKill = levelData.cobraInstantKill;  // Set cobra instant kill mode
                Debug.Log("Level data loaded - spawnMines: " + spawnMines + ", cobraInstantKill: " + cobraInstantKill);
            }
        }

        // Check if player already exists in scene
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            player = existingPlayer;
        }
        else if (playerPrefab != null)
        {
            // Spawn player only if not found in scene
            player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
            player.tag = "Player";
        }

        // Setup health system if player exists
        if (player != null)
        {
            HealthSystem healthSystem = player.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.OnDeath += () => GameOver(false);
            }
        }

        // Find existing cobras in scene
        GameObject[] existingCobras = GameObject.FindGameObjectsWithTag("Cobra");

        if (existingCobras.Length >= 3)
        {
            // Use existing cobras from scene
            cobras = new GameObject[3];
            for (int i = 0; i < 3 && i < existingCobras.Length; i++)
            {
                cobras[i] = existingCobras[i];
            }
        }
        else
        {
            // Spawn cobras as before
            cobras = new GameObject[3];
            if (cobraChasePrefab != null && cobraSpawnPositions.Length > 0)
            {
                cobras[0] = Instantiate(cobraChasePrefab, cobraSpawnPositions[0], Quaternion.identity);
                cobras[0].tag = "Cobra";
            }
            if (cobraAttackPrefab != null && cobraSpawnPositions.Length > 1)
            {
                cobras[1] = Instantiate(cobraAttackPrefab, cobraSpawnPositions[1], Quaternion.identity);
                cobras[1].tag = "Cobra";
            }
            if (cobraRandomPrefab != null && cobraSpawnPositions.Length > 2)
            {
                cobras[2] = Instantiate(cobraRandomPrefab, cobraSpawnPositions[2], Quaternion.identity);
                cobras[2].tag = "Cobra";
            }
        }

        // Configure cobras for instant kill mode (levels 1-3)
        if (cobras != null)
        {
            foreach (GameObject cobra in cobras)
            {
                if (cobra != null)
                {
                    CobraAI cobraAI = cobra.GetComponent<CobraAI>();
                    if (cobraAI != null)
                    {
                        cobraAI.isInstantKillMode = cobraInstantKill;

                        // Make cobras red in instant kill mode
                        if (cobraInstantKill)
                        {
                            SpriteRenderer sr = cobra.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                sr.color = Color.red;
                            }
                        }
                    }
                }
            }
            Debug.Log("Cobras configured - Instant kill mode: " + cobraInstantKill);
        }

        // Check if shards already exist
        GameObject[] existingShards = GameObject.FindGameObjectsWithTag("Shard");

        if (existingShards.Length >= totalShards)
        {
            // Use existing shards
            shards = existingShards;
        }
        else
        {
            // Spawn shards
            SpawnShards(existingShards);
        }

        // Spawn painkillers
        SpawnPainkillers();

        // Spawn mines if enabled
        if (spawnMines)
        {
            Debug.Log("=== MINE SPAWNING ENABLED - Starting mine initialization ===");

            // ALWAYS delete any existing mines and spawn fresh ones
            GameObject[] existingMines = GameObject.FindGameObjectsWithTag("Mine");

            if (existingMines.Length > 0)
            {
                Debug.Log("Found " + existingMines.Length + " existing mines in scene, deleting them...");
                foreach (GameObject existingMine in existingMines)
                {
                    if (existingMine != null)
                    {
                        Destroy(existingMine);
                    }
                }
            }

            // Clear the mines array reference
            mines = null;

            // Always spawn fresh mines
            SpawnMines();

            // Verify mines were spawned
            int mineCount = GameObject.FindGameObjectsWithTag("Mine").Length;
            Debug.Log("=== MINE INITIALIZATION COMPLETE - Total mines in scene: " + mineCount + " ===");
        }
        else
        {
            Debug.Log("Mine spawning is DISABLED for this level (spawnMines = false)");
        }

        // Initialize UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(0);
            uiManager.HideGameOverScreens();
        }
    }

    void SpawnShards(GameObject[] existingShards)
    {
        int currentShardCount = totalShards - existingShards.Length;
        shards = new GameObject[currentShardCount];
        for (int i = 0; i < currentShardCount; i++)
        {
            if (shardPrefab != null)
            {
                // Find valid spawn position
                Vector3 spawnPos = FindValidShardPosition();
                shards[i] = Instantiate(shardPrefab, spawnPos, Quaternion.identity);
                shards[i].tag = "Shard";
            }
        }
    }

    void SpawnPainkillers()
    {
        painkillers = new GameObject[totalPainkillers];
        for (int i = 0; i < totalPainkillers; i++)
        {
            if (painkillerPrefab != null)
            {
                // Spawn painkillers at fixed positions for visibility
                Vector3 spawnPos;
                if (i == 0)
                {
                    spawnPos = new Vector3(-2f, 2f, 0f);  // Top-left area
                }
                else
                {
                    spawnPos = new Vector3(2f, -2f, 0f);  // Bottom-right area
                }

                painkillers[i] = Instantiate(painkillerPrefab, spawnPos, Quaternion.identity);
                painkillers[i].tag = "Painkiller";
            }
        }
    }

    void SpawnMines()
    {
        if (minePrefab == null)
        {
            Debug.LogWarning("Mine prefab not assigned to GameManager!");
            return;
        }

        Debug.Log("Starting to spawn " + totalMines + " mines...");

        // Initialize new array - this ensures we start fresh
        mines = new GameObject[totalMines];

        for (int i = 0; i < totalMines; i++)
        {
            // Spawn mines at random positions
            Vector3 spawnPos = FindValidMinePosition();
            mines[i] = Instantiate(minePrefab, spawnPos, Quaternion.identity);
            mines[i].tag = "Mine";
            mines[i].name = "Mine_" + i; // Give unique names for debugging

            Debug.Log("Spawned mine " + (i+1) + "/" + totalMines + " at: " + spawnPos);
        }

        Debug.Log("Finished spawning " + totalMines + " mines!");
    }

    Vector3 FindValidMinePosition()
    {
        float boundX = 4f;
        float boundY = 3f;
        Vector3 position = Vector3.zero;
        bool validPosition = false;
        int attempts = 0;

        while (!validPosition && attempts < 50)
        {
            position = new Vector3(
                Random.Range(-boundX + 0.5f, boundX - 0.5f),
                Random.Range(-boundY + 0.5f, boundY - 0.5f),
                0f
            );

            // Check if position is not too close to player spawn
            float distanceToPlayer = Vector3.Distance(position, playerSpawnPosition);

            // Check if not too close to existing mines
            bool tooCloseToMine = false;
            if (mines != null)
            {
                for (int i = 0; i < mines.Length; i++)
                {
                    if (mines[i] != null)
                    {
                        float distanceToMine = Vector3.Distance(position, mines[i].transform.position);
                        if (distanceToMine < 1.5f) // Mines should be at least 1.5 units apart
                        {
                            tooCloseToMine = true;
                            break;
                        }
                    }
                }
            }

            // Valid if far enough from player spawn and other mines
            if (distanceToPlayer > 2f && !tooCloseToMine)
            {
                validPosition = true;
            }

            attempts++;
        }

        return position;
    }

    Vector3 FindValidShardPosition()
    {
        // Simple random position, avoiding walls (you may need to refine this)
        float boundX = 4f;
        float boundY = 3f;

        Vector3 position = Vector3.zero;
        bool validPosition = false;
        int attempts = 0;

        while (!validPosition && attempts < 50)
        {
            position = new Vector3(
                Random.Range(-boundX + 0.4f, boundX - 0.4f),
                Random.Range(-boundY + 0.4f, boundY - 0.4f),
                0f
            );

            // Check if position overlaps with walls or is too close to player spawn
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, 0.15f);
            bool hasWall = false;

            foreach (Collider2D col in overlaps)
            {
                if (col.CompareTag("Wall"))
                {
                    hasWall = true;
                    break;
                }
            }

            float distanceToPlayerSpawn = Vector3.Distance(position, playerSpawnPosition);
            validPosition = !hasWall && distanceToPlayerSpawn > 0.8f;

            attempts++;
        }

        return position;
    }

    public void UpdateScore(int score)
    {
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
        }
    }

    public void GameOver(bool won)
    {
        currentState = won ? GameState.Won : GameState.Lost;

        if (uiManager != null)
        {
            int finalScore = 0;
            if (player != null)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    finalScore = playerController.score;
                }
            }

            if (won)
            {
                uiManager.ShowWinScreen(finalScore);
            }
            else
            {
                uiManager.ShowLoseScreen(finalScore);
            }
        }

        // Stop cobras movement
        if (cobras != null)
        {
            foreach (GameObject cobra in cobras)
            {
                if (cobra != null)
                {
                    CobraAI ai = cobra.GetComponent<CobraAI>();
                    if (ai != null)
                    {
                        ai.enabled = false;
                    }
                }
            }
        }

        // Stop player movement
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = false;
            }
        }
    }

    public void ResetGame()
    {
        // Reset time scale in case slow motion was active
        Time.timeScale = 1f;

        // Reset difficulty to starting level
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResetDifficulty();
        }

        // Reset player stats and position
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.score = 0;
                pc.shardsCollected = 0;
                pc.enabled = true;
            }

            // ALWAYS reset position to corner spawn
            player.transform.position = playerSpawnPosition;

            // Reset health system if exists
            HealthSystem hs = player.GetComponent<HealthSystem>();
            if (hs != null)
            {
                hs.ResetHealth();
            }

            // Reset abilities if exists
            AbilitySystem ab = player.GetComponent<AbilitySystem>();
            if (ab != null)
            {
                ab.ResetAbilities();
            }
        }

        // Reset cobras to their corner positions
        if (cobras != null)
        {
            for (int i = 0; i < cobras.Length; i++)
            {
                if (cobras[i] != null)
                {
                    CobraAI ai = cobras[i].GetComponent<CobraAI>();
                    if (ai != null)
                    {
                        ai.enabled = true;
                    }

                    // Reset cobra position to its corner
                    if (i < cobraSpawnPositions.Length)
                    {
                        cobras[i].transform.position = cobraSpawnPositions[i];
                    }
                }
            }
        }

        // Reset painkillers - make them visible and available again
        if (painkillers != null)
        {
            for (int i = 0; i < painkillers.Length; i++)
            {
                if (painkillers[i] != null)
                {
                    Painkiller pk = painkillers[i].GetComponent<Painkiller>();
                    if (pk != null)
                    {
                        pk.ResetPainkiller();
                    }
                }
            }
        }

        // Respawn shards
        if (shards != null)
        {
            foreach (GameObject shard in shards)
            {
                if (shard != null)
                {
                    ShardController sc = shard.GetComponent<ShardController>();
                    if (sc != null)
                    {
                        sc.Respawn();
                    }
                }
            }
        }

        // Respawn mines - destroy old ones and spawn new random positions
        if (spawnMines)
        {
            Debug.Log("=== MINE RESPAWN - Starting mine respawn on level restart ===");

            // Destroy old mines from array
            if (mines != null)
            {
                Debug.Log("Destroying " + mines.Length + " old mines from array...");
                foreach (GameObject mine in mines)
                {
                    if (mine != null)
                    {
                        Destroy(mine);
                    }
                }
                mines = null; // Clear the array reference immediately
            }
            else
            {
                Debug.Log("Mines array was null, no mines to destroy from array");
            }

            // Also destroy any mines left in scene by tag (cleanup)
            GameObject[] leftoverMines = GameObject.FindGameObjectsWithTag("Mine");
            if (leftoverMines.Length > 0)
            {
                Debug.Log("Found " + leftoverMines.Length + " leftover mines in scene by tag, destroying...");
                foreach (GameObject leftover in leftoverMines)
                {
                    if (leftover != null)
                    {
                        Destroy(leftover);
                    }
                }
            }
            else
            {
                Debug.Log("No leftover mines found by tag");
            }

            // Spawn new mines at random positions
            SpawnMines();

            // Verify mines were respawned
            int mineCount = GameObject.FindGameObjectsWithTag("Mine").Length;
            Debug.Log("=== MINE RESPAWN COMPLETE - Total mines in scene after respawn: " + mineCount + " ===");
        }
        else
        {
            Debug.Log("Mine spawning is DISABLED - skipping mine respawn");
        }

        // Update game state
        currentState = GameState.Playing;

        // Reset UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(0);
            uiManager.HideGameOverScreens();
        }
    }
}
