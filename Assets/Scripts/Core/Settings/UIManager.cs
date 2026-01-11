using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneles")]
    public GameObject panelPause;
    public GameObject panelSettingsPause, panelDead, panelWin;

    [Header("Audio")]
    public AudioMixer mainMixer;
    public Slider sMaster, sMusic, sAmbient, sSFX;

    void Awake() => Instance = this;

    void Start()
    {
        CargarAudio();
        CloseAll();
    }

    public void CloseAll()
    {
        panelPause.SetActive(false);
        panelSettingsPause.SetActive(false);
        panelDead.SetActive(false);
        panelWin.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TogglePause()
    {
        if (panelDead.activeSelf || panelWin.activeSelf) return;
        bool pausing = !panelPause.activeSelf && !panelSettingsPause.activeSelf;

        panelPause.SetActive(pausing);
        if (!pausing) panelSettingsPause.SetActive(false);

        Time.timeScale = pausing ? 0f : 1f;
        Cursor.lockState = pausing ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pausing;
    }

    public void Resume() => TogglePause();

    public void ShowDead()
    {
        panelDead.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowWin()
    {
        panelWin.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void NextLevel()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnlockNextLevel();

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            string nextSceneName = NameFromIndex(nextIndex);
            CargarEscenaInterna(nextSceneName);
        }
        else
        {
            GoToMenu();
        }
    }

    public void SetMaster(float v) { if (mainMixer) mainMixer.SetFloat("Master", Mathf.Log10(Mathf.Max(0.0001f, v)) * 20); PlayerPrefs.SetFloat("MasterVol", v); }
    public void SetMusic(float v) { if (mainMixer) mainMixer.SetFloat("Music", Mathf.Log10(Mathf.Max(0.0001f, v)) * 20); PlayerPrefs.SetFloat("MusicVol", v); }
    public void SetAmbient(float v) { if (mainMixer) mainMixer.SetFloat("Ambient", Mathf.Log10(Mathf.Max(0.0001f, v)) * 20); PlayerPrefs.SetFloat("AmbientVol", v); }
    public void SetSFX(float v) { if (mainMixer) mainMixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(0.0001f, v)) * 20); PlayerPrefs.SetFloat("SFXVol", v); }

    void CargarAudio()
    {
        float m = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        float mu = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float a = PlayerPrefs.GetFloat("AmbientVol", 0.75f);
        float s = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        if (sMaster) { sMaster.value = m; SetMaster(m); }
        if (sMusic) { sMusic.value = mu; SetMusic(mu); }
        if (sAmbient) { sAmbient.value = a; SetAmbient(a); }
        if (sSFX) { sSFX.value = s; SetSFX(s); }
    }

    public void OpenSettings() { panelPause.SetActive(false); panelSettingsPause.SetActive(true); }
    public void BackToPause() { panelSettingsPause.SetActive(false); panelPause.SetActive(true); }

    public void GoToMenu() => CargarEscenaInterna("MainMenu");
    public void Retry() => CargarEscenaInterna(SceneManager.GetActiveScene().name);

    private void CargarEscenaInterna(string nombre)
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene(nombre);
        else
            SceneManager.LoadScene(nombre);
    }

    private string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}