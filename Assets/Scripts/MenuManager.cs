using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the level selection menu
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelButtonPrefab;
    public Transform levelButtonContainer;
    public TextMeshProUGUI titleText;

    [Header("Level Button UI Elements")]
    public Button[] levelButtons = new Button[4];
    public TextMeshProUGUI[] levelNameTexts = new TextMeshProUGUI[4];
    public TextMeshProUGUI[] levelDescTexts = new TextMeshProUGUI[4];

    void Start()
    {
        InitializeMenu();
    }

    void InitializeMenu()
    {
        if (titleText != null)
        {
            titleText.text = "SHARD RUNNER\nSelect Level";
        }

        // Setup level buttons
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] != null && LevelManager.Instance != null)
            {
                int levelIndex = i;
                var levelData = LevelManager.Instance.GetLevelData(i);

                // Set button text
                if (levelNameTexts[i] != null)
                {
                    levelNameTexts[i].text = levelData.levelName;
                }

                // Set description text
                if (levelDescTexts[i] != null)
                {
                    string desc = levelData.description;
                    if (levelData.survivalTime > 0)
                    {
                        desc += $"\nGoal: {levelData.targetScore} score OR survive {levelData.survivalTime}s";
                    }
                    else
                    {
                        desc += $"\nGoal: {levelData.targetScore} score";
                    }
                    levelDescTexts[i].text = desc;
                }

                // Add button click listener
                levelButtons[i].onClick.RemoveAllListeners();
                levelButtons[i].onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
            }
        }
    }

    void OnLevelButtonClicked(int levelIndex)
    {
        Debug.Log($"Loading level {levelIndex}");
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(levelIndex);
        }
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}
