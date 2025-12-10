using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "MainGame";
    public string highScoreSceneName = "HighScore";

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenHIghScores()
    {
        SceneManager.LoadScene(highScoreSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else
    Application.Quit();
#endif
    }
}
