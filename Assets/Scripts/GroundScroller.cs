using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 5f;
    public float resetPositionX = -20f;
    public float startPositionX = 20f;

    [Header("References")]
    public Transform groundTile1;
    public Transform groundTile2;

    private RunnerGameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<RunnerGameManager>();
    }

    void Update()
    {
        if (gameManager != null && gameManager.isGameOver)
            return;

        float currentSpeed = gameManager != null ? gameManager.currentSpeed : scrollSpeed;

        // Move both ground tiles
        if (groundTile1 != null)
        {
            groundTile1.position += Vector3.left * currentSpeed * Time.deltaTime;

            // Reset position when off screen
            if (groundTile1.position.x <= resetPositionX)
            {
                groundTile1.position = new Vector3(startPositionX, groundTile1.position.y, groundTile1.position.z);
            }
        }

        if (groundTile2 != null)
        {
            groundTile2.position += Vector3.left * currentSpeed * Time.deltaTime;

            // Reset position when off screen
            if (groundTile2.position.x <= resetPositionX)
            {
                groundTile2.position = new Vector3(startPositionX, groundTile2.position.y, groundTile2.position.z);
            }
        }
    }
}
