using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 200;
    private int currentHealth;

    private bool isDead = false;

    [Header("UI References")]
    public Slider healthBar;
    public TMP_Text healthText;

    [Header("Phase Thresholds")]
    public int phase2Threshold = 150;
    public int phase3Threshold = 100;
    public int phase4Threshold = 50;

    public int currentPhase = 1;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log(gameObject.name + " took " + amount + " damage! Remaining HP: " + currentHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        CheckPhaseChange();
    }

    private void CheckPhaseChange()
    {
        if (currentHealth <= phase4Threshold && currentPhase < 4)
        {
            currentPhase = 4;
            Debug.Log("Boss Phase 4 triggered! (<= 50 HP)");
        }
        else if (currentHealth <= phase3Threshold && currentPhase < 3)
        {
            currentPhase = 3;
            Debug.Log("Boss Phase 3 triggered! (<= 100 HP)");
        }
        else if (currentHealth <= phase2Threshold && currentPhase < 2)
        {
            currentPhase = 2;
            Debug.Log("Boss Phase 2 triggered! (<= 150 HP)");
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " defeated!");
        // Add animation, sound, or cutscene trigger here
        Destroy(gameObject);
    }
}
