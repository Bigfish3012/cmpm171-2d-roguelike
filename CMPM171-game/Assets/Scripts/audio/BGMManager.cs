using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Menu BGM")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private float menuVolume = 0.2f;

    private readonly string[] keepMenuBgmScenes = { "MainMenu", "Credits", "Setting" };

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
        audioSource.spatialBlend = 0f; // 2D
        audioSource.playOnAwake = false;

        PlayMenuBGM();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

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

    private bool IsKeepScene(string sceneName)
    {
        for (int i = 0; i < keepMenuBgmScenes.Length; i++)
            if (keepMenuBgmScenes[i] == sceneName) return true;
        return false;
    }

    private void PlayMenuBGM()
    {
        if (menuClip == null) return;

        audioSource.clip = menuClip;
        audioSource.volume = menuVolume;
        audioSource.Play();
    }
}
