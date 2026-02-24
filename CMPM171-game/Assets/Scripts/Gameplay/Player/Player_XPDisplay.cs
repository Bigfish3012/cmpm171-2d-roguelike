using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Connects UI_Level, UI_XP text and xpSlider to Player_settings experience system.
// Attach to PlayerXP parent or a dedicated controller; assign references in Inspector.
public class Player_XPDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uiLevelText;   // Shows "Lv.X"
    [SerializeField] private TextMeshProUGUI uiXPText;      // Shows "current/max" (e.g. "13/100")
    [SerializeField] private Slider xpSlider;               // Progress bar toward next level (0-1)

    private Player_settings playerSettings;

    void Start()
    {
        playerSettings = Player_settings.Instance;
        if (playerSettings == null)
        {
            Debug.LogError("Player_XPDisplay: Player_settings instance not found!");
            return;
        }
        RefreshDisplay();
    }

    void Update()
    {
        if (playerSettings == null) return;
        RefreshDisplay();
    }

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
