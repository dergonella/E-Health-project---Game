using UnityEngine;

public class CobraAI : MonoBehaviour
{
    public enum AIType { Chase, Attack, Random, Ambusher, Patroller, PackHunter }
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
    private bool isCoordinating;

    // Bounds
    private float boundX = 4f;
    private float boundY = 3f;

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

        // Get sprite renderer for visual feedback
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
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

    void ChaseAI()
    {
        // Simple chase - follow player directly
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    void AttackAI()
    {
        // Advanced Predator AI - analyzes escape routes and cuts off player
        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;

        // Predict player's future position based on velocity
        float predictionMult = Mathf.Min(distance / 0.8f, 4.0f);
        Vector3 predicted = playerTransform.position + (playerController.velocity * predictionMult * predictionMultiplier);

        // Analyze player's position relative to screen edges
        float escapeLeft = playerTransform.position.x + boundX;
        float escapeRight = boundX - playerTransform.position.x;
        float escapeUp = boundY - playerTransform.position.y;
        float escapeDown = playerTransform.position.y + boundY;

        // Determine likely escape direction
        bool escapingLeft = escapeLeft > escapeRight;
        bool escapingUp = escapeUp > escapeDown;

        Vector3 futurePos;

        // If player is moving, predict continuation
        if (playerController.velocity.magnitude > 0.05f)
        {
            futurePos = predicted;
        }
        else
        {
            // Player stationary - predict they'll flee toward largest escape route
            futurePos = playerTransform.position;
            futurePos.x += escapingLeft ? -1f : 1f;
            futurePos.y += escapingUp ? 1f : -1f;
        }

        // Calculate intercept point
        Vector3 interceptPos;
        if (distance > 1f)
        {
            // Far away - aggressively cut off escape route
            Vector3 escapeVector = futurePos - playerTransform.position;
            interceptPos = futurePos + escapeVector * 0.5f;
        }
        else
        {
            // Close range - go straight for predicted position
            interceptPos = futurePos;
        }

        // Keep intercept in bounds
        interceptPos.x = Mathf.Clamp(interceptPos.x, -boundX + 0.5f, boundX - 0.5f);
        interceptPos.y = Mathf.Clamp(interceptPos.y, -boundY + 0.5f, boundY - 0.5f);
        interceptPos.z = 0f;

        // Move toward intercept with slight randomization
        Vector3 interceptDir = (interceptPos - transform.position).normalized;
        float angle = Mathf.Atan2(interceptDir.y, interceptDir.x);

        // Add random variation
        float variation = (Random.value - 0.5f) * 0.3f;
        angle += variation;

        Vector3 moveDir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        transform.position += moveDir * speed * Time.deltaTime;

        // Emergency acceleration when very close - final lunge
        if (distance < closeRangeDistance)
        {
            Vector3 boostDir = (playerTransform.position - transform.position).normalized;
            transform.position += boostDir * (speed * boostMultiplier * Time.deltaTime);
        }
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
            transform.position += direction * speed * Time.deltaTime;
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
        if (other.CompareTag("Player") && isInstantKillMode)
        {
            // Check if player has shield active
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                AbilitySystem abilitySystem = player.GetComponent<AbilitySystem>();
                if (abilitySystem != null && abilitySystem.isShieldActive)
                {
                    Debug.Log("Cobra instant kill blocked by shield!");
                    return;
                }
            }

            // Instant kill mode (levels 1-3) - no shield active
            if (GameManager.Instance != null)
            {
                Debug.Log("Cobra touch! Instant death (Instant Kill Mode - Levels 1-3)");
                GameManager.Instance.GameOver(false);
            }
        }
        // For level 4 (non-instant kill), PlayerController handles damage
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
            transform.position += strikeDirection * strikeSpeed * Time.deltaTime;

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
            transform.position += chaseDirection * chaseSpeed * Time.deltaTime;
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

            transform.position += patrolDirection * patrolSpeed * Time.deltaTime;
        }
    }
    #endregion

    #region Pack Hunter AI
    void InitializePackHunter()
    {
        // Find all cobras in the scene
        allCobras = FindObjectsOfType<CobraAI>();
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

                transform.position += balancedDirection * speed * Time.deltaTime;
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
        transform.position += moveDirection * speed * Time.deltaTime;
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
