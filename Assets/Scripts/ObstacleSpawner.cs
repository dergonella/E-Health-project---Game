using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject obstaclePrefab;
    public float spawnInterval = 2f;
    public float minSpawnInterval = 0.8f;
    public float spawnX = 12f;
    public float groundY = -2f;
    public float lowObstacleY = -1f; // For obstacles player needs to duck under

    [Header("Difficulty")]
    public float difficultyIncreaseRate = 0.98f; // Multiply spawn interval by this each spawn
    public float scoreForDifficultyIncrease = 100f; // Increase difficulty every 100 points

    private float currentSpawnInterval;
    private float nextDifficultyScore = 100f;
    private RunnerGameManager gameManager;
    private Coroutine spawnCoroutine;

    void Start()
    {
        gameManager = FindObjectOfType<RunnerGameManager>();
        currentSpawnInterval = spawnInterval;
        spawnCoroutine = StartCoroutine(SpawnObstacles());
    }

    IEnumerator SpawnObstacles()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnInterval);

            if (gameManager != null && !gameManager.isGameOver)
            {
                SpawnObstacle();

                // Increase difficulty based on score
                if (gameManager.score >= nextDifficultyScore)
                {
                    IncreaseDifficulty();
                    nextDifficultyScore += scoreForDifficultyIncrease;
                }
            }
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null) return;

        // Randomly choose obstacle type: ground (jump over) or low (duck under)
        bool isLowObstacle = Random.value > 0.5f;
        float yPosition = isLowObstacle ? lowObstacleY : groundY;

        GameObject obstacle = Instantiate(obstaclePrefab, new Vector3(spawnX, yPosition, 0), Quaternion.identity);

        // Set obstacle type in its script
        RunnerObstacle obstacleScript = obstacle.GetComponent<RunnerObstacle>();
        if (obstacleScript != null)
        {
            obstacleScript.isLowObstacle = isLowObstacle;
        }
    }

    void IncreaseDifficulty()
    {
        currentSpawnInterval *= difficultyIncreaseRate;
        if (currentSpawnInterval < minSpawnInterval)
        {
            currentSpawnInterval = minSpawnInterval;
        }
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
}
