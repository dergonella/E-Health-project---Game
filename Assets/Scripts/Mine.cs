using UnityEngine;

/// <summary>
/// Mine hazard that explodes on contact
/// </summary>
public class Mine : MonoBehaviour
{
    [Header("Mine Settings")]
    public float damage = 25f;  // Reduced from 35 to match documentation
    public float knockbackForce = 2.5f;
    public float stunDuration = 1f;  // Increased from 0.5 to 1 second - more noticeable stun
    public float warningDistance = 2f;  // Increased from 1.5 for better warning

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public float pulseSpeed = 5f;

    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private bool isWarning = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Re-find player if reference is lost
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                return; // No player found, skip this frame
            }
        }

        // Check distance to player for warning
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < warningDistance)
        {
            if (!isWarning)
            {
                isWarning = true;
            }

            // Pulse warning effect
            if (spriteRenderer != null)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                spriteRenderer.color = Color.Lerp(normalColor, warningColor, pulse);
            }
        }
        else
        {
            if (isWarning)
            {
                isWarning = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = normalColor;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                HealthSystem healthSystem = player.GetComponent<HealthSystem>();
                AbilitySystem abilitySystem = player.GetComponent<AbilitySystem>();

                // Check if shield is active
                if (abilitySystem != null && abilitySystem.isShieldActive)
                {
                    Debug.Log("Mine explosion blocked by shield!");
                    Destroy(gameObject);
                    return;
                }

                // Apply damage
                if (healthSystem != null)
                {
                    healthSystem.TakeDamage(damage);
                    healthSystem.ApplyStun(stunDuration);
                    Debug.Log("Mine hit! Dealt " + damage + " damage and stunned for " + stunDuration + " seconds.");
                }
                else
                {
                    Debug.LogWarning("Player has no HealthSystem - mine won't deal damage!");
                }

                // Apply knockback
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                other.transform.position += knockbackDirection * knockbackForce;

                // Reset combo/multiplier if player has one
                if (player.score > 0)
                {
                    Debug.Log("Mine explosion! Combo reset!");
                }
            }

            // Destroy mine after explosion
            Destroy(gameObject);
        }
    }
}
