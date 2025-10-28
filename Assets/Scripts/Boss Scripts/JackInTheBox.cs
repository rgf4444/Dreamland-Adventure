using UnityEngine;
using System.Collections;

public class JackInTheBox : MonoBehaviour
{
    [Header("Settings")]
    public float travelSpeed = 5f;
    public float popRadius = 1.5f;
    public int damage = 1;
    public float knockbackForce = 8f;

    private PlayerHealth playerHealth;
    private Transform player;
    private Vector2 popPosition;
    private Animator animator;
    private bool hasPopped = false;

    public void Initialize(PlayerHealth health, Transform playerRef, Vector2 targetPos)
    {
        playerHealth = health;
        player = playerRef;
        popPosition = targetPos;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (hasPopped) return;

        // Travel toward target pop point
        transform.position = Vector2.MoveTowards(transform.position, popPosition, travelSpeed * Time.deltaTime);

        // Start pop when close enough
        if (Vector2.Distance(transform.position, popPosition) < 0.1f)
        {
            StartCoroutine(Pop());
        }
    }

    private IEnumerator Pop()
    {
        hasPopped = true;

        // Trigger animation
        if (animator != null)
            animator.SetTrigger("Pop");

        yield return new WaitForSeconds(0.25f); // Delay to sync with pop frame

        // Detect player in pop radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, popRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage);

                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                PlayerMovement playerMovement = hit.GetComponent<PlayerMovement>();

                if (playerRb != null)
                {
                    // Apply knockback direction (slightly upward)
                    Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                    knockDir = new Vector2(Mathf.Sign(knockDir.x), 0.3f).normalized;

                    playerRb.velocity = Vector2.zero; // reset before adding force
                    playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                }

                if (playerMovement != null)
                {
                    // Optional: brief recovery like in JesterProjectile
                    playerMovement.ApplyKnockbackRecovery(0.3f);
                    playerMovement.CancelChargedAttack();
                }
            }
        }

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, popRadius);
    }
}
