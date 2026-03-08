using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Menu BGM")]
    [SerializeField] private AudioSource audioSource;                                    // Audio source for playing BGM
    [SerializeField] private AudioClip menuClip;                                         // Menu background music clip
    [SerializeField] private float menuVolume = 0.1f;                                    // Volume for menu BGM

    [Header("Level 1 BGM")]
    [SerializeField] private AudioClip level1Clip;                                        // BGM for SC_Prototype (level 1)
    [SerializeField] private float level1Volume = 0.3f;

    [Header("Level 2 BGM")]
    [SerializeField] private AudioClip level2Clip;                                        // BGM for SC_Prototype (level 2)
    [SerializeField] private float level2Volume = 0.3f;

    private readonly string[] keepMenuBgmScenes = { "MainMenu", "Credits", "Setting" };  // Scenes that keep menu BGM playing
    private const string Level1SceneName = "SC_Prototype";
    private const string Level2SceneName = "Level2";

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

    // Play BGM based on the loaded scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsKeepScene(scene.name))
        {
            if (audioSource.clip != menuClip)
                PlayMenuBGM();
            else if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else if (scene.name == Level1SceneName && level1Clip != null)
        {
            audioSource.clip = level1Clip;
            audioSource.volume = level1Volume;
            audioSource.Play();
        }
        else if (scene.name == Level2SceneName && level2Clip != null)
        {
            audioSource.clip = level2Clip;
            audioSource.volume = level2Volume;
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

    // Pause BGM (used when game is paused)
    public void PauseBGM()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Pause();
    }

    // Resume BGM (used when game is resumed)
    public void ResumeBGM()
    {
        if (audioSource != null && audioSource.clip != null)
            audioSource.UnPause();
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
