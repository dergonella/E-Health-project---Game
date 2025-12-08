using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Diagnostic tool for Level 0.2 - Reports issues with walls and snakes
/// Attach this to any GameObject in Level 0.2 scene (e.g., GameManager)
/// Check Console for diagnostic messages
/// </summary>
public class Level02Diagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("===== LEVEL 0.2 DIAGNOSTIC =====");

        // Check for Tilemaps
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        Debug.Log($"[Diagnostic] Found {tilemaps.Length} Tilemap(s) in scene");

        foreach (Tilemap tilemap in tilemaps)
        {
            Debug.Log($"  - Tilemap: {tilemap.gameObject.name}");

            // Check for collider
            TilemapCollider2D tileCollider = tilemap.GetComponent<TilemapCollider2D>();
            if (tileCollider != null)
            {
                Debug.Log($"    ✓ Has TilemapCollider2D (UsedByComposite: {tileCollider.usedByComposite})");
            }
            else
            {
                Debug.LogWarning($"    ✗ MISSING TilemapCollider2D!");
            }

            // Check for composite collider
            CompositeCollider2D composite = tilemap.GetComponent<CompositeCollider2D>();
            if (composite != null)
            {
                Debug.Log($"    ✓ Has CompositeCollider2D");
            }

            // Check for rigidbody
            Rigidbody2D rb = tilemap.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"    ✓ Has Rigidbody2D (Type: {rb.bodyType})");
                if (rb.bodyType != RigidbodyType2D.Static)
                {
                    Debug.LogWarning($"    ⚠ Rigidbody should be STATIC, but is {rb.bodyType}!");
                }
            }

            // Check layer
            Debug.Log($"    Layer: {LayerMask.LayerToName(tilemap.gameObject.layer)}");
        }

        // Check for Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"[Diagnostic] Player found: {player.name}");

            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Debug.Log($"  ✓ Player has Rigidbody2D (Type: {playerRb.bodyType})");
            }
            else
            {
                Debug.LogError($"  ✗ Player MISSING Rigidbody2D!");
            }

            Collider2D playerCol = player.GetComponent<Collider2D>();
            if (playerCol != null)
            {
                Debug.Log($"  ✓ Player has Collider2D ({playerCol.GetType().Name})");
            }
            else
            {
                Debug.LogError($"  ✗ Player MISSING Collider2D!");
            }

            Debug.Log($"  Layer: {LayerMask.LayerToName(player.layer)}");
        }
        else
        {
            Debug.LogError("[Diagnostic] NO PLAYER FOUND!");
        }

        // Check for Snakes
        SnakeBodyController[] snakes = FindObjectsOfType<SnakeBodyController>();
        Debug.Log($"[Diagnostic] Found {snakes.Length} Snake(s) with SnakeBodyController");

        foreach (SnakeBodyController snake in snakes)
        {
            Debug.Log($"  - Snake: {snake.gameObject.name}");

            if (snake.segmentPrefab == null)
            {
                Debug.LogError($"    ✗ MISSING SegmentPrefab!");
            }
            else
            {
                Debug.Log($"    ✓ SegmentPrefab assigned: {snake.segmentPrefab.name}");
            }

            Debug.Log($"    Initial Segments: {snake.GetSegmentCount()}");
        }

        // Check for CobraAI
        CobraAI[] cobras = FindObjectsOfType<CobraAI>();
        Debug.Log($"[Diagnostic] Found {cobras.Length} Cobra(s) with CobraAI");

        Debug.Log("===== END DIAGNOSTIC =====");
    }
}
