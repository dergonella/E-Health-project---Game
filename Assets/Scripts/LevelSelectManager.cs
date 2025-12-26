using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Include this if you want to control the Title text

public class LevelSelectManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Type the EXACT name of your scenes here (e.g., Silvergrove_Level1, Stonegrove_Level2, etc.)")]
    public string[] levelSceneNames;

    [Header("UI References")]
    [Tooltip("Drag all Level Buttons here in order (1-9)")]
    public Button[] levelButtons;

    [Tooltip("Optional: Drag your Title Text here if you want to change it via code")]
    public TextMeshProUGUI titleText;

    private void Start()
    {
        // 1. Initialize the Title (Optional, from MenuManager)
        if (titleText != null)
        {
            titleText.text = "SELECT LEVEL";
        }

        // 2. Load the Save Data
        LevelProgressManager.LoadProgress();

        // 3. Setup Buttons (The "Union" Logic)
        for (int i = 0; i < levelButtons.Length; i++)
        {
            // Safety Check: Skip empty slots
            if (levelButtons[i] == null) continue;

            int levelIndex = i;         // 0, 1, 2... (For Array)
            int levelNumber = i + 1;    // 1, 2, 3... (For Game Logic)

            // --- A. LOCKING LOGIC ---
            if (levelNumber > LevelProgressManager.HighestUnlockedLevel)
            {
                // Level is Locked
                levelButtons[i].interactable = false;

                // Dim the button to look disabled
                var colors = levelButtons[i].colors;
                colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Darker Grey
                levelButtons[i].colors = colors;
            }
            else
            {
                // Level is Unlocked
                levelButtons[i].interactable = true;
            }

            // --- B. CLICK LOGIC (Automatic Connection) ---
            // This removes the need to drag things in the Inspector's "On Click()" list!
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
        }
    }

    // This function is called automatically by the code above
    public void LoadLevel(int arrayIndex)
    {
        // 1. Check if the index is valid for our names list
        if (arrayIndex < levelSceneNames.Length)
        {
            string sceneToLoad = levelSceneNames[arrayIndex];

            Debug.Log($"Button {arrayIndex + 1} clicked. Loading Scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError($"Error: You clicked Button {arrayIndex + 1}, but you only have {levelSceneNames.Length} scene names in the Inspector!");
        }
    }
    public void OpenShop()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Market_Scene");
    }
    public void BackToMain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}