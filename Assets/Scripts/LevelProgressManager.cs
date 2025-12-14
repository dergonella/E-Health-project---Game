using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgressManager : MonoBehaviour
{
    // Simple key to save data on the computer
    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";

    // This variable holds the highest level the player can access
    // Default is 1 (only the first level is open)
    public static int HighestUnlockedLevel = 1;

    public static void LoadProgress()
    {
        // Check PlayerPrefs for saved data. If none exists, default to 1.
        HighestUnlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
    }

    public static void UnlockNextLevel(int completedLevelIndex)
    {
        // If the player just finished level 1, we want to unlock level 2.
        int nextLevel = completedLevelIndex + 1;

        // Only save if this is a NEW record (don't re-lock levels if replaying)
        if (nextLevel > HighestUnlockedLevel)
        {
            HighestUnlockedLevel = nextLevel;
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, HighestUnlockedLevel);
            PlayerPrefs.Save(); // Write to disk immediately
            Debug.Log("Progress Saved! Unlocked Level: " + HighestUnlockedLevel);
        }
    }

    // Call this to reset progress (useful for testing!)
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UNLOCKED_LEVEL_KEY);
        HighestUnlockedLevel = 1;
        Debug.Log("Progress Reset");
    }
}
