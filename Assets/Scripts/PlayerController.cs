using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 6f;  // Increased to 6 for easy testing - player much faster!
    public float acceleration = 30f;  // Very responsive for testing
    public float deceleration = 30f;  // Quick stops for testing
    public float size = 0.2f;

    [Header("Bounds Settings")]
    [Tooltip("Use Tilemap walls for bounds instead of hard-coded limits")]
    public bool useWallCollision = true;  // For maze levels (Level 0.2+)
    [Tooltip("Fallback bounds when no walls exist")]
    public float boundX = 4f;
    public float boundY = 3f;

    [Header("Corner Smoothing")]
    [Tooltip("Enable smooth sliding around corners")]
    public bool useCornerSmoothing = true;
    [Tooltip("Distance to check for walls")]
    public float wallCheckDistance = 0.4f;
    [Tooltip("Layer mask for walls")]
    public LayerMask wallLayer;

    [Header("Death Movement Settings")]
    public float deathMovementDistance = 2f; // How far player can move after death (in meters)
    public float deathMovementDelay = 1.5f; // How long player can move after death (in seconds)

    [Header("Stats")]
    public int shardsCollected = 0;
    public int score = 0;

    // Velocity tracking for cobra prediction
    private Vector3 previousPosition;
    public Vector3 velocity { get; private set; }
    private Vector3 currentVelocity = Vector3.zero; // Actual smooth velocity

    // Death movement tracking
    private bool isDead = false;
    private Vector3 deathPosition;
    private float deathTime;

    // Component references (HealthSystem and AbilitySystem are optional - only for advanced levels)
    private HealthSystem healthSystem;  // Optional: Only used in levels with health bars
    private AbilitySystem abilitySystem;  // Optional: Only used in levels with abilities
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        previousPosition = transform.position;

        // Get component references (these may be null for simple levels like 0.2)
        healthSystem = GetComponent<HealthSystem>();  // May be null
        abilitySystem = GetComponent<AbilitySystem>();  // May be null
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Configure Rigidbody for wall collision mode
        if (useWallCollision && rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;  // No gravity for top-down game
            rb.freezeRotation = true;  // Don't rotate when hitting walls
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Ensure collider is NOT a trigger for wall collision to work
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && col.isTrigger)
            {
                Debug.LogWarning("[PlayerController] Collider is set as Trigger - wall collision won't work! Disabling trigger mode.");
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
    }

    void Update()
    {
        // Store position before movement for velocity calculation
        previousPosition = transform.position;

        HandleMovement();
        CalculateVelocity();
    }

    void HandleMovement()
    {
        // If player is dead, check if they can still move
        if (isDead)
        {
            float timeSinceDeath = Time.time - deathTime;
            float distanceSinceDeath = Vector3.Distance(transform.position, deathPosition);

            // Check if death movement period has expired (either by time OR distance)
            if (timeSinceDeath >= deathMovementDelay || distanceSinceDeath >= deathMovementDistance)
            {
                // Stop all movement and trigger game over
                currentVelocity = Vector3.zero;

                // Trigger game over if not already triggered
                if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
                {
                    GameManager.Instance.GameOver(false);
                }
                return;
            }
            // If still within death movement window, allow movement (continue with normal movement code below)
        }

        // Check if stunned - if stunned, decelerate to stop (only if HealthSystem exists)
        if (healthSystem != null && healthSystem.isStunned)
        {
            // Smoothly stop when stunned
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
            if (useWallCollision && rb != null)
            {
                rb.linearVelocity = currentVelocity;
            }
            else
            {
                transform.position += currentVelocity * Time.deltaTime;
            }
            return;
        }

        // Get input
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;

        // Create input direction vector
        Vector3 inputDirection = new Vector3(horizontal, vertical, 0f);

        // Normalize to prevent faster diagonal movement
        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        // Apply speed multiplier (for poison slow effect)
        float speedMultiplier = 1f;
        if (healthSystem != null)
        {
            speedMultiplier = healthSystem.GetSpeedMultiplier();
        }

        // Calculate target velocity
        Vector3 targetVelocity = inputDirection * speed * speedMultiplier;

        // Apply corner smoothing - helps slide around corners
        if (useCornerSmoothing && inputDirection.magnitude > 0.1f && wallLayer != 0)
        {
            targetVelocity = ApplyCornerSmoothing(inputDirection, speed * speedMultiplier);
        }

        // Smooth acceleration/deceleration
        if (inputDirection.magnitude > 0.1f)
        {
            // Accelerating - smoothly increase velocity toward target
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // Decelerating - smoothly decrease velocity to zero
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Move the player
        if (useWallCollision && rb != null)
        {
            // Use Rigidbody velocity for wall collision detection
            rb.linearVelocity = currentVelocity;
        }
        else
        {
            // Direct transform movement (old method)
            transform.position += currentVelocity * Time.deltaTime;

            // Keep player in bounds with clamping
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, -boundX, boundX),
                Mathf.Clamp(transform.position.y, -boundY, boundY),
                0f
            );
        }

        // Update animator parameters for 4-directional animation
        UpdateAnimator(horizontal, vertical);
    }

    void CalculateVelocity()
    {
        velocity = transform.position - previousPosition;
    }

    /// <summary>
    /// Smooths movement around corners by detecting walls and sliding along them.
    /// Prevents getting stuck on corner edges in narrow passages.
    /// </summary>
    Vector3 ApplyCornerSmoothing(Vector3 inputDir, float moveSpeed)
    {
        Vector3 resultVelocity = inputDir * moveSpeed;

        // Check if moving into a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, inputDir, wallCheckDistance, wallLayer);

        if (hit.collider != null)
        {
            // Wall detected - try to slide along it
            Vector3 wallNormal = hit.normal;

            // Calculate slide direction (remove the component going into the wall)
            Vector3 slideDir = inputDir - Vector3.Dot(inputDir, wallNormal) * (Vector3)wallNormal;

            if (slideDir.magnitude > 0.1f)
            {
                resultVelocity = slideDir.normalized * moveSpeed;
            }
            else
            {
                // Check perpendicular directions for corner navigation
                Vector3 perp1 = new Vector3(-inputDir.y, inputDir.x, 0f);
                Vector3 perp2 = new Vector3(inputDir.y, -inputDir.x, 0f);

                RaycastHit2D hit1 = Physics2D.Raycast(transform.position, perp1, wallCheckDistance, wallLayer);
                RaycastHit2D hit2 = Physics2D.Raycast(transform.position, perp2, wallCheckDistance, wallLayer);

                // Choose the direction that's not blocked
                if (hit1.collider == null && hit2.collider != null)
                {
                    resultVelocity = perp1 * moveSpeed * 0.7f;
                }
                else if (hit2.collider == null && hit1.collider != null)
                {
                    resultVelocity = perp2 * moveSpeed * 0.7f;
                }
            }
        }

        return resultVelocity;
    }

    void UpdateAnimator(float horizontal, float vertical)
    {
        if (animator == null) return;

        // Use SetBool for cleaner state transitions
        // Reset all direction bools first
        animator.SetBool("MovingRight", false);
        animator.SetBool("MovingLeft", false);
        animator.SetBool("MovingUp", false);
        animator.SetBool("MovingDown", false);

        // Priority: horizontal movement over vertical
        if (horizontal > 0.01f)
        {
            // Moving right
            animator.SetBool("MovingRight", true);
            Debug.Log("Animation: Moving RIGHT");
        }
        else if (horizontal < -0.01f)
        {
            // Moving left
            animator.SetBool("MovingLeft", true);
        }
        else if (vertical > 0.01f)
        {
            // Moving up
            animator.SetBool("MovingUp", true);
        }
        else if (vertical < -0.01f)
        {
            // Moving down
            animator.SetBool("MovingDown", true);
        }

        // Also keep the float parameters for backward compatibility
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            animator.SetFloat("MoveX", horizontal);
            animator.SetFloat("MoveY", 0f);
        }
        else if (Mathf.Abs(vertical) > 0.01f)
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", vertical);
        }
        else
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
        }
    }

    public void CollectShard(int points)
    {
        shardsCollected++;
        score += points;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateScore(score);
        }
    }

    /// <summary>
    /// Called when player is killed. Allows limited movement before game over.
    /// Player can move for deathMovementDelay seconds OR deathMovementDistance meters (whichever comes first).
    /// </summary>
    public void Die()
    {
        if (isDead) return; // Already dead, prevent multiple calls

        isDead = true;
        deathPosition = transform.position;
        deathTime = Time.time;

        Debug.Log($"Player died! Can move {deathMovementDistance}m or for {deathMovementDelay}s before game over.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    void HandleCollision(GameObject other)
    {
        // Check collision with cobra
        if (other.CompareTag("Cobra"))
        {
            CobraAI cobraAI = other.GetComponent<CobraAI>();

            // INSTANT KILL MODE (Level 0.2 and similar)
            if (cobraAI != null && cobraAI.isInstantKillMode)
            {
                // For timed levels (Level 0.1), immediate game over
                if (LevelManager.Instance != null)
                {
                    var levelData = LevelManager.Instance.GetCurrentLevelData();
                    if (levelData != null && levelData.hasTimedChallenge)
                    {
                        Debug.Log("Cobra hit in timed challenge! Immediate game over!");
                        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
                        {
                            GameManager.Instance.GameOver(false);
                        }
                        return;
                    }
                }
                // For other instant-kill levels (like 0.2), CobraAI calls Die()
                return;
            }

            // DAMAGE MODE (Advanced levels with health system)
            if (healthSystem != null)
            {
                // Check if shield is active (only if AbilitySystem exists)
                if (abilitySystem != null && abilitySystem.isShieldActive)
                {
                    Debug.Log("Cobra attack blocked by shield!");
                    return;
                }

                healthSystem.TakeDamage(15f);
                Debug.Log("Cobra hit! Dealt 15 damage");
            }
            else
            {
                // No health system and not instant kill = immediate game over
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver(false);
                }
            }
        }

        // Check collision with shard
        if (other.CompareTag("Shard"))
        {
            ShardController shard = other.GetComponent<ShardController>();
            if (shard != null)
            {
                CollectShard(100);
                shard.Respawn();
            }
        }
    }
}
