using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 6f;  // Increased to 6 for easy testing - player much faster!
    public float acceleration = 30f;  // Very responsive for testing
    public float deceleration = 30f;  // Quick stops for testing
    public float size = 0.2f;

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

    // Component references
    private HealthSystem healthSystem;
    private AbilitySystem abilitySystem;
    private Animator animator;

    void Start()
    {
        previousPosition = transform.position;

        // Get component references
        healthSystem = GetComponent<HealthSystem>();
        abilitySystem = GetComponent<AbilitySystem>();
        animator = GetComponent<Animator>();
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

        // Check if stunned - if stunned, decelerate to stop
        if (healthSystem != null && healthSystem.isStunned)
        {
            // Smoothly stop when stunned
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
            transform.position += currentVelocity * Time.deltaTime;
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

        // Move the player with smooth velocity
        transform.position += currentVelocity * Time.deltaTime;

        // Update animator parameters for 4-directional animation
        UpdateAnimator(horizontal, vertical);

        // Keep player in bounds
        float boundX = 4f;
        float boundY = 3f;
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -boundX, boundX),
            Mathf.Clamp(transform.position.y, -boundY, boundY),
            0f
        );
    }

    void CalculateVelocity()
    {
        velocity = transform.position - previousPosition;
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
        Debug.Log($"Player collision detected with: {other.name}, Tag: {other.tag}");

        // Check collision with cobra
        if (other.CompareTag("Cobra"))
        {
            // Check if cobra is in instant kill mode (levels 0-3)
            CobraAI cobraAI = other.GetComponent<CobraAI>();
            if (cobraAI != null && cobraAI.isInstantKillMode)
            {
                // CobraAI handles instant kill - it calls Die() which allows brief movement
                // But for timed levels (Level 0.1), we want immediate game over
                if (LevelManager.Instance != null)
                {
                    var levelData = LevelManager.Instance.GetCurrentLevelData();
                    if (levelData != null && levelData.hasTimedChallenge)
                    {
                        // Immediate game over for timed challenges - no delayed movement
                        Debug.Log("Cobra hit in timed challenge! Immediate game over!");
                        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
                        {
                            GameManager.Instance.GameOver(false);
                        }
                        return;
                    }
                }
                // For other instant-kill levels, CobraAI handles it (allows brief movement)
                return;
            }

            // Level 4: Normal damage mode
            // Check if we have health system (levels with health)
            if (healthSystem != null)
            {
                // Check if shield is active
                if (abilitySystem != null && abilitySystem.isShieldActive)
                {
                    Debug.Log("Cobra attack blocked by shield!");
                    return;
                }

                // Take damage (15 HP per collision)
                healthSystem.TakeDamage(15f);
                Debug.Log("Cobra hit! Dealt 15 damage (Level 4 - normal mode)");

                // Death is handled by HealthSystem event
            }
            else
            {
                // Fallback for levels without health system and not in instant kill mode
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver(false);
                }
            }
        }

        // Check collision with shard
        if (other.CompareTag("Shard"))
        {
            Debug.Log($"Player touched shard: {other.name}");
            ShardController shard = other.GetComponent<ShardController>();
            if (shard != null)
            {
                Debug.Log($"ShardController found! Collecting shard...");
                CollectShard(100);
                shard.Respawn();
            }
            else
            {
                Debug.LogError($"Shard {other.name} has no ShardController component!");
            }
        }
        else
        {
            Debug.Log($"Object {other.name} does NOT have 'Shard' tag. It has: '{other.tag}'");
        }
    }
}
