using UnityEngine;

public class RunnerObstacle : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float destroyX = -12f;

    [Header("Obstacle Type")]
    public bool isLowObstacle = false; // If true, player needs to duck. If false, player needs to jump.

    private RunnerGameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<RunnerGameManager>();

        // Adjust move speed based on game manager's speed multiplier
        if (gameManager != null)
        {
            moveSpeed = gameManager.currentSpeed;
        }

        // Change color based on obstacle type
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isLowObstacle ? new Color(1f, 0.5f, 0f) : Color.red; // Orange for low, red for ground
        }
    }

    void Update()
    {
        // Move left
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Destroy if off screen
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}
