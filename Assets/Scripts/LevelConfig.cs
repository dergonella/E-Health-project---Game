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

        // ULTRA HARD - +25% increase across all values
        // Fire rate: 3.1 -> 8.1 -> 19 shots per second
        // Damage: 24 -> 40 -> 63
        // Projectile speed: 7.5 -> 11 -> 15
        // Poison DPS: 19 -> 31 -> 48 per stack
        switch (level)
        {
            case 1:
                config.fireSnakeCount = 3;
                config.poisonSnakeCount = 0;
                config.snakeFireRate = 3.1f;      // +25% (was 2.5)
                config.snakeSpeed = 2.9f;         // +25% (was 2.3)
                config.snakeShootingRange = 11f;  // +22% (was 9)
                config.snakeMinShootDistance = 1.0f;
                config.projectileDamage = 24f;    // +26% (was 19)
                config.projectileSpeed = 7.5f;    // +25% (was 6)
                config.poisonDamagePerSecond = 19f;  // +27% (was 15)
                config.poisonSpeedReduction = 0.5f;  // +25% (was 0.4)
                break;
            case 2:
                config.fireSnakeCount = 0;
                config.poisonSnakeCount = 3;
                config.snakeFireRate = 8.1f;      // +25% (was 6.5)
                config.snakeSpeed = 3.6f;         // +24% (was 2.9)
                config.snakeShootingRange = 15f;  // +25% (was 12)
                config.snakeMinShootDistance = 0.6f;
                config.projectileDamage = 40f;    // +25% (was 32)
                config.projectileSpeed = 11f;     // +22% (was 9)
                config.poisonDamagePerSecond = 31f;  // +24% (was 25)
                config.poisonSpeedReduction = 0.69f; // +25% (was 0.55)
                break;
            case 3:
            default:
                config.fireSnakeCount = 2;
                config.poisonSnakeCount = 1;
                config.snakeFireRate = 19f;       // +27% (was 15) - EXTREME BULLET HELL!
                config.snakeSpeed = 4.4f;         // +26% (was 3.5)
                config.snakeShootingRange = 18f;  // +29% (was 14)
                config.snakeMinShootDistance = 0.2f;
                config.projectileDamage = 63f;    // +26% (was 50)
                config.projectileSpeed = 15f;     // +25% (was 12)
                config.poisonDamagePerSecond = 48f;  // +26% (was 38)
                config.poisonSpeedReduction = 0.81f; // +25% (was 0.65) - nearly frozen!
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
