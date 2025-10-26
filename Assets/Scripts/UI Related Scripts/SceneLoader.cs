using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Loads a scene by its name.
    /// </summary>
    /// <param name="sceneName">The exact name of the scene as listed in Build Settings.</param>
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name is empty or null. Please check the button setup.");
            return;
        }

        // Check if the scene is in build settings
        if (!IsSceneInBuild(sceneName))
        {
            Debug.LogError("Scene \"" + sceneName + "\" not found in Build Settings!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Quits the game (works only in build).
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    // Optional helper: checks if scene exists in build settings
    private bool IsSceneInBuild(string name)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (sceneName == name)
                return true;
        }
        return false;
    }
}
