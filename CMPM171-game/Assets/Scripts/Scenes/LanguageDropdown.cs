using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    // locale codes
    private readonly List<string> localeCodes = new()
    {
        "en",
        "ja",
        "zh-Hans"
    };

    private readonly List<string> optionLabels = new()
    {
        "English",
        "Japanese (日本語)",
        "Simplified Chinese (简体中文)"
    };

    private async void Start()
    {
        if (!dropdown) dropdown = GetComponent<TMP_Dropdown>();

        await LocalizationSettings.InitializationOperation.Task;

        dropdown.ClearOptions();
        dropdown.AddOptions(optionLabels);
        dropdown.SetValueWithoutNotify(GetIndexFromCurrentLocale());
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private int GetIndexFromCurrentLocale()
    {
        Locale cur = LocalizationSettings.SelectedLocale;
        if (cur == null) return 0;

        string code = cur.Identifier.Code;
        int idx = localeCodes.IndexOf(code);
        return idx >= 0 ? idx : 0;
    }

    private void OnDropdownChanged(int index)
    {
        SetLocaleByIndex(index);
    }

    private void SetLocaleByIndex(int index)
    {
        index = Mathf.Clamp(index, 0, localeCodes.Count - 1);
        string targetCode = localeCodes[index];

        var available = LocalizationSettings.AvailableLocales.Locales;
        foreach (var loc in available)
        {
            if (loc != null && loc.Identifier.Code == targetCode)
            {
                LocalizationSettings.SelectedLocale = loc;
                return;
            }
        }

        Debug.LogWarning($"Locale code '{targetCode}' not found. " +
                         $"Check Project Settings > Localization > Locales.");
    }
}
