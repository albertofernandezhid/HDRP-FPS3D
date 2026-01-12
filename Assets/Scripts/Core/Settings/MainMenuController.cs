using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Header("Panel Padre")]
    public GameObject panelMenuInicio;

    [Header("Paneles Hijos")]
    public GameObject panelPlay;
    public GameObject panelSettings;
    public GameObject panelCredits;

    [Header("Botones de Niveles")]
    public Button btnTutorial;
    public Button btnLvl1, btnLvl2, btnLvl3;

    [Header("Configuración de Audio")]
    public AudioMixer mainMixer;
    public Slider sliderMaster, sliderMusic, sliderAmbient, sliderSFX;

    [Header("Configuración de Calidad")]
    public TMP_Dropdown qualityDropdown;

    [Header("Gamepad Navigation")]
    public GameObject firstButtonMainMenu;
    public GameObject firstButtonPlay;
    public GameObject firstButtonSettings;
    public GameObject firstButtonCredits;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        panelMenuInicio.SetActive(true);
        panelPlay.SetActive(false);
        panelSettings.SetActive(false);
        panelCredits.SetActive(false);

        SetInitialSelection(firstButtonMainMenu);

        Invoke("ActualizarBotonesNiveles", 0.1f);
        CargarYConfigurarSliders();
        ConfigurarQualityDropdown();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (panelPlay.activeSelf || panelSettings.activeSelf || panelCredits.activeSelf)
            {
                CloseAllSubPanels();
            }
        }
    }

    private void SetInitialSelection(GameObject target)
    {
        if (target == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }

    private void CargarYConfigurarSliders()
    {
        float m = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        float mu = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float a = PlayerPrefs.GetFloat("AmbientVol", 0.75f);
        float s = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        if (sliderMaster) sliderMaster.value = m;
        if (sliderMusic) sliderMusic.value = mu;
        if (sliderAmbient) sliderAmbient.value = a;
        if (sliderSFX) sliderSFX.value = s;

        ActualizarMixer("Master", m);
        ActualizarMixer("Music", mu);
        ActualizarMixer("Ambient", a);
        ActualizarMixer("SFX", s);
    }

    public void SetMasterVol(float val) { ActualizarMixer("Master", val); PlayerPrefs.SetFloat("MasterVol", val); }
    public void SetMusicVol(float val) { ActualizarMixer("Music", val); PlayerPrefs.SetFloat("MusicVol", val); }
    public void SetAmbientVol(float val) { ActualizarMixer("Ambient", val); PlayerPrefs.SetFloat("AmbientVol", val); }
    public void SetSFXVol(float val) { ActualizarMixer("SFX", val); PlayerPrefs.SetFloat("SFXVol", val); }

    private void ActualizarMixer(string parameter, float value)
    {
        if (mainMixer != null)
        {
            float db = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20f;
            mainMixer.SetFloat(parameter, db);
        }
    }

    private void ConfigurarQualityDropdown()
    {
        if (qualityDropdown != null)
        {
            int savedQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            qualityDropdown.value = savedQuality;
            QualitySettings.SetQualityLevel(savedQuality);
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }

    public void ShowPanel(GameObject panelAActivar)
    {
        if (panelPlay) panelPlay.SetActive(false);
        if (panelSettings) panelSettings.SetActive(false);
        if (panelCredits) panelCredits.SetActive(false);

        if (panelAActivar != null && panelAActivar != panelMenuInicio)
        {
            panelAActivar.SetActive(true);
            if (panelAActivar == panelPlay) SetInitialSelection(firstButtonPlay);
            else if (panelAActivar == panelSettings) SetInitialSelection(firstButtonSettings);
            else if (panelAActivar == panelCredits) SetInitialSelection(firstButtonCredits);
        }
        else
        {
            SetInitialSelection(firstButtonMainMenu);
        }
    }

    public void CloseAllSubPanels()
    {
        if (panelPlay) panelPlay.SetActive(false);
        if (panelSettings) panelSettings.SetActive(false);
        if (panelCredits) panelCredits.SetActive(false);
        SetInitialSelection(firstButtonMainMenu);
    }

    void ActualizarBotonesNiveles()
    {
        if (GameManager.Instance == null) return;
        int progress = GameManager.Instance.levelsUnlocked;
        if (btnTutorial) btnTutorial.interactable = true;
        if (btnLvl1) btnLvl1.interactable = (progress >= 2);
        if (btnLvl2) btnLvl2.interactable = (progress >= 3);
        if (btnLvl3) btnLvl3.interactable = (progress >= 4);
    }

    public void PlayLevel(string sceneName)
    {
        if (GameManager.Instance != null) GameManager.Instance.LoadScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() => Application.Quit();

    private void OnDisable() => PlayerPrefs.Save();
}