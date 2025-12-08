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

    // Reference to spawner (set by ShardSpawner)
    [HideInInspector] public ShardSpawner spawner;
    [HideInInspector] public int shardIndex;

    // Bounds for respawn (legacy - now handled by ShardSpawner)
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

    /// <summary>
    /// Called when player collects this shard.
    /// Notifies the spawner to respawn this shard at a new location.
    /// </summary>
    public void Respawn()
    {
        // If we have a spawner, let it handle respawning (new multi-spawn system)
        if (spawner != null)
        {
            spawner.OnShardCollected(shardIndex);
        }
        else
        {
            // Legacy single-shard respawn (fallback for old scenes)
            StartCoroutine(RespawnWithEffect());
        }
    }

    /// <summary>
    /// Legacy respawn method for backwards compatibility.
    /// Used when shard is not managed by ShardSpawner.
    /// </summary>
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

        // Wait longer to ensure player has moved away
        yield return new WaitForSeconds(0.5f);

        // Find valid position avoiding walls
        bool validPosition = false;
        int attempts = 0;
        Vector3 newPosition = Vector3.zero;

        // Use SpawnValidator if available (for maze levels)
        if (SpawnValidator.Instance != null)
        {
            newPosition = SpawnValidator.Instance.GetValidCollectibleSpawn(
                new Vector3(
                    Random.Range(-boundX + 0.4f, boundX - 0.4f),
                    Random.Range(-boundY + 0.4f, boundY - 0.4f),
                    0f
                )
            );
            validPosition = true;
        }
        else
        {
            // Fallback: Legacy wall detection
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
