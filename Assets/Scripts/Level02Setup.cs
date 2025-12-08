using UnityEngine;

/// <summary>
/// Level 0.2 Setup Script
/// Add this to an empty GameObject in your Level 0.2 scene.
/// It automatically configures all cobras for instant kill mode and growing bodies.
///
/// SIMPLIFIED MECHANICS FOR LEVEL 0.2:
/// - Instant kill on touch (all enemies)
/// - Growing snake bodies
/// - Maze walls
/// - Shard collection (score)
///
/// NO: Health bars, shields, slow motion, fire, poison, mines, painkillers
/// </summary>
public class Level02Setup : MonoBehaviour
{
    [Header("Level 0.2 Settings")]
    [Tooltip("Enable instant kill for all cobras")]
    public bool enableInstantKill = true;

    [Tooltip("Enable wall avoidance for all cobras")]
    public bool enableWallAvoidance = true;

    [Tooltip("Enable growing bodies for all cobras")]
    public bool enableGrowingBodies = true;

    [Header("Snake Growth Settings")]
    [Tooltip("Initial body segments for each snake")]
    public int initialSegments = 3;

    [Tooltip("Seconds between automatic growth")]
    public float growthInterval = 15f;

    [Tooltip("Segments added per growth event")]
    public int segmentsPerGrowth = 1;

    void Start()
    {
        Debug.Log("=== LEVEL 0.2 SETUP ===");
        Debug.Log("Simplified mechanics: Instant kill + Growing snakes + Maze");

        SetupCobras();
        SetupPlayer();

        Debug.Log("=== LEVEL 0.2 READY ===");
    }

    void SetupCobras()
    {
        // Find all cobras in scene
        CobraAI[] cobras = FindObjectsByType<CobraAI>(FindObjectsSortMode.None);
        Debug.Log($"Found {cobras.Length} cobras in scene");

        foreach (CobraAI cobra in cobras)
        {
            // Enable instant kill mode
            if (enableInstantKill)
            {
                cobra.isInstantKillMode = true;
            }

            // Enable wall avoidance
            cobra.useWallAvoidance = enableWallAvoidance;

            // Disable projectiles (simplified for Level 0.2)
            cobra.canShootProjectiles = false;

            // Ensure proper collision setup for walls
            SetupCollisionComponents(cobra.gameObject, cobra.gameObject.name);

            // Setup growing body if enabled
            if (enableGrowingBodies)
            {
                SetupSnakeBody(cobra);
            }

            Debug.Log($"  - {cobra.gameObject.name}: InstantKill={cobra.isInstantKillMode}, WallAvoid={cobra.useWallAvoidance}, AI={cobra.aiType}");
        }
    }

    void SetupSnakeBody(CobraAI cobra)
    {
        // Check if SnakeBodyController already exists
        SnakeBodyController bodyController = cobra.GetComponent<SnakeBodyController>();

        if (bodyController == null)
        {
            Debug.LogWarning($"  ! {cobra.gameObject.name} has no SnakeBodyController - add one manually and assign the segment prefab!");
            return;
        }

        // Check if SnakeGrowthTrigger exists, add if not
        SnakeGrowthTrigger growthTrigger = cobra.GetComponent<SnakeGrowthTrigger>();
        if (growthTrigger == null)
        {
            growthTrigger = cobra.gameObject.AddComponent<SnakeGrowthTrigger>();
            Debug.Log($"  + Added SnakeGrowthTrigger to {cobra.gameObject.name}");
        }

        // Configure growth trigger
        growthTrigger.enableTimedGrowth = true;
        growthTrigger.growthInterval = growthInterval;
        growthTrigger.segmentsPerGrowth = segmentsPerGrowth;
        growthTrigger.growOnPlayerKill = true;
        growthTrigger.segmentsOnKill = 2;
    }

    void SetupPlayer()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("No Player found in scene!");
            return;
        }

        PlayerController player = playerObj.GetComponent<PlayerController>();
        if (player != null)
        {
            // Enable wall collision for maze
            player.useWallCollision = true;
            Debug.Log($"Player: WallCollision={player.useWallCollision}");
        }

        // Ensure proper collision setup for walls
        SetupCollisionComponents(playerObj, "Player");

        // Remove unnecessary components for Level 0.2 (if they exist)
        HealthSystem health = playerObj.GetComponent<HealthSystem>();
        AbilitySystem ability = playerObj.GetComponent<AbilitySystem>();

        if (health != null)
        {
            Debug.Log("  Note: HealthSystem found but will be ignored (instant kill mode)");
        }
        if (ability != null)
        {
            Debug.Log("  Note: AbilitySystem found but will be ignored (no abilities in Level 0.2)");
        }
    }

    /// <summary>
    /// Ensures proper collision setup for wall collision to work.
    /// Colliders must NOT be triggers for physical wall collision.
    /// </summary>
    void SetupCollisionComponents(GameObject obj, string objName)
    {
        // Check Rigidbody2D
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody2D>();
            Debug.Log($"  + Added Rigidbody2D to {objName}");
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Check Collider2D - must NOT be a trigger for wall collision
        Collider2D col = obj.GetComponent<Collider2D>();
        if (col == null)
        {
            // Add a circle collider if none exists
            col = obj.AddComponent<CircleCollider2D>();
            Debug.Log($"  + Added CircleCollider2D to {objName}");
        }

        if (col.isTrigger)
        {
            Debug.LogWarning($"  ! {objName} collider was a Trigger - disabling for wall collision");
            col.isTrigger = false;
        }

        Debug.Log($"  {objName}: Rigidbody2D=Dynamic, Collider2D.isTrigger=false");
    }
}
