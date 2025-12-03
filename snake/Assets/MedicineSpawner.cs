using UnityEngine;

public class MedicineSpawner : MonoBehaviour
{
    [Header("What to Spawn")]
    // Drag your 3 medicine PREFABS here (Life, Speed, Magic)
    public GameObject[] medicinePrefabs; 

    [Header("Timing")]
    public float spawnInterval = 4.0f; // Spawn a new one every 4 seconds
    private float timer;

    [Header("Limits")]
    public int maxTotalSpawns = 3; // The spawner will stop after creating this many items
    private int currentSpawnCount = 0;

    [Header("Spawn Area (Match your Player Bounds)")]
    public float minX = -7.5f; 
    public float maxX = 7.5f;
    public float minY = -1.0f;
    public float maxY = 1.0f;

    void Update()
    {
        // 1. Check if we reached the limit. If so, stop doing anything.
        if (currentSpawnCount >= maxTotalSpawns) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnMedicine();
            timer = 0;
        }
    }

    void SpawnMedicine()
    {
        if (medicinePrefabs.Length == 0) return;

        // 2. Pick a random spot inside your game boundaries
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(randomX, randomY, 0);

        // 3. Pick a random medicine type (Life, Speed, or Magic)
        int randomIndex = Random.Range(0, medicinePrefabs.Length);
        
        // 4. Create it in the world
        Instantiate(medicinePrefabs[randomIndex], spawnPos, Quaternion.identity);

        // 5. Increase the counter
        currentSpawnCount++;
        Debug.Log($"Spawned Medicine {currentSpawnCount} of {maxTotalSpawns}");
    }
}