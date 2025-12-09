using UnityEngine;

public class CobraAI : MonoBehaviour
{
    public enum AIType { Chase, Attack, Random, Ambusher, Patroller, PackHunter, Sniper }
    public enum PersonalityTrait { Aggressive, Cautious, Tactical, Erratic }

    [Header("AI Settings")]
    public AIType aiType = AIType.Chase;
    public PersonalityTrait personality = PersonalityTrait.Tactical;
    public float speed = 1.5f;  // Reduced from 2.8 for easy testing - cobras very slow!
    public float size = 0.18f;

    [Header("Personality Modifiers")]
    [Tooltip("Aggressive: Faster but less accurate | Cautious: Slower but more calculated")]
    public bool usePersonalityModifiers = true;

    [Header("Attack AI Settings")]
    public float predictionMultiplier = 12f;
    public float closeRangeDistance = 0.8f; // Unity units
    public float boostMultiplier = 0.8f;

    [Header("Random AI Settings")]
    public float randomTargetChangeInterval = 1f;

    [Header("Ambusher AI Settings")]
    public float ambushRange = 2f;
    public float hideTime = 2f;
    public float strikeSpeed = 5f;

    [Header("Patroller AI Settings")]
    public float patrolSpeed = 1.5f;
    public float alertRange = 2.5f;
    public float chaseSpeedMultiplier = 1.8f;

    [Header("Pack Hunter AI Settings")]
    public float coordinationRange = 3f;
    public float flankingAngle = 90f;

    [Header("Projectile Settings")]
    public bool canShootProjectiles = false;
    public GameObject projectilePrefab;
    public float fireRate = 2.0f; // Increased from 0.3 - shoot much faster! (2 shots per second)
    public float shootingRange = 5f; // Reduced from 7 - shorter range for testing
    public float minShootingDistance = 2f; // Increased from 1.5 - more safe space
    public Projectile.ProjectileType projectileType = Projectile.ProjectileType.Fire;
    private float fireTimer = 0f;

    [Header("Visual Feedback")]
    public bool showStateColors = true;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color hiddenColor = new Color(1f, 1f, 1f, 0.3f);
    private Color huntingColor = new Color(1f, 0.2f, 0.2f, 1f);
    private Color alertColor = new Color(1f, 0.6f, 0f, 1f);

    [Header("Instant Kill Mode")]
    [HideInInspector] public bool isInstantKillMode = false;  // Set by GameManager for levels 1-3

    [Header("Wall Collision Settings")]
    [Tooltip("Enable wall avoidance using raycasts")]
    public bool useWallAvoidance = true;
    [Tooltip("Distance to check for walls ahead")]
    public float wallCheckDistance = 0.5f;
    [Tooltip("Layer mask for walls (set in Inspector or uses 'Wall' layer)")]
    public LayerMask wallLayer;

    private Transform playerTransform;
    private PlayerController playerController;
    private Vector3 randomTarget;
    private float randomTimer;
    private Vector3 previousPosition;

    // Ambusher state
    private bool isHiding;
    private float hideTimer;
    private Vector3 ambushPosition;

    // Patroller state
    private bool isAlerted;
    private int currentPatrolPoint;
    private Vector3[] patrolPoints;

    // Pack hunter state
    private CobraAI[] allCobras;
    #pragma warning disable CS0414 // Field assigned but never used
    private bool isCoordinating;
    #pragma warning restore CS0414

    // Bounds
    private float boundX = 4f;
    private float boundY = 3f;

    // Wall collision
    private Rigidbody2D rb;

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        previousPosition = transform.position;
        lastPosition = transform.position;
        stuckDirection = Random.value > 0.5f ? 1 : -1; // Randomize initial unstuck direction

