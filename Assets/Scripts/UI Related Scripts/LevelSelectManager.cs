using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public Button[] levelButtons;

    void Start()
    {
        Debug.Log("=== LEVELSELECTMANAGER START ===");

        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("LevelProgressManager Instance is NULL in LevelSelect!");
        }
        else
        {
            Debug.Log("LevelProgressManager Instance found in LevelSelect");

            // Check current status
            Debug.Log("Level unlock status in LevelSelect:");
            for (int i = 1; i <= 4; i++)
            {
                bool unlocked = LevelProgressManager.Instance.IsLevelUnlocked(i);
                Debug.Log("Level " + i + ": " + unlocked);
            }
        }

        UpdateLevelButtons();
    }

    void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            bool isUnlocked = LevelProgressManager.Instance.IsLevelUnlocked(levelNumber);

            Debug.Log("Level " + levelNumber + " unlocked: " + isUnlocked);

            if (isUnlocked)
            {
                levelButtons[i].interactable = true;
                Text buttonText = levelButtons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = levelNumber.ToString();
            }
            else
            {
                levelButtons[i].interactable = false;
                Text buttonText = levelButtons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = "🔒";
            }
        }
    }

    public void LoadLevel1()
    {
        Debug.Log("LoadLevel1 button clicked!");
        LoadLevel(1);
    }
    public void LoadLevel2()
    {
        Debug.Log("LoadLevel2 button clicked!");
        LoadLevel(2);
    }
    public void LoadLevel3()
    {
        Debug.Log("LoadLevel3 button clicked!");
        LoadLevel(3);
    }
    public void LoadLevel4()
    {
        Debug.Log("LoadLevel4 button clicked!");
        LoadLevel(4);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void ResetAllProgress()
    {
        Debug.Log("=== RESETTING ALL PROGRESS ===");

        PlayerPrefs.DeleteAll();

        // Only Level 1 should be unlocked
        PlayerPrefs.SetInt("Level1_Unlocked", 1);
        PlayerPrefs.SetInt("Level2_Unlocked", 0);
        PlayerPrefs.SetInt("Level3_Unlocked", 0);
        PlayerPrefs.SetInt("Level4_Unlocked", 0);

        PlayerPrefs.Save();

        // Update the buttons
        UpdateLevelButtons();

        Debug.Log("Progress reset! Only Level 1 should be unlocked now.");
    }

    void LoadLevel(int levelNumber)
    {
        Debug.Log("Trying to load Level " + levelNumber);

        // Test with exact scene name
        string sceneName = "LevelTest" + levelNumber;
        Debug.Log("Trying to load scene: " + sceneName);

        // Check if scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log("Scene in build settings: " + sceneInBuild);

            if (sceneInBuild == sceneName)
            {
                sceneExists = true;
                break;
            }
        }

        Debug.Log("Scene exists in build: " + sceneExists);

        if (LevelProgressManager.Instance.IsLevelUnlocked(levelNumber) && sceneExists)
        {
            Debug.Log("Loading: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Cannot load - Unlocked: " + LevelProgressManager.Instance.IsLevelUnlocked(levelNumber) + ", Exists: " + sceneExists);
        }
    }
}