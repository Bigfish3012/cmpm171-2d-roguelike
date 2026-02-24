using System.Collections;
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
        public int healthMin, healthMax;      // e.g. 2, 4
        public int speedMin, speedMax;         // e.g. 1, 3
        public int critRateMin, critRateMax;   // e.g. 5, 25
        public int critDamageMin, critDamageMax; // e.g. 30, 80
        public int damageMin, damageMax;      // e.g. 2, 5
    }

    [SerializeField] private GameObject levelUpMenuUI;
    [SerializeField] private float clickDelaySeconds = 1f;
    [SerializeField] private UpgradeRanges ranges = new UpgradeRanges
    {
        healthMin = 2, healthMax = 4,
        speedMin = 1, speedMax = 3,
        critRateMin = 5, critRateMax = 25,
        critDamageMin = 30, critDamageMax = 80,
        damageMin = 2, damageMax = 5
    };

    private Button[] optionButtons;
    private UpgradeOption[] currentOptions;

    private struct UpgradeOption
    {
        public UpgradeType type;
        public int value;
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
        GenerateUpgradeOptions();
        RefreshButtonTexts();
        levelUpMenuUI.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log($"[Level Up] Level {newLevel} - Options: " +
            string.Join(", ", System.Array.ConvertAll(currentOptions, o => GetDisplayText(o))));

        foreach (var btn in optionButtons)
        {
            btn.interactable = false;
        }

        StartCoroutine(EnableButtonsAfterDelay());
    }

    private void GenerateUpgradeOptions()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            UpgradeType type = (UpgradeType)Random.Range(0, 5);
            int value = GetRandomValueForType(type);
            currentOptions[i] = new UpgradeOption { type = type, value = value };
        }
    }

    private int GetRandomValueForType(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Health: return Random.Range(ranges.healthMin, ranges.healthMax + 1);
            case UpgradeType.Speed: return Random.Range(ranges.speedMin, ranges.speedMax + 1);
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
            {
                text.text = GetDisplayText(currentOptions[i]);
            }
        }
    }

    private string GetDisplayText(UpgradeOption opt)
    {
        switch (opt.type)
        {
            case UpgradeType.Health: return $"Health +{opt.value}";
            case UpgradeType.Speed: return $"Speed +{opt.value}";
            case UpgradeType.CritRate: return $"Crit Rate +{opt.value}";
            case UpgradeType.CritDamage: return $"Crit Damage +{opt.value}";
            case UpgradeType.Damage: return $"Damage +{opt.value}";
            default: return "";
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
                if (playerSettings != null) playerSettings.AddMaxHealth(opt.value);
                break;
            case UpgradeType.Speed:
                if (playerController != null) playerController.AddMoveSpeed(opt.value);
                break;
            case UpgradeType.CritRate:
                if (playerSettings != null) playerSettings.AddCritRate(opt.value);
                break;
            case UpgradeType.CritDamage:
                if (playerSettings != null) playerSettings.AddCritDamage(opt.value);
                break;
            case UpgradeType.Damage:
                if (rangedShooter != null) rangedShooter.AddAttackDamage(opt.value);
                break;
        }
    }
}
