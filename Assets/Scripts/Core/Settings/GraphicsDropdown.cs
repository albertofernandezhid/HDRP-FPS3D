using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GraphicsSettingsManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private string tableName = "LanguagesTable";
    [SerializeField] private List<string> optionKeys;

    private void Awake()
    {
        if (!dropdown) dropdown = GetComponent<TMP_Dropdown>();

        int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(savedQuality, true);
        dropdown.value = savedQuality;
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        dropdown.onValueChanged.AddListener(SetQuality);
        RefreshOptions();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        dropdown.onValueChanged.RemoveListener(SetQuality);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("GraphicsQuality", index);
        PlayerPrefs.Save();
    }

    private void OnLocaleChanged(Locale locale)
    {
        RefreshOptions();
    }

    private async void RefreshOptions()
    {
        int currentValue = dropdown.value;
        dropdown.options.Clear();

        List<Task<string>> tasks = new List<Task<string>>();
        foreach (var key in optionKeys)
        {
            tasks.Add(LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key).Task);
        }

        string[] results = await Task.WhenAll(tasks);

        foreach (string translatedText in results)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(translatedText));
        }

        dropdown.value = Mathf.Clamp(currentValue, 0, dropdown.options.Count - 1);
        dropdown.RefreshShownValue();
    }
}