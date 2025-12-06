using UnityEngine;

/// <summary>
/// Snake Growth Manager - Automatically grows snakes based on time or events
/// Attach this to a GameObject in Level 0.2 to make snakes grow during gameplay
/// </summary>
public class SnakeGrowthManager : MonoBehaviour
{
    [Header("Growth Triggers")]
    [Tooltip("Should snakes grow over time?")]
    [SerializeField] private bool growOverTime = false;  // DISABLED: We use shard collection instead

    [Tooltip("How many seconds between growth")]
    [SerializeField] private float growthInterval = 10f;

    [Tooltip("How many segments to add each growth")]
    [SerializeField] private int segmentsPerGrowth = 1;

    [Header("Growth on Events (Level 0.2 - Option B)")]
    [Tooltip("Grow snakes when player collects shards?")]
    [SerializeField] private bool growOnShardCollect = true;  // ENABLED: Snakes grow when player collects shards

    [Tooltip("How many shards before snakes grow")]
    [SerializeField] private int shardsPerGrowth = 3;  // Every 3 shards = snakes grow

    [Tooltip("Segments to add when threshold reached")]
    [SerializeField] private int segmentsPerGrowthEvent = 1;

    private int shardsCollected = 0;  // Track shards collected

    [Header("Max Growth Limit")]
    [Tooltip("Maximum body segments per snake (0 = unlimited)")]
    [SerializeField] private int maxSegmentsPerSnake = 20;

    private float growthTimer = 0f;
    private SnakeBodyController[] allSnakes;

    void Start()
    {
        // Find all snakes in the scene
        allSnakes = Object.FindObjectsByType<SnakeBodyController>(FindObjectsSortMode.None);

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
    /// LEVEL 0.2: Every 3 shards = snakes grow by 1 segment
    /// </summary>
    public void OnShardCollected()
    {
        if (!growOnShardCollect) return;

        shardsCollected++;
        Debug.Log($"SnakeGrowthManager: Shard collected ({shardsCollected}/{shardsPerGrowth})");

        // Check if we've collected enough shards to trigger growth
        if (shardsCollected >= shardsPerGrowth)
        {
            GrowAllSnakes(segmentsPerGrowthEvent);
            Debug.Log($"SnakeGrowthManager: ALL SNAKES GREW! {shardsPerGrowth} shards collected.");

            // Reset counter
            shardsCollected = 0;
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
