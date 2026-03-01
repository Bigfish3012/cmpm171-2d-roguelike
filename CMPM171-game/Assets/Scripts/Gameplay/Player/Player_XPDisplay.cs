using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Connects UI_Level, UI_XP text and xpSlider to Player_settings experience system.
// Attach to PlayerXP parent or a dedicated controller; assign references in Inspector.
public class Player_XPDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uiLevelText;                                // Shows "Lv.X"
    [SerializeField] private TextMeshProUGUI uiXPText;                                   // Shows "current/max" (e.g. "13/100")
    [SerializeField] private Slider xpSlider;                                            // Progress bar toward next level (0-1)

    private Player_settings playerSettings;                                              // Reference to player settings

    // Initialize player settings reference and refresh display
    void Start()
    {
        playerSettings = Player_settings.Instance;
        // playerSettings may be null in MainMenu (no Player); we retry in Update when entering gameplay
        if (playerSettings != null)
            RefreshDisplay();
    }

    // Retry finding player settings and refresh display each frame
    void Update()
    {
        if (playerSettings == null)
        {
            playerSettings = Player_settings.Instance;
            if (playerSettings == null) return;
        }
        RefreshDisplay();
    }

    // Update level text, XP text and slider with current player data
    private void RefreshDisplay()
    {
        int level = playerSettings.GetCurrentLevel();
        int progress = playerSettings.GetXPProgressTowardsNextLevel();
        int xpPerLevel = playerSettings.GetXPPerLevel();

        if (uiLevelText != null)
            uiLevelText.text = $"Lv.{level}";

        if (uiXPText != null)
            uiXPText.text = $"{progress}/{xpPerLevel}";

        if (xpSlider != null && xpPerLevel > 0)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.value = (float)progress / xpPerLevel;
        }
    }
}
