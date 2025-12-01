using UnityEngine;
using System;
using System.Collections.Generic; // Required for Queue

public class AttackerMovementState : MonoBehaviour
{
    public enum AttackerState
    {
        Idle,
        AttackerRunRight,
        AttackerRunLeft,
        AttackerRunUp,
        AttackerRunDown,
        AttackerAttack // New State for attacking
    }

    // Struct to store history of Player's movement
    private struct GhostSnapshot
    {
        public Vector3 position;
        public AttackerState state; 
        public float timestamp;

        public GhostSnapshot(Vector3 pos, AttackerState st, float time)
        {
            position = pos;
            state = st;
            timestamp = time;
        }
    }

    public AttackerState CurrentAttackerState { get; private set; }

    [Header("Movement Settings")]
    public float speed = 3.5f;
    public float stopDistance = 0.1f;

    [Header("Delay Settings")]
    [Tooltip("How many seconds the attacker waits before copying the player's move")]
    public float movementDelay = 0.5f; 

    [Header("Interaction Settings")]
    [Tooltip("Distance at which the attacker catches the player")]
    public float catchDistance = 0.5f;
    private bool isGameOver = false;

    [Header("Visual Settings")]
    [Tooltip("Check this if your character 'Moonwalks' (moves left but looks right)")]
    public bool flipSpriteOnLeft = true; 

    // ANIMATION NAMES
    private const string IDLE_ANIM = "Idle"; 
    private const string RIGHT_ANIM = "attacker run right";
    private const string LEFT_ANIM = "attacker run left"; 
    private const string UP_ANIM = "attacker run up";
    private const string DOWN_ANIM = "attacker run down";
    private const string ATTACK_ANIM = "attacker attack"; 
    
    // PLAYER ANIMATION NAME
    private const string PLAYER_DEATH_ANIM = "owlet death";

    public static Action<AttackerState> OnAttackerMoveStateChange;

    private Animator animator;
    private Transform playerTransform;
    private playermovementstate playerScript;

    // The History Buffer
    private Queue<GhostSnapshot> ghostTrail = new Queue<GhostSnapshot>();
    private Vector3 currentTargetPosition;
    
    // LOGIC FLAGS
    private bool isFacingLeft = false;
    private bool hasStartedChasing = false; // Logic for Idle Start

    private void Awake()
    {
        animator = GetComponent<Animator>();

        playerScript = FindObjectOfType<playermovementstate>();
        if (playerScript != null)
        {
            playerTransform = playerScript.transform;
            currentTargetPosition = transform.position; 
        }
        else
        {
            Debug.LogError("Attacker Error: Could not find 'playermovementstate' script in the scene!");
        }
    }

    private void LateUpdate()
    {
        // 0. Safety Checks
        if (playerTransform == null || playerScript == null) return;
        
        // Stop everything if the game is over
        if (isGameOver) return;

        // --- STEP 1: CHECK FOR CATCH (Game Over Condition) ---
        // We check distance to the ACTUAL player, not the delayed ghost position
        // Using Vector2 to ignore small Z-axis height differences which can break detection
        float distanceToRealPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToRealPlayer <= catchDistance)
        {
            TriggerGameOver();
            return;
        }

        // --- STEP 2: IDLE START LOGIC ---
        if (!hasStartedChasing)
        {
            if (playerScript.CurrentMoveState != playermovementstate.MoveState.Idle)
            {
                hasStartedChasing = true; 
            }
            else
            {
                SetAttackerMoveState(AttackerState.Idle);
                return; 
            }
        }

        // --- STEP 3: RECORD PLAYER (Position AND State) ---
        // This records Up, Down, Left, and Right automatically
        AttackerState recordedState = MapPlayerStateToAttacker(playerScript.CurrentMoveState);
        ghostTrail.Enqueue(new GhostSnapshot(playerTransform.position, recordedState, Time.time));


        // --- STEP 4: REPLAY HISTORY (After Delay) ---
        AttackerState stateToPlay = AttackerState.Idle; // Default temp variable

        // Retrieve the snapshot that matches our delay time
        if (ghostTrail.Count > 0 && Time.time - ghostTrail.Peek().timestamp >= movementDelay)
        {
            GhostSnapshot targetSnapshot = ghostTrail.Dequeue();
            currentTargetPosition = targetSnapshot.position;
            stateToPlay = targetSnapshot.state; // Get the state the player was in (e.g., RunLeft)
        }

        // --- STEP 5: MOVE PHYSICALLY & OVERRIDE ANIMATION IF NEEDED ---
        float distToTarget = Vector3.Distance(transform.position, currentTargetPosition);
        
