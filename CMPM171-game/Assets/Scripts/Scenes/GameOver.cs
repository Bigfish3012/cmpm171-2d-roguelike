using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOver : MonoBehaviour
{
    // [SerializeField] private TextMeshProUGUI highScoreText;
    // [SerializeField] private TextMeshProUGUI currentScoreText;
    // [SerializeField] private AudioSource audioSource;

    // Start method to initialize the game over screen
    private void Start()
    {
        // source from: https://www.youtube.com/watch?v=6PkdHcVFM6M
        // int highScore = PlayerPrefs.GetInt("HighScore", 0);
        // highScoreText.text = "High Score: " + highScore.ToString();

        // int lastScore = PlayerPrefs.GetInt("LastScore", 0);
        // currentScoreText.text = "Score: " + lastScore.ToString();

        // audioSource.Play();
    }

    // Restart method to restart the game
    public void Restart()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ClearSavedData();
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("SC_Prototype");
        else
            SceneManager.LoadScene("SC_Prototype");
    }

    // Home method to return to the main menu
    public void Home()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ClearSavedData();
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }
}