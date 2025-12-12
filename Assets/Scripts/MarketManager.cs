using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Market Scene Manager - Buy medkits, shields, and slow motion.
/// Penguin shopkeeper on the left, items on the right.
/// </summary>
public class MarketManager : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Main menu scene name")]
    public string mainMenuSceneName = "MainMenu";
    [Tooltip("Dino level scene name (Level 4)")]
    public string dinoLevelSceneName = "Level_4";

    [Header("Prices")]
    public int medkitPrice = 300;
    public int shieldPrice = 100;
    public int slowMotionPrice = 100;

    [Header("Penguin Image")]
    public Image penguinImage;
    public Sprite penguinSprite;

    [Header("Money Display")]
    public TextMeshProUGUI moneyText;

    [Header("Medkit UI")]
    public TextMeshProUGUI medkitCountText;
    public TextMeshProUGUI medkitPriceText;
    public Button buyMedkitButton;

    [Header("Shield UI")]
    public TextMeshProUGUI shieldCountText;
    public TextMeshProUGUI shieldPriceText;
    public Button buyShieldButton;

    [Header("Slow Motion UI")]
    public TextMeshProUGUI slowMotionCountText;
    public TextMeshProUGUI slowMotionPriceText;
    public Button buySlowMotionButton;

    [Header("Navigation Buttons")]
    public Button mainMenuButton;
    public Button dinoLevelButton;

    [Header("Developer Options")]
    [Tooltip("Set starting money here (only applies once when you click the button)")]
    public int devSetMoney = 1000;
    [Tooltip("Click Play after checking this to SET money to devSetMoney amount")]
    public bool devApplyMoney = false;
    [Tooltip("Click Play after checking this to RESET all data to zero")]
    public bool devResetAllData = false;

    // Base starting amounts (from level config)
    private int baseMedkits = 3;
    private int baseShields = 3;
    private int baseSlowMotion = 3;

    void Awake()
    {
        // Developer options - run in Awake so it happens before Start
        if (devResetAllData)
        {
            MarketData.ResetAllData();
            Debug.Log("[MarketManager] DEV: All data reset to zero!");
        }

        if (devApplyMoney)
        {
            MarketData.Money = devSetMoney; // SET to exact amount, not add
            Debug.Log($"[MarketManager] DEV: Money SET to {devSetMoney}");
        }
    }

    void Start()
    {
        Debug.Log($"[MarketManager] Current Money: {MarketData.Money}");

        SetupPenguin();
        SetupButtons();
        UpdateAllUI();
    }

    void SetupPenguin()
    {
        if (penguinImage != null && penguinSprite != null)
        {
            penguinImage.sprite = penguinSprite;
        }
    }

    void SetupButtons()
    {
        // Buy buttons
        if (buyMedkitButton != null)
            buyMedkitButton.onClick.AddListener(BuyMedkit);

        if (buyShieldButton != null)
            buyShieldButton.onClick.AddListener(BuyShield);

        if (buySlowMotionButton != null)
            buySlowMotionButton.onClick.AddListener(BuySlowMotion);

        // Navigation buttons
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (dinoLevelButton != null)
            dinoLevelButton.onClick.AddListener(GoToDinoLevel);
    }

    void UpdateAllUI()
    {
        UpdateMoneyUI();
        UpdateMedkitUI();
        UpdateShieldUI();
        UpdateSlowMotionUI();
        UpdateButtonStates();
    }

    void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: {MarketData.Money}";
        }
    }

    void UpdateMedkitUI()
    {
        if (medkitCountText != null)
        {
            int total = MarketData.GetTotalMedkits(baseMedkits);
            medkitCountText.text = $"Medkit: {total}";
        }

        if (medkitPriceText != null)
        {
            medkitPriceText.text = $"Price: {medkitPrice}";
        }
    }

    void UpdateShieldUI()
    {
        if (shieldCountText != null)
        {
            int total = MarketData.GetTotalShields(baseShields);
            shieldCountText.text = $"Shield: {total}";
        }

        if (shieldPriceText != null)
        {
            shieldPriceText.text = $"Price: {shieldPrice}";
        }
    }

    void UpdateSlowMotionUI()
    {
        if (slowMotionCountText != null)
        {
            int total = MarketData.GetTotalSlowMotion(baseSlowMotion);
            slowMotionCountText.text = $"Slow Motion: {total}";
        }

        if (slowMotionPriceText != null)
        {
            slowMotionPriceText.text = $"Price: {slowMotionPrice}";
        }
    }

    void UpdateButtonStates()
    {
        // Disable buy buttons if not enough money
        if (buyMedkitButton != null)
            buyMedkitButton.interactable = MarketData.Money >= medkitPrice;

        if (buyShieldButton != null)
            buyShieldButton.interactable = MarketData.Money >= shieldPrice;

        if (buySlowMotionButton != null)
            buySlowMotionButton.interactable = MarketData.Money >= slowMotionPrice;
    }

    // === BUY FUNCTIONS ===

    public void BuyMedkit()
    {
        if (MarketData.BuyMedkit(medkitPrice))
        {
            Debug.Log($"[Market] Bought Medkit! Total: {MarketData.GetTotalMedkits(baseMedkits)}");
            UpdateAllUI();
        }
        else
        {
            Debug.Log("[Market] Not enough money for Medkit!");
        }
    }

    public void BuyShield()
    {
        if (MarketData.BuyShield(shieldPrice))
        {
            Debug.Log($"[Market] Bought Shield! Total: {MarketData.GetTotalShields(baseShields)}");
            UpdateAllUI();
        }
        else
        {
            Debug.Log("[Market] Not enough money for Shield!");
        }
    }

    public void BuySlowMotion()
    {
        if (MarketData.BuySlowMotion(slowMotionPrice))
        {
            Debug.Log($"[Market] Bought Slow Motion! Total: {MarketData.GetTotalSlowMotion(baseSlowMotion)}");
            UpdateAllUI();
        }
        else
        {
            Debug.Log("[Market] Not enough money for Slow Motion!");
        }
    }

    // === NAVIGATION ===

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void GoToDinoLevel()
    {
        SceneManager.LoadScene(dinoLevelSceneName);
    }
}
