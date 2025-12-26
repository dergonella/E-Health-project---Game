using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows Restart and Menu buttons when game ends.
/// </summary>
public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;

    [Header("Scene Settings")]
    [Tooltip("Type the exact name of your menu scene (e.g. LevelSelect, MainMenu)")]
    public string menuSceneName = "LevelSelect";

    [Header("Show Buttons")]
    public bool showRestartButton = true;
    public bool showMenuButton = true;

    [Header("Restart Button")]
    public Sprite restartSprite;
    public Vector2 restartSize = new Vector2(200, 60);
    public Vector2 restartPosition = new Vector2(0, -100);
    public bool showRestartText = true;

    [Header("Menu Button")]
    public Sprite menuSprite;
    public Vector2 menuSize = new Vector2(200, 60);
    public Vector2 menuPosition = new Vector2(0, -180);
    public bool showMenuText = true;

    // Private
    private GameObject restartButtonObj;
    private GameObject menuButtonObj;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateButtons();
        HideButtons();
    }

    void CreateButtons()
    {
        // Create Canvas
        GameObject canvas = new GameObject("ButtonCanvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        canvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.AddComponent<GraphicRaycaster>();

        // Restart Button
        if (showRestartButton)
        {
            restartButtonObj = MakeButton(canvas.transform, "Restart", restartSprite, restartSize, restartPosition, showRestartText);
            restartButtonObj.GetComponent<Button>().onClick.AddListener(Restart);
        }

        // Menu Button
        if (showMenuButton)
        {
            menuButtonObj = MakeButton(canvas.transform, "Menu", menuSprite, menuSize, menuPosition, showMenuText);
            menuButtonObj.GetComponent<Button>().onClick.AddListener(GoToMenu);
        }
    }

    GameObject MakeButton(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 pos, bool showText)
    {
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent, false);

        // Image
        Image img = btn.AddComponent<Image>();
        if (sprite != null)
            img.sprite = sprite;
        else
            img.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);

        // Button
        btn.AddComponent<Button>();

        // Position & Size
        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        // Text
        if (showText)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = name;
            tmp.fontSize = 28;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        return btn;
    }

    // === PUBLIC METHODS ===

    public void ShowButtons()
    {
        if (restartButtonObj != null) restartButtonObj.SetActive(true);
        if (menuButtonObj != null) menuButtonObj.SetActive(true);
    }

    public void HideButtons()
    {
        if (restartButtonObj != null) restartButtonObj.SetActive(false);
        if (menuButtonObj != null) menuButtonObj.SetActive(false);
    }

    public void ShowRestartButton()
    {
        ShowButtons();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
