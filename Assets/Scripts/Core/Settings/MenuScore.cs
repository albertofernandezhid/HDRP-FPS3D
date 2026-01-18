using UnityEngine;
using TMPro;

public class MenuScore : MonoBehaviour
{
    [Header("Textos de Score")]
    public TextMeshProUGUI txtScoreTutorial;
    public TextMeshProUGUI txtScoreLvl1;
    public TextMeshProUGUI txtScoreLvl2;

    [Header("UI General")]
    public GameObject btnResetScore;

    void OnEnable()
    {
        RefreshScores();
    }

    public void RefreshScores()
    {
        int scoreTut = PlayerPrefs.GetInt("TopScore_Tutorial", 0);
        int score1 = PlayerPrefs.GetInt("TopScore_Lvl1", 0);
        int score2 = PlayerPrefs.GetInt("TopScore_Lvl2", 0);

        UpdateScoreDisplay(txtScoreTutorial, scoreTut);
        UpdateScoreDisplay(txtScoreLvl1, score1);
        UpdateScoreDisplay(txtScoreLvl2, score2);

        bool anyScoreExists = (scoreTut > 0 || score1 > 0 || score2 > 0);

        if (btnResetScore != null)
        {
            btnResetScore.SetActive(anyScoreExists);
        }
    }

    private void UpdateScoreDisplay(TextMeshProUGUI textComp, int score)
    {
        if (textComp != null)
        {
            if (score > 0)
            {
                textComp.gameObject.SetActive(true);
                textComp.text = "Top Score: " + score;
            }
            else
            {
                textComp.gameObject.SetActive(false);
                textComp.text = "";
            }
        }
    }

    public void ResetAllScores()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        RefreshScores();
    }
}