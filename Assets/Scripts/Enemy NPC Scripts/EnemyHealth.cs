using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    private bool isDead = false;
    private ClubAI clubAI; 

    private void Start()
    {
        currentHealth = maxHealth;
        clubAI = GetComponent<ClubAI>();
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

        // Instead of destroying, transform to friendly
        if (clubAI != null)
        {
            clubAI.TransformToFriendly();
        }
        else
        {
            Debug.LogWarning("ClubAI component not found! Destroying instead.");
            Destroy(gameObject);
        }


        enabled = false;

     
    }
}