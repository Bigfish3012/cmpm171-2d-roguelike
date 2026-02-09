using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes the heart UI flash (change alpha) when player health is at or below 1/4.
/// Attach this to the heart Image GameObject (e.g. UI_Heart).
/// </summary>
public class HeartLowHealthFlash : MonoBehaviour
{
    [SerializeField] private float flashSpeed = 4f;           // How fast the flash cycles
    [SerializeField] private float minAlpha = 0.3f;            // Minimum alpha when flashing
    [SerializeField] private float maxAlpha = 1f;              // Maximum alpha when flashing

    private Image heartImage;
    private Player_settings playerSettings;
    private Color originalColor;
    private const float LowHealthThreshold = 0.25f;

    void Start()
    {
        heartImage = GetComponent<Image>();
        playerSettings = Player_settings.Instance;

        if (heartImage == null)
        {
            Debug.LogError("HeartLowHealthFlash: Image component not found!");
            return;
        }

        if (playerSettings == null)
        {
            Debug.LogError("HeartLowHealthFlash: Player_settings instance not found!");
            return;
        }

        originalColor = heartImage.color;
    }

    void Update()
    {
        if (heartImage == null || playerSettings == null) return;

        int maxHp = playerSettings.GetMaxHealth();
        if (maxHp <= 0) return;

        float healthRatio = (float)playerSettings.GetCurrentHealth() / maxHp;

        if (healthRatio <= LowHealthThreshold)
        {
            // Oscillate alpha between minAlpha and maxAlpha
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            heartImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
        else
        {
            // Restore full opacity when health is above threshold
            heartImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a);
        }
    }
}
