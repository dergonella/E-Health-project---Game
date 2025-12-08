using UnityEngine;

/// <summary>
/// Handles projectile movement and collision
/// </summary>
public class Projectile : MonoBehaviour
{
    public enum ProjectileType { Fire, Poison }

    [Header("Projectile Settings")]
    public ProjectileType type = ProjectileType.Fire;
    public float speed = 3f;  // Reduced from 6 for easy testing - slow projectiles!
    public float damage = 10f;  // Reduced from 15 for easy testing
    public float lifetime = 3f;  // Reduced from 4 for less clutter

    [Header("Poison Settings")]
    public bool appliesPoison = false;

    private Vector3 direction;
    private float lifeTimer;

    public void Initialize(Vector3 dir, ProjectileType projType = ProjectileType.Fire)
    {
        direction = dir.normalized;
        type = projType;
        lifeTimer = lifetime;

        // Rotate sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        // Move projectile
        transform.position += direction * speed * Time.deltaTime;

        // Handle lifetime
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Hit player
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
                    Debug.Log("Projectile blocked by shield!");
                    Destroy(gameObject);
                    return;
                }

                // Apply damage
                if (healthSystem != null)
                {
                    // TakeDamage already checks invulnerability, so just call it
                    healthSystem.TakeDamage(damage);
                    Debug.Log("Projectile hit player! Dealing " + damage + " damage.");

                    // Apply poison if this is a poison projectile
                    if (appliesPoison)
                    {
                        healthSystem.ApplyPoison();
                    }
                }
                else
                {
                    Debug.LogWarning("Player has no HealthSystem - projectile won't deal damage!");
                }
            }

            Destroy(gameObject);
        }

        // Hit wall
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
