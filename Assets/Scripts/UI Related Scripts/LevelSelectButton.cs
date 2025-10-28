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
    public Color lockedColor = Color.gray;
    public Color completedColor = Color.green; // Optional: color for completed levels

    void Start()
    {
        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        bool isUnlocked = PlayerPrefs.GetInt("Level" + levelNumber + "_Unlocked", 0) == 1;
        bool isCompleted = PlayerPrefs.GetInt("Level" + levelNumber + "_Completed", 0) == 1;

        // Update button interactability
        button.interactable = isUnlocked;

        // Update TMP text
        if (levelText != null)
        {
            if (isUnlocked)
            {
                levelText.text = levelNumber.ToString();

                // Optional: Add checkmark for completed levels
                if (isCompleted)
                {
                    levelText.text += " ✓";
                    levelText.color = completedColor;
                }
                else
                {
                    levelText.color = unlockedColor;
                }
            }
            else
            {
                levelText.text = "🔒"; // Or just keep as number but grayed out
                levelText.color = lockedColor;
            }
        }
    }

    public void LoadLevel()
    {
        bool isUnlocked = PlayerPrefs.GetInt("Level" + levelNumber + "_Unlocked", 0) == 1;

        if (isUnlocked && !string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}