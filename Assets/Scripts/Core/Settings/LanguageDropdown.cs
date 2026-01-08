using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private List<Locale> locales;

    private void Awake()
    {
        if (!dropdown)
            dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        locales = LocalizationSettings.AvailableLocales.Locales;

        dropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (var locale in locales)
            options.Add(locale.LocaleName);

        dropdown.AddOptions(options);

        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        int savedIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
        dropdown.value = Mathf.Clamp(savedIndex, 0, locales.Count - 1);
        dropdown.RefreshShownValue();

        LocalizationSettings.SelectedLocale = locales[dropdown.value];
    }

    private void OnDisable()
    {
        dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        LocalizationSettings.SelectedLocale = locales[index];
        PlayerPrefs.SetInt("LanguageIndex", index);
    }
}