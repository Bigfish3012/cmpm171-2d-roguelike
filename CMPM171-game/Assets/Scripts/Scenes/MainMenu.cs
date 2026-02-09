using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // PlayGame method to load the game scene
    public void PlayGame()
    {
        SceneManager.LoadScene("SC_Prototype");
    }

    // QuitGame method to quit the application
    public void QuitGame()
    {
        Application.Quit();
    }
}