using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Pause : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuUI;

    bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}