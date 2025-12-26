using UnityEngine;
using UnityEngine.SceneManagement; // Required to change scenes

public class IntroSelectionManager : MonoBehaviour
{
    // This function is linked to your buttons in the Unity Inspector.
    // Index 1 = Charlie, Index 2 = Andrea, Index 3 = Alex
    public void PickPersona(int personaIndex)
    {
        // 1. Save the player's choice so we remember it later
        GameContext.SetPersona(personaIndex);

        Debug.Log("Persona Selected: " + personaIndex + ". Loading Level Menu...");

        // 2. Load the Level Menu (The Hub)
        // From here, the player can click "Level 1" to start the story.
        SceneManager.LoadScene("LevelMenu");
    }
}