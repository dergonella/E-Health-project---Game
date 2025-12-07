using UnityEngine;

/// <summary>
/// Simple component that triggers snake growth based on various events.
/// Attach this to snake GameObjects that have SnakeBodyController.
///
/// GROWTH TRIGGERS:
/// - Time-based: Grow every X seconds automatically
/// - Player kill: Grow when killing the player
/// - Score-based: Grow when player reaches certain score thresholds
///
/// IMPORTANT: This script does NOT modify GameManager/score/money logic.
/// It only calls SnakeBodyController.Grow() based on triggers.
/// </summary>
public class SnakeGrowthTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SnakeBodyController on this snake (auto-found if empty)")]
    public SnakeBodyController bodyController;

    [Header("Time-Based Growth")]
    [Tooltip("Enable automatic growth over time")]
    public bool enableTimedGrowth = true;

    [Tooltip("Seconds between each growth")]
    [Range(5f, 60f)]
    public float growthInterval = 10f;

    [Tooltip("How many segments to add each time")]
    [Range(1, 5)]
    public int segmentsPerGrowth = 1;

    [Header("Event-Based Growth")]
    [Tooltip("Grow when this snake kills the player")]
    public bool growOnPlayerKill = true;

    [Tooltip("Segments to add when killing player")]
    [Range(1, 10)]
    public int segmentsOnKill = 3;

    [Header("Score-Based Growth")]
    [Tooltip("Grow when player reaches score milestones")]
    public bool growOnScoreMilestones = false;

    [Tooltip("Score milestones (e.g., 500, 1000, 1500)")]
    public int[] scoreMilestones = new int[] { 500, 1000, 1500, 2000 };

    // Internals
    private float growthTimer = 0f;
    private int lastMilestoneIndex = -1;

    void Start()
    {
        // Auto-find SnakeBodyController if not assigned
        if (bodyController == null)
        {
            bodyController = GetComponent<SnakeBodyController>();
        }

        // Validation
        if (bodyController == null)
        {
            Debug.LogError($"[SnakeGrowthTrigger] {gameObject.name}: No SnakeBodyController found! Disabling...");
            enabled = false;
            return;
        }

        // Initialize timer
        growthTimer = growthInterval;

        Debug.Log($"[SnakeGrowthTrigger] {gameObject.name}: Growth triggers enabled. Timed={enableTimedGrowth}, OnKill={growOnPlayerKill}, Score={growOnScoreMilestones}");
    }

    void Update()
    {
        // Time-based growth
        if (enableTimedGrowth)
        {
            UpdateTimedGrowth();
        }

        // Score-based growth
        if (growOnScoreMilestones)
        {
            CheckScoreMilestones();
        }
    }

    /// <summary>
    /// Time-based growth: Grow every X seconds
    /// </summary>
    private void UpdateTimedGrowth()
    {
        growthTimer -= Time.deltaTime;

        if (growthTimer <= 0f)
        {
            // Trigger growth
            bodyController.Grow(segmentsPerGrowth);

            // Reset timer
            growthTimer = growthInterval;

            Debug.Log($"[SnakeGrowthTrigger] {gameObject.name}: Timed growth! Added {segmentsPerGrowth} segments.");
        }
    }

    /// <summary>
    /// Score-based growth: Check if player reached new milestones
    /// </summary>
    private void CheckScoreMilestones()
    {
        if (GameManager.Instance == null) return;

        int currentScore = GameManager.Instance.GetScore();

        // Check each milestone
        for (int i = 0; i < scoreMilestones.Length; i++)
        {
            // If player passed this milestone and we haven't triggered it yet
            if (currentScore >= scoreMilestones[i] && i > lastMilestoneIndex)
            {
                // Trigger growth
                bodyController.Grow(segmentsPerGrowth);
                lastMilestoneIndex = i;

                Debug.Log($"[SnakeGrowthTrigger] {gameObject.name}: Score milestone {scoreMilestones[i]} reached! Grew by {segmentsPerGrowth} segments.");
                break; // Only trigger once per frame
            }
        }
    }

    /// <summary>
    /// Call this from CobraAI when this snake kills the player.
    /// Example: In CobraAI.HandlePlayerCollision(), add:
    ///   SnakeGrowthTrigger trigger = GetComponent<SnakeGrowthTrigger>();
    ///   if (trigger != null) trigger.OnPlayerKilled();
    /// </summary>
    public void OnPlayerKilled()
    {
        if (!growOnPlayerKill) return;

        bodyController.Grow(segmentsOnKill);

        Debug.Log($"[SnakeGrowthTrigger] {gameObject.name}: Killed player! Grew by {segmentsOnKill} segments.");
    }

    /// <summary>
    /// Manually trigger growth (for custom events)
    /// </summary>
    public void TriggerGrowth(int amount)
    {
        bodyController.Grow(amount);
    }
}
