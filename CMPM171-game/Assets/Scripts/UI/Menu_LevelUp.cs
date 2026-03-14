using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;

// Shows Level Up Menu when player levels up. Each button shows a random upgrade option.
// Attach to Canvas or another always-active parent; assign levelUpMenuUI in Inspector.
public class Menu_LevelUp : MonoBehaviour
{
    public static bool IsMenuOpen { get; private set; }

    public enum UpgradeType
    {
        Health, Speed, CritRate, CritDamage, Damage,
        ProjectilePath, Ricochet, Flame,
        Armor, DamageReduction, Dodge, Regeneration
    }

    [System.Serializable]
public struct UpgradeRanges
{
    public int healthMin, healthMax;          // e.g. 2, 4
    public float speedMin, speedMax;          // e.g. 0.1, 1
    public int critRateMin, critRateMax;      // e.g. 5, 15
    public int critDamageMin, critDamageMax;  // e.g. 10, 40
    public int damageMin, damageMax;          // e.g. 2, 5

    public int armorMin, armorMax;            // e.g. 1, 2
    public float damageReductionMin, damageReductionMax; // e.g. 0.05, 0.10
    public float dodgeMin, dodgeMax;          // e.g. 5, 10 (percent)
    public int regenerationMin, regenerationMax; // e.g. 1, 1
}

    [SerializeField] private GameObject levelUpMenuUI;
    [SerializeField] private float clickDelaySeconds = 1f;

    [SerializeField] private AudioClip levelUpClip;
    [Range(0f, 1f)][SerializeField] private float levelUpVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float doubleValueChance = 0.15f;
    [Range(0f, 1f)][SerializeField] private float specialUpgradeChance = 0.15f;

    [Header("Upgrade Icons")]
    [SerializeField] private Sprite damageIcon;
    [SerializeField] private Sprite critIcon;
    [SerializeField] private Sprite speedIcon;
    [SerializeField] private Sprite healthIcon;
    [SerializeField] private Sprite armorIcon;
    [SerializeField] private Sprite damageReductionIcon;
    [SerializeField] private Sprite dodgeIcon;
    [SerializeField] private Sprite regenerationIcon;

    private AudioSource _sfxSource;

    [Header("Penalty Rules")]
    [SerializeField] private int penaltyStartLevel = 6;               // Level >= 6 starts having drawbacks
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
        damageMax = 5,

