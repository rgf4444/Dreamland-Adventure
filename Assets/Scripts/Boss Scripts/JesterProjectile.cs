using UnityEngine;

public class JesterProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 6f;
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
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();

            if (playerRb != null)
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                knockDir = new Vector2(Mathf.Sign(knockDir.x), 0.3f).normalized;

                playerRb.velocity = Vector2.zero;
                playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

            if (playerMovement != null)
            {
                playerMovement.ApplyKnockbackRecovery(0.3f);
                playerMovement.CancelChargedAttack();
            }

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
