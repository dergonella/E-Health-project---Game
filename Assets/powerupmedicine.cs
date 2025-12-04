using UnityEngine;
using System.Collections;

public class PowerUpMedicine : MonoBehaviour
{
    public enum MedicineType
    {
        LifeMed,
        SpeedMed,
        MagicMed
    }

    [Header("Configuration")]
    public MedicineType medicineType;
    public float respawnTime = 5.0f; // How many seconds before it appears again?

    [Header("Effect Settings")]
    public float speedMultiplier = 2.0f;
    public float effectDuration = 3.0f;

    [Header("Audio Settings")]
    public AudioClip pickupSound; // Drag your MP3/WAV sound here

    // Components to hide/show
    private SpriteRenderer spriteRenderer;
    private Collider2D circleCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 1. Apply the Power-Up
                ApplyEffect(playerHealth);
                
                // 2. Play Sound Effect (Fire and Forget)
                if (pickupSound != null)
                {
                    // This creates a temporary object to play the sound so it isn't cut off
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // 3. Start the Respawn Cycle (Disappear -> Wait -> Reappear)
                StartCoroutine(RespawnRoutine());
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // --- DISAPPEAR ---
        spriteRenderer.enabled = false; // Hide the image
        circleCollider.enabled = false; // Stop it from being touched
        Debug.Log($"{medicineType} picked up! Respawning in {respawnTime} seconds...");

        // --- WAIT ---
        yield return new WaitForSeconds(respawnTime);

        // --- REAPPEAR ---
        spriteRenderer.enabled = true; // Show image
        circleCollider.enabled = true; // Enable touch
        Debug.Log($"{medicineType} has respawned!");
    }

    private void ApplyEffect(PlayerHealth player)
    {
        switch (medicineType)
        {
            case MedicineType.LifeMed:
                player.Heal();
                break;

            case MedicineType.SpeedMed:
                player.ActivateSpeedBoost(effectDuration, speedMultiplier);
                break;

            case MedicineType.MagicMed:
                player.ActivateInvisibility(effectDuration);
                break;
        }
    }
}