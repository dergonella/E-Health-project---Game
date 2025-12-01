using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    // --- Reference to your state script ---
    private playermovementstate stateScript;

    // --- Variables to hold button states (for touch) ---
    private bool isPressingUp = false;
    private bool isPressingDown = false;
    private bool isPressingLeft = false;
    private bool isPressingRight = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stateScript = GetComponent<playermovementstate>();

        if (rb == null) Debug.LogWarning("PlayerMovement: Rigidbody2D missing!");
        if (stateScript == null) Debug.LogError("PlayerMovement: playermovementstate script missing!");
    }

    void Update()
    {
        // 1. Reset Input
        movement = Vector2.zero;

        // 2. Keyboard Input (GetAxisRaw returns 0 immediately when released)
        movement.x = Input.GetAxisRaw("Horizontal"); 
        movement.y = Input.GetAxisRaw("Vertical");

        // 3. Touch Input (Only if keyboard is not being used)
        if (movement.x == 0 && movement.y == 0)
        {
            if (isPressingLeft) movement.x = -1;
            else if (isPressingRight) movement.x = 1;
            
            if (isPressingDown) movement.y = -1;
            else if (isPressingUp) movement.y = 1;
        }

        // 4. Normalize (Prevents diagonal movement from being faster)
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // 5. Send Animation State
        // This logic decides which animation to play
        if (stateScript != null)
        {
            // Check Vertical Movement
            if (movement.y > 0.01f)
            {
                stateScript.SetMoveState(playermovementstate.MoveState.OwletRunUp);
            }
            else if (movement.y < -0.01f)
            {
                stateScript.SetMoveState(playermovementstate.MoveState.OwletRunDown);
            }
            // Check Horizontal Movement
            else if (movement.x > 0.01f)
            {
                stateScript.SetMoveState(playermovementstate.MoveState.OwletRunRight);
            }
            else if (movement.x < -0.01f)
            {
                stateScript.SetMoveState(playermovementstate.MoveState.OwletRunLeft);
            }
            else
            {
                // --------------------------------------------------
                // THIS IS THE IDLE LOGIC
                // If x is 0 and y is 0, we enter this block.
                // --------------------------------------------------
                stateScript.SetMoveState(playermovementstate.MoveState.Idle);
            }
        }
    }

    void FixedUpdate()
    {
        // Physics Movement
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // --- Public Methods for UI Buttons ---
    public void OnPointerDown_Up() { isPressingUp = true; }
    public void OnPointerDown_Down() { isPressingDown = true; }
    public void OnPointerDown_Left() { isPressingLeft = true; }
    public void OnPointerDown_Right() { isPressingRight = true; }

    public void OnPointerUp_Up() { isPressingUp = false; }
    public void OnPointerUp_Down() { isPressingDown = false; }
    public void OnPointerUp_Left() { isPressingLeft = false; }
    public void OnPointerUp_Right() { isPressingRight = false; }
}