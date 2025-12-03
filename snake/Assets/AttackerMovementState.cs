using UnityEngine;
using System;
using System.Collections.Generic;

public class AttackerMovementState : MonoBehaviour
{
    public enum AttackerState
    {
        Idle,
        AttackerRunRight,
        AttackerRunLeft,
        AttackerRunUp,
        AttackerRunDown,
        AttackerAttack
    }

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
    public float movementDelay = 0.5f; 

    [Header("Interaction Settings")]
    public float catchDistance = 0.5f;
    
    // NEW: Cooldown to prevent instant 3-hit kill
    public float hitCooldown = 2.0f; 
    private float lastHitTime = -10f;

    [Header("Visual Settings")]
    public bool flipSpriteOnLeft = true; 

    // ANIMATION NAMES
    private const string IDLE_ANIM = "Idle"; 
    private const string RIGHT_ANIM = "attacker run right";
    private const string LEFT_ANIM = "attacker run left"; 
    private const string UP_ANIM = "attacker run up";
    private const string DOWN_ANIM = "attacker run down";
    private const string ATTACK_ANIM = "attacker attack"; 
    
    // ACTION EVENT
    public static Action<AttackerState> OnAttackerMoveStateChange;

    private Animator animator;
    private Transform playerTransform;
    private playermovementstate playerScript;
    private PlayerHealth playerHealthScript; 

    private Queue<GhostSnapshot> ghostTrail = new Queue<GhostSnapshot>();
    private Vector3 currentTargetPosition;
    
    private bool isFacingLeft = false;
    private bool hasStartedChasing = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        playerScript = FindObjectOfType<playermovementstate>();
        if (playerScript != null)
        {
            playerTransform = playerScript.transform;
            currentTargetPosition = transform.position; 
            
            // GET HEALTH SCRIPT
            playerHealthScript = playerScript.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogError("Attacker Error: Could not find 'playermovementstate'!");
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null || playerScript == null) return;
        
        // If player is dead, stop everything
        if (playerHealthScript != null && playerHealthScript.IsDead()) 
        {
            SetAttackerMoveState(AttackerState.Idle);
            return;
        }

        // --- STEP 1: CHECK FOR HIT ---
        float distanceToRealPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToRealPlayer <= catchDistance)
        {
            if (Time.time - lastHitTime > hitCooldown)
            {
                PerformAttack();
            }
            return;
        }

        // --- STEP 2: IDLE START LOGIC ---
        if (!hasStartedChasing)
        {
            if (playerScript.CurrentMoveState != playermovementstate.MoveState.Idle) hasStartedChasing = true; 
            else { SetAttackerMoveState(AttackerState.Idle); return; }
        }

        // --- STEP 3: RECORD PLAYER ---
        AttackerState recordedState = MapPlayerStateToAttacker(playerScript.CurrentMoveState);
        ghostTrail.Enqueue(new GhostSnapshot(playerTransform.position, recordedState, Time.time));

        // --- STEP 4: REPLAY HISTORY ---
        AttackerState stateToPlay = AttackerState.Idle;
        if (ghostTrail.Count > 0 && Time.time - ghostTrail.Peek().timestamp >= movementDelay)
        {
            GhostSnapshot targetSnapshot = ghostTrail.Dequeue();
            currentTargetPosition = targetSnapshot.position;
            stateToPlay = targetSnapshot.state;
        }

        // --- STEP 5: MOVE ---
        float distToTarget = Vector3.Distance(transform.position, currentTargetPosition);
        if (distToTarget > stopDistance)
        {
            float xDiff = currentTargetPosition.x - transform.position.x;
            if (Mathf.Abs(xDiff) > 0.01f) isFacingLeft = xDiff < 0; 

            if (stateToPlay == AttackerState.Idle)
            {
                Vector3 moveDirection = (currentTargetPosition - transform.position).normalized;
                stateToPlay = CalculateStateFromVector(moveDirection);
            }
            SetAttackerMoveState(stateToPlay);
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, speed * Time.deltaTime);
        }
        else
        {
            if (ghostTrail.Count == 0) SetAttackerMoveState(AttackerState.Idle);
        }

        EnforceFacingDirection();
    }

    private void PerformAttack()
    {
        Debug.Log("Attacker Hits Player!");
        lastHitTime = Time.time; 

        // 1. Face Player
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        if (Mathf.Abs(directionToPlayer.x) > 0.01f) { isFacingLeft = directionToPlayer.x < 0; EnforceFacingDirection(); }

        // 2. Play Attack Animation
        SetAttackerMoveState(AttackerState.AttackerAttack);

        // 3. Deal Damage AND Pass Position for Knockback
        if (playerHealthScript != null)
        {
            // CHANGE: We now pass 'transform.position' so the player knows where to fly away from
            playerHealthScript.TakeDamage(transform.position);
        }
    }

    // --- HELPERS ---
    private AttackerState CalculateStateFromVector(Vector3 dir) {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) return dir.x > 0 ? AttackerState.AttackerRunRight : AttackerState.AttackerRunLeft;
        else return dir.y > 0 ? AttackerState.AttackerRunUp : AttackerState.AttackerRunDown;
    }

    private AttackerState MapPlayerStateToAttacker(playermovementstate.MoveState pState) {
        switch (pState) {
            case playermovementstate.MoveState.OwletRunRight: return AttackerState.AttackerRunRight;
            case playermovementstate.MoveState.OwletRunLeft: return AttackerState.AttackerRunLeft;
            case playermovementstate.MoveState.OwletRunUp: return AttackerState.AttackerRunUp;
            case playermovementstate.MoveState.OwletRunDown: return AttackerState.AttackerRunDown;
            default: return AttackerState.Idle;
        }
    }

    private void EnforceFacingDirection() { if (!flipSpriteOnLeft) return; FlipSprite(isFacingLeft); }

    public void SetAttackerMoveState(AttackerState newState) {
        if (newState == CurrentAttackerState) return;
        switch (newState) {
            case AttackerState.Idle: PlayAnim(IDLE_ANIM); break;
            case AttackerState.AttackerRunRight: PlayAnim(RIGHT_ANIM); break;
            case AttackerState.AttackerRunLeft: PlayAnim(LEFT_ANIM); break;
            case AttackerState.AttackerRunUp: PlayAnim(UP_ANIM); break;
            case AttackerState.AttackerRunDown: PlayAnim(DOWN_ANIM); break;
            case AttackerState.AttackerAttack: PlayAnim(ATTACK_ANIM); break;
        }
        OnAttackerMoveStateChange?.Invoke(newState);
        CurrentAttackerState = newState;
    }

    private void PlayAnim(string animName) { if (animator != null) animator.Play(animName); }
    private void FlipSprite(bool faceLeft) {
        float xVal = Mathf.Abs(transform.localScale.x);
        if (faceLeft) xVal = -xVal; 
        transform.localScale = new Vector3(xVal, transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); 
        Gizmos.DrawSphere(transform.position, catchDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(currentTargetPosition, 0.2f);
        Gizmos.DrawLine(transform.position, currentTargetPosition);

        if (ghostTrail != null && ghostTrail.Count > 0)
        {
            Gizmos.color = Color.green;
            Vector3 lastPos = transform.position;
            foreach (var snapshot in ghostTrail)
            {
                Gizmos.DrawLine(lastPos, snapshot.position);
                Gizmos.DrawSphere(snapshot.position, 0.05f); 
                lastPos = snapshot.position;
            }
        }
    }
}