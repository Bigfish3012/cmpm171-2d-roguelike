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
    [SerializeField] private int xpPerLevel = 20;                                      // Current XP required for next level (starts at 20)
    [SerializeField] private int xpIncreasePerLevel = 10;                              // XP increase required after each level up
    [SerializeField] private float critRate = 15f;                                     // Critical hit chance (default 20%)
    [SerializeField] private float critDamage = 100f;                                  // Critical damage bonus (default 100% = 2x damage)

    private int currentHealth;                                                           // Current health of the player
    private int currentExperience;                                                       // Current XP progress toward next level
    private int currentLevel = 1;                                                       // Player level (starts at 1)
    private int startingXPPerLevel;                                                     // Base XP requirement used to reconstruct level after restore
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

        startingXPPerLevel = xpPerLevel;
    }

    // Start method to initialize the player
    void Start()
    {
        var gm = GameManager.Instance;
        var pc = GetComponent<PlayerController>();
        var rs = GetComponent<RangedShooter>();

        if (gm != null && gm.HasSavedData)
        {
            gm.RestoreTo(this, pc, rs);
            lastPrintedHealth = currentHealth;
            Debug.Log($"Player Health restored: {currentHealth}/{maxHealth}");
        }
        else
        {
            currentHealth = maxHealth;
            lastPrintedHealth = currentHealth;
            Debug.Log($"Player Health: {currentHealth}");
        }
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
    public void TakeDamage(int damage, bool isCrit = false)
    {
        // Don't take damage if invincible
        if (isInvincible)
        {
            return;
        }

        currentHealth -= damage;
        SaveToGameManager();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player Died!");
            if (GameManager.Instance != null)
                GameManager.Instance.ClearSavedData();
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.LoadScene("Gameover");
            else
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
        currentExperience += amount;
        while (currentExperience >= xpPerLevel)
        {
            currentExperience -= xpPerLevel;
            currentLevel++;
            xpPerLevel += xpIncreasePerLevel;

            Debug.Log($"Level Up! Now Level {currentLevel}");
            currentHealth = maxHealth;  // Restore full health on level up
            OnLevelUp?.Invoke(currentLevel);
        }
        SaveToGameManager();
        Debug.Log($"Player Experience: {currentExperience}/{xpPerLevel} (Level {currentLevel})");
    }

    // Get the current experience of the player
    public int GetCurrentExperience()
    {
        return currentExperience;
    }

    // Get current level (starts at 1)
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    // Get XP progress toward next level (0 to xpPerLevel-1)
    public int GetXPProgressTowardsNextLevel()
    {
        return currentExperience;
    }

    // Get XP required for next level (e.g. 100)
    public int GetXPPerLevel()
    {
        return xpPerLevel;
    }

    // Calculate damage with critical hit roll. Returns (damage, isCrit).
    public (int damage, bool isCrit) CalculateDamageWithCrit(int baseDamage)
    {
        bool isCrit = UnityEngine.Random.Range(0f, 100f) < critRate;
        if (isCrit)
        {
            return (Mathf.RoundToInt(baseDamage * (1f + critDamage / 100f)), true);
        }
        return (baseDamage, false);
    }

    // Get crit rate for display or other use
    public float GetCritRate()
    {
        return critRate;
    }

    // Get crit damage for display or other use
    public float GetCritDamage()
    {
        return critDamage;
    }

    // For GameManager persistence - restore saved data when entering a new scene
    public void RestoreData(int currentHp, int maxHp, int currentExp, int xpPerLvl, float critR, float critD)
    {
        currentHealth = currentHp;
        maxHealth = maxHp;
        currentExperience = currentExp;
        xpPerLevel = xpPerLvl;
        if (xpIncreasePerLevel > 0 && xpPerLevel >= startingXPPerLevel)
            currentLevel = 1 + ((xpPerLevel - startingXPPerLevel) / xpIncreasePerLevel);
        else
            currentLevel = 1;
        critRate = critR;
        critDamage = critD;
        lastPrintedHealth = currentHealth;
    }

    // Upgrade methods for Level Up Menu
    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        SaveToGameManager();
    }

    public void AddCritRate(float amount)
    {
        critRate += amount;
        SaveToGameManager();
    }

    public void AddCritDamage(float amount)
    {
        critDamage += amount;
        SaveToGameManager();
    }

    private void SaveToGameManager()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SaveFrom(this, GetComponent<PlayerController>(), GetComponent<RangedShooter>());
    }

    // Save when destroyed (e.g. scene transition) so data is preserved for next scene.
    // Skip if data was explicitly cleared (e.g. GoHome, death) to avoid overwriting the clear.
    private void OnDestroy()
    {
        var gm = GameManager.Instance;
        if (currentHealth > 0 && gm != null && gm.HasSavedData)
            SaveToGameManager();
        if (Instance == this)
            Instance = null;
    }
}
