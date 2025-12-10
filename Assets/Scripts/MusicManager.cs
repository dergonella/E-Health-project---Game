using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        // Check if there is already a music manager in the game
        if (instance == null)
        {
            // If not, set this as the main one
            instance = this;
            // The Magic Line: Keeps this object alive across scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If a music manager already exists, destroy this new duplicate
            // so the music doesn't restart or play over itself.
            Destroy(gameObject);
        }
    }
}
