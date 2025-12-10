using UnityEngine;

/// <summary>
/// Static game state that persists between scenes.
/// Stores the selected persona and tracks level progress.
/// </summary>
public static class GameState
{
    public enum Persona
    {
        Brightgrove,
        Silvergrove,
        Stonegrove
    }

    // Currently selected persona
    public static Persona SelectedPersona { get; set; } = Persona.Brightgrove;

    // Current level (1, 2, or 3)
    public static int CurrentLevel { get; set; } = 1;

    // Progress tracking per persona
    private static int[] highestUnlockedLevel = { 1, 1, 1 }; // Index matches Persona enum

    /// <summary>
    /// Gets the scene name for the current persona and level.
    /// Format: "Brightgrove_Level1", "Silvergrove_Level2", etc.
    /// </summary>
    public static string GetCurrentSceneName()
    {
        return $"{SelectedPersona}_Level{CurrentLevel}";
    }

    /// <summary>
    /// Gets scene name for a specific persona and level.
    /// </summary>
    public static string GetSceneName(Persona persona, int level)
    {
        return $"{persona}_Level{level}";
    }

    /// <summary>
    /// Unlocks the next level for the current persona.
    /// Called when player completes a level.
    /// </summary>
    public static void UnlockNextLevel()
    {
        int personaIndex = (int)SelectedPersona;
        if (CurrentLevel < 3 && CurrentLevel >= highestUnlockedLevel[personaIndex])
        {
            highestUnlockedLevel[personaIndex] = CurrentLevel + 1;
            Debug.Log($"Unlocked {SelectedPersona} Level {CurrentLevel + 1}");
        }
    }

    /// <summary>
    /// Checks if a level is unlocked for the current persona.
    /// </summary>
    public static bool IsLevelUnlocked(int level)
    {
        return level <= highestUnlockedLevel[(int)SelectedPersona];
    }

    /// <summary>
    /// Checks if a level is unlocked for a specific persona.
    /// </summary>
    public static bool IsLevelUnlocked(Persona persona, int level)
    {
        return level <= highestUnlockedLevel[(int)persona];
    }

    /// <summary>
    /// Gets the highest unlocked level for the current persona.
    /// </summary>
    public static int GetHighestUnlockedLevel()
    {
        return highestUnlockedLevel[(int)SelectedPersona];
    }

    /// <summary>
    /// Resets all progress (for testing or new game).
    /// </summary>
    public static void ResetProgress()
    {
        highestUnlockedLevel = new int[] { 1, 1, 1 };
        SelectedPersona = Persona.Brightgrove;
        CurrentLevel = 1;
    }

    /// <summary>
    /// Sets up for a specific level. Call before loading scene.
    /// </summary>
    public static void SetLevel(Persona persona, int level)
    {
        SelectedPersona = persona;
        CurrentLevel = Mathf.Clamp(level, 1, 3);
    }
}
