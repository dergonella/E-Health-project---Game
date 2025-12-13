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

    [Tooltip("Shots per second (higher = faster shooting). Level 1: 1, Level 2: 3, Level 3: 9")]
    public float snakeFireRate = 1f;

    [Tooltip("Snake movement speed")]
    public float snakeSpeed = 2f;

    [Tooltip("How far snakes can shoot")]
    public float snakeShootingRange = 8f;

    [Tooltip("Minimum distance before snakes shoot (fairness)")]
    public float snakeMinShootDistance = 2f;

    [Header("Projectile Settings")]
    [Tooltip("Damage per projectile hit. Level 1: 10, Level 2: 15, Level 3: 20")]
    public float projectileDamage = 10f;

    [Tooltip("How fast projectiles travel. Level 1: 3, Level 2: 4, Level 3: 5")]
    public float projectileSpeed = 3f;

    [Header("Poison Settings (Level 2+ has poison snakes)")]
    [Tooltip("Base poison damage per second per stack. Level 1: 8, Level 2: 12, Level 3: 16")]
    public float poisonDamagePerSecond = 8f;

    [Tooltip("Speed reduction while poisoned (0.4 = 40% slower)")]
    public float poisonSpeedReduction = 0.4f;

    [Header("Optional Features")]
    [Tooltip("Enable snake body growth")]
    public bool enableSnakeGrowth = true;

    [Tooltip("Seconds between snake growth")]
    public float growthInterval = 15f;

    /// <summary>
    /// Get the total number of snakes
    /// </summary>
    public int TotalSnakes => fireSnakeCount + poisonSnakeCount;

    // ========================================
    // LEVEL CONFIG FACTORY METHODS
    // Fire rate TRIPLES each level: L1=1, L2=3, L3=9
    // Damage increases: L1=10, L2=15, L3=20
    // ========================================

    /// <summary>
    /// Create config for any persona and level combination
    /// </summary>
    public static LevelConfig CreateConfig(string persona, int level)
    {
        var config = CreateInstance<LevelConfig>();
        config.levelName = $"{persona} Level {level}";
        config.levelNumber = level;
        config.targetScore = 2000;
        config.enableMedkit = true;
        config.enableShield = true;
        config.enableBulletSlowdown = true;

        // EVEN HARDER - +25-30% from previous values
        // Fire rate: 2.5 -> 6.5 -> 15 shots per second
        // Damage: 19 -> 32 -> 50
        // Projectile speed: 6 -> 9 -> 12
        // Poison DPS: 15 -> 25 -> 38 per stack
        switch (level)
        {
            case 1:
                config.fireSnakeCount = 3;
                config.poisonSnakeCount = 0;
                config.snakeFireRate = 2.5f;      // +25% (was 2)
                config.snakeSpeed = 2.3f;         // +15% (was 2.0)
                config.snakeShootingRange = 9f;   // +12% (was 8)
                config.snakeMinShootDistance = 1.2f;
                config.projectileDamage = 19f;    // +27% (was 15)
                config.projectileSpeed = 6f;      // +20% (was 5)
                config.poisonDamagePerSecond = 15f;  // +25% (was 12)
                config.poisonSpeedReduction = 0.4f;  // +14% (was 0.35)
                break;
            case 2:
                config.fireSnakeCount = 0;
                config.poisonSnakeCount = 3;
                config.snakeFireRate = 6.5f;      // +30% (was 5)
                config.snakeSpeed = 2.9f;         // +16% (was 2.5)
                config.snakeShootingRange = 12f;  // +20% (was 10)
                config.snakeMinShootDistance = 0.8f;
                config.projectileDamage = 32f;    // +28% (was 25)
                config.projectileSpeed = 9f;      // +29% (was 7)
                config.poisonDamagePerSecond = 25f;  // +25% (was 20)
                config.poisonSpeedReduction = 0.55f; // +10% (was 0.5)
                break;
            case 3:
            default:
                config.fireSnakeCount = 2;
                config.poisonSnakeCount = 1;
                config.snakeFireRate = 15f;       // +25% (was 12) - BULLET HELL!
                config.snakeSpeed = 3.5f;         // +17% (was 3.0)
                config.snakeShootingRange = 14f;  // +17% (was 12)
                config.snakeMinShootDistance = 0.3f;
                config.projectileDamage = 50f;    // +25% (was 40)
                config.projectileSpeed = 12f;     // +20% (was 10)
                config.poisonDamagePerSecond = 38f;  // +27% (was 30)
                config.poisonSpeedReduction = 0.65f; // +8% (was 0.6) - nearly immobile!
                break;
        }

        Debug.Log($"[LevelConfig] Created {persona} Level {level}: FireRate={config.snakeFireRate}, Damage={config.projectileDamage}, PoisonDPS={config.poisonDamagePerSecond}");
        return config;
    }

    // ========================================
    // BRIGHTGROVE PERSONA (3 levels)
    // ========================================

    public static LevelConfig CreateBrightgroveLevel1()
    {
        return CreateConfig("Brightgrove", 1);
    }

    public static LevelConfig CreateBrightgroveLevel2()
    {
        return CreateConfig("Brightgrove", 2);
    }

    public static LevelConfig CreateBrightgroveLevel3()
    {
        return CreateConfig("Brightgrove", 3);
    }

    // ========================================
    // SILVERGROVE PERSONA (3 levels)
    // ========================================

    public static LevelConfig CreateSilvergroveLevel1()
    {
        return CreateConfig("Silvergrove", 1);
    }

    public static LevelConfig CreateSilvergroveLevel2()
    {
        return CreateConfig("Silvergrove", 2);
    }

    public static LevelConfig CreateSilvergroveLevel3()
    {
        return CreateConfig("Silvergrove", 3);
    }

    // ========================================
    // STONEGROVE PERSONA (3 levels)
    // ========================================

    public static LevelConfig CreateStonegroveLevel1()
    {
        return CreateConfig("Stonegrove", 1);
    }

    public static LevelConfig CreateStonegroveLevel2()
    {
        return CreateConfig("Stonegrove", 2);
    }

    public static LevelConfig CreateStonegroveLevel3()
    {
        return CreateConfig("Stonegrove", 3);
    }

    // ========================================
    // LEGACY METHODS (for backward compatibility)
    // ========================================

    /// <summary>
    /// Create a default Level 1 config (backward compatible)
    /// </summary>
    public static LevelConfig CreateLevel1Default()
    {
        return CreateConfig("Default", 1);
    }

    /// <summary>
    /// Create a default Level 2 config (backward compatible)
    /// </summary>
    public static LevelConfig CreateLevel2Default()
    {
        return CreateConfig("Default", 2);
    }

    /// <summary>
    /// Create a default Level 3 config (backward compatible)
    /// </summary>
    public static LevelConfig CreateLevel3Default()
    {
        return CreateConfig("Default", 3);
    }
}
