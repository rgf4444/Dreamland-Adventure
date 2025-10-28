using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string levelSelectScene = "LevelSelect";
    public string currentGameScene = "GameScene";

    [Header("References")]
    public GameObject pauseMenuUI; // Assign your pause menu panel here in the Inspector

    private bool isPaused = false;

    void Update()
    {
        // Optional: Allow pausing via Escape or P key
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Debug.Log("Game Paused");

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        Debug.Log("Resuming Game...");

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");
        Time.timeScale = 1f; // Ensure time resumes when restarting
        SceneManager.LoadScene(currentGameScene);
    }

    public void GoToLevelSelect()
    {
        Debug.Log("Going to Level Select...");
        Time.timeScale = 1f; // Resume before changing scene
        SceneManager.LoadScene(levelSelectScene);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Time.timeScale = 1f; // Resume before quitting

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}