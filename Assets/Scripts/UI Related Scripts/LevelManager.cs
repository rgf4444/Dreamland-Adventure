using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Info")]
    public int levelNumber; // e.g., Level 1 = 1, Level 2 = 2, etc.
    public string nextSceneName; // name of next scene to load

    public void CompleteLevel()
    {
        // Save completion
        PlayerPrefs.SetInt("Level" + levelNumber + "_Completed", 1);
        PlayerPrefs.Save();

        Debug.Log("Level " + levelNumber + " completed!");

        // Optionally, unlock next level automatically
        int nextLevel = levelNumber + 1;
        if (!PlayerPrefs.HasKey("Level" + nextLevel + "_Unlocked"))
        {
            PlayerPrefs.SetInt("Level" + nextLevel + "_Unlocked", 1);
        }

        // Load next scene or main menu
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
