using UnityEngine;

public class ShardController : MonoBehaviour
{
    [Header("Visual Settings")]
    public float pulseSpeed = 0.1f;
    public float pulseAmount = 0.02f;
    public int shardValue = 100;

    private Vector3 baseScale;
    private float pulseTimer = 0f;
    private SpriteRenderer spriteRenderer;

    // Bounds for respawn
    private float boundX = 4f;
    private float boundY = 3f;

    void Start()
    {
        baseScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Pulsing effect
        pulseTimer += pulseSpeed;
        float pulseFactor = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
        transform.localScale = baseScale * pulseFactor;
    }

    public void Respawn()
    {
        // Brief visual feedback - disable then re-enable
        StartCoroutine(RespawnWithEffect());
    }

    private System.Collections.IEnumerator RespawnWithEffect()
    {
        // Hide shard briefly
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Disable collider temporarily to prevent multiple collections
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Wait longer to ensure player has moved away (increased from 0.1 to 0.5)
        yield return new WaitForSeconds(0.5f);

        // Find valid position avoiding walls
        bool validPosition = false;
        int attempts = 0;
        Vector3 newPosition = Vector3.zero;

        while (!validPosition && attempts < 50)
        {
            newPosition = new Vector3(
                Random.Range(-boundX + 0.4f, boundX - 0.4f),
                Random.Range(-boundY + 0.4f, boundY - 0.4f),
                0f
            );

            // Check if position overlaps with walls
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(newPosition, 0.15f);
            validPosition = true;

            foreach (Collider2D overlap in overlaps)
            {
                if (overlap.CompareTag("Wall"))
                {
                    validPosition = false;
                    break;
                }
            }

            attempts++;
        }

        transform.position = newPosition;

        // Re-enable shard
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        if (col != null)
        {
            col.enabled = true;
        }
    }
}
