using UnityEngine;

/// <summary>
/// Snake Growth Manager - Automatically grows snakes based on time or events
/// Attach this to a GameObject in Level 0.2 to make snakes grow during gameplay
/// </summary>
public class SnakeGrowthManager : MonoBehaviour
{
    [Header("Growth Triggers")]
    [Tooltip("Should snakes grow over time?")]
    [SerializeField] private bool growOverTime = true;

    [Tooltip("How many seconds between growth")]
    [SerializeField] private float growthInterval = 10f;

    [Tooltip("How many segments to add each growth")]
    [SerializeField] private int segmentsPerGrowth = 1;

    [Header("Growth on Events")]
    [Tooltip("Grow snakes when player collects shards?")]
    [SerializeField] private bool growOnShardCollect = false;

    [Tooltip("Segments to add per shard collected")]
    [SerializeField] private int segmentsPerShard = 1;

    [Header("Max Growth Limit")]
    [Tooltip("Maximum body segments per snake (0 = unlimited)")]
    [SerializeField] private int maxSegmentsPerSnake = 20;

    private float growthTimer = 0f;
    private SnakeBodyController[] allSnakes;

    void Start()
    {
        // Find all snakes in the scene
        allSnakes = FindObjectsByType<SnakeBodyController>(FindObjectsSortMode.None);

        if (allSnakes.Length == 0)
        {
            Debug.LogWarning("SnakeGrowthManager: No snakes with SnakeBodyController found in scene!");
            enabled = false;
            return;
        }

        Debug.Log($"SnakeGrowthManager: Managing {allSnakes.Length} snakes");

        // Initialize timer
        growthTimer = growthInterval;
    }

    void Update()
    {
        if (growOverTime)
        {
            HandleTimeBasedGrowth();
        }
    }

    /// <summary>
    /// Handles automatic growth over time
    /// </summary>
    private void HandleTimeBasedGrowth()
    {
        // Don't grow if game is over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            return;
        }

        growthTimer -= Time.deltaTime;

        if (growthTimer <= 0f)
        {
            // Time to grow!
            GrowAllSnakes(segmentsPerGrowth);

            // Reset timer
            growthTimer = growthInterval;

            Debug.Log($"SnakeGrowthManager: Snakes grew by {segmentsPerGrowth} segments!");
        }
    }

    /// <summary>
    /// Grows all snakes by the specified amount
    /// </summary>
    public void GrowAllSnakes(int amount = 1)
    {
        if (allSnakes == null || allSnakes.Length == 0) return;

        foreach (var snake in allSnakes)
        {
            if (snake != null)
            {
                // Check max limit
                if (maxSegmentsPerSnake > 0 && snake.GetSegmentCount() >= maxSegmentsPerSnake)
                {
                    Debug.Log($"SnakeGrowthManager: {snake.name} reached max segment limit ({maxSegmentsPerSnake})");
                    continue;
                }

                snake.Grow(amount);
            }
        }
    }

    /// <summary>
    /// Call this when player collects a shard (if growOnShardCollect is enabled)
    /// Hook this up to your shard collection event
    /// </summary>
    public void OnShardCollected()
    {
        if (growOnShardCollect)
        {
            GrowAllSnakes(segmentsPerShard);
            Debug.Log($"SnakeGrowthManager: Snakes grew by {segmentsPerShard} from shard collection!");
        }
    }

    /// <summary>
    /// Manually trigger growth (for testing or special events)
    /// </summary>
    [ContextMenu("Grow All Snakes Now")]
    public void GrowAllSnakesNow()
    {
        GrowAllSnakes(1);
    }

    /// <summary>
    /// Gets the average segment count across all snakes
    /// </summary>
    public float GetAverageSegmentCount()
    {
        if (allSnakes == null || allSnakes.Length == 0) return 0;

        int totalSegments = 0;
        int validSnakes = 0;

        foreach (var snake in allSnakes)
        {
            if (snake != null)
            {
                totalSegments += snake.GetSegmentCount();
                validSnakes++;
            }
        }

        return validSnakes > 0 ? (float)totalSegments / validSnakes : 0;
    }
}
