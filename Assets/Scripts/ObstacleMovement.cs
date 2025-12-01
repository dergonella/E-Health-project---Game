using UnityEngine;

/// <summary>
/// Handles obstacle movement in the endless runner
/// Obstacles move left and destroy themselves when off-screen
/// </summary>
public class ObstacleMovement : MonoBehaviour
{
    private float moveSpeed;
    private float destroyXPosition = -15f;

    public void Initialize(float speed)
    {
        moveSpeed = speed;
    }

    void Update()
    {
        // Move obstacle to the left
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Destroy when off-screen
        if (transform.position.x < destroyXPosition)
        {
            Destroy(gameObject);
        }
    }

    public void Stop()
    {
        moveSpeed = 0f;
    }
}
