using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    private bool isDead = false;
    private ClubAI clubAI;
    private SpadeAI spadeAI;

    private void Start()
    {
        currentHealth = maxHealth;
        clubAI = GetComponent<ClubAI>();
        spadeAI = GetComponent<SpadeAI>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} defeated! Transforming to friendly...");

        // Try to transform to friendly for both enemy types
        if (clubAI != null)
        {
            clubAI.TransformToFriendly();
        }

        if (spadeAI != null)
        {
            spadeAI.TransformToFriendly();
        }

        // If neither AI component was found, destroy the object
        if (clubAI == null && spadeAI == null)
        {
            Debug.LogWarning("No AI component found! Destroying instead.");
            Destroy(gameObject);
        }

        enabled = false;
    }
}