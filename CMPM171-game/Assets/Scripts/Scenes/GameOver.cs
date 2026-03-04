using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private AudioClip gameOverClip;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;

    [Header("Loop BGM")]
    [SerializeField] private AudioClip bgmLoopClip;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 1f;

    private void Start()
    {
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