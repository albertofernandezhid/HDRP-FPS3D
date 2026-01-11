using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Progreso")]
    public int levelsUnlocked = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            levelsUnlocked = PlayerPrefs.GetInt("LevelsUnlocked", 1);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UnlockNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex >= levelsUnlocked)
        {
            levelsUnlocked = currentSceneIndex + 1;
            PlayerPrefs.SetInt("LevelsUnlocked", levelsUnlocked);
            PlayerPrefs.Save();
        }
    }

    public void LoadScene(string name)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(name);
    }

    public void ResetProgress()
    {
        levelsUnlocked = 1;
        PlayerPrefs.SetInt("LevelsUnlocked", 1);
        PlayerPrefs.Save();
    }
}