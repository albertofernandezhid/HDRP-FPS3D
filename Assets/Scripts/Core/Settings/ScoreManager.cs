using UnityEngine;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    [Header("Configuración de Nivel")]
    public string levelName = "Tutorial";

    private int totalEnemiesAtStart;
    private int enemiesKilled = 0;
    private int projectilesShot = 0;
    private int projectilesHit = 0;

    void Start()
    {
        var melee = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(x => x.GetType().Name == "MeleeFinal").Count();
        var range = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(x => x.GetType().Name == "RangeFinal").Count();

        totalEnemiesAtStart = melee + range;
        Debug.Log($"Enemigos totales detectados: {totalEnemiesAtStart}");
    }

    public void RegisterShot() => projectilesShot++;

    public void RegisterHit() => projectilesHit++;

    public void RegisterKill() => enemiesKilled++;

    public void FinishLevel()
    {
        float accuracy = 0;
        if (projectilesShot > 0)
        {
            accuracy = ((float)projectilesHit / projectilesShot) * 100f;
        }

        int finalScore = (enemiesKilled * 100) + (int)(accuracy * 10);

        int currentTopScore = PlayerPrefs.GetInt("TopScore_" + levelName, 0);

        if (finalScore > currentTopScore)
        {
            PlayerPrefs.SetInt("TopScore_" + levelName, finalScore);
            PlayerPrefs.Save();
        }

        Debug.Log($"Escena terminada. Score: {finalScore}. Precisión: {accuracy}%");
    }
}