using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu_LevelUp : MonoBehaviour
{
    public enum UpgradeType
    {
        Health,
        Speed,
        CritRate,
        CritDamage,
        Damage,

        Armor,
        DamageReduction,
        Dodge,
        Regeneration
    }

    [System.Serializable]
    public struct UpgradeRanges
    {
        public int healthMin, healthMax;
        public float speedMin, speedMax;
        public int critRateMin, critRateMax;
        public int critDamageMin, critDamageMax;
        public int damageMin, damageMax;

        public int armorMin, armorMax;
        public float damageReductionMin, damageReductionMax;
        public float dodgeMin, dodgeMax;
        public int regenerationMin, regenerationMax;
    }

    [SerializeField] private GameObject levelUpMenuUI;
    [SerializeField] private float clickDelaySeconds = 1f;
    [SerializeField] private AudioClip levelUpClip;

    [Header("Upgrade Icons")]
    [SerializeField] private Sprite damageIcon;
    [SerializeField] private Sprite healthIcon;
    [SerializeField] private Sprite speedIcon;
    [SerializeField] private Sprite critIcon;

    [Header("Penalty Rules")]
    [SerializeField] private int penaltyStartLevel = 6;
    [SerializeField] private float damagePickDamageTakenPlus = 0.1f;
    [SerializeField] private float speedPickDamageMultMinus = 0.1f;
    [SerializeField] private float healthPickSpeedMinus = 0.3f;

    [SerializeField]
    private UpgradeRanges ranges = new UpgradeRanges
    {
        healthMin = 2,
        healthMax = 4,

        speedMin = 0.1f,
        speedMax = 1f,

        critRateMin = 5,
        critRateMax = 15,

        critDamageMin = 10,
        critDamageMax = 40,

        damageMin = 2,
        damageMax = 5,

        armorMin = 1,
        armorMax = 2,

        damageReductionMin = 0.05f,
        damageReductionMax = 0.15f,

        dodgeMin = 0.05f,
        dodgeMax = 0.15f,

        regenerationMin = 1,
        regenerationMax = 1
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

    private void ShowMenu(int newLevel)
    {
        penaltyMode = newLevel >= penaltyStartLevel;

        GenerateUpgradeOptions();
        RefreshButtonTexts();

        levelUpMenuUI.SetActive(true);
        Time.timeScale = 0f;

        if (levelUpClip != null)
        {
            AudioSource.PlayClipAtPoint(levelUpClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }

        Debug.Log($"[Level Up] Level {newLevel} (PenaltyMode={penaltyMode}) - Options: " +
                  string.Join(", ", System.Array.ConvertAll(currentOptions, o => GetDisplayText(o))));

        foreach (var btn in optionButtons)
        {
            btn.interactable = false;
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

        for (int i = 0; i < types.Count; i++)
        {
            int swapIndex = Random.Range(i, types.Count);
            UpgradeType temp = types[i];
            types[i] = types[swapIndex];
            types[swapIndex] = temp;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            UpgradeType type = i < types.Count
                ? types[i]
                : (UpgradeType)Random.Range(0, System.Enum.GetValues(typeof(UpgradeType)).Length);

            float value = GetRandomValueForType(type);
            currentOptions[i] = new UpgradeOption { type = type, value = value };
        }
    }

    private float GetRandomValueForType(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Health:
                return Random.Range(ranges.healthMin, ranges.healthMax + 1);

            case UpgradeType.Speed:
                return Mathf.Round(Random.Range(ranges.speedMin, ranges.speedMax) * 10f) / 10f;

            case UpgradeType.CritRate:
                return Random.Range(ranges.critRateMin, ranges.critRateMax + 1);

            case UpgradeType.CritDamage:
                return Random.Range(ranges.critDamageMin, ranges.critDamageMax + 1);

            case UpgradeType.Damage:
                return Random.Range(ranges.damageMin, ranges.damageMax + 1);

            case UpgradeType.Armor:
                return Random.Range(ranges.armorMin, ranges.armorMax + 1);

            case UpgradeType.DamageReduction:
                return Mathf.Round(Random.Range(ranges.damageReductionMin, ranges.damageReductionMax) * 100f) / 100f;

            case UpgradeType.Dodge:
                return Mathf.Round(Random.Range(ranges.dodgeMin, ranges.dodgeMax) * 100f) / 100f;

            case UpgradeType.Regeneration:
                return Random.Range(ranges.regenerationMin, ranges.regenerationMax + 1);

            default:
                return 0f;
        }
    }

    private void RefreshButtonTexts()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var text = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.text = GetDisplayText(currentOptions[i]);
            }

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
            case UpgradeType.Armor:
            case UpgradeType.DamageReduction:
            case UpgradeType.Dodge:
            case UpgradeType.Regeneration:
                return healthIcon;

            default:
                return null;
        }
    }

    private string GetDisplayText(UpgradeOption opt)
    {
        switch (opt.type)
        {
            case UpgradeType.Health:
                return $"Health +{Mathf.RoundToInt(opt.value)}";

            case UpgradeType.Speed:
                return $"Speed +{opt.value:0.0}";

            case UpgradeType.CritRate:
                return $"Crit Rate +{Mathf.RoundToInt(opt.value)}";

            case UpgradeType.CritDamage:
                return $"Crit Damage +{Mathf.RoundToInt(opt.value)}";

            case UpgradeType.Damage:
                return $"Damage +{Mathf.RoundToInt(opt.value)}";

            case UpgradeType.Armor:
                return $"Armor +{Mathf.RoundToInt(opt.value)}";

            case UpgradeType.DamageReduction:
                return $"Damage Reduction +{Mathf.RoundToInt(opt.value * 100f)}%";

            case UpgradeType.Dodge:
                return $"Dodge +{Mathf.RoundToInt(opt.value * 100f)}%";

            case UpgradeType.Regeneration:
                return $"Regeneration +{Mathf.RoundToInt(opt.value)}";

            default:
                return "";
        }
    }

    private IEnumerator EnableButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(clickDelaySeconds);

        foreach (var btn in optionButtons)
        {
            btn.interactable = true;
        }
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
                if (playerSettings != null)
                    playerSettings.AddMaxHealth(Mathf.RoundToInt(opt.value));

                if (penaltyMode && playerController != null)
                    playerController.AddMoveSpeed(-healthPickSpeedMinus);
                break;

            case UpgradeType.Speed:
                if (playerController != null)
                    playerController.AddMoveSpeed(opt.value);

                if (penaltyMode && rangedShooter != null)
                    rangedShooter.AddDamageMultiplier(-speedPickDamageMultMinus);
                break;

            case UpgradeType.CritRate:
                if (playerSettings != null)
                    playerSettings.AddCritRate(opt.value);
                break;

            case UpgradeType.CritDamage:
                if (playerSettings != null)
                    playerSettings.AddCritDamage(opt.value);
                break;

            case UpgradeType.Damage:
                if (rangedShooter != null)
                    rangedShooter.AddAttackDamage(Mathf.RoundToInt(opt.value));

                if (penaltyMode && playerSettings != null)
                    playerSettings.AddDamageTakenMultiplier(damagePickDamageTakenPlus);
                break;

            case UpgradeType.Armor:
                if (playerSettings != null)
                    playerSettings.AddArmor(Mathf.RoundToInt(opt.value));
                break;

            case UpgradeType.DamageReduction:
                if (playerSettings != null)
                    playerSettings.AddDamageReduction(opt.value);
                break;

            case UpgradeType.Dodge:
                if (playerSettings != null)
                    playerSettings.AddDodge(opt.value);
                break;

            case UpgradeType.Regeneration:
                if (playerSettings != null)
                    playerSettings.AddRegeneration(Mathf.RoundToInt(opt.value));
                break;
        }
    }
}