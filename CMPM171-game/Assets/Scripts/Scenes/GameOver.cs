using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private AudioClip gameOverClip;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI waveText;

    [Header("Loop BGM")]
    [SerializeField] private AudioClip bgmLoopClip;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 1f;

    private void Start()
    {
        RefreshStatsPanel();

        // One-shot game over sound effect
        if (gameOverClip != null)
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(gameOverClip, pos, volume);
        }

        // Looping background music starts 0.2s after gameOverClip ends
        if (bgmLoopClip != null)
            StartCoroutine(PlayBgmAfterDelay());
    }

    private void RefreshStatsPanel()
    {
        if (scoreText == null)
            scoreText = SceneSearchHelper.FindSceneTMPByName("ScoreValue");
        if (scoreText == null)
            scoreText = SceneSearchHelper.FindSceneTMPByName("Score");
        if (timeText == null)
            timeText = SceneSearchHelper.FindSceneTMPByName("TimeValue");
        if (enemiesText == null)
            enemiesText = SceneSearchHelper.FindSceneTMPByName("EnemiesValue");
        if (waveText == null)
            waveText = SceneSearchHelper.FindSceneTMPByName("WaveValue");

        int score = GameManager.Instance != null ? GameManager.Instance.GetScore() : 0;
        int enemiesKilled = GameManager.Instance != null ? GameManager.Instance.GetEnemiesKilled() : 0;
        int waveReached = GameManager.Instance != null ? GameManager.Instance.GetSavedWaveProgress() : 0;
        float survivedSeconds = GameManager.Instance != null ? GameManager.Instance.GetRunDurationSeconds() : 0f;

        if (scoreText != null)
            scoreText.text = score.ToString();
        if (timeText != null)
            timeText.text = FormatDuration(survivedSeconds);
        if (enemiesText != null)
            enemiesText.text = enemiesKilled.ToString();
        if (waveText != null)
            waveText.text = waveReached.ToString();
    }

    private string FormatDuration(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }

    private IEnumerator PlayBgmAfterDelay()
    {
        float delay = (gameOverClip != null ? gameOverClip.length : 0f) + 0.2f;
        yield return new WaitForSeconds(delay);

        var source = gameObject.AddComponent<AudioSource>();
        source.clip = bgmLoopClip;
        source.loop = true;
        source.volume = bgmVolume;
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.Play();
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