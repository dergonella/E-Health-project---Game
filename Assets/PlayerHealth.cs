using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Required for UI manipulation if you have it

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxLives = 3;
    private int currentLives;
    public bool isInvisible = false; // Is the player currently invisible?

    [Header("Hurt Effects")]
    public Color hurtColor = new Color(1f, 0.5f, 0.5f, 0.7f); 
    public Color invisibleColor = new Color(1f, 1f, 1f, 0.3f); // Ghostly transparent
    public float knockbackForce = 3f;
    public float knockbackDuration = 0.2f;

    [Header("Components")]
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    private playermovementstate movementScript;
    
    // Optional: Reference to a Text Mesh in the scene to show status updates
    public Text statusText; 

    private const string DEATH_ANIM = "owlet death";
    private const string HURT_ANIM = "owlet hurt"; 

    private void Start()
    {
        currentLives = maxLives;
        
        if (animator == null) animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movementScript = GetComponent<playermovementstate>();
        
        if (statusText != null) statusText.text = ""; // Clear text at start
    }

    public void TakeDamage(Vector3 attackerPosition)
    {
        // 1. CHECK INVISIBILITY: If invisible, ignore damage!
        if (isInvisible) return;

        if (currentLives <= 0) return; 

        currentLives--;
        Debug.Log("Player Hit! Lives remaining: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.Play(HURT_ANIM);
            StartCoroutine(FlashEffect());
            StartCoroutine(KnockbackRoutine(attackerPosition));
        }
    }

    // --- POWER UP FUNCTIONS ---

    public void Heal()
    {
        if (currentLives < maxLives)
        {
            currentLives++;
            ShowStatusText("Life Added!");
            Debug.Log("Medicine Taken! Lives: " + currentLives);
        }
        else
        {
            ShowStatusText("Max Lives!");
        }
    }

    public void ActivateSpeedBoost(float duration, float multiplier)
    {
        StartCoroutine(SpeedBoostRoutine(duration, multiplier));
    }

    public void ActivateInvisibility(float duration)
    {
        StartCoroutine(InvisibilityRoutine(duration));
    }

    // --- COROUTINES ---

    private IEnumerator SpeedBoostRoutine(float duration, float multiplier)
    {
        if (movementScript != null)
        {
            // ACTIVATE SPEED BOOST
            // We multiply the speed variable in your movement script
            movementScript.speed *= multiplier; 
            
            Debug.Log("Speed Boost Activated!");
            ShowStatusText("Speed Up!");
        }

        yield return new WaitForSeconds(duration);

        if (movementScript != null)
        {
            // DEACTIVATE SPEED BOOST
            movementScript.speed /= multiplier; // Return to normal speed
            Debug.Log("Speed Boost Ended");
        }
    }

    private IEnumerator InvisibilityRoutine(float duration)
    {
        isInvisible = true;
        ShowStatusText("Invisible!");
        
        // Visual Effect: Make sprite transparent
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = invisibleColor;

        yield return new WaitForSeconds(duration);

        // Reset
        isInvisible = false;
        spriteRenderer.color = originalColor;
        ShowStatusText("");
    }

    private IEnumerator FlashEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = hurtColor;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator KnockbackRoutine(Vector3 sourcePos)
    {
        if (movementScript != null) movementScript.enabled = false;

        Vector3 knockbackDir = (transform.position - sourcePos).normalized;
        float timer = 0;

        while (timer < knockbackDuration)
        {
            transform.position += knockbackDir * knockbackForce * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        if (movementScript != null) movementScript.enabled = true;
        if (animator != null && currentLives > 0) animator.Play("Idle"); 
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        ShowStatusText("Game Over");
        if (animator != null) animator.Play(DEATH_ANIM);
        if (movementScript != null) movementScript.enabled = false;
    }

    public bool IsDead()
    {
        return currentLives <= 0;
    }

    private void ShowStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            StartCoroutine(ClearTextDelay());
        }
    }

    private IEnumerator ClearTextDelay()
    {
        yield return new WaitForSeconds(2f);
        if (statusText != null) statusText.text = "";
    }
}