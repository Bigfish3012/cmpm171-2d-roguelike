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

    [SerializeField] private AudioClip playerGotHitClip;
    [Range(0f, 1f)] [SerializeField] private float playerGotHitVolume = 1f;

    private AudioSource _sfxSource;

    [SerializeField] private int maxHealth = 10;                                        // Maximum health of the player
    [SerializeField] private int xpPerLevel = 20;                                        // Current XP required for next level
    [SerializeField] private float xpIncreasePerLevel = 1.2f;                            // XP multiplier required after each level up
    [SerializeField] private float critRate = 15f;                                     // Critical hit chance (default 20%)
    [SerializeField] private float critDamage = 100f;                                  // Critical damage bonus (default 100% = 2x damage)

    private int currentHealth;                                                         // Current health of the player
    private int currentExperience;                                                     // Current XP progress toward next level
    private int currentLevel = 1;                                                      // Player level (starts at 1)
    private int startingXPPerLevel;                                                    // Base XP requirement used to reconstruct level after restore
    private int lastPrintedHealth;                                                     // Last printed health value for debug logging
    private bool isInvincible = false;                                                 // Whether the player is currently invincible

    // NEW: damage taken multiplier (1 = normal, 1.1 = +10% damage taken)
    private float damageTakenMultiplier = 1f;

    void Awake()
    {
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
        EnsureSFXSource();
    }

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

    void Update()
    {
        if (currentHealth != lastPrintedHealth)
        {
            Debug.Log($"Player Health: {currentHealth}");
            lastPrintedHealth = currentHealth;
        }
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (isInvincible) return;

        // NEW: apply damage taken multiplier
        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage * damageTakenMultiplier));

        currentHealth -= finalDamage;
        SaveToGameManager();

        if (playerGotHitClip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(playerGotHitClip, playerGotHitVolume);

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

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    /// <summary>Restore player health to maximum. Used by debug mode and similar.</summary>
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        SaveToGameManager();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;  
        //currentExperience += 9999;  //testing new upgrades
        while (currentExperience >= xpPerLevel)
        {
            currentExperience -= xpPerLevel;
            currentLevel++;
            xpPerLevel = ComputeNextXPRequirement(xpPerLevel);

            Debug.Log($"Level Up! Now Level {currentLevel}");
            currentHealth = maxHealth;  // Restore full health on level up
            OnLevelUp?.Invoke(currentLevel);
        }
        SaveToGameManager();
        Debug.Log($"Player Experience: {currentExperience}/{xpPerLevel} (Level {currentLevel})");
    }

    public int GetCurrentExperience()
    {
        return currentExperience;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetXPProgressTowardsNextLevel()
    {
        return currentExperience;
    }

    public int GetXPPerLevel()
    {
        return xpPerLevel;
    }

    public (int damage, bool isCrit) CalculateDamageWithCrit(int baseDamage)
    {
        bool isCrit = UnityEngine.Random.Range(0f, 100f) < critRate;
        if (isCrit)
        {
            return (Mathf.RoundToInt(baseDamage * (1f + critDamage / 100f)), true);
        }
        return (baseDamage, false);
    }

    public float GetCritRate()
    {
        return critRate;
    }

    public float GetCritDamage()
    {
        return critDamage;
    }

    public void RestoreData(int currentHp, int maxHp, int currentExp, int xpPerLvl, float critR, float critD)
    {
        currentHealth = currentHp;
        maxHealth = maxHp;
        currentExperience = currentExp;
        xpPerLevel = xpPerLvl;
        currentLevel = RecalculateLevelFromXPRequirement(xpPerLevel);
        critRate = critR;
        critDamage = critD;
        lastPrintedHealth = currentHealth;

        // NOTE: damageTakenMultiplier is not restored (kept simple for now)
    }

    private int ComputeNextXPRequirement(int currentRequirement)
    {
        float multiplier = Mathf.Max(1.01f, xpIncreasePerLevel);
        int nextRequirement = Mathf.CeilToInt(currentRequirement * multiplier);
        return Mathf.Max(currentRequirement + 1, nextRequirement);
    }

    private int RecalculateLevelFromXPRequirement(int currentRequirement)
    {
        if (currentRequirement <= startingXPPerLevel) return 1;

        int level = 1;
        int requirement = startingXPPerLevel;

        while (requirement < currentRequirement && level < 9999)
        {
            requirement = ComputeNextXPRequirement(requirement);
            level++;
        }

        return level;
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

    // NEW: called by penalty system (e.g. +0.10f => +10% more damage taken)
    public void AddDamageTakenMultiplier(float amount)
    {
        damageTakenMultiplier = Mathf.Max(0.1f, damageTakenMultiplier + amount);
        // NOTE: not saved/restored yet (we can add persistence later if you want)
    }

    private void EnsureSFXSource()
    {
        if (_sfxSource != null) return;
        _sfxSource = GetComponent<AudioSource>();
        if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.spatialBlend = 0f;
        _sfxSource.playOnAwake = false;
    }

    private void SaveToGameManager()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SaveFrom(this, GetComponent<PlayerController>(), GetComponent<RangedShooter>());
    }

    private void OnDestroy()
    {
        var gm = GameManager.Instance;
        if (currentHealth > 0 && gm != null && gm.HasSavedData)
            SaveToGameManager();
        if (Instance == this)
            Instance = null;
    }
}
