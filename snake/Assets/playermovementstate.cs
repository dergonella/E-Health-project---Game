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

    public void SetMoveState(MoveState movestate)
    {
        // If we are already playing this animation, don't restart it
        if (movestate == CurrentMoveState) return;

        // --- FIX 2: Print to Console so we know if input is working ---
        // If you don't see this message in the Console when you move, the PlayerMovement script is broken.
        Debug.Log("Attempting to play animation: " + movestate);

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
}