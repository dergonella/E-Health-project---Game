using UnityEngine;

public class attackermovement : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 4f; // Speed of the chaser
    public float stopDistance = 0.5f; // Stop moving if this close (prevents glitchy overlapping)

    private Transform playerTransform;

    void Start()
    {
        // We look for the 'playermovementstate' script to find the Player Object
        // Then we grab its TRANSFORM (which holds position data)
        playermovementstate playerScript = FindObjectOfType<playermovementstate>();

        if (playerScript != null)
        {
            playerTransform = playerScript.transform;
        }
        else
        {
            Debug.LogError("Homing Attacker: Could not find the Player!");
        }
    }

    void Update()
    {
        // If player is dead or missing, do nothing
        if (playerTransform == null) return;

        // 1. Calculate the Distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 2. Only move if we are further away than the stopping distance
        if (distanceToPlayer > stopDistance)
        {
            // Calculate the direction: Destination - Current Position
            Vector3 direction = playerTransform.position - transform.position;

            // "Normalize" the vector so it only has direction, not magnitude (Length = 1)
            // This ensures the enemy moves at a constant speed
            direction.Normalize();

            // Move the Attacker
            transform.Translate(direction * speed * Time.deltaTime);

            // --- VISUALS: FACE THE PLAYER ---
            // If direction.x is positive, player is to the right.
            if (direction.x > 0)
            {
                // Face Right
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                // Face Left
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
