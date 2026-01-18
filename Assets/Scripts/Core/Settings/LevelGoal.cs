using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private ParticleSystem victoryParticles;

    private bool levelFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (levelFinished) return;

        if (other.CompareTag(playerTag))
        {
            levelFinished = true;
            FinishLevel();
        }
    }

    private void FinishLevel()
    {
        ScoreManager scManager = Object.FindFirstObjectByType<ScoreManager>();
        if (scManager != null)
        {
            scManager.FinishLevel();
        }

        PlayerPrefs.Save();

        MenuScore menuScore = Object.FindFirstObjectByType<MenuScore>();
        if (menuScore != null)
        {
            menuScore.RefreshScores();
        }

        if (victoryParticles != null)
        {
            var main = victoryParticles.main;
            main.useUnscaledTime = true;
            victoryParticles.Play();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockNextLevel();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWin();
        }
    }
}
