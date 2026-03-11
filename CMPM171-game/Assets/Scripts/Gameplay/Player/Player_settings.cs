using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_settings : MonoBehaviour, IDamageable
{
    public static event Action<int> OnLevelUp;
    public static Player_settings Instance { get; private set; }

    public Transform PlayerTransform => transform;

    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int xpPerLevel = 20;
    [SerializeField] private int xpIncreasePerLevel = 10;

    [SerializeField] private float critRate = 15f;
    [SerializeField] private float critDamage = 100f;

    // ===== Defensive Stats =====
    [SerializeField] private int armor = 0;                  // Flat reduction
    [SerializeField] private float damageReduction = 0f;     // Percent reduction (0.1 = 10%)
    [SerializeField] private float dodgeChance = 0f;         // Chance to avoid hit
    [SerializeField] private int regeneration = 0;           // HP per regen tick
    [SerializeField] private float regenInterval = 5f;       // Seconds between regen ticks

    // ===== Penalty System Support =====
    [SerializeField] private float damageTakenMultiplier = 1f; // 1 = normal, 1.1 = +10% damage taken

    private float regenTimer = 0f;

    private int currentHealth;
    private int currentExperience;
    private int currentLevel = 1;

    private int startingXPPerLevel;
    private int lastPrintedHealth;

    private bool isInvincible = false;

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

        // Regeneration
        if (regeneration > 0 && currentHealth > 0 && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= regenInterval)
            {
                regenTimer = 0f;
                currentHealth = Mathf.Min(maxHealth, currentHealth + regeneration);
                Debug.Log("Regenerated HP");
                SaveToGameManager();
            }
        }
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (isInvincible)
        {
            return;
        }

        // Dodge check
        if (UnityEngine.Random.value < dodgeChance)
        {
            Debug.Log("Player Dodged!");
            return;
        }

        // Flat armor reduction
        int finalDamage = Mathf.Max(1, damage - armor);

        // Percent reduction
        finalDamage = Mathf.RoundToInt(finalDamage * (1f - damageReduction));
        finalDamage = Mathf.Max(1, finalDamage);

        // Penalty multiplier (used by level-up tradeoff system)
        finalDamage = Mathf.RoundToInt(finalDamage * damageTakenMultiplier);
        finalDamage = Mathf.Max(1, finalDamage);

        currentHealth -= finalDamage;
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

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
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

        while (currentExperience >= xpPerLevel)
        {
            currentExperience -= xpPerLevel;
            currentLevel++;
            xpPerLevel += xpIncreasePerLevel;

            Debug.Log($"Level Up! Now Level {currentLevel}");

            currentHealth = maxHealth;
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

    public float GetDodgeRate()
    {
        return dodgeChance;
    }

    public int GetArmor()
    {
        return armor;
    }

    public float GetDamageReduction()
    {
        return damageReduction;
    }

    public int GetRegeneration()
    {
        return regeneration;
    }

    public float GetDamageTakenMultiplier()
    {
        return damageTakenMultiplier;
    }

    // ===== Restore compatibility =====

    // Original-style restore
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

    // 7-arg restore version for current project compatibility
    public void RestoreData(int currentHp, int maxHp, int currentExp, int xpPerLvl, float critR, float critD, float dodgeR)
    {
        RestoreData(currentHp, maxHp, currentExp, xpPerLvl, critR, critD);
        dodgeChance = dodgeR;
    }

    // ===== Upgrade Methods =====

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        SaveToGameManager();
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
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

    public void AddArmor(int amount)
    {
        armor += amount;
        SaveToGameManager();
    }

    public void AddDamageReduction(float amount)
    {
        damageReduction += amount;
        damageReduction = Mathf.Clamp(damageReduction, 0f, 0.8f);
        SaveToGameManager();
    }

    public void AddDodge(float amount)
    {
        dodgeChance += amount;
        dodgeChance = Mathf.Clamp(dodgeChance, 0f, 0.75f);
        SaveToGameManager();
    }

    public void AddRegeneration(int amount)
    {
        regeneration += amount;
        SaveToGameManager();
    }

    public void AddDamageTakenMultiplier(float amount)
    {
        damageTakenMultiplier += amount;
        damageTakenMultiplier = Mathf.Max(0.1f, damageTakenMultiplier);
        SaveToGameManager();
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