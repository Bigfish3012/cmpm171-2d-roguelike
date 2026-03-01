using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent game manager. Saves and restores player data across scene transitions.
/// Attach to GameManager GameObject under GameRoot. Use DontDestroyOnLoad on GameRoot.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Gameplay UI Visibility")]
    [Tooltip("UI_Root GameObject containing HUD, PauseButton, LevelUp etc. Hidden in menu scenes.")]
    [SerializeField] private GameObject gameplayUIRoot;

    private static readonly string[] MenuScenes = { "MainMenu", "Credits", "Setting", "HowToPlay", "Gameover" };

    private int _currentHealth;                                                          // Saved current health
    private int _maxHealth;                                                              // Saved maximum health
    private int _currentExperience;                                                      // Saved experience points
    private int _xpPerLevel;                                                             // Saved XP per level
    private float _critRate;                                                             // Saved critical hit rate
    private float _critDamage;                                                           // Saved critical damage bonus
    private float _moveSpeed;                                                            // Saved movement speed
    private int _attackDamage;                                                           // Saved attack damage
    private int _savedWave;                                                              // Saved current wave index

    /// <summary>
    /// True if we have valid saved data from a previous gameplay scene.
    /// </summary>
    public bool HasSavedData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }
        Instance = this;

        // Persist entire GameRoot (parent) so GameManager, UI_Root, EventSystem, SceneTransition survive scene loads
        DontDestroyOnLoad(transform.root.gameObject);

        if (gameplayUIRoot == null)
        {
            var uiRoot = transform.root.Find("UI_Root");
            if (uiRoot != null)
                gameplayUIRoot = uiRoot.gameObject;
        }

        if (gameplayUIRoot == null)
            Debug.LogWarning("GameManager: gameplayUIRoot (UI_Root) not found. Assign it in Inspector.");

        SceneManager.sceneLoaded += OnSceneLoaded;
        // Apply visibility for initial scene
        RefreshHudVisibility(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshHudVisibility(scene.name);
    }

    private void RefreshHudVisibility(string sceneName)
    {
        if (gameplayUIRoot == null) return;

        bool isMenuScene = false;
        for (int i = 0; i < MenuScenes.Length; i++)
        {
            if (MenuScenes[i] == sceneName)
            {
                isMenuScene = true;
                break;
            }
        }

        gameplayUIRoot.SetActive(!isMenuScene);
    }

    /// <summary>
    /// Save player data from the given components. Call when HP/XP/upgrades change.
    /// </summary>
    public void SaveFrom(Player_settings ps, PlayerController pc, RangedShooter rs)
    {
        if (ps == null) return;

        _currentHealth = ps.GetCurrentHealth();
        _maxHealth = ps.GetMaxHealth();
        _currentExperience = ps.GetCurrentExperience();
        _xpPerLevel = ps.GetXPPerLevel();
        _critRate = ps.GetCritRate();
        _critDamage = ps.GetCritDamage();
        _moveSpeed = pc != null ? pc.moveSpeed : 6f;
        _attackDamage = rs != null ? rs.GetAttackDamage() : 10;

        HasSavedData = true;
    }

    /// <summary>
    /// Restore saved data into the given components. Call from Player_settings.Start when entering a gameplay scene.
    /// </summary>
    public void RestoreTo(Player_settings ps, PlayerController pc, RangedShooter rs)
    {
        if (ps == null || !HasSavedData) return;

        ps.RestoreData(_currentHealth, _maxHealth, _currentExperience, _xpPerLevel, _critRate, _critDamage);

        if (pc != null)
            pc.moveSpeed = _moveSpeed;

        if (rs != null)
            rs.SetAttackDamage(_attackDamage);
    }

    /// <summary>
    /// Save current wave index so the next gameplay scene can continue from the next wave.
    /// </summary>
    public void SaveWaveProgress(int wave)
    {
        _savedWave = Mathf.Max(0, wave);
    }

    /// <summary>
    /// Get saved wave index. Returns 0 if no wave has been saved yet.
    /// </summary>
    public int GetSavedWaveProgress()
    {
        return _savedWave;
    }

    /// <summary>
    /// Clear saved data. Call when starting a new game from main menu.
    /// </summary>
    public void ClearSavedData()
    {
        HasSavedData = false;
        _savedWave = 0;
    }
}
