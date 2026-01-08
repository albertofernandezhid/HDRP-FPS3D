using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Collections.Generic;

public class GraphicsDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private string tableName = "LanguagesTable";
    [SerializeField] private List<string> optionKeys;

    private void Awake()
    {
        if (!dropdown)
            dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += Refresh;
        Refresh(LocalizationSettings.SelectedLocale);
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= Refresh;
    }

    private async void Refresh(Locale locale)
    {
        int value = dropdown.value;
        dropdown.options.Clear();

        foreach (var key in optionKeys)
        {
            var handle = LocalizationSettings.StringDatabase
                .GetLocalizedStringAsync(tableName, key);

            string text = await handle.Task;

            dropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }

        dropdown.value = Mathf.Clamp(value, 0, dropdown.options.Count - 1);
        dropdown.RefreshShownValue();
    }
}
