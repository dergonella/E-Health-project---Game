using UnityEngine;

public class MedicinePickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object touching the medicine is the Player
        if (other.CompareTag("Player"))
        {
            // 1. Find the health script on the player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 2. Heal the player
                playerHealth.Heal();

                // 3. Make this medicine disappear ("Gone")
                Destroy(gameObject);
            }
        }
    }
}

