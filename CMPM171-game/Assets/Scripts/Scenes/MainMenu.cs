using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // PlayGame method to load the game scene
    public void PlayGame()
    {
        SceneManager.LoadScene("SC_Prototype");
    }
    public void OpenCredits()
    {
        SceneManager.LoadScene("Credits");
    }
        public void OpenHowToPlay()
    {
        SceneManager.LoadScene("HowToPlay");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}