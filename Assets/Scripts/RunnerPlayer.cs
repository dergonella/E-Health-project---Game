using UnityEngine;

public class RunnerPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float jumpForce = 15f;
    public float gravity = -40f;
    public float groundY = -2f;

    [Header("Duck")]
    public float duckScaleY = 0.5f;
    public float normalScaleY = 1f;
    public float duckDuration = 0.5f;

    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    private bool isDucking = false;
    private float duckTimer = 0f;
    private Vector3 originalScale;

    [Header("References")]
    public RunnerGameManager gameManager;

    void Start()
    {
        originalScale = transform.localScale;
        transform.position = new Vector3(transform.position.x, groundY, 0);
    }

    void Update()
    {
        if (gameManager != null && gameManager.isGameOver)
            return;

        HandleInput();
        HandleMovement();
        HandleDuck();
    }

    void HandleInput()
    {
        // Jump - Space or Up Arrow
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded && !isDucking)
        {
            Jump();
        }

        // Duck - Down Arrow or S
        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && isGrounded)
        {
            StartDuck();
        }
    }

    void Jump()
    {
        verticalVelocity = jumpForce;
        isGrounded = false;
    }

    void StartDuck()
    {
        isDucking = true;
        duckTimer = duckDuration;
        transform.localScale = new Vector3(originalScale.x, duckScaleY, originalScale.z);
    }

    void HandleDuck()
    {
        if (isDucking)
        {
            duckTimer -= Time.deltaTime;
            if (duckTimer <= 0)
            {
                isDucking = false;
                transform.localScale = new Vector3(originalScale.x, normalScaleY, originalScale.z);
            }
        }
    }

    void HandleMovement()
    {
        // Apply gravity
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Update position
        Vector3 pos = transform.position;
        pos.y += verticalVelocity * Time.deltaTime;

        // Ground check
        if (pos.y <= groundY)
        {
            pos.y = groundY;
            verticalVelocity = 0f;
            isGrounded = true;
        }

        transform.position = pos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        }
    }
}
