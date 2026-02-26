using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Level2";                          // Name of the scene to load
    [Tooltip("Tag to check for (default: Player)")]
    [SerializeField] private string playerTag = "Player";                                // Tag used to identify the player
    [SerializeField] private KeyCode interactKey = KeyCode.F;                            // Key to confirm scene transition
    [SerializeField] private GameObject promptRoot;                                       // Optional "Press F" UI root near portal

    private bool playerInside = false;

    private void Start()
    {
        SetPromptVisible(false);
    }

    private void Update()
    {
        if (!playerInside) return;
        if (!Input.GetKeyDown(interactKey)) return;
        TryLoadTargetScene();
    }

    // Enable interaction when player enters trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        SetPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        SetPromptVisible(false);
    }

    private void TryLoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("SceneLoadTrigger: targetSceneName is not set.");
            return;
        }

        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene(targetSceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptRoot != null)
            promptRoot.SetActive(visible);
    }
}
