using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_settings : MonoBehaviour, IDamageable
{
    // Singleton instance
    public static Player_settings Instance { get; private set; }
    
    // Public property to get player transform
    public Transform PlayerTransform => transform;
    
    [SerializeField] private int maxHealth = 10;                                        // Maximum health of the player
    
    private int currentHealth;                                                           // Current health of the player
    private int lastPrintedHealth;                                                      // Last printed health value for debug logging
    private bool isInvincible = false;                                                  // Whether the player is currently invincible

    // Awake method to initialize singleton
    void Awake()
    {
        // Set singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple Player_settings instances found. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    // Start method to initialize the player
    void Start()
    {
        currentHealth = maxHealth;
        lastPrintedHealth = currentHealth;
        Debug.Log($"Player Health: {currentHealth}");
    }

    // Update method to print health to console when it changes
    void Update()
    {
        // Print health to console when it changes
        if (currentHealth != lastPrintedHealth)
        {
            Debug.Log($"Player Health: {currentHealth}");
            lastPrintedHealth = currentHealth;
        }
    }

    // Take damage method to reduce player health and handle death
    public void TakeDamage(int damage)
    {
        // Don't take damage if invincible
        if (isInvincible)
        {
            return;
        }

        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Handle player death here by reloading the scene
            Debug.Log("Player Died!");
            SceneManager.LoadScene("Gameover");
        }
    }

    // Set invincible state (used during roll)
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    // Get the current health of the player
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Get the maximum health of the player
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
