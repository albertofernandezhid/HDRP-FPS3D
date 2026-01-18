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

    private bool _isRefreshing = false;

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
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                await LocalizationSettings.InitializationOperation.Task;
            }

            int currentValue = QualitySettings.GetQualityLevel();
            string[] qualityNames = QualitySettings.names;

            List<Task<string>> tasks = new List<Task<string>>();

            foreach (string name in qualityNames)
            {
                tasks.Add(LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, name).Task);
            }

            string[] results = await Task.WhenAll(tasks);

            dropdown.options.Clear();
            foreach (string translatedText in results)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(translatedText));
            }

            dropdown.value = currentValue;
            dropdown.RefreshShownValue();
        }
        finally
        {
            _isRefreshing = false;
        }
    }
}