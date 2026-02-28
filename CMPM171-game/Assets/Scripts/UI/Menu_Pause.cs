using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Menu_Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;                                   // UI panel shown when game is paused
    [SerializeField] private GameObject defaultSelectedOnOpen;                         // Optional: first selected button when menu opens

    private bool isPaused = false;                                                      // Whether the game is currently paused

    // Start method to initialize pause menu state
    void Start()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogError("Menu_Pause: pauseMenuUI is not assigned!");
            return;
        }
        pauseMenuUI.SetActive(false);
        ClearUiSelection();
        Time.timeScale = 1f;
        isPaused = false;
    }

    // Update method to check for Escape key to toggle pause
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // Pause the game and show pause menu
    public void PauseGame()
    {
        if (pauseMenuUI == null) return;
        pauseMenuUI.SetActive(true);
        ClearUiSelection();
        if (defaultSelectedOnOpen != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(defaultSelectedOnOpen);
        Time.timeScale = 0;
        isPaused = true;
    }

    // Resume the game and hide pause menu
    public void ResumeGame()
    {
        if (pauseMenuUI == null) return;
        ClearUiSelection();
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    // Load main menu scene and resume time scale
    public void GoHome()
    {
        ResumeGame();
        if (GameManager.Instance != null)
            GameManager.Instance.ClearSavedData();
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }

    private void ClearUiSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
