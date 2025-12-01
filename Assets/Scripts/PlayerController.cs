using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 6f;  // Increased to 6 for easy testing - player much faster!
    public float acceleration = 30f;  // Very responsive for testing
    public float deceleration = 30f;  // Quick stops for testing
    public float size = 0.2f;

    [Header("Stats")]
    public int shardsCollected = 0;
    public int score = 0;

    // Velocity tracking for cobra prediction
    private Vector3 previousPosition;
    public Vector3 velocity { get; private set; }
    private Vector3 currentVelocity = Vector3.zero; // Actual smooth velocity

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

        // Update MoveX and MoveY parameters for directional animation
        animator.SetFloat("MoveX", horizontal);
        animator.SetFloat("MoveY", vertical);
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

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check collision with cobra
        if (other.CompareTag("Cobra"))
        {
            // Check if cobra is in instant kill mode (levels 1-3)
            CobraAI cobraAI = other.GetComponent<CobraAI>();
            if (cobraAI != null && cobraAI.isInstantKillMode)
            {
                // CobraAI handles instant kill - do nothing here
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
            ShardController shard = other.GetComponent<ShardController>();
            if (shard != null)
            {
                CollectShard(100);
                shard.Respawn();
            }
        }
    }
}
