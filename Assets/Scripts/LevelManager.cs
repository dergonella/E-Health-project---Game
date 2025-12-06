using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages level data and scene transitions
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public string sceneName;
        public string description;
        public int targetScore;
        public float survivalTime;
        public bool hasHealthSystem;
        public bool hasShield;
        public bool hasSlowMotion;
        public bool hasFire;
        public bool hasPoison;
        public bool hasMines;
        public bool cobraInstantKill;
        public bool hasTimedChallenge;  // NEW: For Level 0.1 timed mode
        public float timeLimitSeconds;  // NEW: Time limit for timed challenges
        public bool convertExcessPointsToMoney; // NEW: Enable points-to-money conversion
    }

    [Header("Level Definitions")]
    public LevelData[] levels = new LevelData[7]; // Expanded to include Level 0.1 and 0.2

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLevels();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeLevels()
    {
        levels[0] = new LevelData
        {
            levelName = "Core Level",
            sceneName = "Level0_Core",
            description = "Basic chase mechanics - One hit death!",
            targetScore = 2000,
            survivalTime = 0f,
            hasHealthSystem = false,
            hasShield = false,
            hasSlowMotion = false,
            hasFire = false,
            hasPoison = false,
            hasMines = false,
            cobraInstantKill = true,
            hasTimedChallenge = false,
            timeLimitSeconds = 0f,
            convertExcessPointsToMoney = false
        };

        // NEW: Level 0.1 - Timed Challenge
        levels[1] = new LevelData
        {
            levelName = "Timed Challenge",
            sceneName = "Level0_1_TimedChallenge",
            description = "Reach 2000 points in 30 seconds! Excess points = Money!",
            targetScore = 2000,
            survivalTime = 0f,
            hasHealthSystem = false,
            hasShield = false,
            hasSlowMotion = false,
            hasFire = false,
            hasPoison = false,
            hasMines = false,
            cobraInstantKill = true,
            hasTimedChallenge = true,
            timeLimitSeconds = 30f,
            convertExcessPointsToMoney = true
        };

        // NEW: Level 0.2 - Growing Snakes Maze (with Health System)
        levels[2] = new LevelData
        {
            levelName = "Growing Snakes Maze",
            sceneName = "Level0.2",
            description = "Navigate the maze - Avoid growing snakes! 30 seconds to collect 2000 points.",
            targetScore = 2000,
            survivalTime = 0f,
            hasHealthSystem = true,  // ENABLED: Use health damage instead of instant kill
            hasShield = true,        // Player can use shield to block damage
            hasSlowMotion = false,
            hasFire = false,
            hasPoison = false,
            hasMines = false,
            cobraInstantKill = false,  // DISABLED: Snakes deal damage, not instant kill
            hasTimedChallenge = true,   // ENABLED: 30 second timer like Level 0.1
            timeLimitSeconds = 30f,
            convertExcessPointsToMoney = true  // ENABLED: Convert points to money on win
        };

        levels[3] = new LevelData
        {
            levelName = "Contact Zone",
            sceneName = "Level1_ContactZone",
            description = "Fire projectiles - Use Shield (Q) wisely!",
            targetScore = 2000,
            survivalTime = 90f,
            hasHealthSystem = true,
            hasShield = true,
            hasSlowMotion = false,
            hasFire = true,
            hasPoison = false,
            hasMines = false,
            cobraInstantKill = true,
            hasTimedChallenge = false,
            timeLimitSeconds = 0f,
            convertExcessPointsToMoney = false
        };

        levels[4] = new LevelData
        {
            levelName = "Toxic Grounds",
            sceneName = "Level2_ToxicGrounds",
            description = "Poison clouds - Manage status effects!",
            targetScore = 2500,
            survivalTime = 120f,
            hasHealthSystem = true,
            hasShield = false,
            hasSlowMotion = true,
            hasFire = false,
            hasPoison = true,
            hasMines = false,
            cobraInstantKill = true,
            hasTimedChallenge = false,
            timeLimitSeconds = 0f,
            convertExcessPointsToMoney = false
        };

        levels[5] = new LevelData
        {
            levelName = "Divorce Papers",
            sceneName = "Level3_DivorcePapers",
            description = "Final challenge - All mechanics combined!",
            targetScore = 3000,
            survivalTime = 0f,
            hasHealthSystem = true,
            hasShield = true,
            hasSlowMotion = true,
            hasFire = true,
            hasPoison = true,
            hasMines = true,
            cobraInstantKill = false,
            hasTimedChallenge = false,
            timeLimitSeconds = 0f,
            convertExcessPointsToMoney = false
        };

        levels[6] = new LevelData
        {
            levelName = "Dino Runner",
            sceneName = "Level4_DinoRunner",
            description = "Chrome Dino endless runner - Jump to survive!",
            targetScore = 1000,
            survivalTime = 0f,
            hasHealthSystem = false,
            hasShield = false,
            hasSlowMotion = false,
            hasFire = false,
            hasPoison = false,
            hasMines = false,
            cobraInstantKill = false,
            hasTimedChallenge = false,
            timeLimitSeconds = 0f,
            convertExcessPointsToMoney = false
        };
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            SceneManager.LoadScene(levels[levelIndex].sceneName);
        }
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public LevelData GetLevelData(int index)
    {
        if (index >= 0 && index < levels.Length)
        {
            return levels[index];
        }
        return null;
    }

    public LevelData GetCurrentLevelData()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        foreach (var level in levels)
        {
            if (level.sceneName == currentScene)
            {
                return level;
            }
        }
        return null;
    }
}
