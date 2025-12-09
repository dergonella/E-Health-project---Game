using UnityEngine;

public enum PlayerPersona { None, PersonaA, PersonaB, PersonaC }

public static class GameContext
{
    // This variable stays alive the whole game
    public static PlayerPersona SelectedPersona = PlayerPersona.None;

    public static void SetPersona(int selectionIndex)
    {
        // map buttons 1, 2, 3 to Personas
        switch (selectionIndex)
        {
            case 1: SelectedPersona = PlayerPersona.PersonaA; break;
            case 2: SelectedPersona = PlayerPersona.PersonaB; break;
            case 3: SelectedPersona = PlayerPersona.PersonaC; break;
        }
        Debug.Log("Persona Selected: " + SelectedPersona);
    }
}
