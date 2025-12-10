using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int score = 0;
    public TextMeshProUGUI scoreText;

    public List<int> highScores = new List<int>();
    public int maxHighScores = 5;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadHighScores();
            UpdateScoreUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public bool hasSavedScore = false;

    public void SaveScoretoHighScores()
    {
        if (hasSavedScore) return;
        
        highScores.Add(score);
        highScores = highScores.OrderByDescending(s => s).Take(maxHighScores).ToList();

        SaveHighScores();
        hasSavedScore = true;
    }

    void SaveHighScores()
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetInt("HighScore" + i, highScores[i]);
        }

        PlayerPrefs.SetInt("HighScoreCount", highScores.Count);
        PlayerPrefs.Save();
    }

    void LoadHighScores()
    {
        highScores.Clear();
        int count = PlayerPrefs.GetInt("HighScoreCount", 0);
        for (int i = 0; i < count; i++)
        {
            highScores.Add(PlayerPrefs.GetInt("HighScore" + i, 0));
        }
    }
}
