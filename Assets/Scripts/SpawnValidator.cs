using UnityEngine;

/// <summary>
/// SpawnValidator - Utility class for finding valid spawn positions in maze levels.
/// Use this to ensure player, enemies, and collectibles don't spawn inside walls.
///
/// USAGE:
/// 1. Add this component to any GameObject in your Level 0.2 scene (e.g., GameManager)
/// 2. Call SpawnValidator.Instance.GetValidSpawnPosition(desiredPosition, radius)
///    from any script to get a position that doesn't overlap walls
/// 3. Or use SpawnValidator.Instance.IsPositionValid(position, radius) to check manually
/// </summary>
public class SpawnValidator : MonoBehaviour
{
    public static SpawnValidator Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Layer mask for walls/obstacles to avoid")]
    public LayerMask obstacleLayer;

    [Tooltip("Default radius to check around spawn points")]
    public float defaultCheckRadius = 0.5f;

    [Tooltip("Maximum attempts to find a valid position")]
    public int maxAttempts = 50;

    [Tooltip("How far to search from original position if blocked")]
    public float searchRadius = 2f;

    [Header("Play Area Bounds")]
    [Tooltip("Minimum X position for spawning")]
    public float minX = -3.5f;
    [Tooltip("Maximum X position for spawning")]
    public float maxX = 3.5f;
    [Tooltip("Minimum Y position for spawning")]
    public float minY = -2.5f;
    [Tooltip("Maximum Y position for spawning")]
    public float maxY = 2.5f;

    [Header("Debug")]
    public bool showDebugGizmos = false;
    private Vector3 lastCheckedPosition;
    private bool lastCheckResult;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-setup obstacle layer if not set
        if (obstacleLayer == 0)
        {
            int wallLayerIndex = LayerMask.NameToLayer("Wall");
            if (wallLayerIndex != -1)
            {
                obstacleLayer = 1 << wallLayerIndex;
                Debug.Log("[SpawnValidator] Auto-detected Wall layer");
            }
            else
            {
                Debug.LogWarning("[SpawnValidator] No 'Wall' layer found! Create it in Tags and Layers.");
            }
        }
    }

    /// <summary>
    /// Checks if a position is valid (not overlapping walls)
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <param name="radius">Radius around position to check (optional)</param>
    /// <returns>True if position is safe to spawn</returns>
    public bool IsPositionValid(Vector3 position, float radius = -1f)
    {
        if (radius < 0) radius = defaultCheckRadius;

        // Check if position is within bounds
        if (position.x < minX || position.x > maxX ||
            position.y < minY || position.y > maxY)
        {
            return false;
        }

        // Check for wall overlap using OverlapCircle
        Collider2D hit = Physics2D.OverlapCircle(position, radius, obstacleLayer);

        // Debug tracking
        lastCheckedPosition = position;
        lastCheckResult = (hit == null);

        return hit == null;
    }

    /// <summary>
    /// Gets a valid spawn position near the desired position.
    /// If the desired position is blocked, searches nearby for a valid spot.
    /// </summary>
    /// <param name="desiredPosition">Where you want to spawn</param>
    /// <param name="radius">Radius to check for obstacles</param>
    /// <returns>A valid spawn position, or the original if none found</returns>
    public Vector3 GetValidSpawnPosition(Vector3 desiredPosition, float radius = -1f)
    {
        if (radius < 0) radius = defaultCheckRadius;

        // First, check if desired position is already valid
        if (IsPositionValid(desiredPosition, radius))
        {
            return desiredPosition;
        }

        // Search in a spiral pattern around the desired position
        for (int i = 0; i < maxAttempts; i++)
        {
            // Calculate offset using golden angle for even distribution
            float angle = i * 137.5f * Mathf.Deg2Rad; // Golden angle
            float distance = Mathf.Sqrt(i) * (searchRadius / Mathf.Sqrt(maxAttempts));

            Vector3 testPosition = desiredPosition + new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0f
            );

            if (IsPositionValid(testPosition, radius))
            {
                Debug.Log($"[SpawnValidator] Found valid position after {i + 1} attempts: {testPosition}");
                return testPosition;
            }
        }

        // Fallback: Try random positions within bounds
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0f
            );

            if (IsPositionValid(randomPos, radius))
            {
                Debug.Log($"[SpawnValidator] Found random valid position: {randomPos}");
                return randomPos;
            }
        }

        // If all else fails, return original (this shouldn't happen with good maze design)
        Debug.LogWarning($"[SpawnValidator] Could not find valid position near {desiredPosition}! Check your maze design.");
        return desiredPosition;
    }

    /// <summary>
    /// Gets a random valid spawn position within the play area
    /// </summary>
    /// <param name="radius">Radius to check for obstacles</param>
    /// <returns>A random valid spawn position</returns>
    public Vector3 GetRandomValidPosition(float radius = -1f)
    {
        if (radius < 0) radius = defaultCheckRadius;

        for (int i = 0; i < maxAttempts * 2; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0f
            );

            if (IsPositionValid(randomPos, radius))
            {
                return randomPos;
            }
        }

        // Fallback to center if nothing found
        Debug.LogWarning("[SpawnValidator] Could not find random valid position!");
        return Vector3.zero;
    }

    /// <summary>
    /// Validates and adjusts a spawn position for a specific entity type
    /// </summary>
    public Vector3 GetValidPlayerSpawn(Vector3 preferredPosition)
    {
        return GetValidSpawnPosition(preferredPosition, 0.4f); // Slightly larger radius for player
    }

    public Vector3 GetValidEnemySpawn(Vector3 preferredPosition)
    {
        return GetValidSpawnPosition(preferredPosition, 0.35f);
    }

    public Vector3 GetValidCollectibleSpawn(Vector3 preferredPosition)
    {
        return GetValidSpawnPosition(preferredPosition, 0.4f); // Increased radius for better wall clearance
    }

    /// <summary>
    /// Debug visualization
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw play area bounds
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
        Gizmos.DrawWireCube(center, size);

        // Draw last checked position
        if (Application.isPlaying)
        {
            Gizmos.color = lastCheckResult ? Color.green : Color.red;
            Gizmos.DrawWireSphere(lastCheckedPosition, defaultCheckRadius);
        }
    }
}
