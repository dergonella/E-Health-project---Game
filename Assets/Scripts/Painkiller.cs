using System.Collections;
using UnityEngine;

/// <summary>
/// Painkiller pickup that cures poison and heals
/// Respawns after being collected
/// </summary>
public class Painkiller : MonoBehaviour
{
    [Header("Painkiller Settings")]
    public float healAmount = 15f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;
    public float respawnDelay = 15f; // Respawn after 15 seconds

    private Vector3 baseScale;
    private Vector3 spawnPosition;
    private float pulseTimer;
    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D painkillerCollider;

    void Start()
    {
        baseScale = transform.localScale;
        pulseTimer = 0f;
        spawnPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        painkillerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!isCollected)
        {
            // Pulsing effect
            pulseTimer += pulseSpeed * Time.deltaTime;
            float pulseFactor = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
            transform.localScale = baseScale * pulseFactor;

            // Optional: Rotate slowly
            transform.Rotate(Vector3.forward, 50f * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                HealthSystem healthSystem = player.GetComponent<HealthSystem>();
                if (healthSystem != null)
                {
                    // Cure poison
                    healthSystem.CurePoison();

                    // Heal player
                    healthSystem.Heal(healAmount);

                    Debug.Log("Painkiller collected! Will respawn in " + respawnDelay + " seconds.");

                    // Start respawn countdown
                    StartCoroutine(RespawnAfterDelay());
                }
            }
        }
    }

    IEnumerator RespawnAfterDelay()
    {
        // Mark as collected
        isCollected = true;

        // Hide painkiller
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        if (painkillerCollider != null)
        {
            painkillerCollider.enabled = false;
        }

        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);

        // Respawn painkiller
        isCollected = false;
        transform.position = spawnPosition;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        if (painkillerCollider != null)
        {
            painkillerCollider.enabled = true;
        }

        Debug.Log("Painkiller respawned!");
    }

    // Called by GameManager when level restarts
    public void ResetPainkiller()
    {
        StopAllCoroutines();
        isCollected = false;
        transform.position = spawnPosition;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        if (painkillerCollider != null)
        {
            painkillerCollider.enabled = true;
        }
    }
}
