using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralized scene loading. Attach to SceneTransition GameObject under GameRoot.
/// Used by MainMenu buttons, LevelExit triggers, etc.
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }
    private bool isLoading = false;                                                      // Prevents overlapping scene loads

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }
        Instance = this;
        // GameRoot is persisted by GameManager; SceneTransition is a child, so it persists too
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoading = false;
    }

    /// <summary>
    /// Load a scene immediately.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        SaveCurrentPlayerData();
        isLoading = true;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Load a scene after a delay (e.g. for Play button with brief delay).
    /// </summary>
    public void LoadSceneDelayed(string sceneName, float delaySeconds)
    {
        if (isLoading) return;
        SaveCurrentPlayerData();
        isLoading = true;
        StartCoroutine(LoadSceneDelayedCoroutine(sceneName, delaySeconds));
    }

    private IEnumerator LoadSceneDelayedCoroutine(string sceneName, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(sceneName);
    }

    private void SaveCurrentPlayerData()
    {
        if (GameManager.Instance == null || Player_settings.Instance == null) return;

        var ps = Player_settings.Instance;
        GameManager.Instance.SaveFrom(ps, ps.GetComponent<PlayerController>(), ps.GetComponent<RangedShooter>());
    }
}
