using System;
using System.Collections;
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
    [Range(0f, 1f)][SerializeField] private float playerGotHitVolume = 1f;

    [SerializeField] private AudioClip experienceGainClip;
    [Range(0f, 1f)][SerializeField] private float experienceGainVolume = 1f;

    private AudioSource _sfxSource;

    [SerializeField] private int maxHealth = 40;                                        // Maximum health of the player
    [SerializeField] private int xpPerLevel = 20;                                       // Current XP required for next level
    [SerializeField] private float xpIncreasePerLevel = 1.2f;                           // XP multiplier required after each level up
    [SerializeField] private float critRate = 15f;                                      // Critical hit chance
    [SerializeField] private float critDamage = 100f;                                   // Critical damage bonus
    [SerializeField] private float dodgeRate = 0f;                                      // Dodge chance in percent
    [SerializeField] private float spawnDelaySeconds = 1f;                              // Delay before showing player on a fresh run

    // New defensive stats
    [SerializeField] private int armor = 0;                                             // Flat damage reduction
    [SerializeField] private float damageReduction = 0f;                                // Percentage reduction, 0~1
    [SerializeField] private int regeneration = 0;                                      // HP restored every tick
    [SerializeField] private float regenerationInterval = 5f;                           // Seconds between regen ticks

    private int currentHealth;                                                          // Current health of the player
    private int currentExperience;                                                      // Current XP progress toward next level
    private int currentLevel = 1;                                                       // Player level (starts at 1)
    private int startingXPPerLevel;                                                     // Base XP requirement used to reconstruct level after restore
    private bool isInvincible = false;                                                  // Whether the player is currently invincible
    private PlayerController playerController;
    private RangedShooter rangedShooter;
    private Rigidbody2D rb;
    private Collider2D[] playerColliders;
    private Renderer[] playerRenderers;

    // Existing penalty-system support
    private float damageTakenMultiplier = 1f;
    private const float MaxDodgeRate = 70f;
    private const float MaxDamageReduction = 0.8f;
    private float regenerationTimer = 0f;

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
        playerController = GetComponent<PlayerController>();
        rangedShooter = GetComponent<RangedShooter>();
        rb = GetComponent<Rigidbody2D>();
        playerColliders = GetComponentsInChildren<Collider2D>(true);
        playerRenderers = GetComponentsInChildren<Renderer>(true);
    }

    void Start()
    {
        var gm = GameManager.Instance;
        var pc = playerController;
        var rs = rangedShooter;

        if (gm != null && gm.HasSavedData)
        {
            gm.RestoreTo(this, pc, rs);
        }
        else
        {
            currentHealth = maxHealth;

            if (spawnDelaySeconds > 0f)
                StartCoroutine(ShowPlayerAfterDelay());
        }
    }

    private IEnumerator ShowPlayerAfterDelay()
    {
        SetPlayerVisible(false);
        SetPlayerInteractable(false);
        SetInvincible(true);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(spawnDelaySeconds);

        SetPlayerVisible(true);
        SetPlayerInteractable(true);
        SetInvincible(false);
    }

    private void SetPlayerVisible(bool visible)
    {
        if (playerRenderers == null) return;

        for (int i = 0; i < playerRenderers.Length; i++)
            playerRenderers[i].enabled = visible;
    }

    private void SetPlayerInteractable(bool enabledState)
    {
        if (playerController != null)
            playerController.enabled = enabledState;

        if (rangedShooter != null)
            rangedShooter.enabled = enabledState;

        if (playerColliders == null) return;

        for (int i = 0; i < playerColliders.Length; i++)
            playerColliders[i].enabled = enabledState;
    }

    void Update()
    {
        // Regeneration
        if (regeneration > 0 && currentHealth > 0 && currentHealth < maxHealth)
        {
            regenerationTimer += Time.deltaTime;
            if (regenerationTimer >= regenerationInterval)
            {
                regenerationTimer = 0f;
                currentHealth = Mathf.Min(maxHealth, currentHealth + regeneration);
                SaveToGameManager();
            }
        }
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (isInvincible) return;

        if (UnityEngine.Random.Range(0f, 100f) < dodgeRate)
        {
            return;
        }

        int finalDamage = damage;

        // Armor: flat reduction
        finalDamage = Mathf.Max(1, finalDamage - armor);

        // Damage reduction: percentage reduction
        finalDamage = Mathf.RoundToInt(finalDamage * (1f - damageReduction));
        finalDamage = Mathf.Max(1, finalDamage);

        // Existing penalty multiplier
        finalDamage = Mathf.Max(0, Mathf.RoundToInt(finalDamage * damageTakenMultiplier));

        currentHealth -= finalDamage;
        SaveToGameManager();

        if (playerGotHitClip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(playerGotHitClip, playerGotHitVolume);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (GameManager.Instance != null)
                GameManager.Instance.PrepareForGameOver();
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

    // Restore player health to maximum. Used by debug mode and similar.
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

    public void AddExperience(int amount)
    {
        if (amount > 0 && experienceGainClip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(experienceGainClip, experienceGainVolume);

        currentExperience += amount;
        if (amount > 0 && GameManager.Instance != null)
            GameManager.Instance.AddScore(amount);

        //currentExperience += 9999;  //testing new upgrades

        while (currentExperience >= xpPerLevel)
        {
            currentExperience -= xpPerLevel;
            currentLevel++;
            xpPerLevel = ComputeNextXPRequirement(xpPerLevel);

            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.RoundToInt(maxHealth * 0.3f));  //level up restore 30% hp
            OnLevelUp?.Invoke(currentLevel);
        }

        SaveToGameManager();
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
        return dodgeRate;
    }

    public void RestoreData(int currentHp, int maxHp, int currentExp, int xpPerLvl, float critR, float critD, float dodgeR)
    {
        currentHealth = currentHp;
        maxHealth = maxHp;
        currentExperience = currentExp;
        xpPerLevel = xpPerLvl;
        currentLevel = RecalculateLevelFromXPRequirement(xpPerLevel);
        critRate = critR;
        critDamage = critD;
        dodgeRate = Mathf.Clamp(dodgeR, 0f, MaxDodgeRate);

        // damageTakenMultiplier / armor / damageReduction / regeneration
        // are not restored yet to keep this patch minimal
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

    public void AddDodgeRate(float amount)
    {
        dodgeRate = Mathf.Clamp(dodgeRate + amount, 0f, MaxDodgeRate);
        SaveToGameManager();
    }

    public void AddArmor(int amount)
    {
        armor += amount;
        SaveToGameManager();
    }

    public void AddDamageReduction(float amount)
    {
        damageReduction = Mathf.Clamp(damageReduction + amount, 0f, MaxDamageReduction);
        SaveToGameManager();
    }

    public void AddRegeneration(int amount)
    {
        regeneration += amount;
        SaveToGameManager();
    }

    // Existing penalty-system method
    public void AddDamageTakenMultiplier(float amount)
    {
        damageTakenMultiplier = Mathf.Max(0.1f, damageTakenMultiplier + amount);
        // not saved/restored yet
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