using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Call this from your RESTART button
    public void RestartLevel()
    {
        // This automatically loads the last level the player was on
        int lastLevel = PlayerPrefs.GetInt("LastLevel", 1);
        SceneManager.LoadScene("LevelTest" + lastLevel);
    }

    // Call this from your LEVEL SELECT button
    public void LevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    // Call this from your MAIN MENU button
    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    // Call this from your QUIT button
    public void QuitGame()
    {
        Application.Quit();
    }
}