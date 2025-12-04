using UnityEngine;

/// <summary>
/// ShardSpawner - Manages spawning 10 shards at random locations constantly during gameplay.
/// Ensures there are always exactly 10 shards active in the level at all times.
/// When a shard is collected, it immediately respawns at a new random location.
/// Also repositions shards based on difficulty level to keep gameplay dynamic.
/// </summary>
public class ShardSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private int shardCount = 10; // Always maintain 10 shards

    [Header("Spawn Boundaries (Must Match Your Level Size!)")]
    [SerializeField] private float boundX = 3.5f; // Reduced from 4 to prevent spawning outside
    [SerializeField] private float boundY = 2.5f; // Reduced from 3 to prevent spawning outside
    [SerializeField] private float wallBuffer = 0.6f; // Increased safety buffer from walls

    [Header("Difficulty-Based Repositioning")]
    [SerializeField] private bool enableDifficultyRepositioning = true;
    [SerializeField] private float repositionInterval = 30f; // Reposition all shards every 30 seconds
    [SerializeField] private float repositionPerDifficultyLevel = 15f; // Faster repositioning as difficulty increases

    private GameObject[] activeShards;
    private float repositionTimer = 0f;

    void Start()
    {
        if (shardPrefab == null)
        {
            Debug.LogError("ShardSpawner: Shard prefab is not assigned!");
            return;
        }

        // Initialize array to track shards
        activeShards = new GameObject[shardCount];

        // Spawn initial 10 shards
        SpawnAllShards();

        // Initialize reposition timer
        repositionTimer = repositionInterval;
    }

    void Update()
    {
        // Difficulty-based shard repositioning
        if (enableDifficultyRepositioning && GameManager.Instance != null && !GameManager.Instance.IsGameOver())
        {
            repositionTimer -= Time.deltaTime;

            if (repositionTimer <= 0f)
            {
                // Calculate repositioning interval based on difficulty
                float currentInterval = repositionInterval;

                if (DifficultyManager.Instance != null)
                {
                    float difficultyLevel = DifficultyManager.Instance.GetCurrentDifficultyLevel();
                    // As difficulty increases, shards reposition more frequently
                    currentInterval = repositionInterval - (repositionPerDifficultyLevel * (difficultyLevel - 1f));
                    currentInterval = Mathf.Max(currentInterval, 10f); // Minimum 10 seconds between repositions
                }

                // Reposition all shards to new random locations
                RepositionAllShards();

                // Reset timer
                repositionTimer = currentInterval;

                Debug.Log($"ShardSpawner: Repositioned all shards! Next reposition in {currentInterval:F1}s");
            }
        }
    }

    /// <summary>
    /// Spawn all shards at random valid positions.
    /// </summary>
    private void SpawnAllShards()
    {
        for (int i = 0; i < shardCount; i++)
        {
            SpawnShard(i);
        }

        Debug.Log($"ShardSpawner: Spawned {shardCount} shards");
    }

    /// <summary>
    /// Spawn a single shard at a specific index in the array.
    /// </summary>
    private void SpawnShard(int index)
    {
        // Find a valid spawn position
        Vector3 spawnPosition = GetValidSpawnPosition();

        // Instantiate the shard
        GameObject newShard = Instantiate(shardPrefab, spawnPosition, Quaternion.identity);
        newShard.transform.SetParent(transform); // Organize under ShardSpawner
        newShard.name = $"Shard_{index}";

        // CRITICAL FIX #1: Ensure shard has proper tag for collision detection
        if (!newShard.CompareTag("Shard"))
        {
            newShard.tag = "Shard";
            Debug.Log($"ShardSpawner: Set tag 'Shard' on {newShard.name}");
        }

        // CRITICAL FIX #2: Ensure shard has trigger collider
        Collider2D col = newShard.GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true; // Must be trigger for player to collect
            Debug.Log($"ShardSpawner: Shard {newShard.name} - Tag: {newShard.tag}, Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}");
        }
        else
        {
            Debug.LogError($"ShardSpawner: Shard {newShard.name} has NO COLLIDER! Player won't be able to collect it!");
        }

        // Store reference
        activeShards[index] = newShard;

        // Add listener for when this shard is collected
        ShardController shardController = newShard.GetComponent<ShardController>();
        if (shardController != null)
        {
            // Set index so we know which shard was collected
            shardController.shardIndex = index;
            shardController.spawner = this;
        }
    }

    /// <summary>
    /// Reposition all active shards to new random locations (difficulty mechanic).
    /// </summary>
    private void RepositionAllShards()
    {
        for (int i = 0; i < activeShards.Length; i++)
        {
            if (activeShards[i] != null)
            {
                // Get new valid position
                Vector3 newPosition = GetValidSpawnPosition();

                // Move shard to new position
                activeShards[i].transform.position = newPosition;
            }
        }
    }

    /// <summary>
    /// Called by ShardController when a shard is collected.
    /// Immediately respawns it at a new random location.
    /// </summary>
    public void OnShardCollected(int index)
    {
        if (index < 0 || index >= activeShards.Length)
            return;

        // Destroy old shard
        if (activeShards[index] != null)
        {
            Destroy(activeShards[index]);
        }

        // Spawn new shard immediately at a random location
        SpawnShard(index);
    }

    /// <summary>
    /// Find a valid random spawn position that doesn't overlap with walls.
    /// IMPROVED: Better boundary checking and validation.
    /// </summary>
    private Vector3 GetValidSpawnPosition()
    {
        bool validPosition = false;
        int attempts = 0;
        Vector3 position = Vector3.zero;

        while (!validPosition && attempts < 100)
        {
            // Generate random position within SAFE bounds (with buffer)
            float randomX = Random.Range(-boundX + wallBuffer, boundX - wallBuffer);
            float randomY = Random.Range(-boundY + wallBuffer, boundY - wallBuffer);

            // Clamp to ensure position is ALWAYS within bounds
            randomX = Mathf.Clamp(randomX, -boundX + wallBuffer, boundX - wallBuffer);
            randomY = Mathf.Clamp(randomY, -boundY + wallBuffer, boundY - wallBuffer);

            position = new Vector3(randomX, randomY, 0f);

            // Validate position doesn't overlap with walls or cobras
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, 0.25f);
            validPosition = true;

            foreach (Collider2D overlap in overlaps)
            {
                // Avoid spawning on walls
                if (overlap.CompareTag("Wall"))
                {
                    validPosition = false;
                    break;
                }
                // Avoid spawning on cobras
                if (overlap.CompareTag("Cobra"))
                {
                    validPosition = false;
                    break;
                }
                // Avoid spawning on player
                if (overlap.CompareTag("Player"))
                {
                    validPosition = false;
                    break;
                }
            }

            attempts++;
        }

        if (attempts >= 100)
        {
            Debug.LogWarning($"ShardSpawner: Could not find valid spawn position after 100 attempts. Using safe fallback position. Check your boundX ({boundX}) and boundY ({boundY}) settings!");
            // Use a safe center position as fallback
            position = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f
            );
        }

        return position;
    }

    /// <summary>
    /// Public method to manually trigger shard repositioning (for testing or special events).
    /// </summary>
    public void ManualRepositionAllShards()
    {
        RepositionAllShards();
        Debug.Log("ShardSpawner: Manual reposition triggered!");
    }

    /// <summary>
    /// Get current difficulty-adjusted reposition interval.
    /// </summary>
    public float GetCurrentRepositionInterval()
    {
        if (DifficultyManager.Instance != null)
        {
            float difficultyLevel = DifficultyManager.Instance.GetCurrentDifficultyLevel();
            float currentInterval = repositionInterval - (repositionPerDifficultyLevel * (difficultyLevel - 1f));
            return Mathf.Max(currentInterval, 10f);
        }
        return repositionInterval;
    }

    /// <summary>
    /// Debug visualization in Scene view.
    /// Shows spawn boundaries (yellow box) and actual game area (green box).
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw ACTUAL spawn boundaries (with buffer)
        Gizmos.color = Color.yellow;
        Vector3 spawnSize = new Vector3(
            (boundX - wallBuffer) * 2,
            (boundY - wallBuffer) * 2,
            0
        );
        Gizmos.DrawWireCube(Vector3.zero, spawnSize);

        // Draw FULL game area boundaries (without buffer)
        Gizmos.color = Color.green;
        Vector3 fullSize = new Vector3(boundX * 2, boundY * 2, 0);
        Gizmos.DrawWireCube(Vector3.zero, fullSize);

        // Draw label
        UnityEngine.GUI.color = Color.yellow;
    }
}
