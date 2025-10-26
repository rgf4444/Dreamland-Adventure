using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public int health = 5;
    public int maxHealth;

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
}
