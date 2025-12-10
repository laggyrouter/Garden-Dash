using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class HighScoreUI : MonoBehaviour
{
    public TextMeshProUGUI[] scoreTexts;
    public string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        List<int> scores = new List<int>();

        if (ScoreManager.instance != null && 
            ScoreManager.instance.highScores != null &&
            ScoreManager.instance.highScores.Count > 0)
        {
            scores.AddRange(ScoreManager.instance.highScores);
        }
        else
        {
            int count = PlayerPrefs.GetInt("HighScoreCount", 0);

            for (int i = 0; i < count; i++)
            {
                scores.Add(PlayerPrefs.GetInt("HighScore" + i, 0));
            }
        }

        for (int i = 0; i < scoreTexts.Length; i++)
        {
            if (i < scores.Count)
            {
                scoreTexts[i].text = (i + 1) + ". " + scores[i];
            }
            else
            {
                scoreTexts[i].text = (i + 1) + ". ---";
            }
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

}
