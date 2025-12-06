using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Snake Body Controller - Makes snakes grow with a following body trail
/// Attach this to the Snake parent GameObject (or the head)
/// The body segments smoothly follow the head like in classic Snake game
/// </summary>
public class SnakeBodyController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The snake's head transform (this moves via AI scripts)")]
    public Transform head;

    [Tooltip("Prefab for body segments (must have SpriteRenderer + Collider2D)")]
    public GameObject segmentPrefab;

    [Header("Body Settings")]
    [Tooltip("Starting number of body segments")]
    [SerializeField] private int initialSegments = 3;

    [Tooltip("Distance between each segment")]
    [SerializeField] private float segmentSpacing = 0.3f;

    [Tooltip("How fast segments follow (0-1, higher = tighter following)")]
    [SerializeField] private float followSpeed = 0.15f;

    [Header("Layer Setup")]
    [Tooltip("Layer for body segments (should not collide with each other)")]
    [SerializeField] private string bodyLayerName = "SnakeBody";

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    // Internal tracking
    private List<Transform> segments = new List<Transform>();
    private List<Vector3> positionHistory = new List<Vector3>();
    private int maxHistorySize = 200; // Limits memory usage

    void Start()
    {
        // Validation
        if (head == null)
        {
            head = transform; // Use self if not assigned
            Debug.LogWarning($"SnakeBodyController: Head not assigned on {gameObject.name}, using self");
        }

        if (segmentPrefab == null)
        {
            Debug.LogError($"SnakeBodyController: Segment Prefab not assigned on {gameObject.name}!");
            enabled = false;
            return;
        }

        // Initialize position history with head's starting position
        positionHistory.Add(head.position);

        // Spawn initial body segments
        for (int i = 0; i < initialSegments; i++)
        {
            Grow(1);
        }

        Debug.Log($"SnakeBodyController: {gameObject.name} initialized with {segments.Count} body segments");
    }

    void FixedUpdate()
    {
        if (head == null || segments.Count == 0) return;

        // 1. Record head's current position in history
        RecordHeadPosition();

        // 2. Update all body segments to follow the position history
        UpdateSegmentPositions();
    }

    /// <summary>
    /// Records the head's current position for body segments to follow
    /// </summary>
    private void RecordHeadPosition()
    {
        // Add current head position to history
        positionHistory.Insert(0, head.position);

        // Limit history size to prevent memory issues
        if (positionHistory.Count > maxHistorySize)
        {
            positionHistory.RemoveAt(positionHistory.Count - 1);
        }
    }

    /// <summary>
    /// Updates each body segment to follow the position history with spacing
    /// </summary>
    private void UpdateSegmentPositions()
    {
        // Each segment follows a point in the position history
        // Segment 0 follows closest to head, Segment N follows furthest back

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;

            // Calculate which history index this segment should target
            // We want segments spaced by 'segmentSpacing' distance
            int targetIndex = GetHistoryIndexForSegment(i);

            if (targetIndex < positionHistory.Count)
            {
                Vector3 targetPosition = positionHistory[targetIndex];

                // Smoothly move segment toward target position
                segments[i].position = Vector3.Lerp(
                    segments[i].position,
                    targetPosition,
                    followSpeed
                );

                // Optional: Rotate segment to face movement direction
                RotateSegmentTowardsTarget(segments[i], targetPosition);
            }
        }
    }

    /// <summary>
    /// Finds the correct history index for a segment based on spacing
    /// </summary>
    private int GetHistoryIndexForSegment(int segmentIndex)
    {
        // We want each segment to be 'segmentSpacing' distance behind the previous one
        // So we search through the position history to find a point that is
        // approximately 'segmentSpacing * (segmentIndex + 1)' distance from the head

        float targetDistance = segmentSpacing * (segmentIndex + 1);
        float accumulatedDistance = 0f;

        for (int i = 1; i < positionHistory.Count; i++)
        {
            float stepDistance = Vector3.Distance(positionHistory[i - 1], positionHistory[i]);
            accumulatedDistance += stepDistance;

            if (accumulatedDistance >= targetDistance)
            {
                return i;
            }
        }

        // If history is too short, use the last available position
        return positionHistory.Count - 1;
    }

    /// <summary>
    /// Rotates segment to face the direction it's moving (optional visual polish)
    /// </summary>
    private void RotateSegmentTowardsTarget(Transform segment, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - segment.position;
        if (direction.magnitude > 0.01f) // Only rotate if moving significantly
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            segment.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90 if sprite faces up
        }
    }

    /// <summary>
    /// Grows the snake by adding new segments to the tail
    /// Call this from other scripts when snake should grow (e.g., after eating food)
    /// </summary>
    /// <param name="amount">Number of segments to add</param>
    public void Grow(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            // Spawn new segment
            Vector3 spawnPosition = GetNewSegmentSpawnPosition();
            GameObject newSegment = Instantiate(segmentPrefab, spawnPosition, Quaternion.identity, transform);

            // Configure segment
            newSegment.name = $"BodySegment_{segments.Count}";

            // Set layer (prevents self-collision)
            int bodyLayer = LayerMask.NameToLayer(bodyLayerName);
            if (bodyLayer != -1)
            {
                newSegment.layer = bodyLayer;
            }
            else
            {
                Debug.LogWarning($"SnakeBodyController: Layer '{bodyLayerName}' not found! Create it in Project Settings → Tags and Layers");
            }

            // Add to segments list
            segments.Add(newSegment.transform);

            Debug.Log($"SnakeBodyController: {gameObject.name} grew! Total segments: {segments.Count}");
        }
    }

    /// <summary>
    /// Calculates where to spawn a new segment (at the tail)
    /// </summary>
    private Vector3 GetNewSegmentSpawnPosition()
    {
        if (segments.Count == 0)
        {
            // First segment spawns behind the head
            return head.position - head.up * segmentSpacing;
        }
        else
        {
            // New segments spawn behind the last segment
            Transform lastSegment = segments[segments.Count - 1];
            Vector3 direction = (lastSegment.position - head.position).normalized;
            return lastSegment.position + direction * segmentSpacing;
        }
    }

    /// <summary>
    /// Gets the current body segment count
    /// </summary>
    public int GetSegmentCount()
    {
        return segments.Count;
    }

    /// <summary>
    /// Removes segments from the tail (e.g., if snake takes damage)
    /// </summary>
    public void Shrink(int amount = 1)
    {
        amount = Mathf.Min(amount, segments.Count); // Don't shrink more than we have

        for (int i = 0; i < amount; i++)
        {
            if (segments.Count > 0)
            {
                int lastIndex = segments.Count - 1;
                Transform segmentToRemove = segments[lastIndex];
                segments.RemoveAt(lastIndex);

                Destroy(segmentToRemove.gameObject);
                Debug.Log($"SnakeBodyController: {gameObject.name} shrank! Total segments: {segments.Count}");
            }
        }
    }

    /// <summary>
    /// Debug visualization
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;

        // Draw position history
        Gizmos.color = Color.yellow;
        for (int i = 0; i < positionHistory.Count - 1; i++)
        {
            Gizmos.DrawLine(positionHistory[i], positionHistory[i + 1]);
        }

        // Draw segment target positions
        Gizmos.color = Color.green;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] != null)
            {
                int targetIndex = GetHistoryIndexForSegment(i);
                if (targetIndex < positionHistory.Count)
                {
                    Gizmos.DrawWireSphere(positionHistory[targetIndex], 0.1f);
                }
            }
        }
    }
}
