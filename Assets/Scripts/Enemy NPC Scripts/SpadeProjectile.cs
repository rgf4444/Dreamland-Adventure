using UnityEngine;

public class SpadeProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 6f;
    public float lifetime = 3f;
    public int damage = 1;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float knockbackUpwardForce = 2f;

    private Transform target;
    private PlayerHealth playerHealth;
    private Vector2 direction;

    public void Initialize(Transform player, PlayerHealth healthRef)
    {
        target = player;
        playerHealth = healthRef;
        direction = (player.position - transform.position).normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            Rigidbody2D playerRb = collision.collider.GetComponent<Rigidbody2D>();
            PlayerMovement playerMovement = collision.collider.GetComponent<PlayerMovement>();

            if (playerRb != null)
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                knockDir = new Vector2(Mathf.Sign(knockDir.x), 0.3f).normalized;

                playerRb.velocity = new Vector2(0f, 0f);
                playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

            if (playerMovement != null)
            {
                playerMovement.ApplyKnockbackRecovery(0.3f);
                playerMovement.CancelChargedAttack();
            }

            Destroy(gameObject);
        }
        else if (collision.collider.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}