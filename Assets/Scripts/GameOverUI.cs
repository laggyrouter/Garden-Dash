using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI instance;

    public GameObject panel;
    public string mainMenuSceneName = "MainMenu";

    public AudioClip gameOverClip;
    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void Start()
    {
        panel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (audioSource != null && gameOverClip != null)
        {
            audioSource.PlayOneShot(gameOverClip);
        }

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.SaveScoretoHighScores(); //highscore guard
        }

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.SaveScoretoHighScores();
            ScoreManager.instance.score = 0;
            ScoreManager.instance.UpdateScoreUI();
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