        armorMin = 1,
        armorMax = 1,
        damageReductionMin = 0.05f,
        damageReductionMax = 0.05f,
        dodgeMin = 10f,
        dodgeMax = 10f,
        regenerationMin = 1,
        regenerationMax = 1
    };

    [Header("Special Upgrades")]
    [SerializeField] private int projectilePathAmount = 1;
    [SerializeField] private int ricochetAmount = 1;
    [SerializeField] private float flameFinalDamageBonus = 0.25f;
    [SerializeField] private float speedPickDodgeRatePlus = 10f;

    private Button[] optionButtons;
    private UpgradeOption[] currentOptions;

    private bool penaltyMode = false;
    private float projectilePathChance;
    private float ricochetChance;
    private float flameChance;

    private struct UpgradeOption
    {
        public UpgradeType type;
        public float value;
        public bool isDoubled;
    }

    private string L(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("UpgradeTable", key);
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
        ClearUiSelection();
        ResetSpecialUpgradeChances();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
        }

        Player_settings.OnLevelUp += ShowMenu;
    }

    void OnDestroy()
    {
        IsMenuOpen = false;
        ClearUiSelection();
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

        ClearUiSelection();
        ResetOptionVisualStates();
        IsMenuOpen = true;
        levelUpMenuUI.SetActive(true);
        Time.timeScale = 0f;

        foreach (var btn in optionButtons)
        {
            btn.interactable = false;
            SetButtonRaycastEnabled(btn, false);
        }

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
            UpgradeType.Damage,
            UpgradeType.Armor,
            UpgradeType.DamageReduction,
            UpgradeType.Dodge,
            UpgradeType.Regeneration
        };

        TryAddSpecialUpgrade(types, UpgradeType.ProjectilePath, projectilePathChance);
        TryAddSpecialUpgrade(types, UpgradeType.Ricochet, ricochetChance);
        TryAddSpecialUpgrade(types, UpgradeType.Flame, flameChance);

        for (int i = 0; i < types.Count; i++)
        {
            int swapIndex = Random.Range(i, types.Count);
            (types[i], types[swapIndex]) = (types[swapIndex], types[i]);
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            UpgradeType type = i < types.Count ? types[i] : (UpgradeType)Random.Range(0, types.Count);
            float value = GetFixedValueForType(type);
            bool isDoubled = !IsSpecialUpgrade(type) && Random.value < doubleValueChance;
            if (isDoubled)
                value *= 2f;

            currentOptions[i] = new UpgradeOption { type = type, value = value, isDoubled = isDoubled };
        }

        UpdateSpecialUpgradeChancesFromFinalOptions();
    }

    private void TryAddSpecialUpgrade(List<UpgradeType> types, UpgradeType specialType, float currentChance)
    {
        if (Random.value < Mathf.Clamp01(currentChance))
            types.Add(specialType);
    }

    private void UpdateSpecialUpgradeChancesFromFinalOptions()
    {
        UpdateSpecialUpgradeChance(ref projectilePathChance, UpgradeType.ProjectilePath);
        UpdateSpecialUpgradeChance(ref ricochetChance, UpgradeType.Ricochet);
        UpdateSpecialUpgradeChance(ref flameChance, UpgradeType.Flame);
    }

    private void UpdateSpecialUpgradeChance(ref float currentChance, UpgradeType specialType)
    {
        if (ContainsFinalOption(specialType))
        {
            currentChance = specialUpgradeChance;
            return;
        }

        currentChance = Mathf.Min(1f, currentChance + specialUpgradeChance);
    }

    private bool ContainsFinalOption(UpgradeType type)
    {
        for (int i = 0; i < currentOptions.Length; i++)
        {
            if (currentOptions[i].type == type)
                return true;
        }

        return false;
    }

    private void ResetSpecialUpgradeChances()
    {
        projectilePathChance = specialUpgradeChance;
        ricochetChance = specialUpgradeChance;
        flameChance = specialUpgradeChance;
    }

    private float GetFixedValueForType(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Health: return ranges.healthMin;
            case UpgradeType.Speed: return Mathf.Round(ranges.speedMin * 10f) / 10f;
            case UpgradeType.CritRate: return ranges.critRateMin;
            case UpgradeType.CritDamage: return ranges.critDamageMin;
            case UpgradeType.Damage: return ranges.damageMin;

            case UpgradeType.Armor: return ranges.armorMin;
            case UpgradeType.DamageReduction: return ranges.damageReductionMin;
            case UpgradeType.Dodge: return ranges.dodgeMin;
            case UpgradeType.Regeneration: return ranges.regenerationMin;

            case UpgradeType.ProjectilePath: return projectilePathAmount;
            case UpgradeType.Ricochet: return ricochetAmount;
            case UpgradeType.Flame: return flameFinalDamageBonus;
            default: return 0;
        }
    }

    private bool IsSpecialUpgrade(UpgradeType type)
    {
        return type == UpgradeType.ProjectilePath || type == UpgradeType.Ricochet || type == UpgradeType.Flame;
    }

    private void RefreshButtonTexts()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var text = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
                text.text = GetDisplayText(currentOptions[i]);

            Transform iconTransform = optionButtons[i].transform.Find("Image_Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = GetIconForUpgradeType(currentOptions[i].type);
                    iconImage.enabled = iconImage.sprite != null;
                }
            }
        }
    }

    private Sprite GetIconForUpgradeType(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                return damageIcon;

            case UpgradeType.CritRate:
            case UpgradeType.CritDamage:
                return critIcon;

            case UpgradeType.Speed:
                return speedIcon;

            case UpgradeType.Health:
                return healthIcon;

            case UpgradeType.Armor:
                return armorIcon;

            case UpgradeType.DamageReduction:
                return damageReductionIcon;

            case UpgradeType.Dodge:
                return dodgeIcon;

            case UpgradeType.Regeneration:
                return regenerationIcon;

            case UpgradeType.ProjectilePath:
            case UpgradeType.Ricochet:
            case UpgradeType.Flame:
                return damageIcon;

            default:
                return null;
        }
    }

    private string GetDisplayText(UpgradeOption opt)
    {
        string rawValueText = opt.type switch
        {
            UpgradeType.Speed => opt.value.ToString("0.0"),
            UpgradeType.Flame => $"{Mathf.RoundToInt(opt.value * 100f)}%",
            UpgradeType.DamageReduction => $"{Mathf.RoundToInt(opt.value * 100f)}%",
            UpgradeType.Dodge => $"{Mathf.RoundToInt(opt.value)}%",
            _ => Mathf.RoundToInt(opt.value).ToString()
        };

        string valueText = (opt.isDoubled || IsSpecialUpgrade(opt.type))
            ? $"<color=#FFD54A>{rawValueText}</color>"
            : rawValueText;

        string baseText = opt.type switch
        {
            UpgradeType.Health => $"{L("upgrade_health")} +{valueText}",
            UpgradeType.Speed => $"{L("upgrade_speed")} +{valueText} / {L("upgrade_dodge")} +{Mathf.RoundToInt(speedPickDodgeRatePlus)}%",
            UpgradeType.CritRate => $"{L("upgrade_crit_rate")} +{valueText}",
            UpgradeType.CritDamage => $"{L("upgrade_crit_damage")} +{valueText}",
            UpgradeType.Damage => $"{L("upgrade_damage")} +{valueText}",

            UpgradeType.Armor => $"{L("upgrade_armor")} +{valueText}",
            UpgradeType.DamageReduction => $"{L("upgrade_damage_reduction")} +{valueText}",
            UpgradeType.Dodge => $"{L("upgrade_dodge")} +{valueText}",
            UpgradeType.Regeneration => $"{L("upgrade_regeneration")} +{valueText}",

            UpgradeType.ProjectilePath => $"{L("upgrade_projectile")} +{valueText}",
            UpgradeType.Ricochet => $"{L("upgrade_ricochet")} +{valueText}",
            UpgradeType.Flame => $"{L("upgrade_total_damage")} +{valueText}",
            _ => ""
        };

        if (penaltyMode)
        {
            string drawback = opt.type switch
            {
                UpgradeType.Damage => $"  <color=#FF5555>({L("penalty_take")} +{Mathf.RoundToInt(damagePickDamageTakenPlus * 100f)}% {L("penalty_dmg")})</color>",
                UpgradeType.Speed => $"  <color=#FF5555>({L("upgrade_damage")} -{Mathf.RoundToInt(speedPickDamageMultMinus * 100f)}%)</color>",
                UpgradeType.Health => $"  <color=#FF5555>({L("upgrade_speed")} -{healthPickSpeedMinus:0.0})</color>",
                _ => ""
            };

            baseText += drawback;
        }

        string currentStatText = GetCurrentStatText(opt);
        if (!string.IsNullOrEmpty(currentStatText))
        {
            baseText += $"\n<size=90%><color=#444444>{currentStatText}</color></size>";
        }

        return baseText;
    }

        private string GetCurrentStatText(UpgradeOption opt)
        {
            var playerSettings = Player_settings.Instance;
            var playerController = playerSettings != null ? playerSettings.GetComponent<PlayerController>() : null;
            var rangedShooter = playerSettings != null ? playerSettings.GetComponent<RangedShooter>() : null;

            switch (opt.type)
            {
                case UpgradeType.Health:
                    if (playerSettings != null)
                    {
                        int current = playerSettings.GetMaxHealth();
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("stat_max_hp")}: {current} {L("stat_increase_to")} <color=#FF5555>{next}</color>";
                    }
                    break;

                case UpgradeType.Speed:
                    if (playerController != null)
                    {
                        float current = playerController.GetMoveSpeed();
                        float next = current + opt.value;
                        return $"{L("stat_speed")}: {current:0.0} {L("stat_increase_to")} <color=#FF5555>{next:0.0}</color>";
                    }
                    break;

                case UpgradeType.CritRate:
                    if (playerSettings != null)
                    {
                        int current = Mathf.RoundToInt(playerSettings.GetCritRate());
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("stat_crit_rate")}: {current}% {L("stat_increase_to")} <color=#FF5555>{next}%</color>";
                    }
                    break;

                case UpgradeType.CritDamage:
                    if (playerSettings != null)
                    {
                        int current = Mathf.RoundToInt(playerSettings.GetCritDamage());
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("stat_crit_damage")}: {current}% {L("stat_increase_to")} <color=#FF5555>{next}%</color>";
                    }
                    break;

                case UpgradeType.Damage:
                    if (rangedShooter != null)
                    {
                        int current = rangedShooter.GetAttackDamage();
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("stat_atk")}: {current} {L("stat_increase_to")} <color=#FF5555>{next}</color>";
                    }
                    break;

                case UpgradeType.Armor:
                    if (playerSettings != null)
                    {
                        int current = playerSettings.GetArmor();
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("stat_armor")}: {current} {L("stat_increase_to")} <color=#FF5555>{next}</color>";
                    }
                    break;

                case UpgradeType.DamageReduction:
                    if (playerSettings != null)
                    {
                        int current = Mathf.RoundToInt(playerSettings.GetDamageReduction() * 100f);
                        int next = Mathf.RoundToInt((playerSettings.GetDamageReduction() + opt.value) * 100f);
                        return $"{L("stat_damage_reduction")}: {current}% {L("stat_increase_to")} <color=#FF5555>{next}%</color>";
                    }
                    break;

                case UpgradeType.Dodge:
                    if (playerSettings != null)
                    {
                        int current = Mathf.RoundToInt(playerSettings.GetDodgeRate());
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("stat_dodge")}: {current}% {L("stat_increase_to")} <color=#FF5555>{next}%</color>";
                    }
                    break;

                case UpgradeType.Regeneration:
                    if (playerSettings != null)
                    {
                        int current = playerSettings.GetRegeneration();
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("stat_regeneration")}: {current} {L("stat_increase_to")} <color=#FF5555>{next}</color>";
                    }
                    break;

                case UpgradeType.ProjectilePath:
                    if (rangedShooter != null)
                    {
                        int current = rangedShooter.GetProjectileCount();
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("stat_projectiles")}: {current} {L("stat_increase_to")} <color=#FF5555>{next}</color>";
                    }
                    break;

                case UpgradeType.Ricochet:
                    if (rangedShooter != null)
                    {
                        int current = rangedShooter.GetChainBounceCount();
                        int next = current + Mathf.RoundToInt(opt.value);
                        return $"{L("upgrade_ricochet")}: {current} {L("stat_increase_to")} <color=#FF5555>{next}</color>";
                    }
                    break;

                case UpgradeType.Flame:
                    if (rangedShooter != null)
                    {
                        int current = Mathf.RoundToInt(rangedShooter.GetFinalDamageMultiplier() * 100f);
                        int next = Mathf.RoundToInt((rangedShooter.GetFinalDamageMultiplier() + opt.value) * 100f);
                        return $"{L("stat_total_damage")}: {current}% {L("stat_increase_to")} <color=#FF5555>{next}%</color>";
                    }
                    break;
            }

            return "";
        }


    private IEnumerator EnableButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(clickDelaySeconds);

        foreach (var btn in optionButtons)
        {
            btn.interactable = true;
            SetButtonRaycastEnabled(btn, true);
        }
    }

    private void OnOptionClicked(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= currentOptions.Length) return;

        var chosen = currentOptions[buttonIndex];
        ApplyUpgrade(chosen);

        ClearUiSelection();
        ResetOptionVisualStates();
        IsMenuOpen = false;
        Time.timeScale = 1f;
        levelUpMenuUI.SetActive(false);
    }

    private void ClearUiSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void SetButtonRaycastEnabled(Button button, bool enabled)
    {
        if (button == null) return;

        Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = enabled;
    }

    private void ResetOptionVisualStates()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            UIHoverScaleGlow hoverGlow = optionButtons[i].GetComponent<UIHoverScaleGlow>();
            if (hoverGlow != null)
                hoverGlow.ResetVisualStateImmediate();
        }
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

                if (penaltyMode && playerController != null) playerController.AddMoveSpeed(-healthPickSpeedMinus);
                break;

            case UpgradeType.Speed:
                if (playerController != null) playerController.AddMoveSpeed(opt.value);
                if (playerSettings != null) playerSettings.AddDodgeRate(speedPickDodgeRatePlus);

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

                if (penaltyMode && playerSettings != null) playerSettings.AddDamageTakenMultiplier(damagePickDamageTakenPlus);
                break;

            case UpgradeType.Armor:
                if (playerSettings != null) playerSettings.AddArmor(Mathf.RoundToInt(opt.value));
                break;

            case UpgradeType.DamageReduction:
                if (playerSettings != null) playerSettings.AddDamageReduction(opt.value);
                break;

            case UpgradeType.Dodge:
                if (playerSettings != null) playerSettings.AddDodgeRate(opt.value);
                break;

            case UpgradeType.Regeneration:
                if (playerSettings != null) playerSettings.AddRegeneration(Mathf.RoundToInt(opt.value));
                break;

            case UpgradeType.ProjectilePath:
                if (rangedShooter != null) rangedShooter.AddProjectileCount(Mathf.RoundToInt(opt.value));
                break;

            case UpgradeType.Ricochet:
                if (rangedShooter != null) rangedShooter.AddChainBounceCount(Mathf.RoundToInt(opt.value));
                break;

            case UpgradeType.Flame:
                if (rangedShooter != null) rangedShooter.AddFinalDamageMultiplier(opt.value);
                break;
        }
    }
}
