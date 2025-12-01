using UnityEngine;

/// <summary>
/// Controls the penguin character in the endless runner game
/// W key to jump, S key to duck
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PenguinController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Collision Settings")]
    [SerializeField] private Vector2 normalColliderSize = new Vector2(1f, 2f);
    [SerializeField] private Vector2 normalColliderOffset = new Vector2(0f, 0f);
    [SerializeField] private Vector2 duckColliderSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 duckColliderOffset = new Vector2(0f, -0.5f);

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Animator animator;
    private bool isGrounded;
    private bool isDucking;
    private bool isAlive = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        // Setup rigidbody
        rb.gravityScale = 0f; // We'll handle gravity manually
        rb.freezeRotation = true;
    }

    void Start()
    {
        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -normalColliderSize.y / 2f, 0);
            groundCheck = gc.transform;
        }
    }

    void Update()
    {
        if (!isAlive) return;

        CheckGroundStatus();
        HandleInput();
    }

    void FixedUpdate()
    {
        if (!isAlive) return;

        // Apply custom gravity
        rb.velocity += Vector2.up * gravity * Time.fixedDeltaTime;
    }

    void HandleInput()
    {
        // Jump with W key
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            Jump();
        }

        // Duck with S key
        if (Input.GetKey(KeyCode.S) && isGrounded)
        {
            if (!isDucking)
            {
                Duck();
            }
        }
        else
        {
            if (isDucking)
            {
                StandUp();
            }
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;

        if (animator != null)
        {
            animator.SetTrigger("Jump");
            animator.SetBool("IsGrounded", false);
        }
    }

    void Duck()
    {
        isDucking = true;
        boxCollider.size = duckColliderSize;
        boxCollider.offset = duckColliderOffset;

        if (animator != null)
        {
            animator.SetBool("IsDucking", true);
        }
    }

    void StandUp()
    {
        isDucking = false;
        boxCollider.size = normalColliderSize;
        boxCollider.offset = normalColliderOffset;

        if (animator != null)
        {
            animator.SetBool("IsDucking", false);
        }
    }

    void CheckGroundStatus()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            // Just landed
            if (animator != null)
            {
                animator.SetBool("IsGrounded", true);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Notify game manager
        PenguinGameManager gameManager = FindObjectOfType<PenguinGameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
