using UnityEngine;

public class PowerUpMedicine : MonoBehaviour
{
    // Define the 3 types you requested
    public enum MedicineType
    {
        LifeMed,   // Adds a life
        SpeedMed,  // Makes player faster
        MagicMed   // Makes player invisible
    }

    [Header("Configuration")]
    public MedicineType medicineType;
    public int charges = 3; // Item stays until touched 3 times
    public float cooldown = 1.0f; // Wait time between touches

    [Header("Effect Settings")]
    public float speedMultiplier = 2.0f; // How much faster? (2x)
    public float effectDuration = 3.0f;  // How long does Speed/Magic last?

    private float lastPickupTime;
    private bool canBePickedUp = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBePickedUp) return;

        if (other.CompareTag("Player"))
        {
            // Check Cooldown so you don't use all 3 charges instantly
            if (Time.time - lastPickupTime < cooldown) return;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                ApplyEffect(playerHealth);
                
                // Reduce charges
                charges--;
                lastPickupTime = Time.time;
                
                Debug.Log($"Used {medicineType}. Charges left: {charges}");

                // Visual: If out of charges, destroy the object
                if (charges <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void ApplyEffect(PlayerHealth player)
    {
        switch (medicineType)
        {
            case MedicineType.LifeMed:
                player.Heal(); // Adds 1 Life
                break;

            case MedicineType.SpeedMed:
                // Calls the Speed Boost function in PlayerHealth
                player.ActivateSpeedBoost(effectDuration, speedMultiplier);
                break;

            case MedicineType.MagicMed:
                // Calls the Invisibility function in PlayerHealth
                player.ActivateInvisibility(effectDuration);
                break;
        }
    }
}