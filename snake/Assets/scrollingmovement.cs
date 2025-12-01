using UnityEngine;

public class scrollingmovement : MonoBehaviour
{
    public float speed;

    // Limit for the background loop (based on your previous code)
    private float scrollLimit = 135f;

    private playermovementstate playerStateScript;

    void Start()
    {
        playerStateScript = FindObjectOfType<playermovementstate>();

        if (playerStateScript == null)
        {
            Debug.LogError("Background Error: Could not find 'playermovementstate'!");
        }
    }

    void Update()
    {
        if (playerStateScript == null) return;

        // --- LOGIC FOR RUNNING RIGHT ---
        if (playerStateScript.CurrentMoveState == playermovementstate.MoveState.OwletRunRight)
        {
            // Move Background LEFT to simulate player moving Right
            transform.Translate(Vector3.left * speed * Time.deltaTime);

            // If background goes too far Left, snap it to the Right side
            if (transform.position.x < -scrollLimit)
            {
                transform.position = new Vector2(scrollLimit, transform.position.y);
            }
        }
        // --- LOGIC FOR RUNNING LEFT ---
        else if (playerStateScript.CurrentMoveState == playermovementstate.MoveState.OwletRunLeft)
        {
            // Move Background RIGHT to simulate player moving Left
            transform.Translate(Vector3.right * speed * Time.deltaTime);

            // If background goes too far Right, snap it to the Left side
            // (This prevents falling off when running left)
            if (transform.position.x > scrollLimit)
            {
                transform.position = new Vector2(-scrollLimit, transform.position.y);
            }
        }
    }
}
