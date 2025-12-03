using UnityEngine;
using System;

public class playermovementstate : MonoBehaviour
{
    public enum MoveState
    {
        Idle,
        OwletRunRight,
        OwletRunLeft,
        OwletRunUp,
        OwletRunDown,
    }

    public MoveState CurrentMoveState { get; private set; }

    [Header("Movement Settings")]
    public float speed = 5f;

    // --- ADDED: Boundary Settings ---
    // Change these numbers in the Inspector to match the size of your background image
    [Header("Map Boundaries")]
    public float minX = -7.5f; // Left Limit
    public float maxX = 7.5f;  // Right Limit
    // Updated based on user request
    public float minY = -1.0f; // Bottom Limit
    public float maxY = 1.0f;  // Top Limit

    // Removed [SerializeField] to avoid confusion. We now find it automatically in Awake().
    private Animator animator;

    // MUST MATCH ANIMATOR WINDOW EXACTLY
    private const string IDLE_ANIM = "Idle"; 
    private const string RIGHT_ANIM = "owlet run right";
    private const string LEFT_ANIM = "owlet run left";
    private const string UP_ANIM = "owlet run up";
    private const string DOWN_ANIM = "owlet run down";

    public static Action<MoveState> onplayeermovestatechange;

    private void Awake()
    {
        // --- FIX 1: Automatically find the Animator ---
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("CRITICAL ERROR: There is no 'Animator' component on this GameObject! Please Add Component -> Animator.");
        }
    }

    // --- ADDED: Update Loop to handle Movement & Input ---
    private void Update()
    {
        // 1. Read Input (WASD or Arrow Keys)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 2. Move the Player based on Speed
        Vector3 movement = new Vector3(moveX, moveY, 0).normalized;
        transform.Translate(movement * speed * Time.deltaTime);

        // 3. --- ADDED: Keep Player Inside Boundaries ---
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;

        // 4. Determine Animation State based on Input
        if (moveX > 0)
        {
            SetMoveState(MoveState.OwletRunRight);
        }
        else if (moveX < 0)
        {
            SetMoveState(MoveState.OwletRunLeft);
        }
        else if (moveY > 0)
        {
            SetMoveState(MoveState.OwletRunUp);
        }
        else if (moveY < 0)
        {
            SetMoveState(MoveState.OwletRunDown);
        }
        else
        {
            SetMoveState(MoveState.Idle);
        }
    }

    public void SetMoveState(MoveState movestate)
    {
        // If we are already playing this animation, don't restart it
        if (movestate == CurrentMoveState) return;

        switch (movestate)
        {
            case MoveState.Idle:
                PlayAnim(IDLE_ANIM);
                break;
            case MoveState.OwletRunRight:
                PlayAnim(RIGHT_ANIM);
                break;
            case MoveState.OwletRunLeft:
                PlayAnim(LEFT_ANIM);
                break;
            case MoveState.OwletRunUp:
                PlayAnim(UP_ANIM);
                break;
            case MoveState.OwletRunDown:
                PlayAnim(DOWN_ANIM);
                break;
            default:
                Debug.LogWarning($"Invalid movement state: {movestate}");
                break;
        }

        onplayeermovestatechange?.Invoke(movestate);
        CurrentMoveState = movestate;
    }

    private void PlayAnim(string animName)
    {
        if (animator != null)
        {
            animator.Play(animName);
        }
        else
        {
            Debug.LogError("Cannot play animation because the Animator component is missing!");
        }
    }

    // --- DEBUG VISUALS ---
    // This draws a Green Box in the Scene View to show you the current limits
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        // Calculate the center and size of the box based on min/max values
        Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, transform.position.z);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 1);
        Gizmos.DrawWireCube(center, size);
    }
}