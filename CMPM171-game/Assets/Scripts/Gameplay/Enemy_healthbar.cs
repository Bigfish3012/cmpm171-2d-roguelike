using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic health bar that works with any object implementing IHealth interface
/// Can be used for enemy with health
/// </summary>
public class Enemy_healthbar : MonoBehaviour
{
    private Slider healthSlider;
    private IHealth healthComponent;

    void Start()
    {
        // Get the Slider component
        healthSlider = GetComponent<Slider>();
        
        // Find any component implementing IHealth in the parent hierarchy
        healthComponent = GetComponentInParent<IHealth>();
        
        if (healthSlider == null)
        {
            Debug.LogError("Enemy_healthbar: Slider component not found!");
        }
        
        if (healthComponent == null)
        {
            Debug.LogError("Enemy_healthbar: No component implementing IHealth found in parent!");
        }
        
        // Initialize slider max value
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
    }

    void Update()
    {
        if (healthComponent == null || healthSlider == null) return;
        
        // Calculate health percentage
        float healthRatio = (float)healthComponent.GetCurrentHealth() / healthComponent.GetMaxHealth();
        healthSlider.value = healthRatio;
    }
}