        // Get sprite renderer for visual feedback
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Get Rigidbody2D for wall collision
        rb = GetComponent<Rigidbody2D>();
        if (rb != null && useWallAvoidance)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Ensure collider is NOT a trigger for wall collision to work
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && col.isTrigger)
            {
                Debug.LogWarning($"[CobraAI] {gameObject.name} collider is set as Trigger - wall collision won't work! Disabling trigger mode.");
                col.isTrigger = false;
            }
        }

        // Auto-setup wall layer if not set
        if (wallLayer == 0)
        {
            int wallLayerIndex = LayerMask.NameToLayer("Wall");
            if (wallLayerIndex != -1)
            {
                wallLayer = 1 << wallLayerIndex;
            }
        }

        // Initialize based on AI type
        switch (aiType)
        {
            case AIType.Random:
                SetNewRandomTarget();
                break;
            case AIType.Ambusher:
                InitializeAmbusher();
                break;
            case AIType.Patroller:
                InitializePatroller();
                break;
            case AIType.PackHunter:
                InitializePackHunter();
                break;
        }

        // Apply personality modifiers
        ApplyPersonalityModifiers();
    }

    void ApplyPersonalityModifiers()
    {
        if (!usePersonalityModifiers) return;

        switch (personality)
        {
            case PersonalityTrait.Aggressive:
                speed *= 1.2f;
                predictionMultiplier *= 0.8f; // Less accurate prediction
                closeRangeDistance *= 1.3f; // Boost earlier
                break;
            case PersonalityTrait.Cautious:
                speed *= 0.85f;
                predictionMultiplier *= 1.2f; // More accurate prediction
                closeRangeDistance *= 0.7f; // Boost later
                break;
            case PersonalityTrait.Tactical:
                // Balanced - no modifications
                break;
            case PersonalityTrait.Erratic:
                speed *= Random.Range(0.8f, 1.3f); // Random speed variation
                randomTargetChangeInterval *= Random.Range(0.5f, 2f);
                break;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Store previous position
        previousPosition = transform.position;

        // Handle projectile shooting
        HandleProjectileShooting();

        // Execute AI based on type
        switch (aiType)
        {
            case AIType.Chase:
                ChaseAI();
                break;
            case AIType.Attack:
                AttackAI();
                break;
            case AIType.Random:
                RandomAI();
                break;
            case AIType.Ambusher:
                AmbusherAI();
                break;
            case AIType.Patroller:
                PatrollerAI();
                break;
            case AIType.PackHunter:
                PackHunterAI();
                break;
            case AIType.Sniper:
                SniperAI();
                break;
        }
    }

    void HandleProjectileShooting()
    {
        if (!canShootProjectiles || projectilePrefab == null || playerTransform == null) return;

        // Update fire timer
        fireTimer += Time.deltaTime;

        // Check if ready to fire
        float fireInterval = 1f / fireRate;
        if (fireTimer >= fireInterval)
        {
            // Check distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Only shoot if within range AND not too close (for fairness)
            if (distanceToPlayer <= shootingRange && distanceToPlayer >= minShootingDistance)
            {
                ShootProjectile();
                fireTimer = 0f;
            }
        }
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null || playerTransform == null) return;

        // Calculate direction to player
        Vector3 direction = (playerTransform.position - transform.position).normalized;

        // Spawn projectile
        GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile proj = projObj.GetComponent<Projectile>();

        if (proj != null)
        {
            proj.Initialize(direction, projectileType);
        }
    }

    // Stuck detection variables
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private int stuckDirection = 1; // 1 or -1 for alternating unstuck direction
    private float unstuckForceTimer = 0f; // Timer to force movement when severely stuck
    private float pathfindTimer = 0f; // Timer for pathfinding around obstacles
    private Vector3 pathfindTarget; // Temporary target when navigating around walls
    private bool isPathfinding = false; // Currently navigating around an obstacle
    private Vector3 pathfindDirection; // Direction to follow when pathfinding
    private float pathfindDuration = 0f; // How long to follow current path direction

    void ChaseAI()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 directToPlayer = toPlayer.normalized;

        // Check if stuck (not moving much)
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        if (movementDelta < 0.008f) // Slightly higher threshold
        {
            stuckTimer += Time.deltaTime;
            unstuckForceTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
            unstuckForceTimer = 0f;
            // If we're moving well, we can stop pathfinding
            if (movementDelta > 0.02f)
            {
                isPathfinding = false;
            }
        }
        lastPosition = transform.position;

        // Decrease pathfind duration
        if (pathfindDuration > 0)
        {
            pathfindDuration -= Time.deltaTime;
        }

        // If severely stuck for too long, find open space and nudge there
        if (unstuckForceTimer > 1.2f)
        {
            Vector3 escapeDir = FindOpenDirection();
            if (escapeDir != Vector3.zero)
            {
                // Start pathfinding in this direction
                isPathfinding = true;
                pathfindDirection = escapeDir;
                pathfindDuration = 0.8f; // Follow this direction for a bit
                transform.position += escapeDir * 0.3f; // Small nudge
            }
            else
            {
                // Last resort - random offset
                Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
                transform.position += randomOffset;
            }
            unstuckForceTimer = 0f;
            stuckTimer = 0f;
            return;
        }

        // Determine movement direction
        Vector3 moveDirection;

        // Check if direct path to player is blocked
        bool directPathBlocked = IsPathBlocked(directToPlayer);

        if (isPathfinding && pathfindDuration > 0)
        {
            // Continue following pathfind direction
            moveDirection = pathfindDirection;

            // But check if we can now see the player directly
            if (!directPathBlocked)
            {
                isPathfinding = false;
                moveDirection = directToPlayer;
            }
        }
        else if (directPathBlocked || stuckTimer > 0.15f)
        {
            // Need to find a way around
            moveDirection = FindPathAroundWall(directToPlayer);

            // If we found a good alternate path, commit to it briefly
            if (moveDirection != directToPlayer)
            {
                isPathfinding = true;
                pathfindDirection = moveDirection;
                pathfindDuration = 0.5f; // Follow this direction for half a second
            }
        }
        else
        {
            // Direct path is clear
            moveDirection = directToPlayer;
        }

        MoveWithWallAvoidance(moveDirection, speed);
    }

    /// <summary>
    /// Check if the path in a direction is blocked by a wall
    /// </summary>
    bool IsPathBlocked(Vector3 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance * 1.2f, wallLayer);
        return hit.collider != null;
    }

    /// <summary>
    /// Find a path around a wall to reach the player
    /// </summary>
    Vector3 FindPathAroundWall(Vector3 toPlayer)
    {
        // Check perpendicular directions first (go around the wall)
        Vector3 perpLeft = new Vector3(-toPlayer.y, toPlayer.x, 0f).normalized;
        Vector3 perpRight = new Vector3(toPlayer.y, -toPlayer.x, 0f).normalized;

        // Score each perpendicular direction
        float leftScore = ScoreDirection(perpLeft, toPlayer);
        float rightScore = ScoreDirection(perpRight, toPlayer);

        // Also check diagonal directions (45 degrees from perpendicular)
        Vector3 diagLeft = (toPlayer + perpLeft).normalized;
        Vector3 diagRight = (toPlayer + perpRight).normalized;
        float diagLeftScore = ScoreDirection(diagLeft, toPlayer);
        float diagRightScore = ScoreDirection(diagRight, toPlayer);

        // Find the best scoring direction
        Vector3 bestDir = toPlayer;
        float bestScore = -999f;

        if (leftScore > bestScore && leftScore > 0) { bestScore = leftScore; bestDir = perpLeft; }
        if (rightScore > bestScore && rightScore > 0) { bestScore = rightScore; bestDir = perpRight; }
        if (diagLeftScore > bestScore && diagLeftScore > 0) { bestScore = diagLeftScore; bestDir = diagLeft; }
        if (diagRightScore > bestScore && diagRightScore > 0) { bestScore = diagRightScore; bestDir = diagRight; }

        // If nothing good found, try the 8-direction search
        if (bestScore <= 0)
        {
            bestDir = GetUnstuckDirection(toPlayer);
        }

        // Alternate direction preference to avoid oscillation
        if (stuckTimer > 0.3f)
        {
            stuckDirection *= -1;
            stuckTimer = 0.15f;
        }

        return bestDir;
    }

    /// <summary>
    /// Score a direction based on how clear it is and how well it leads toward player
    /// </summary>
    float ScoreDirection(Vector3 direction, Vector3 toPlayer)
    {
        // Check if direction is blocked
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance * 2f, wallLayer);

        if (hit.collider != null)
        {
            return -1f; // Blocked
        }

        // Check how far we can go
        RaycastHit2D farHit = Physics2D.Raycast(transform.position, direction, wallCheckDistance * 4f, wallLayer);
        float clearDistance = farHit.collider != null ? farHit.distance : wallCheckDistance * 4f;

        // Also check if the direction eventually leads toward player
        // by checking if after moving in this direction, we'd be closer to player
        Vector3 futurePos = transform.position + direction * clearDistance * 0.5f;
        Vector3 currentToPlayer = playerTransform.position - transform.position;
        Vector3 futureToPlayer = playerTransform.position - futurePos;

        // Bonus if future position is closer to player
        float proximityBonus = (currentToPlayer.magnitude - futureToPlayer.magnitude) * 2f;

        return clearDistance + proximityBonus;
    }

    /// <summary>
    /// Find the best direction to move when stuck
    /// </summary>
    Vector3 GetUnstuckDirection(Vector3 originalDirection)
    {
        // Try 8 directions and find the best one that leads toward player
        float[] angles = { 0f, 45f, -45f, 90f, -90f, 135f, -135f, 180f };
        Vector3 bestDir = originalDirection;
        float bestScore = -999f;

        Vector3 toPlayer = (playerTransform.position - transform.position).normalized;

        foreach (float angle in angles)
        {
            Vector3 testDir = Quaternion.Euler(0, 0, angle) * originalDirection;

            // Check if this direction is clear
            RaycastHit2D hit = Physics2D.Raycast(transform.position, testDir, wallCheckDistance * 1.5f, wallLayer);

            if (hit.collider == null)
            {
                // Direction is clear - score based on how much it leads toward player
                float dotToPlayer = Vector3.Dot(testDir, toPlayer);
                float score = dotToPlayer + (1f - Mathf.Abs(angle) / 180f) * 0.5f; // Prefer smaller angles

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = testDir;
                }
            }
        }

        // Alternate stuck direction for next time
        if (stuckTimer > 0.3f)
        {
            stuckDirection *= -1;
            stuckTimer = 0.1f;
        }

        return bestDir.normalized;
    }

    /// <summary>
    /// Find an open direction when completely stuck
    /// </summary>
    Vector3 FindOpenDirection()
    {
        // Check 8 directions
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, wallCheckDistance * 2f, wallLayer);

            if (hit.collider == null)
            {
                return dir;
            }
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Moves the cobra in the given direction, avoiding walls if enabled.
    /// Uses wall sliding for smooth movement through narrow passages.
    /// </summary>
    private void MoveWithWallAvoidance(Vector3 direction, float moveSpeed)
    {
        if (useWallAvoidance && wallLayer != 0)
        {
            // Check multiple rays for better wall detection (more directions)
            float[] rayAngles = { 0f, 30f, -30f, 60f, -60f, 90f, -90f };
            Vector3 bestDirection = direction;
            float bestScore = -999f;
            bool forwardBlocked = false;

            // Get direction to player for scoring
            Vector3 toPlayer = playerTransform != null ?
                (playerTransform.position - transform.position).normalized : direction;

            foreach (float angle in rayAngles)
            {
                Vector3 testDir = Quaternion.Euler(0, 0, angle) * direction;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, testDir, wallCheckDistance, wallLayer);

                if (angle == 0f && hit.collider != null)
                {
                    forwardBlocked = true;
                }

                if (hit.collider == null)
                {
                    // Direction is clear - check how far we can go
                    RaycastHit2D farHit = Physics2D.Raycast(transform.position, testDir, wallCheckDistance * 3f, wallLayer);
                    float clearDistance = farHit.collider != null ? farHit.distance : wallCheckDistance * 3f;

                    // Score = distance * alignment with player direction
                    float dotToPlayer = Vector3.Dot(testDir, toPlayer);
                    float angleBonus = 1f - (Mathf.Abs(angle) / 90f) * 0.3f; // Small penalty for big angles
                    float score = clearDistance * (0.5f + dotToPlayer * 0.5f) * angleBonus;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDirection = testDir;
                    }
                }
            }

            // If forward is blocked, use the best alternative
            if (forwardBlocked && bestScore > -999f)
            {
                direction = bestDirection.normalized;
            }
            else if (forwardBlocked)
            {
                // All directions blocked - try wall sliding
                RaycastHit2D directHit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);
                if (directHit.collider != null)
                {
                    Vector3 wallNormal = directHit.normal;
                    Vector3 slideDir = direction - Vector3.Dot(direction, wallNormal) * (Vector3)wallNormal;

                    if (slideDir.magnitude > 0.1f)
                    {
                        direction = slideDir.normalized;
                    }
                    else
                    {
                        // Last resort - perpendicular
                        direction = new Vector3(-direction.y, direction.x, 0f) * stuckDirection;
                    }
                }
            }
        }

        // Apply movement
        if (rb != null && useWallAvoidance)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    void AttackAI()
    {
        // Advanced Predator AI - predicts player movement and intercepts
        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 directToPlayer = toPlayer.normalized;

        // Check if stuck
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        if (movementDelta < 0.008f)
        {
            stuckTimer += Time.deltaTime;
            unstuckForceTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
            unstuckForceTimer = 0f;
            if (movementDelta > 0.02f) isPathfinding = false;
        }
        lastPosition = transform.position;

        // Decrease pathfind duration
        if (pathfindDuration > 0) pathfindDuration -= Time.deltaTime;

        // If severely stuck, nudge toward open space
        if (unstuckForceTimer > 1.2f)
        {
            Vector3 escapeDir = FindOpenDirection();
            if (escapeDir != Vector3.zero)
            {
                isPathfinding = true;
                pathfindDirection = escapeDir;
                pathfindDuration = 0.8f;
                transform.position += escapeDir * 0.3f;
            }
            unstuckForceTimer = 0f;
            stuckTimer = 0f;
            return;
        }

        // Determine movement direction with smart pathfinding
        Vector3 direction;
        bool directPathBlocked = IsPathBlocked(directToPlayer);

        if (isPathfinding && pathfindDuration > 0)
        {
            direction = pathfindDirection;
            if (!directPathBlocked)
            {
                isPathfinding = false;
                direction = directToPlayer;
            }
        }
        else if (directPathBlocked || stuckTimer > 0.15f)
        {
            direction = FindPathAroundWall(directToPlayer);
            if (direction != directToPlayer)
            {
                isPathfinding = true;
                pathfindDirection = direction;
                pathfindDuration = 0.5f;
            }
        }
        else
        {
            direction = directToPlayer;
        }

        // Predict where player will be
        Vector3 targetPos;
        if (playerController != null && playerController.velocity.magnitude > 0.1f)
        {
            // Player is moving - predict future position
            float predictionTime = Mathf.Min(distance / speed, 1.5f);
            targetPos = playerTransform.position + playerController.velocity * predictionTime * predictionMultiplier;
        }
        else
        {
            // Player stationary - go directly toward them
            targetPos = playerTransform.position;
        }

        // Keep target in bounds
        targetPos.x = Mathf.Clamp(targetPos.x, -boundX + 0.3f, boundX - 0.3f);
        targetPos.y = Mathf.Clamp(targetPos.y, -boundY + 0.3f, boundY - 0.3f);

        // Move toward predicted position
        direction = (targetPos - transform.position).normalized;

        // Speed boost when close
        float currentSpeed = speed;
        if (distance < closeRangeDistance)
        {
            currentSpeed = speed * (1f + boostMultiplier);
        }

        MoveWithWallAvoidance(direction, currentSpeed);
    }

    void RandomAI()
    {
        // Random movement - picks random points
        randomTimer += Time.deltaTime;

        if (randomTimer >= randomTargetChangeInterval)
        {
            SetNewRandomTarget();
            randomTimer = 0f;
        }

        // Move toward random target
        Vector3 direction = (randomTarget - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, randomTarget);

        if (distanceToTarget > 0.05f)
        {
            MoveWithWallAvoidance(direction, speed);
        }
    }

    void SetNewRandomTarget()
    {
        randomTarget = new Vector3(
            Random.Range(-boundX + 0.5f, boundX - 0.5f),
            Random.Range(-boundY + 0.5f, boundY - 0.5f),
            0f
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerCollision(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    void HandlePlayerCollision(GameObject other)
    {
        if (other.CompareTag("Player") && isInstantKillMode)
        {
            // Simple instant kill - no shield check for Level 0.2
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log("Cobra caught player! Instant kill!");
                player.Die();

                // LEVEL 0.2: Trigger snake growth on player kill (if SnakeGrowthTrigger exists)
                SnakeGrowthTrigger growthTrigger = GetComponent<SnakeGrowthTrigger>();
                if (growthTrigger != null)
                {
                    growthTrigger.OnPlayerKilled();
                }
            }
        }
    }

    // ===== NEW AI BEHAVIORS =====

    #region Ambusher AI
    void InitializeAmbusher()
    {
        isHiding = true;
        hideTimer = 0f;
        ambushPosition = transform.position;
        SetVisualState(hiddenColor);
    }

    void AmbusherAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (isHiding)
        {
            // Wait in hiding until player gets close
            hideTimer += Time.deltaTime;

            // Subtle movement while hiding (slight drift)
            if (hideTimer < hideTime)
            {
                float drift = Mathf.Sin(Time.time * 2f) * 0.2f;
                Vector3 driftPos = ambushPosition + new Vector3(drift, drift * 0.5f, 0f);
                transform.position = Vector3.Lerp(transform.position, driftPos, Time.deltaTime * 0.5f);
            }

            // Strike when player is in range
            if (distanceToPlayer < ambushRange)
            {
                isHiding = false;
                SetVisualState(huntingColor);
            }

            // If hiding too long, relocate
            if (hideTimer > hideTime * 2f)
            {
                ambushPosition = new Vector3(
                    Random.Range(-boundX + 0.5f, boundX - 0.5f),
                    Random.Range(-boundY + 0.5f, boundY - 0.5f),
                    0f
                );
                hideTimer = 0f;
            }
        }
        else
        {
            // Aggressive strike at player
            Vector3 strikeDirection = (playerTransform.position - transform.position).normalized;
            MoveWithWallAvoidance(strikeDirection, strikeSpeed);

            // Return to hiding if player escapes
            if (distanceToPlayer > ambushRange * 2f)
            {
                isHiding = true;
                hideTimer = 0f;
                ambushPosition = transform.position;
                SetVisualState(hiddenColor);
            }
        }
    }
    #endregion

    #region Patroller AI
    void InitializePatroller()
    {
        isAlerted = false;
        currentPatrolPoint = 0;

        // Create patrol route - rectangular patrol
        patrolPoints = new Vector3[4];
        patrolPoints[0] = new Vector3(-2f, 2f, 0f);
        patrolPoints[1] = new Vector3(2f, 2f, 0f);
        patrolPoints[2] = new Vector3(2f, -2f, 0f);
        patrolPoints[3] = new Vector3(-2f, -2f, 0f);
    }

    void PatrollerAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Check if player is in alert range
        if (distanceToPlayer < alertRange)
        {
            if (!isAlerted)
            {
                isAlerted = true;
                SetVisualState(alertColor);
            }
        }
        else
        {
            if (isAlerted)
            {
                isAlerted = false;
                SetVisualState(originalColor);
            }
        }

        if (isAlerted)
        {
            // Aggressive chase when alerted
            Vector3 chaseDirection = (playerTransform.position - transform.position).normalized;
            float chaseSpeed = speed * chaseSpeedMultiplier;
            MoveWithWallAvoidance(chaseDirection, chaseSpeed);
        }
        else
        {
            // Patrol the route
            Vector3 targetPoint = patrolPoints[currentPatrolPoint];
            Vector3 patrolDirection = (targetPoint - transform.position).normalized;
            float distanceToPoint = Vector3.Distance(transform.position, targetPoint);

            if (distanceToPoint < 0.2f)
            {
                // Move to next patrol point
                currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
            }

            MoveWithWallAvoidance(patrolDirection, patrolSpeed);
        }
    }
    #endregion

    #region Pack Hunter AI
    void InitializePackHunter()
    {
        // Find all cobras in the scene
        allCobras = Object.FindObjectsByType<CobraAI>(FindObjectsSortMode.None);
        isCoordinating = false;
    }

    void PackHunterAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Check if other cobras are nearby for coordination
        CobraAI nearestAlly = FindNearestCobra();

        if (nearestAlly != null)
        {
            float distanceToAlly = Vector3.Distance(transform.position, nearestAlly.transform.position);

            if (distanceToAlly < coordinationRange)
            {
                isCoordinating = true;
                SetVisualState(huntingColor);

                // Coordinated flanking attack
                PerformFlankingManeuver(nearestAlly);
            }
            else
            {
                isCoordinating = false;
                SetVisualState(originalColor);

                // Move toward ally to coordinate
                Vector3 toAlly = (nearestAlly.transform.position - transform.position).normalized;
                Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
                Vector3 balancedDirection = (toAlly + toPlayer * 2f).normalized;

                MoveWithWallAvoidance(balancedDirection, speed);
            }
        }
        else
        {
            // No allies nearby - use basic chase
            ChaseAI();
        }
    }

    void PerformFlankingManeuver(CobraAI ally)
    {
        // Calculate flanking position
        Vector3 playerToAlly = (ally.transform.position - playerTransform.position).normalized;

        // Create perpendicular flanking angle
        float angleRad = flankingAngle * Mathf.Deg2Rad;
        Vector3 flankDirection = new Vector3(
            playerToAlly.x * Mathf.Cos(angleRad) - playerToAlly.y * Mathf.Sin(angleRad),
            playerToAlly.x * Mathf.Sin(angleRad) + playerToAlly.y * Mathf.Cos(angleRad),
            0f
        );

        Vector3 flankTarget = playerTransform.position + flankDirection * 1.5f;

        // Keep flank target in bounds
        flankTarget.x = Mathf.Clamp(flankTarget.x, -boundX + 0.5f, boundX - 0.5f);
        flankTarget.y = Mathf.Clamp(flankTarget.y, -boundY + 0.5f, boundY - 0.5f);

        // Move toward flanking position
        Vector3 moveDirection = (flankTarget - transform.position).normalized;
        MoveWithWallAvoidance(moveDirection, speed);
    }

    CobraAI FindNearestCobra()
    {
        CobraAI nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (CobraAI cobra in allCobras)
        {
            if (cobra == this || cobra == null) continue;

            float distance = Vector3.Distance(transform.position, cobra.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = cobra;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Sniper AI - Keeps distance from player and focuses on shooting.
    /// Perfect for poison snakes that want to apply DoT from range.
    /// </summary>
    void SniperAI()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
        Vector3 awayFromPlayer = -toPlayer;

        // Check if stuck
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        if (movementDelta < 0.005f)
        {
            stuckTimer += Time.deltaTime;
            unstuckForceTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
            unstuckForceTimer = 0f;
        }
        lastPosition = transform.position;

        // If severely stuck, find open space
        if (unstuckForceTimer > 1.5f)
        {
            Vector3 escapeDir = FindOpenDirection();
            if (escapeDir != Vector3.zero)
            {
                transform.position += escapeDir * 0.5f;
            }
            else
            {
                Vector3 randomOffset = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f), 0f);
                transform.position += randomOffset;
            }
            unstuckForceTimer = 0f;
            stuckTimer = 0f;
            return;
        }

        // Ideal range: between minShootingDistance and shootingRange
        float idealMinDistance = minShootingDistance + 0.5f;
        float idealMaxDistance = shootingRange - 1f;

        Vector3 moveDirection = Vector3.zero;

        if (distanceToPlayer < idealMinDistance)
        {
            // Too close - back away while strafing
            Vector3 strafe = new Vector3(-toPlayer.y, toPlayer.x, 0f) * (Mathf.Sin(Time.time * 2f) * 0.5f);
            moveDirection = (awayFromPlayer + strafe).normalized;
            SetVisualState(alertColor); // Orange = retreating

            // If stuck while retreating, use smart pathfinding
            if (stuckTimer > 0.1f)
            {
                moveDirection = GetUnstuckDirection(awayFromPlayer);
            }
        }
        else if (distanceToPlayer > idealMaxDistance)
        {
            // Too far - move closer but cautiously
            moveDirection = toPlayer * 0.7f;
            SetVisualState(originalColor);

            // Handle stuck while approaching - use smart pathfinding
            if (stuckTimer > 0.1f)
            {
                moveDirection = GetUnstuckDirection(toPlayer);
            }
        }
        else
        {
            // In ideal range - strafe to make it harder to dodge projectiles
            float strafeSpeed = 1.5f;
            float strafeDirection = Mathf.Sin(Time.time * strafeSpeed + GetInstanceID());
            moveDirection = new Vector3(-toPlayer.y, toPlayer.x, 0f) * strafeDirection;
            SetVisualState(huntingColor); // Red = attacking

            // Check if strafe direction is blocked
            if (stuckTimer > 0.1f)
            {
                // Try opposite strafe direction
                moveDirection = -moveDirection;
                stuckTimer = 0f;
            }
        }

        // Apply movement with wall avoidance
        if (moveDirection.magnitude > 0.1f)
        {
            MoveWithWallAvoidance(moveDirection, speed * 0.8f); // Slightly slower for tactical movement
        }
    }
    #endregion

    #region Visual Feedback
    void SetVisualState(Color targetColor)
    {
        if (showStateColors && spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }
    }
    #endregion

    #region Difficulty Scaling
    private float baseSpeed;
    private float basePredictionMultiplier;
    private float baseAlertRange;
    private bool difficultyInitialized = false;

    public void ApplyDifficultyScaling(float speedMult, float predictionMult, float alertRangeMult)
    {
        // Store base values on first call
        if (!difficultyInitialized)
        {
            baseSpeed = speed;
            basePredictionMultiplier = predictionMultiplier;
            baseAlertRange = alertRange;
            difficultyInitialized = true;
        }

        // Apply multipliers to base values
        speed = baseSpeed * speedMult;
        predictionMultiplier = basePredictionMultiplier * predictionMult;
        alertRange = baseAlertRange * alertRangeMult;
    }
    #endregion
}
