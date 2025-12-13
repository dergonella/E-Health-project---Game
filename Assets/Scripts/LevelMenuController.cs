using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelMenuController : MonoBehaviour
{
    [Header("Drag all Level Buttons here in order (1-9)")]
    public Button[] levelButtons;

    private void Start()
    {
        // 1. Load the saved data
        LevelProgressManager.LoadProgress();

        // 2. Loop through all buttons to Lock or Unlock them
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNum = i + 1; // Array starts at 0, but levels start at 1

            if (levelNum > LevelProgressManager.HighestUnlockedLevel)
            {
                // LOCK this level
                levelButtons[i].interactable = false;

                // Optional: Make it look dark/disabled
                // You can also change the sprite to a "Lock" icon here if you want
            }
            else
            {
                // UNLOCK this level
                levelButtons[i].interactable = true;
            }
        }
    }

    // Connect this function to your buttons in the Inspector
    public void LoadLevel(int levelIndex)
    {
        // Double check just in case
        if (levelIndex <= LevelProgressManager.HighestUnlockedLevel)
        {
            // Assumes your scenes are named "Level1_ContactZone", etc.
            // You might need to adjust this string to match your exact scene names!
            // Or use Build Index if you prefer.
            SceneManager.LoadScene("Level" + levelIndex);
            // OR if using Build Index: SceneManager.LoadScene(levelIndex + offset);
        }
    }
}
