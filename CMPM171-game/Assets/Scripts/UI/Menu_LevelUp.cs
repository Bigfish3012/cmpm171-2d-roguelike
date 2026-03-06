using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shows Level Up Menu when player levels up. Each button shows a random upgrade option.
// Attach to Canvas or another always-active parent; assign levelUpMenuUI in Inspector.
public class Menu_LevelUp : MonoBehaviour
{
    public enum UpgradeType { Health, Speed, CritRate, CritDamage, Damage }

    [System.Serializable]
    public struct UpgradeRanges
    {
        public int healthMin, healthMax;          // e.g. 2, 4
        public float speedMin, speedMax;         // e.g. 0.1, 1
        public int critRateMin, critRateMax;     // e.g. 5, 15
        public int critDamageMin, critDamageMax; // e.g. 10, 40
        public int damageMin, damageMax;         // e.g. 2, 5
    }

    [SerializeField] private GameObject levelUpMenuUI;
    [SerializeField] private float clickDelaySeconds = 1f;

    [SerializeField] private AudioClip levelUpClip;
    [Range(0f, 1f)] [SerializeField] private float levelUpVolume = 1f;

    private AudioSource _sfxSource;

    [Header("Penalty Rules")]
    [SerializeField] private int penaltyStartLevel = 6;          // Level >= 6 starts having drawbacks
    [SerializeField] private float damagePickDamageTakenPlus = 0.10f; // Picking Damage => +10% damage taken
    [SerializeField] private float speedPickDamageMultMinus = 0.10f;  // Picking Speed  => -10% damage dealt
    [SerializeField] private float healthPickSpeedMinus = 0.30f;      // Picking Health => -0.3 move speed

    [SerializeField]
    private UpgradeRanges ranges = new UpgradeRanges
    {
        healthMin = 3,
        healthMax = 3,
        speedMin = 0.5f,
        speedMax = 0.5f,
        critRateMin = 15,
        critRateMax = 15,
        critDamageMin = 30,
        critDamageMax = 30,
        damageMin = 5,
        damageMax = 5
    };

    private Button[] optionButtons;
    private UpgradeOption[] currentOptions;

    private bool penaltyMode = false;

    private struct UpgradeOption
    {
        public UpgradeType type;
        public float value;
    }

    void Start()
    {
        EnsureSFXSource();

        if (levelUpMenuUI == null)
        {
            Debug.LogError("Menu_LevelUp: levelUpMenuUI is not assigned!");
            return;
        }

        optionButtons = levelUpMenuUI.GetComponentsInChildren<Button>(true);
        currentOptions = new UpgradeOption[optionButtons.Length];
        levelUpMenuUI.SetActive(false);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
        }

