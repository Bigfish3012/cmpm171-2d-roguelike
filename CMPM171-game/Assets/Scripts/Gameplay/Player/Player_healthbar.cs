using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player health bar UI. Attach to a Slider under a Canvas.
/// Position the parent RectTransform at top-left (anchor preset: Top Left) so the bar stays fixed there.
/// Bounces when player takes damage.
/// </summary>
public class Player_healthbar : MonoBehaviour
{
    [SerializeField] private float bounceDuration = 0.2f;     // Duration of bounce animation
    [SerializeField] private float bounceScale = 1.15f;       // Max scale during bounce

    private Slider healthSlider;
    private Player_settings playerSettings;
    private RectTransform rectTransform;
    private float previousHealth;
    private float bounceTimer;
    private Vector3 originalScale;

    void Start()
    {
        healthSlider = GetComponent<Slider>();
        playerSettings = Player_settings.Instance;
        rectTransform = GetComponent<RectTransform>();

        if (healthSlider == null)
        {
            Debug.LogError("Player_healthbar: Slider component not found!");
            return;
        }

        if (playerSettings == null)
        {
            Debug.LogError("Player_healthbar: Player_settings instance not found!");
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = 1f;

        previousHealth = playerSettings.GetCurrentHealth();
        originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
    }

    void Update()
    {
        if (playerSettings == null || healthSlider == null) return;

        int maxHp = playerSettings.GetMaxHealth();
        if (maxHp <= 0) return;

        int currentHealth = playerSettings.GetCurrentHealth();
        float ratio = (float)currentHealth / maxHp;
        healthSlider.value = ratio;

        // Detect damage and trigger bounce
        if (currentHealth < previousHealth)
        {
            bounceTimer = bounceDuration;
        }
        previousHealth = currentHealth;

        // Apply bounce animation
        if (bounceTimer > 0f && rectTransform != null)
        {
            bounceTimer -= Time.deltaTime;
            float t = 1f - (bounceTimer / bounceDuration);  // 0 -> 1 over duration
            // Sine curve: scale goes 1 -> bounceScale -> 1
            float scale = 1f + (bounceScale - 1f) * Mathf.Sin(t * Mathf.PI);
            rectTransform.localScale = originalScale * scale;
        }
        else if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
    }
}
