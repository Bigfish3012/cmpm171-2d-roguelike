using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_settings : MonoBehaviour, IDamageable
{
    // Fired when player levels up; parameter is the new level
    public static event Action<int> OnLevelUp;
    // Singleton instance
    public static Player_settings Instance { get; private set; }
    
    // Public property to get player transform
    public Transform PlayerTransform => transform;
    
    [SerializeField] private int maxHealth = 10;                                        // Maximum health of the player
    [SerializeField] private int xpPerLevel = 100;                                      // XP required per level (e.g. 100 XP = level 2)

    private int currentHealth;                                                           // Current health of the player
    private int currentExperience;                                                       // Experience points gained from killing enemies
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

    // Add experience when killing an enemy
    public void AddExperience(int amount)
    {
        int levelBefore = GetCurrentLevel();
        currentExperience += amount;
        int levelAfter = GetCurrentLevel();

        if (levelAfter > levelBefore)
        {
            Debug.Log($"Level Up! Now Level {levelAfter}");
            OnLevelUp?.Invoke(levelAfter);
        }
        Debug.Log($"Player Experience: {currentExperience} (Level {levelAfter})");
    }

    // Get the current experience of the player
    public int GetCurrentExperience()
    {
        return currentExperience;
    }

    // Get current level (starts at 1, +1 per 100 XP by default)
    public int GetCurrentLevel()
    {
        return 1 + (currentExperience / xpPerLevel);
    }

    // Get XP progress toward next level (0 to xpPerLevel-1)
    public int GetXPProgressTowardsNextLevel()
    {
        return currentExperience % xpPerLevel;
    }

    // Get XP required for next level (e.g. 100)
    public int GetXPPerLevel()
    {
        return xpPerLevel;
    }
}
