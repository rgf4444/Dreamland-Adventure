using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    [Header("Scene Names")]
    public string levelSelectScene = "LevelSelect";
    public string currentGameScene = "GameScene";

    // Public methods that buttons can call directly
    public void ResumeGame()
    {
        Debug.Log("Resuming Game...");
        // gameObject.SetActive(false);
        // Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");
        SceneManager.LoadScene(currentGameScene);
    }

    public void GoToLevelSelect()
    {
     

        // Load level select
        SceneManager.LoadScene("LevelSelect");
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}