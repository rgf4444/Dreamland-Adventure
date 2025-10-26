using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button playButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("Scene Names")]
    public string levelSelectionScene = "LevelSelection";

    void Start()
    {
        // Add listeners to buttons
        playButton.onClick.AddListener(GoToLevelSelection);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void GoToLevelSelection()
    {
        Debug.Log("Loading Level Selection...");
        SceneManager.LoadScene(levelSelectionScene);
    }

    public void OpenOptions()
    {
        Debug.Log("Options button clicked - functionality to be implemented");
        // TODO: Add popup window logic here later
        // You can enable a options panel GameObject when ready
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}