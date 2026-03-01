using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start a new game by clearing saved data and loading the first gameplay scene
    public void PlayGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ClearSavedData();
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadSceneDelayed("SC_Prototype", 0.5f);
        else
            SceneManager.LoadScene("SC_Prototype");
    }

    // Open the credits scene
    public void OpenCredits()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("Credits");
        else
            SceneManager.LoadScene("Credits");
    }

    // Open the how to play scene
    public void OpenHowToPlay()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("HowToPlay");
        else
            SceneManager.LoadScene("HowToPlay");
    }

    // Open the settings scene
    public void OpenSetting()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("Setting");
        else
            SceneManager.LoadScene("Setting");
    }

    // Quit the application
    public void QuitGame()
    {
        Application.Quit();
    }
}
