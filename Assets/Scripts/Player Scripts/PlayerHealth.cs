using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public int health = 5;
    public int maxHealth;

    public string gameOverSceneName = "GameOver"; // Set this to your game over scene name

    private void Start()
    {
        maxHealth = health;
        UpdateHearts();
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHearts();

        // Check if health reached 0
        if (health <= 0)
        {
            GameOver();
        }
    }

    public void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < health)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }

    private void GameOver()
    {
        Debug.Log("Player died! Loading game over scene...");

        // Make sure to save the current level for restart functionality
        SaveCurrentLevel();

        // Load the game over scene
        SceneManager.LoadScene(gameOverSceneName);
    }

    private void SaveCurrentLevel()
    {
        // Get current scene name
        string currentScene = SceneManager.GetActiveScene().name;

        // Extract level number from scene name (assuming "LevelTest1", "LevelTest2", etc.)
        if (currentScene.StartsWith("LevelTest"))
        {
            string levelNumberStr = currentScene.Replace("LevelTest", "");
            if (int.TryParse(levelNumberStr, out int levelNumber))
            {
                PlayerPrefs.SetInt("LastLevel", levelNumber);
                PlayerPrefs.Save();
                Debug.Log("Saved last level: " + levelNumber);
            }
        }
    }
}