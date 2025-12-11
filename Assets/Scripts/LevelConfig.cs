using UnityEngine;

/// <summary>
/// ScriptableObject that defines level configuration.
/// Create one for each level variant.
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "E-Health Game/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    public int levelNumber = 1;

    [Header("Win Condition")]
    [Tooltip("Score needed to complete the level")]
    public int targetScore = 2000;

    [Header("Snake Configuration")]
    [Tooltip("Number of fire snakes (shoot fire projectiles)")]
    public int fireSnakeCount = 3;

    [Tooltip("Number of poison snakes (shoot poison projectiles)")]
    public int poisonSnakeCount = 0;

    [Header("Player Abilities")]
    [Tooltip("Enable medkit usage (H key)")]
    public bool enableMedkit = true;

    [Tooltip("Enable shield usage (F key)")]
    public bool enableShield = true;

    [Tooltip("Enable bullet slowdown (O key) - All persona levels")]
    public bool enableBulletSlowdown = true;

    [Header("Player Starting Resources")]
    public int startingMedkits = 3;
    public int startingShields = 3;

    [Header("Snake Settings")]
    [Tooltip("Enable projectile shooting for snakes")]
    public bool snakesCanShoot = true;

    [Tooltip("Time between snake shots")]
    public float snakeShootInterval = 3f;

    [Tooltip("Snake movement speed")]
    public float snakeSpeed = 2f;

    [Header("Optional Features")]
    [Tooltip("Enable snake body growth")]
    public bool enableSnakeGrowth = true;

    [Tooltip("Seconds between snake growth")]
    public float growthInterval = 15f;

    /// <summary>
    /// Get the total number of snakes
    /// </summary>
    public int TotalSnakes => fireSnakeCount + poisonSnakeCount;

    /// <summary>
    /// Create a default Level 1 config (3 fire snakes)
    /// </summary>
    public static LevelConfig CreateLevel1Default()
    {
        var config = CreateInstance<LevelConfig>();
        config.levelName = "Level 1";
        config.levelNumber = 1;
        config.targetScore = 2000;
        config.fireSnakeCount = 3;
        config.poisonSnakeCount = 0;
        config.enableMedkit = true;
        config.enableShield = true;
        config.enableBulletSlowdown = true; // Slowdown enabled for all levels
        return config;
    }

    /// <summary>
    /// Create a default Level 2 config (3 poison snakes + bullet slowdown)
    /// </summary>
    public static LevelConfig CreateLevel2Default()
    {
        var config = CreateInstance<LevelConfig>();
        config.levelName = "Level 2";
        config.levelNumber = 2;
        config.targetScore = 2000;
        config.fireSnakeCount = 0;
        config.poisonSnakeCount = 3;
        config.enableMedkit = true;
        config.enableShield = true;
        config.enableBulletSlowdown = true; // Slowdown enabled in Level 2
        return config;
    }

    /// <summary>
    /// Create a default Level 3 config (2 fire + 1 poison + bullet slowdown)
    /// </summary>
    public static LevelConfig CreateLevel3Default()
    {
        var config = CreateInstance<LevelConfig>();
        config.levelName = "Level 3";
        config.levelNumber = 3;
        config.targetScore = 2000;
        config.fireSnakeCount = 2;
        config.poisonSnakeCount = 1;
        config.enableMedkit = true;
        config.enableShield = true;
        config.enableBulletSlowdown = true; // Slowdown enabled in Level 3
        return config;
    }
}
