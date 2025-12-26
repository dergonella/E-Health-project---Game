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

        Debug.Log("Persona Selected: " + personaIndex + ". Loading Prologue...");

        // 2. Load the NEW Prologue Scene
        // We go here first so the character can speak before the game starts.
        SceneManager.LoadScene("Story_PostLvl0");
    }
}
