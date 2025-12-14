using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Needed for TextMeshPro

public class MainMenuController : MonoBehaviour
{
    public TextMeshProUGUI soundButtonText;
    private bool isSoundOn = true;

    private void Start()
    {
        // Load saved sound preference
        isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
        UpdateSoundState();
    }

    public void OnPlayClicked()
    {
        // Load the Level Selection Menu
        SceneManager.LoadScene("Dialogue IntroScene");
    }

    public void OnLevelsClicked()
    {
        SceneManager.LoadScene("LevelMenu");
    }
    public void OnShopClicked()
    {
        SceneManager.LoadScene("Market_Scene");
    }

    public void OnSoundToggleClicked()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("Sound", isSoundOn ? 1 : 0); // Save preference
        UpdateSoundState();
    }

    private void UpdateSoundState()
    {
        // Mute or Unmute global audio
        AudioListener.volume = isSoundOn ? 1f : 0f;

        // Update Button Text
        if (soundButtonText != null)
            soundButtonText.text = isSoundOn ? "SOUND: ON" : "SOUND: OFF";
    }
}