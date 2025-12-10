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
    [Tooltip("Distance to check for walls ahead - higher = earlier detection")]
    public float wallCheckDistance = 1.0f; // Increased for earlier wall detection
    [Tooltip("Layer mask for walls (set in Inspector or uses 'Wall' layer)")]
    public LayerMask wallLayer;
    [Tooltip("How smoothly the snake turns around corners (higher = smoother)")]
    [Range(5f, 20f)]
    public float cornerSmoothness = 12f; // Increased for faster response

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

    // ===== ADVANCED PATHFINDING SYSTEM =====
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private int stuckDirection = 1; // 1 or -1 for alternating unstuck direction
    private float unstuckForceTimer = 0f;

    // Smart pathfinding state
    private bool isNavigatingCorner = false;
    private Vector3 cornerDirection; // Direction to follow around corner
    private float cornerNavigationTimer = 0f;
    private float cornerCommitTime = 0.6f; // How long to commit to a corner direction

    // Predictive corner detection
    private bool cornerAhead = false;
    private float lastCornerCheckTime = 0f;
    private float cornerCheckInterval = 0.1f; // Check for corners every 0.1s

    void ChaseAI()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 directToPlayer = toPlayer.normalized;

        // Track movement for stuck detection
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        UpdateStuckState(movementDelta);
        lastPosition = transform.position;

        // Update corner navigation timer
        if (cornerNavigationTimer > 0)
        {
            cornerNavigationTimer -= Time.deltaTime;
        }

        // Emergency unstuck
        if (unstuckForceTimer > 1.0f)
        {
            EmergencyUnstuck();
            return;
        }

        // Get the best movement direction using smart pathfinding
        Vector3 moveDirection = GetSmartDirection(directToPlayer, distance);

        MoveWithWallAvoidance(moveDirection, speed);
    }

    /// <summary>
    /// Update stuck detection state
    /// </summary>
    void UpdateStuckState(float movementDelta)
    {
        if (movementDelta < 0.006f)
        {
            stuckTimer += Time.deltaTime;
            unstuckForceTimer += Time.deltaTime;
        }
        else
        {
            // Moving well
            if (movementDelta > 0.015f)
            {
                stuckTimer = 0f;
                unstuckForceTimer = 0f;

                // If moving well and direct path is clear, stop corner navigation
                if (!isNavigatingCorner || cornerNavigationTimer <= 0)
                {
                    isNavigatingCorner = false;
                }
            }
            else
            {
                // Moving slowly - reduce timers but don't reset
                stuckTimer = Mathf.Max(0, stuckTimer - Time.deltaTime * 0.5f);
                unstuckForceTimer = Mathf.Max(0, unstuckForceTimer - Time.deltaTime * 0.5f);
            }
        }
    }

    /// <summary>
    /// Emergency escape when severely stuck - no teleporting, just change direction
    /// </summary>
    void EmergencyUnstuck()
    {
        Vector3 escapeDir = FindBestEscapeDirection();
        if (escapeDir != Vector3.zero)
        {
            isNavigatingCorner = true;
            cornerDirection = escapeDir;
            cornerNavigationTimer = cornerCommitTime * 1.5f; // Longer commit to escape

            // DON'T teleport - just set velocity if using rigidbody
            if (rb != null && useWallAvoidance)
            {
                rb.linearVelocity = escapeDir * speed * 1.2f; // Slight boost to escape
            }
            // Reset smoothed direction to escape direction
            smoothedDirection = escapeDir;
        }
        unstuckForceTimer = 0f;
        stuckTimer = 0f;
    }

    /// <summary>
    /// Get the smartest direction to move considering walls and corners
    /// </summary>
    Vector3 GetSmartDirection(Vector3 directToPlayer, float distanceToPlayer)
    {
        // If we're committed to navigating a corner, continue that direction
        if (isNavigatingCorner && cornerNavigationTimer > 0)
        {
            // Check if the corner direction is still valid
            if (!IsPathBlocked(cornerDirection))
            {
                // Also check if we can now see the player directly
                if (!IsPathBlocked(directToPlayer))
                {
                    // Path to player is clear - stop corner navigation
                    isNavigatingCorner = false;
                    return directToPlayer;
                }
                return cornerDirection;
            }
            else
            {
                // Corner direction blocked - need new direction
                isNavigatingCorner = false;
            }
        }

        // Check if direct path to player is clear
        bool directBlocked = IsPathBlocked(directToPlayer);

        // Predictive corner detection - look ahead for walls
        bool cornerApproaching = DetectCornerAhead(directToPlayer);

        if (!directBlocked && !cornerApproaching)
        {
            // Clear path to player - go direct
            return directToPlayer;
        }

        // Need to navigate around obstacle
        Vector3 bestDirection = FindBestCornerDirection(directToPlayer);

        // Commit to this direction for smooth navigation
        if (bestDirection != directToPlayer)
        {
            isNavigatingCorner = true;
            cornerDirection = bestDirection;
            cornerNavigationTimer = cornerCommitTime;
        }

        return bestDirection;
    }

    /// <summary>
    /// Detect if there's a corner/wall approaching in our path
    /// </summary>
    bool DetectCornerAhead(Vector3 direction)
    {
        // Check at extended distance for early corner detection
        float lookAheadDistance = wallCheckDistance * 2.5f;

        RaycastHit2D centerHit = Physics2D.Raycast(transform.position, direction, lookAheadDistance, wallLayer);

        if (centerHit.collider != null)
        {
            // Wall ahead - check if it's close enough to start turning
            return centerHit.distance < wallCheckDistance * 1.8f;
        }

        // Also check slightly to the sides for corner edges
        Vector3 leftDir = Quaternion.Euler(0, 0, 25f) * direction;
        Vector3 rightDir = Quaternion.Euler(0, 0, -25f) * direction;

        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, leftDir, lookAheadDistance * 0.8f, wallLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, rightDir, lookAheadDistance * 0.8f, wallLayer);

        // If one side is blocked but not the other, it's a corner
        if ((leftHit.collider != null && rightHit.collider == null) ||
            (leftHit.collider == null && rightHit.collider != null))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Find the best direction to navigate around a corner/wall
    /// </summary>
    Vector3 FindBestCornerDirection(Vector3 toPlayer)
    {
        // Test many directions and find the best one
        float[] testAngles = { 30f, -30f, 45f, -45f, 60f, -60f, 75f, -75f, 90f, -90f, 120f, -120f };

        Vector3 bestDir = toPlayer;
        float bestScore = -9999f;

        foreach (float angle in testAngles)
        {
            Vector3 testDir = Quaternion.Euler(0, 0, angle) * toPlayer;

            // Check if this direction is clear
            RaycastHit2D nearHit = Physics2D.Raycast(transform.position, testDir, wallCheckDistance, wallLayer);

            if (nearHit.collider == null)
            {
                // Direction is clear - calculate score
                float score = ScoreNavigationDirection(testDir, toPlayer, angle);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = testDir;
                }
            }
        }

        // If no good direction found, try to find any open direction
        if (bestScore < -999f)
        {
            bestDir = FindBestEscapeDirection();
            if (bestDir == Vector3.zero)
            {
                bestDir = toPlayer; // Last resort
            }
        }

        return bestDir.normalized;
    }

    /// <summary>
    /// Score a potential navigation direction
    /// </summary>
    float ScoreNavigationDirection(Vector3 testDir, Vector3 toPlayer, float angle)
    {
        float score = 0f;

        // 1. Check how far we can go in this direction
        RaycastHit2D farHit = Physics2D.Raycast(transform.position, testDir, wallCheckDistance * 5f, wallLayer);
        float clearDistance = farHit.collider != null ? farHit.distance : wallCheckDistance * 5f;
        score += clearDistance * 2f;

        // 2. How well does this direction lead toward player?
        float dotToPlayer = Vector3.Dot(testDir, toPlayer);
        score += dotToPlayer * 3f;

        // 3. Prefer smaller turn angles (more natural movement)
        float anglePenalty = Mathf.Abs(angle) / 180f;
        score -= anglePenalty * 1.5f;

        // 4. Check if going this way would get us closer to player
        Vector3 futurePos = transform.position + testDir * Mathf.Min(clearDistance, 2f);
        float currentDist = Vector3.Distance(transform.position, playerTransform.position);
        float futureDist = Vector3.Distance(futurePos, playerTransform.position);
        float progressBonus = (currentDist - futureDist) * 2f;
        score += progressBonus;

        // 5. Bonus for directions that have long clear paths (good for corridors)
        if (clearDistance > wallCheckDistance * 3f)
        {
            score += 2f;
        }

        // 6. Prefer consistent direction with current corner navigation
        if (isNavigatingCorner && cornerDirection != Vector3.zero)
        {
            float consistency = Vector3.Dot(testDir, cornerDirection);
            score += consistency * 1f;
        }

        return score;
    }

    /// <summary>
    /// Find the best escape direction when stuck
    /// </summary>
    Vector3 FindBestEscapeDirection()
    {
        float[] angles = { 0f, 45f, -45f, 90f, -90f, 135f, -135f, 180f };
        Vector3 bestDir = Vector3.zero;
        float bestClearance = 0f;

        Vector3 toPlayer = playerTransform != null ?
            (playerTransform.position - transform.position).normalized : Vector3.right;

        foreach (float angle in angles)
        {
            Vector3 testDir = Quaternion.Euler(0, 0, angle) * toPlayer;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, testDir, wallCheckDistance * 3f, wallLayer);

            float clearance = hit.collider != null ? hit.distance : wallCheckDistance * 3f;

            // Prefer directions toward player
            float playerBonus = Vector3.Dot(testDir, toPlayer) * 0.5f;
            float totalScore = clearance + playerBonus;

            if (totalScore > bestClearance)
            {
                bestClearance = totalScore;
                bestDir = testDir;
            }
        }

        return bestDir.normalized;
    }

    /// <summary>
    /// Check if the path in a direction is blocked by a wall
    /// </summary>
    bool IsPathBlocked(Vector3 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance * 1.2f, wallLayer);
        return hit.collider != null;
    }

    // Smooth direction for corner navigation
    private Vector3 smoothedDirection = Vector3.zero;

    /// <summary>
    /// Moves the cobra in the given direction, avoiding walls if enabled.
    /// SIMPLE AND RELIABLE - when blocked, slide along wall or turn perpendicular.
    /// </summary>
    private void MoveWithWallAvoidance(Vector3 direction, float moveSpeed)
    {
        if (useWallAvoidance && wallLayer != 0)
        {
            // Simple, reliable wall avoidance
            RaycastHit2D forwardHit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);

            if (forwardHit.collider != null)
            {
                // Wall ahead! Try to slide along it
                Vector3 wallNormal = forwardHit.normal;

                // Calculate slide direction (remove the component going into the wall)
                Vector3 slideDir = direction - Vector3.Dot(direction, wallNormal) * (Vector3)wallNormal;

                if (slideDir.magnitude > 0.1f)
                {
                    // Check if slide direction is clear
                    RaycastHit2D slideHit = Physics2D.Raycast(transform.position, slideDir.normalized, wallCheckDistance * 0.8f, wallLayer);
                    if (slideHit.collider == null)
                    {
                        direction = slideDir.normalized;
                    }
                    else
                    {
                        // Slide blocked - try perpendicular directions
                        direction = TryPerpendicularDirections(direction, wallNormal);
                    }
                }
                else
                {
                    // Moving directly into wall - turn perpendicular
                    direction = TryPerpendicularDirections(direction, wallNormal);
                }
            }
            else
            {
                // Forward is clear, but check slightly ahead for early corner detection
                RaycastHit2D aheadHit = Physics2D.Raycast(transform.position, direction, wallCheckDistance * 2f, wallLayer);
                if (aheadHit.collider != null && aheadHit.distance < wallCheckDistance * 1.5f)
                {
                    // Corner approaching - check which side is more open
                    Vector3 leftDir = Quaternion.Euler(0, 0, 45f) * direction;
                    Vector3 rightDir = Quaternion.Euler(0, 0, -45f) * direction;

                    RaycastHit2D leftHit = Physics2D.Raycast(transform.position, leftDir, wallCheckDistance * 2f, wallLayer);
                    RaycastHit2D rightHit = Physics2D.Raycast(transform.position, rightDir, wallCheckDistance * 2f, wallLayer);

                    float leftClear = leftHit.collider != null ? leftHit.distance : wallCheckDistance * 3f;
                    float rightClear = rightHit.collider != null ? rightHit.distance : wallCheckDistance * 3f;

                    // Bias toward player direction
                    if (playerTransform != null)
                    {
                        Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
                        leftClear += Vector3.Dot(leftDir, toPlayer) * 0.5f;
                        rightClear += Vector3.Dot(rightDir, toPlayer) * 0.5f;
                    }

                    // Start turning toward the more open side
                    if (leftClear > rightClear && leftHit.collider == null)
                    {
                        direction = Vector3.Lerp(direction, leftDir, 0.4f).normalized;
                    }
                    else if (rightHit.collider == null)
                    {
                        direction = Vector3.Lerp(direction, rightDir, 0.4f).normalized;
                    }
                }
            }

            // Smooth direction changes (but faster than before)
            if (smoothedDirection == Vector3.zero)
            {
                smoothedDirection = direction;
            }
            else
            {
                // Faster smoothing for more responsive turning
                float smoothSpeed = cornerSmoothness * 1.5f;
                smoothedDirection = Vector3.Lerp(smoothedDirection, direction, smoothSpeed * Time.deltaTime);

                // Safety check - don't smooth into walls
                RaycastHit2D smoothCheck = Physics2D.Raycast(transform.position, smoothedDirection, wallCheckDistance * 0.5f, wallLayer);
                if (smoothCheck.collider != null)
                {
                    smoothedDirection = direction;
                }
            }

            direction = smoothedDirection.normalized;
        }

        // Apply movement
        if (rb != null && useWallAvoidance)
        {
            Vector3 targetVelocity = direction * moveSpeed;

            // Faster velocity transitions for more responsive movement
            Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 15f * Time.deltaTime);

            rb.linearVelocity = newVelocity;
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Try perpendicular directions when sliding fails
    /// </summary>
    Vector3 TryPerpendicularDirections(Vector3 originalDir, Vector3 wallNormal)
    {
        // Get perpendicular directions to the wall
        Vector3 perp1 = new Vector3(-wallNormal.y, wallNormal.x, 0f);
        Vector3 perp2 = new Vector3(wallNormal.y, -wallNormal.x, 0f);

        // Check which perpendicular is clear
        RaycastHit2D hit1 = Physics2D.Raycast(transform.position, perp1, wallCheckDistance, wallLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, perp2, wallCheckDistance, wallLayer);

        bool perp1Clear = hit1.collider == null;
        bool perp2Clear = hit2.collider == null;

        // Prefer direction toward player
        if (playerTransform != null)
        {
            Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
            float dot1 = Vector3.Dot(perp1, toPlayer);
            float dot2 = Vector3.Dot(perp2, toPlayer);

            if (perp1Clear && perp2Clear)
            {
                return dot1 > dot2 ? perp1 : perp2;
            }
            else if (perp1Clear)
            {
                return perp1;
            }
            else if (perp2Clear)
            {
                return perp2;
            }
        }
        else
        {
            if (perp1Clear) return perp1;
            if (perp2Clear) return perp2;
        }

        // Both blocked - try diagonal away from wall
        Vector3 awayFromWall = wallNormal;
        RaycastHit2D awayHit = Physics2D.Raycast(transform.position, awayFromWall, wallCheckDistance * 0.5f, wallLayer);
        if (awayHit.collider == null)
        {
            return awayFromWall;
        }

        // Last resort - alternate direction
        stuckDirection *= -1;
        return stuckDirection > 0 ? perp1 : perp2;
    }

    void AttackAI()
    {
        // Advanced Predator AI - predicts player movement and intercepts
        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;

        // Track movement for stuck detection
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        UpdateStuckState(movementDelta);
        lastPosition = transform.position;

        // Update corner navigation timer
        if (cornerNavigationTimer > 0)
        {
            cornerNavigationTimer -= Time.deltaTime;
        }

        // Emergency unstuck
        if (unstuckForceTimer > 1.0f)
        {
            EmergencyUnstuck();
            return;
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

        // Get direction to predicted position
        Vector3 directToTarget = (targetPos - transform.position).normalized;

        // Get the best movement direction using smart pathfinding
        Vector3 moveDirection = GetSmartDirection(directToTarget, distance);

        // Speed boost when close
        float currentSpeed = speed;
        if (distance < closeRangeDistance)
        {
            currentSpeed = speed * (1f + boostMultiplier);
        }

        MoveWithWallAvoidance(moveDirection, currentSpeed);
    }

    void RandomAI()
    {
        // Random movement - picks random points
        randomTimer += Time.deltaTime;

        // Track movement for stuck detection
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        UpdateStuckState(movementDelta);
        lastPosition = transform.position;

        // Update corner navigation timer
        if (cornerNavigationTimer > 0)
        {
            cornerNavigationTimer -= Time.deltaTime;
        }

        // Emergency unstuck
        if (unstuckForceTimer > 1.0f)
        {
            EmergencyUnstuck();
            SetNewRandomTarget(); // Pick a new target after unstuck
            return;
        }

        if (randomTimer >= randomTargetChangeInterval)
        {
            SetNewRandomTarget();
            randomTimer = 0f;
        }

        // Move toward random target using smart pathfinding
        float distanceToTarget = Vector3.Distance(transform.position, randomTarget);
        Vector3 directToTarget = (randomTarget - transform.position).normalized;

        if (distanceToTarget > 0.05f)
        {
            Vector3 moveDirection = GetSmartDirection(directToTarget, distanceToTarget);
            MoveWithWallAvoidance(moveDirection, speed);
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
            // Track movement for stuck detection
            float movementDelta = Vector3.Distance(transform.position, lastPosition);
            UpdateStuckState(movementDelta);
            lastPosition = transform.position;

            // Update corner navigation timer
            if (cornerNavigationTimer > 0)
            {
                cornerNavigationTimer -= Time.deltaTime;
            }

            // Emergency unstuck
            if (unstuckForceTimer > 1.0f)
            {
                EmergencyUnstuck();
                return;
            }

            // AGGRESSIVE strike at player - Ambusher should always go toward player
            Vector3 directToPlayer = (playerTransform.position - transform.position).normalized;

            // Check if direct path is blocked
            bool directBlocked = IsPathBlocked(directToPlayer);

            Vector3 strikeDirection;
            if (!directBlocked)
            {
                // Direct path is clear - GO STRAIGHT TO PLAYER (ignore corner navigation)
                strikeDirection = directToPlayer;
                isNavigatingCorner = false; // Reset corner navigation
            }
            else
            {
                // Path blocked - use smart direction but with shorter corner commit
                strikeDirection = GetSmartDirection(directToPlayer, distanceToPlayer);

                // For ambusher, reduce corner navigation time to be more aggressive
                if (isNavigatingCorner)
                {
                    cornerNavigationTimer = Mathf.Min(cornerNavigationTimer, 0.3f);
                }
            }

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

        // Track movement for stuck detection
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        UpdateStuckState(movementDelta);
        lastPosition = transform.position;

        // Update corner navigation timer
        if (cornerNavigationTimer > 0)
        {
            cornerNavigationTimer -= Time.deltaTime;
        }

        // Emergency unstuck
        if (unstuckForceTimer > 1.0f)
        {
            EmergencyUnstuck();
            return;
        }

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
            // Aggressive chase when alerted - use smart pathfinding
            Vector3 directToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 chaseDirection = GetSmartDirection(directToPlayer, distanceToPlayer);
            float chaseSpeed = speed * chaseSpeedMultiplier;
            MoveWithWallAvoidance(chaseDirection, chaseSpeed);
        }
        else
        {
            // Patrol the route with smart pathfinding
            Vector3 targetPoint = patrolPoints[currentPatrolPoint];
            Vector3 directToPoint = (targetPoint - transform.position).normalized;
            float distanceToPoint = Vector3.Distance(transform.position, targetPoint);

            if (distanceToPoint < 0.2f)
            {
                // Move to next patrol point
                currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
            }

            // Use smart direction for patrol as well
            Vector3 patrolDirection = GetSmartDirection(directToPoint, distanceToPoint);
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

        // Track movement for stuck detection
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        UpdateStuckState(movementDelta);
        lastPosition = transform.position;

        // Update corner navigation timer
        if (cornerNavigationTimer > 0)
        {
            cornerNavigationTimer -= Time.deltaTime;
        }

        // Emergency unstuck
        if (unstuckForceTimer > 1.0f)
        {
            EmergencyUnstuck();
            return;
        }

        // Check if other cobras are nearby for coordination
        CobraAI nearestAlly = FindNearestCobra();

        if (nearestAlly != null)
        {
            float distanceToAlly = Vector3.Distance(transform.position, nearestAlly.transform.position);

            if (distanceToAlly < coordinationRange)
            {
                isCoordinating = true;
                SetVisualState(huntingColor);

                // Coordinated flanking attack with smart pathfinding
                PerformFlankingManeuver(nearestAlly);
            }
            else
            {
                isCoordinating = false;
                SetVisualState(originalColor);

                // Move toward ally to coordinate using smart pathfinding
                Vector3 toAlly = (nearestAlly.transform.position - transform.position).normalized;
                Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
                Vector3 balancedDirection = (toAlly + toPlayer * 2f).normalized;

                // Use smart direction
                Vector3 moveDirection = GetSmartDirection(balancedDirection, distanceToPlayer);
                MoveWithWallAvoidance(moveDirection, speed);
            }
        }
        else
        {
            // No allies nearby - use basic chase (already uses smart pathfinding)
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

        // Move toward flanking position using smart pathfinding
        float distanceToTarget = Vector3.Distance(transform.position, flankTarget);
        Vector3 directToTarget = (flankTarget - transform.position).normalized;
        Vector3 moveDirection = GetSmartDirection(directToTarget, distanceToTarget);
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

        // Track movement for stuck detection
        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        UpdateStuckState(movementDelta);
        lastPosition = transform.position;

        // Update corner navigation timer
        if (cornerNavigationTimer > 0)
        {
            cornerNavigationTimer -= Time.deltaTime;
        }

        // Emergency unstuck
        if (unstuckForceTimer > 1.0f)
        {
            EmergencyUnstuck();
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
            Vector3 retreatDir = (awayFromPlayer + strafe).normalized;

            // Use smart pathfinding for retreat
            moveDirection = GetSmartDirection(retreatDir, distanceToPlayer);
            SetVisualState(alertColor); // Orange = retreating
        }
        else if (distanceToPlayer > idealMaxDistance)
        {
            // Too far - move closer using smart pathfinding
            moveDirection = GetSmartDirection(toPlayer, distanceToPlayer);
            SetVisualState(originalColor);
        }
        else
        {
            // In ideal range - strafe to make it harder to dodge projectiles
            float strafeSpeed = 1.5f;
            float strafeDirection = Mathf.Sin(Time.time * strafeSpeed + GetInstanceID());
            Vector3 strafeDir = new Vector3(-toPlayer.y, toPlayer.x, 0f) * strafeDirection;

            // Use smart pathfinding for strafing
            moveDirection = GetSmartDirection(strafeDir.normalized, distanceToPlayer);
            SetVisualState(huntingColor); // Red = attacking
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
