using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        StartCoroutine(PlayGameDelayed());
    }
    private IEnumerator PlayGameDelayed()
    {
        yield return new WaitForSeconds(0.5f);
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
    public void OpenSetting()
    {
        SceneManager.LoadScene("Setting");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}