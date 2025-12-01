using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns obstacles (stalactites and stalagmites) for the endless runner
/// Dynamically adjusts spawn rate and obstacle speed based on difficulty
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] stalagmitePrefabs; // Ground obstacles (jump over)
    [SerializeField] private GameObject[] stalactitePrefabs;  // Ceiling obstacles (duck under)

    [Header("Spawn Settings")]
    [SerializeField] private float spawnXPosition = 12f;
    [SerializeField] private float groundYPosition = -3f;
    [SerializeField] private float ceilingYPosition = 4f;
    [SerializeField] private float baseSpawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.8f;

    [Header("Difficulty Settings")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float maxSpeed = 13f;
    [SerializeField] private float speedIncreaseRate = 0.2f;
    [SerializeField] private float difficultyFactor = 0.99f; // For exponential curve

    private float currentSpeed;
    private float currentSpawnInterval;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private float gameTime = 0f;

    void Start()
    {
        currentSpeed = baseSpeed;
        currentSpawnInterval = baseSpawnInterval;
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            gameTime = 0f;
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // Stop all existing obstacles
        ObstacleMovement[] obstacles = FindObjectsOfType<ObstacleMovement>();
        foreach (var obstacle in obstacles)
        {
            obstacle.Stop();
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            SpawnObstacle();
            UpdateDifficulty();
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    void SpawnObstacle()
    {
        // Randomly choose between ground and ceiling obstacle
        bool isGroundObstacle = Random.value > 0.5f;

        GameObject obstaclePrefab;
        float yPosition;

        if (isGroundObstacle)
        {
            // Spawn stalagmite (ground obstacle)
            if (stalagmitePrefabs.Length > 0)
            {
                obstaclePrefab = stalagmitePrefabs[Random.Range(0, stalagmitePrefabs.Length)];
                yPosition = groundYPosition;
            }
            else
            {
                Debug.LogWarning("No stalagmite prefabs assigned!");
                return;
            }
        }
        else
        {
            // Spawn stalactite (ceiling obstacle)
            if (stalactitePrefabs.Length > 0)
            {
                obstaclePrefab = stalactitePrefabs[Random.Range(0, stalactitePrefabs.Length)];
                yPosition = ceilingYPosition;
            }
            else
            {
                Debug.LogWarning("No stalactite prefabs assigned!");
                return;
            }
        }

        // Create obstacle
        Vector3 spawnPosition = new Vector3(spawnXPosition, yPosition, 0f);
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
        obstacle.transform.SetParent(transform);

        // Initialize movement
        ObstacleMovement movement = obstacle.GetComponent<ObstacleMovement>();
        if (movement == null)
        {
            movement = obstacle.AddComponent<ObstacleMovement>();
        }
        movement.Initialize(currentSpeed);
    }

    void UpdateDifficulty()
    {
        gameTime += currentSpawnInterval;

        // Update speed using exponential curve: speed = baseSpeed + (maxSpeed - baseSpeed) * (1 - C^t)
        float t = gameTime / 10f; // Scale time for smoother progression
        float difficultyProgress = 1f - Mathf.Pow(difficultyFactor, t);
        currentSpeed = baseSpeed + (maxSpeed - baseSpeed) * difficultyProgress;

        // Update spawn interval (decreases over time)
        currentSpawnInterval = Mathf.Lerp(baseSpawnInterval, minSpawnInterval, difficultyProgress);
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetGameTime()
    {
        return gameTime;
    }

    void OnDrawGizmos()
    {
        // Visualize spawn positions
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(spawnXPosition, groundYPosition, 0f), 0.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector3(spawnXPosition, ceilingYPosition, 0f), 0.5f);
    }
}