        Player_settings.OnLevelUp += ShowMenu;
    }

    void OnDestroy()
    {
        Player_settings.OnLevelUp -= ShowMenu;
    }

    private void EnsureSFXSource()
    {
        if (_sfxSource != null) return;
        _sfxSource = GetComponent<AudioSource>();
        if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.spatialBlend = 0f;
        _sfxSource.playOnAwake = false;
    }

    private void ShowMenu(int newLevel)
    {
        if (levelUpClip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(levelUpClip, levelUpVolume);

        penaltyMode = newLevel >= penaltyStartLevel;

        GenerateUpgradeOptions();
        RefreshButtonTexts();

        levelUpMenuUI.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log($"[Level Up] Level {newLevel} (PenaltyMode={penaltyMode}) - Options: " +
            string.Join(", ", System.Array.ConvertAll(currentOptions, o => GetDisplayText(o))));

        foreach (var btn in optionButtons)
            btn.interactable = false;

        StartCoroutine(EnableButtonsAfterDelay());
    }

    private void GenerateUpgradeOptions()
    {
        var types = new List<UpgradeType>
        {
            UpgradeType.Health,
            UpgradeType.Speed,
            UpgradeType.CritRate,
            UpgradeType.CritDamage,
            UpgradeType.Damage
        };

        for (int i = 0; i < types.Count; i++)
        {
            int swapIndex = Random.Range(i, types.Count);
            (types[i], types[swapIndex]) = (types[swapIndex], types[i]);
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            UpgradeType type = i < types.Count ? types[i] : (UpgradeType)Random.Range(0, types.Count);
            float value = GetRandomValueForType(type);
            currentOptions[i] = new UpgradeOption { type = type, value = value };
        }
    }

    private float GetRandomValueForType(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Health: return Random.Range(ranges.healthMin, ranges.healthMax + 1);
            case UpgradeType.Speed: return Mathf.Round(Random.Range(ranges.speedMin, ranges.speedMax) * 10f) / 10f;
            case UpgradeType.CritRate: return Random.Range(ranges.critRateMin, ranges.critRateMax + 1);
            case UpgradeType.CritDamage: return Random.Range(ranges.critDamageMin, ranges.critDamageMax + 1);
            case UpgradeType.Damage: return Random.Range(ranges.damageMin, ranges.damageMax + 1);
            default: return 0;
        }
    }

    private void RefreshButtonTexts()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var text = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
                text.text = GetDisplayText(currentOptions[i]);
        }
    }

    private string GetDisplayText(UpgradeOption opt)
    {
        string valueText = opt.type == UpgradeType.Speed ? opt.value.ToString("0.0") : Mathf.RoundToInt(opt.value).ToString();

        string baseText = opt.type switch
        {
            UpgradeType.Health => $"Health +{valueText}",
            UpgradeType.Speed => $"Speed +{valueText}",
            UpgradeType.CritRate => $"Crit Rate +{valueText}",
            UpgradeType.CritDamage => $"Crit Damage +{valueText}",
            UpgradeType.Damage => $"Damage +{valueText}",
            _ => ""
        };

        if (!penaltyMode) return baseText;

        // Optional: show short drawback hint after level 6+
        string drawback = opt.type switch
        {
            UpgradeType.Damage => $"  <color=#FF5555>(Take +{Mathf.RoundToInt(damagePickDamageTakenPlus * 100f)}% dmg)</color>",
            UpgradeType.Speed => $"  <color=#FF5555>(Dmg -{Mathf.RoundToInt(speedPickDamageMultMinus * 100f)}%)</color>",
            UpgradeType.Health => $"  <color=#FF5555>(Speed -{healthPickSpeedMinus:0.0})</color>",
            _ => ""
        };

        return baseText + drawback;
    }

    private IEnumerator EnableButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(clickDelaySeconds);

        foreach (var btn in optionButtons)
            btn.interactable = true;
    }

    private void OnOptionClicked(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= currentOptions.Length) return;

        var chosen = currentOptions[buttonIndex];
        Debug.Log($"[Level Up] Selected: {GetDisplayText(chosen)}");
        ApplyUpgrade(chosen);

        Time.timeScale = 1f;
        levelUpMenuUI.SetActive(false);
    }

    private void ApplyUpgrade(UpgradeOption opt)
    {
        var playerSettings = Player_settings.Instance;
        var playerController = playerSettings != null ? playerSettings.GetComponent<PlayerController>() : null;
        var rangedShooter = playerSettings != null ? playerSettings.GetComponent<RangedShooter>() : null;

        switch (opt.type)
        {
            case UpgradeType.Health:
                if (playerSettings != null) playerSettings.AddMaxHealth(Mathf.RoundToInt(opt.value));

                // Penalty: health pick => slower move
                if (penaltyMode && playerController != null) playerController.AddMoveSpeed(-healthPickSpeedMinus);
                break;

            case UpgradeType.Speed:
                if (playerController != null) playerController.AddMoveSpeed(opt.value);

                // Penalty: speed pick => lower damage dealt
                if (penaltyMode && rangedShooter != null) rangedShooter.AddDamageMultiplier(-speedPickDamageMultMinus);
                break;

            case UpgradeType.CritRate:
                if (playerSettings != null) playerSettings.AddCritRate(opt.value);
                break;

            case UpgradeType.CritDamage:
                if (playerSettings != null) playerSettings.AddCritDamage(opt.value);
                break;

            case UpgradeType.Damage:
                if (rangedShooter != null) rangedShooter.AddAttackDamage(Mathf.RoundToInt(opt.value));

                // Penalty: damage pick => take more damage
                if (penaltyMode && playerSettings != null) playerSettings.AddDamageTakenMultiplier(damagePickDamageTakenPlus);
                break;
        }
    }
}