using UnityEngine;
using UnityEngine.SceneManagement;

// Debug mode shortcuts for testing. Only active in gameplay scenes.
// J = Level up (trigger upgrade menu)
// K = Next wave (clear enemies, start next wave)
// N = Next map (load next scene, clear enemies)
// B = Previous map (load previous scene, clear enemies)
// H = Restore full health
// X = +10 XP
// I = Toggle God Mode (enemies cannot damage player)
public class Debug_mode : MonoBehaviour
{
    [SerializeField] private bool enableDebugKeys = true;

    // Gameplay scene order for N/B map switching
    private static readonly string[] GameplayScenes = { "SC_Prototype", "Level2", "Level3" };

    private void Update()
    {
        if (!enableDebugKeys) return;

        if (Input.GetKeyDown(KeyCode.J))
            DebugLevelUp();

        if (Input.GetKeyDown(KeyCode.K))
            DebugNextWave();

        if (Input.GetKeyDown(KeyCode.N))
            DebugNextMap();

        if (Input.GetKeyDown(KeyCode.B))
            DebugPreviousMap();

        if (Input.GetKeyDown(KeyCode.H))
            DebugRestoreHealth();

        if (Input.GetKeyDown(KeyCode.X))
            DebugAddXP();

        if (Input.GetKeyDown(KeyCode.I))
            DebugToggleGodMode();
    }

    private void DebugLevelUp()
    {
        var player = Player_settings.Instance;
        if (player == null) return;

        // Add enough XP to trigger at least one level up
        int needed = player.GetXPPerLevel() - player.GetXPProgressTowardsNextLevel();
        if (needed <= 0) needed = player.GetXPPerLevel();
        player.AddExperience(needed);
        Debug.Log("[Debug] J: Level up triggered.");
    }

    private void DebugNextWave()
    {
        ClearAllEnemies();

        var waveController = FindFirstObjectByType<WaveFlowController>();
        if (waveController != null)
        {
            waveController.DebugStartNextWave();
            Debug.Log("[Debug] K: Next wave started.");
        }
        else
        {
            Debug.LogWarning("[Debug] K: WaveFlowController not found.");
        }
    }

    private void DebugNextMap()
    {
        ClearAllEnemies();
        string next = GetNextScene(SceneManager.GetActiveScene().name);
        if (!string.IsNullOrEmpty(next))
        {
            LoadScene(next);
            Debug.Log($"[Debug] N: Loading next map: {next}");
        }
        else
        {
            Debug.Log("[Debug] N: Already at last map.");
        }
    }

    private void DebugPreviousMap()
    {
        ClearAllEnemies();
        string prev = GetPreviousScene(SceneManager.GetActiveScene().name);
        if (!string.IsNullOrEmpty(prev))
        {
            LoadScene(prev);
            Debug.Log($"[Debug] B: Loading previous map: {prev}");
        }
        else
        {
            Debug.Log("[Debug] B: Already at first map.");
        }
    }

    private void DebugRestoreHealth()
    {
        var player = Player_settings.Instance;
        if (player != null)
        {
            player.RestoreFullHealth();
            Debug.Log("[Debug] H: Health restored to full.");
        }
    }

    private void DebugAddXP()
    {
        var player = Player_settings.Instance;
        if (player != null)
        {
            player.AddExperience(10);
            Debug.Log("[Debug] X: +10 XP added.");
        }
    }

    private void DebugToggleGodMode()
    {
        var player = Player_settings.Instance;
        if (player != null)
        {
            // Use a simple toggle: we need to track state. Player_settings has SetInvincible but no getter.
            // We could add GetInvincible, or use a local static. For minimal changes, use a static in Debug_mode.
            godModeEnabled = !godModeEnabled;
            player.SetInvincible(godModeEnabled);
            Debug.Log($"[Debug] I: God Mode {(godModeEnabled ? "ON" : "OFF")}.");
        }
    }

    private static bool godModeEnabled = false;

    private void ClearAllEnemies()
    {
        var members = FindObjectsByType<EnemyWaveMember>(FindObjectsSortMode.None);
        foreach (var m in members)
        {
            if (m != null && m.gameObject != null)
                Destroy(m.gameObject);
        }
    }

    private static string GetNextScene(string current)
    {
        for (int i = 0; i < GameplayScenes.Length; i++)
        {
            if (GameplayScenes[i] == current)
            {
                int next = (i + 1) % GameplayScenes.Length;
                return GameplayScenes[next];
            }
        }
        return GameplayScenes.Length > 0 ? GameplayScenes[0] : null;
    }

    private static string GetPreviousScene(string current)
    {
        for (int i = 0; i < GameplayScenes.Length; i++)
        {
            if (GameplayScenes[i] == current)
            {
                int prev = (i - 1 + GameplayScenes.Length) % GameplayScenes.Length;
                return GameplayScenes[prev];
            }
        }
        return GameplayScenes.Length > 0 ? GameplayScenes[0] : null;
    }

    private void LoadScene(string sceneName)
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
