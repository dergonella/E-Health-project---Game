using UnityEngine;

public class DinoPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpForce = 10f;
    public float gravity = 25f;
    public float groundY = -2.5f; // Adjust this to match your ground position

    [Header("Ducking")]
    public float duckScaleY = 0.5f;

    [Header("Animation Sprites")]
    [Tooltip("Idle penguin sprite (standing still)")]
    public Sprite pinguinIdle;
    [Tooltip("Running sprite 1 (leg up)")]
    public Sprite pinguin1;
    [Tooltip("Running sprite 2 (leg down)")]
    public Sprite pinguin2;
    [Tooltip("How fast to switch between sprites (lower = faster)")]
    public float animationSpeed = 0.1f;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isDucking;
    private Vector3 normalScale;

    // Animation
    private SpriteRenderer spriteRenderer;
    private float animationTimer;
    private int currentFrame = 0;
    private Sprite[] runSprites;

    private void Start()
    {
        normalScale = transform.localScale;

        // Get sprite renderer for animation
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Setup run animation sprites array
        runSprites = new Sprite[] { pinguin1, pinguin2 };

        // Ensure we have a Rigidbody2D for collision detection
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        ResetPosition();
    }

    void ResetPosition()
    {
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        velocity = Vector3.zero;
        isGrounded = true;
    }

    private void Update()
    {
        HandleInput();
        ApplyGravity();
        ApplyMovement();
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        // When jumping or not grounded - show idle sprite
        if (!isGrounded)
        {
            if (pinguinIdle != null)
            {
                spriteRenderer.sprite = pinguinIdle;
            }
            return;
        }

        // When grounded - animate running (cycle through pinguin1 and pinguin2)
        if (pinguin1 != null && pinguin2 != null)
        {
            animationTimer += Time.deltaTime;

            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % 2;
                spriteRenderer.sprite = runSprites[currentFrame];
            }
        }
    }

    void HandleInput()
    {
        // Jump with Space or Up Arrow (only when grounded and not ducking)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded && !isDucking)
        {
            velocity.y = jumpForce;
            isGrounded = false;
        }

        // Duck with S or Down Arrow
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
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

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    void ApplyMovement()
    {
        // Apply velocity
        Vector3 pos = transform.position;
        pos += velocity * Time.deltaTime;

        // Ground check
        if (pos.y <= groundY)
        {
            pos.y = groundY;
            velocity.y = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        transform.position = pos;
    }

    void Duck()
    {
        isDucking = true;
        Vector3 scale = transform.localScale;
        scale.y = normalScale.y * duckScaleY;
        transform.localScale = scale;
    }

    void StandUp()
    {
        isDucking = false;
        transform.localScale = normalScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("DinoPlayer: Collision detected with " + collision.gameObject.name + " (Tag: " + collision.tag + ")");

        if (collision.CompareTag("Obstacle"))
        {
            Debug.Log("DinoPlayer: Hit an obstacle! Calling GameOver()");
            GameOver();
        }
        else
        {
            Debug.Log("DinoPlayer: Collision but not tagged as Obstacle");
        }
    }

    void GameOver()
    {
        Debug.Log("DinoPlayer: GameOver() called");
        if (DinoGameManager.Instance != null)
        {
            Debug.Log("DinoPlayer: Calling DinoGameManager.GameOver()");
            DinoGameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError("DinoPlayer: DinoGameManager.Instance is NULL!");
        }
    }
}
