using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneles")]
    public GameObject panelPause;
    public GameObject panelSettingsPause, panelControls, panelDead, panelWin;

    [Header("Audio")]
    public AudioMixer mainMixer;
    public Slider sMaster, sMusic, sAmbient, sSFX;

    [Header("Efecto de Pausa")]
    public Volume globalVolume;
    public float blurFadeSpeed = 5f;

    [Header("Configuración Muerte")]
    public float deathDelay = 3f;

    [Header("Gamepad Navigation")]
    public GameObject firstButtonPause;
    public GameObject firstButtonSettings;
    public GameObject firstButtonControls;
    public GameObject firstButtonDead;
    public GameObject firstButtonWin;

    void Awake() => Instance = this;

    void Start()
    {
        CargarAudio();
        CloseAll();
    }

    void Update() => HandleBlurFade();

    private void SetInitialSelection(GameObject target)
    {
        if (target == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }

    private void HandleBlurFade()
    {
        if (globalVolume == null) return;
        bool shouldBlur = (Time.timeScale == 0 || panelDead.activeSelf || panelWin.activeSelf);
        float targetWeight = shouldBlur ? 1f : 0f;
        globalVolume.weight = Mathf.MoveTowards(globalVolume.weight, targetWeight, blurFadeSpeed * Time.unscaledDeltaTime);
    }

    public void CloseAll()
    {
        panelPause.SetActive(false);
        panelSettingsPause.SetActive(false);
        panelControls.SetActive(false);
        panelDead.SetActive(false);
        panelWin.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void TogglePause()
    {
        if (panelDead.activeSelf || panelWin.activeSelf) return;
        bool pausing = !panelPause.activeSelf && !panelSettingsPause.activeSelf && !panelControls.activeSelf;
        panelPause.SetActive(pausing);
        if (!pausing)
        {
            panelSettingsPause.SetActive(false);
            panelControls.SetActive(false);
        }
        Time.timeScale = pausing ? 0f : 1f;
        Cursor.lockState = pausing ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pausing;
        if (pausing) SetInitialSelection(firstButtonPause);
    }

    public void Resume() => TogglePause();

    public void ShowDead() => StartCoroutine(DeathSequence());

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);
        panelDead.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetInitialSelection(firstButtonDead);
    }

    public void ShowWin()
    {
        panelWin.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetInitialSelection(firstButtonWin);
    }

    public void SetMaster(float v) { if (mainMixer) mainMixer.SetFloat("Master", Mathf.Log10(Mathf.Max(0.0001f, v)) * 20); PlayerPrefs.SetFloat("MasterVol", v); }
    public void SetMusic(float v) { if (mainMixer) mainMixer.SetFloat("Music", Mathf.Log10(Mathf.Max(0.0001f, v)) * 20); PlayerPrefs.SetFloat("MusicVol", v); }
    public void SetAmbient(float v) { if (mainMixer) mainMixer.SetFloat("Ambient", Mathf.Log10(Mathf.Max(0.0001f, v)) * 20); PlayerPrefs.SetFloat("AmbientVol", v); }
    public void SetSFX(float v) { if (mainMixer) mainMixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(0.0001f, v)) * 20); PlayerPrefs.SetFloat("SFXVol", v); }

    void CargarAudio()
    {
        float m = PlayerPrefs.GetFloat("MasterVol", 0.75f), mu = PlayerPrefs.GetFloat("MusicVol", 0.75f), a = PlayerPrefs.GetFloat("AmbientVol", 0.75f), s = PlayerPrefs.GetFloat("SFXVol", 0.75f);
        if (sMaster) { sMaster.value = m; SetMaster(m); }
        if (sMusic) { sMusic.value = mu; SetMusic(mu); }
        if (sAmbient) { sAmbient.value = a; SetAmbient(a); }
        if (sSFX) { sSFX.value = s; SetSFX(s); }
    }

    public void OpenSettings() { panelPause.SetActive(false); panelSettingsPause.SetActive(true); SetInitialSelection(firstButtonSettings); }

    public void OpenControls() { panelPause.SetActive(false); panelControls.SetActive(true); SetInitialSelection(firstButtonControls); }

    public void BackToPause()
    {
        panelSettingsPause.SetActive(false);
        panelControls.SetActive(false);
        panelPause.SetActive(true);
        SetInitialSelection(firstButtonPause);
    }

    public void GoToMenu() => CargarEscenaInterna("MainMenu");
    public void Retry() => CargarEscenaInterna(SceneManager.GetActiveScene().name);

    public void NextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockNextLevel();
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                GoToMenu();
            }
        }
    }

    private void CargarEscenaInterna(string nombre)
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.LoadScene(nombre);
        else SceneManager.LoadScene(nombre);
    }

    private void OnDisable() => PlayerPrefs.Save();
}