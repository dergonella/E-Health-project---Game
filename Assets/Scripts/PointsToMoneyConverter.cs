using UnityEngine;

/// <summary>
/// Converts excess points above target score to money after level completion.
/// Integrates with CurrencyManager to award bonus money for high scores.
/// Used in Level 0.1 timed challenge to reward players who score above 2000 points.
/// </summary>
public class PointsToMoneyConverter : MonoBehaviour
{
    [Header("Conversion Settings")]
    [Tooltip("How many points = 1 coin (e.g., 10 points = 1 coin)")]
    [SerializeField] private int pointsToMoneyRatio = 10;

    // Force correct ratio constant - ignores Inspector value
    private const int FORCED_RATIO = 10;

    [Header("Bonus Settings")]
    [Tooltip("Multiplier for excess points (points above target score)")]
    [SerializeField] private float excessPointsMultiplier = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    void Awake()
    {
        // Force the correct ratio regardless of Inspector value
        // This fixes scenes that have old serialized value of 100
        if (pointsToMoneyRatio != FORCED_RATIO)
        {
            Debug.Log($"[PointsToMoneyConverter] Forcing ratio from {pointsToMoneyRatio} to {FORCED_RATIO}");
            pointsToMoneyRatio = FORCED_RATIO;
        }
    }

    /// <summary>
    /// Convert final score to money and award it to the player.
    /// Splits into base score and excess score for different conversion rates.
    /// Works with or without CurrencyManager - always awards to MarketData.
    /// </summary>
    /// <param name="finalScore">Player's total score at level end</param>
    /// <param name="targetScore">Target score needed to complete level</param>
    /// <param name="levelWon">Whether the player won the level</param>
    public void ConvertAndAwardMoney(int finalScore, int targetScore, bool levelWon)
    {
        if (!levelWon)
        {
            if (showDebugLogs)
                Debug.Log("[PointsToMoneyConverter] Player did not win. No money awarded.");
            return;
        }

        // Calculate base money from target score
        int baseMoney = Mathf.FloorToInt((float)targetScore / pointsToMoneyRatio);

        // Calculate bonus money from excess points (points above target)
        int excessPoints = Mathf.Max(0, finalScore - targetScore);
        int bonusMoney = 0;

        if (excessPoints > 0)
        {
            float bonusMoneyFloat = ((float)excessPoints / pointsToMoneyRatio) * excessPointsMultiplier;
            bonusMoney = Mathf.FloorToInt(bonusMoneyFloat);
        }

        // Total money to award
        int totalMoney = baseMoney + bonusMoney;

        // Award the money - use CurrencyManager if available, otherwise direct to MarketData
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddMoney(totalMoney);
        }
        else
        {
            // Fallback: Add directly to MarketData (works in all scenes!)
            MarketData.AddMoney(totalMoney);
            Debug.Log($"[PointsToMoneyConverter] CurrencyManager not found - added {totalMoney} directly to MarketData");
        }

        if (showDebugLogs)
        {
            Debug.Log($"[PointsToMoneyConverter] Conversion Complete!");
            Debug.Log($"  - Final Score: {finalScore}");
            Debug.Log($"  - Target Score: {targetScore}");
            Debug.Log($"  - Excess Points: {excessPoints}");
            Debug.Log($"  - Base Money: {baseMoney} coins (from target score)");
            Debug.Log($"  - Bonus Money: {bonusMoney} coins (from excess points)");
            Debug.Log($"  - Total Awarded: {totalMoney} coins");
            Debug.Log($"  - Player Total Money: {MarketData.Money} coins");
        }
    }

    /// <summary>
    /// Calculate how much money would be earned without awarding it.
    /// Useful for showing preview to player before level ends.
    /// </summary>
    public int CalculatePotentialMoney(int currentScore, int targetScore)
    {
        if (currentScore < targetScore)
        {
            // Not enough to win yet
            return 0;
        }

        // Calculate base money
        int baseMoney = Mathf.FloorToInt((float)targetScore / pointsToMoneyRatio);

        // Calculate bonus money
        int excessPoints = currentScore - targetScore;
        int bonusMoney = Mathf.FloorToInt(((float)excessPoints / pointsToMoneyRatio) * excessPointsMultiplier);

        return baseMoney + bonusMoney;
    }

    /// <summary>
    /// Get the conversion ratio (for UI display).
    /// </summary>
    public int GetConversionRatio()
    {
        return pointsToMoneyRatio;
    }

    /// <summary>
    /// Get the excess points multiplier (for UI display).
    /// </summary>
    public float GetExcessMultiplier()
    {
        return excessPointsMultiplier;
    }
}
