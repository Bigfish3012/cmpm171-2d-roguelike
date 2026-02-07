using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_settings : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 10;                                        // Maximum health of the player
    
    private int currentHealth;                                                           // Current health of the player
    private int lastPrintedHealth;                                                      // Last printed health value for debug logging

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
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Handle player death here by reloading the scene
            Debug.Log("Player Died!");
            SceneManager.LoadScene("Gameover");
        }
    }

    // Get the current health of the player
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
