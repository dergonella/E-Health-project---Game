using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    public void LoadLevel(int levelIndex)
    {
        // This will now load by the Index number on the right of Build list
        SceneManager.LoadScene(levelIndex);
    }

    public void BackToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OpenShop()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Market_Scene");
    }
}
