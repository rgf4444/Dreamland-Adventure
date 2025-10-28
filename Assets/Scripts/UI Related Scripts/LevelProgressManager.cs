using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Destroying duplicate LevelProgressManager");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeProgress();
        Debug.Log("LevelProgressManager Awake completed");
    }

    void InitializeProgress()
    {
        Debug.Log("Initializing progress...");

        if (!PlayerPrefs.HasKey("Level1_Unlocked"))
        {
            Debug.Log("First time - setting up levels");
            PlayerPrefs.SetInt("Level1_Unlocked", 1);
            PlayerPrefs.SetInt("Level2_Unlocked", 0);
            PlayerPrefs.SetInt("Level3_Unlocked", 0);
            PlayerPrefs.SetInt("Level4_Unlocked", 0);
            PlayerPrefs.Save();
        }

        // Debug current state
        Debug.Log("Current unlock status:");
        for (int i = 1; i <= 4; i++)
        {
            Debug.Log("Level " + i + ": " + (PlayerPrefs.GetInt("Level" + i + "_Unlocked", 0) == 1));
        }
    }

    public void UpdateUnlocks(int currentLevel)
    {
        Debug.Log("Unlocking only Level " + currentLevel);

        // ONLY unlock the current level
        PlayerPrefs.SetInt("Level" + currentLevel + "_Unlocked", 1);

        PlayerPrefs.Save();

        // Debug
        Debug.Log("After UpdateUnlocks:");
        for (int i = 1; i <= 4; i++)
        {
            Debug.Log("Level " + i + ": " + (PlayerPrefs.GetInt("Level" + i + "_Unlocked", 0) == 1));
        }
    }
    public bool IsLevelUnlocked(int levelNumber)
    {
        bool unlocked = PlayerPrefs.GetInt("Level" + levelNumber + "_Unlocked", 0) == 1;
        Debug.Log("IsLevelUnlocked(" + levelNumber + ") returning: " + unlocked);
        return unlocked;
    }
}