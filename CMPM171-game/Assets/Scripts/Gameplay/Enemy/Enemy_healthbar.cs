using UnityEngine;
using UnityEngine.UI;

// Generic health bar that works with any object implementing IHealth interface.
// Can be used for enemy with health. Bounces when entity takes damage.
public class Enemy_healthbar : MonoBehaviour
{
    [SerializeField] private float bounceDuration = 0.2f;                               // Duration of bounce animation
    [SerializeField] private float bounceScale = 1.15f;                                 // Max scale during bounce

    private Slider healthSlider;                                                        // Slider component for health display
    private IHealth healthComponent;                                                    // Component implementing IHealth (e.g. enemy)
    private RectTransform rectTransform;                                                 // RectTransform for bounce animation
    private float previousHealth;                                                       // Previous health value to detect damage
    private float bounceTimer;                                                          // Timer for bounce animation
    private Vector3 originalScale;                                                      // Original scale before bounce

    // Start method to initialize health bar components
    void Start()
    {
        healthSlider = GetComponent<Slider>();
        healthComponent = GetComponentInParent<IHealth>();
        rectTransform = GetComponent<RectTransform>();

        if (healthSlider == null)
        {
            Debug.LogError("Enemy_healthbar: Slider component not found!");
        }

        if (healthComponent == null)
        {
            Debug.LogError("Enemy_healthbar: No component implementing IHealth found in parent!");
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        if (healthComponent != null)
        {
            previousHealth = healthComponent.GetCurrentHealth();
        }
        originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
    }

    // Update method to sync health bar with entity health and handle bounce
    void Update()
    {
        if (healthComponent == null || healthSlider == null) return;

        float currentHealth = healthComponent.GetCurrentHealth();
        float healthRatio = currentHealth / healthComponent.GetMaxHealth();
        healthSlider.value = healthRatio;

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
            float t = 1f - (bounceTimer / bounceDuration);
            float scale = 1f + (bounceScale - 1f) * Mathf.Sin(t * Mathf.PI);
            rectTransform.localScale = originalScale * scale;
        }
        else if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
    }
}