        if (distToTarget > stopDistance)
        {
            // PHYSICAL FACING OVERRIDE:
            // This ensures sprite flipping (Left/Right) happens based on actual movement direction
            float xDiff = currentTargetPosition.x - transform.position.x;
            if (Mathf.Abs(xDiff) > 0.01f) 
            {
                isFacingLeft = xDiff < 0; 
            }

            // ANIMATION FAILSAFE (Crucial for Flexibility):
            // If the recorded state is IDLE (e.g. player stopped inputs), but the attacker 
            // is still physically moving to catch up, we must calculate the correct 
            // run animation (Up/Down/Left/Right) from the movement vector.
            if (stateToPlay == AttackerState.Idle)
            {
                Vector3 moveDirection = (currentTargetPosition - transform.position).normalized;
                stateToPlay = CalculateStateFromVector(moveDirection);
            }

            // Apply the final decided state to the Animator
            SetAttackerMoveState(stateToPlay);

            // Move towards the "Ghost" target position
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, speed * Time.deltaTime);
        }
        else
        {
            // Only go Idle if we have caught up to the trail
            if (ghostTrail.Count == 0) 
            {
                SetAttackerMoveState(AttackerState.Idle);
            }
        }

        // --- STEP 6: FORCE FACE DIRECTION ---
        EnforceFacingDirection();
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        Debug.Log("GAME OVER: Attacker caught the player!");

        // 1. Play Attacker Attack Animation
        SetAttackerMoveState(AttackerState.AttackerAttack);

        // 2. DISABLE THE PLAYER SCRIPT
        // Stops the player from overriding the death animation with Idle/Run
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

        // 3. Play Player Death Animation
        Animator playerAnim = playerTransform.GetComponent<Animator>();
        if (playerAnim != null)
        {
            playerAnim.Play(PLAYER_DEATH_ANIM);
        }
        else
        {
            Debug.LogError("Could not find Animator on Player to play death animation!");
        }
    }

    // New Helper: Calculates animation based on math (Vector) 
    // This allows flexible animation changes even if player inputs stop.
    private AttackerState CalculateStateFromVector(Vector3 dir)
    {
        // Compare horizontal vs vertical movement speed
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // Horizontal is dominant
            return dir.x > 0 ? AttackerState.AttackerRunRight : AttackerState.AttackerRunLeft;
        }
        else
        {
            // Vertical is dominant
            return dir.y > 0 ? AttackerState.AttackerRunUp : AttackerState.AttackerRunDown;
        }
    }

    // Convert Player State to Attacker State
    private AttackerState MapPlayerStateToAttacker(playermovementstate.MoveState pState)
    {
        switch (pState)
        {
            case playermovementstate.MoveState.OwletRunRight:
                return AttackerState.AttackerRunRight;
            case playermovementstate.MoveState.OwletRunLeft:
                return AttackerState.AttackerRunLeft;
            case playermovementstate.MoveState.OwletRunUp:
                return AttackerState.AttackerRunUp;
            case playermovementstate.MoveState.OwletRunDown:
                return AttackerState.AttackerRunDown;
            default:
                return AttackerState.Idle;
        }
    }

    // Helper to keep the sprite facing the right way constantly
    private void EnforceFacingDirection()
    {
        if (!flipSpriteOnLeft) return;
        FlipSprite(isFacingLeft);
    }

    public void SetAttackerMoveState(AttackerState newState)
    {
        if (newState == CurrentAttackerState) return;

        // DEBUG: Check if we are switching to Left
        if (newState == AttackerState.AttackerRunLeft) {
             Debug.Log("Attacker Logic: Playing Left Animation: " + LEFT_ANIM);
        }

        switch (newState)
        {
            case AttackerState.Idle:
                PlayAnim(IDLE_ANIM);
                break;

            case AttackerState.AttackerRunRight:
                PlayAnim(RIGHT_ANIM);
                break;

            case AttackerState.AttackerRunLeft:
                PlayAnim(LEFT_ANIM);
                break;

            case AttackerState.AttackerRunUp:
                PlayAnim(UP_ANIM);
                break;

            case AttackerState.AttackerRunDown:
                PlayAnim(DOWN_ANIM);
                break;

            case AttackerState.AttackerAttack:
                PlayAnim(ATTACK_ANIM);
                break;
        }

        OnAttackerMoveStateChange?.Invoke(newState);
        CurrentAttackerState = newState;
    }

    private void PlayAnim(string animName)
    {
        if (animator != null) animator.Play(animName);
    }

    private void FlipSprite(bool faceLeft)
    {
        float xVal = Mathf.Abs(transform.localScale.x);
        if (faceLeft) xVal = -xVal; 
        
        transform.localScale = new Vector3(xVal, transform.localScale.y, transform.localScale.z);
    }
}