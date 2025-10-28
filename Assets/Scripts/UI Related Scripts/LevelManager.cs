using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Info")]
    public int levelNumber;
    public string nextSceneName;

    public void CompleteLevel()
    {
        Debug.Log("Level " + levelNumber + " completed!");

        // Load next scene or level select
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }
}