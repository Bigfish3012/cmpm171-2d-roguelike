using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Player health bar UI. Attach to a Slider under a Canvas.
// Position the parent RectTransform at top-left (anchor preset: Top Left) so the bar stays fixed there.
// Bounces when player takes damage.
public class Player_healthbar : MonoBehaviour
{
    [SerializeField] private float bounceDuration = 0.2f;                               // Duration of bounce animation
    [SerializeField] private float bounceScale = 1.15f;                                 // Max scale during bounce
    [SerializeField] private TextMeshProUGUI healthText;                                 // Optional: shows "current/max" (e.g. 12/12)

    private Slider healthSlider;                                                        // Slider component for health display
    private Player_settings playerSettings;                                             // Reference to player health data
    private RectTransform rectTransform;                                                 // RectTransform for bounce animation
    private float previousHealth;                                                       // Previous health value to detect damage
    private float bounceTimer;                                                          // Timer for bounce animation
    private Vector3 originalScale;                                                      // Original scale before bounce

    // Start method to initialize health bar components
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

        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = 1f;

        if (healthText == null)
            healthText = GetComponentInChildren<TextMeshProUGUI>(true);

        // playerSettings may be null in MainMenu (no Player); we retry in Update when entering gameplay
        if (playerSettings != null)
        {
            previousHealth = playerSettings.GetCurrentHealth();
            originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        }
    }

    // Update method to sync health bar with player health and handle bounce
    void Update()
    {
        if (healthSlider == null) return;
        if (playerSettings == null)
        {
            playerSettings = Player_settings.Instance;
            if (playerSettings == null) return;
            previousHealth = playerSettings.GetCurrentHealth();
            originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        }

        int maxHp = playerSettings.GetMaxHealth();
        if (maxHp <= 0) return;

        int currentHealth = playerSettings.GetCurrentHealth();
        float ratio = (float)currentHealth / maxHp;
        healthSlider.value = ratio;

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHp}";
        }

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
