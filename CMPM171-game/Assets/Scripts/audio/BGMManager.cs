using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Menu BGM")]
    [SerializeField] private AudioSource audioSource;                                    // Audio source for playing BGM
    [SerializeField] private AudioClip menuClip;                                         // Menu background music clip
    [SerializeField] private float menuVolume = 0.2f;                                    // Volume for menu BGM

    private readonly string[] keepMenuBgmScenes = { "MainMenu", "Credits", "Setting" };  // Scenes that keep menu BGM playing

    // Initialize singleton, set up audio source and subscribe to scene events
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        PlayMenuBGM();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Unsubscribe from scene events and clear singleton
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
            Instance = null;
    }

    // Play or stop menu BGM based on the loaded scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldKeep = IsKeepScene(scene.name);

        if (shouldKeep)
        {
            if (audioSource.clip != menuClip)
                PlayMenuBGM();
            else if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }

    // Check if the given scene should keep the menu BGM playing
    private bool IsKeepScene(string sceneName)
    {
        for (int i = 0; i < keepMenuBgmScenes.Length; i++)
            if (keepMenuBgmScenes[i] == sceneName) return true;
        return false;
    }

    // Play menu BGM with the configured clip and volume
    private void PlayMenuBGM()
    {
        if (menuClip == null) return;

        audioSource.clip = menuClip;
        audioSource.volume = menuVolume;
        audioSource.Play();
    }
}
