using UnityEngine;
using TMPro;

/// <summary>
/// Displays player's money on the Level Menu scene.
/// Attach to a Canvas and assign a TextMeshProUGUI component.
/// </summary>
public class LevelMenuMoneyDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Text component to display money (e.g., in top corner)")]
    public TextMeshProUGUI moneyText;

    [Header("Display Settings")]
    [Tooltip("Format string for money display")]
    public string displayFormat = "Money: {0}";

    void Start()
    {
        UpdateMoneyDisplay();
    }

    void OnEnable()
    {
        UpdateMoneyDisplay();
    }

    /// <summary>
    /// Update the money display text
    /// </summary>
    public void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = string.Format(displayFormat, MarketData.Money);
        }
    }
}
