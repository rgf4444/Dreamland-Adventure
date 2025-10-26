using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectButtonTMP : MonoBehaviour
{
    [Header("Level Info")]
    public int levelNumber;
    public string sceneName;

    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI levelText; // TMP text component

    [Header("Colors")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = Color.red;

    void Start()
    {
        // Unlock Level 1 by default if not already
        if (levelNumber == 1 && PlayerPrefs.GetInt("Level1_Unlocked", 0) == 0)
        {
            PlayerPrefs.SetInt("Level1_Unlocked", 1);
        }

        bool isUnlocked = PlayerPrefs.GetInt("Level" + levelNumber + "_Unlocked", 0) == 1;

        // Update button state and TMP text color
        button.interactable = isUnlocked;
        if (levelText != null)
        {
            levelText.color = isUnlocked ? unlockedColor : lockedColor;
        }
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(sceneName);
    }
}
