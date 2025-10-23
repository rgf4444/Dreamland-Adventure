using UnityEngine;

public class ClubAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    public float idleDuration = 3f;

    [Header("Detection & Attack Settings")]
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 4f;

    [Header("References")]
    public Transform player;
    public PlayerHealth playerHealth; // reference to player’s heart system

    private Vector3 targetPoint;
    private bool isIdling = false;
    private float idleTimer;
    private bool hasDetectedPlayer = false;
    private bool facingRight = true;

    private float attackTimer = 0f;

    void Start()
    {
        targetPoint = pointA.position;
    }

    void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!hasDetectedPlayer)
        {
            if (distanceToPlayer <= detectionRange)
            {
                hasDetectedPlayer = true;
                Debug.Log($"{gameObject.name} detected the player!");
            }
            else
            {
                Patrol();
                return;
            }
        }

        // Once detected, keep chasing forever (until player or enemy dies)
        if (distanceToPlayer <= attackRange)
            AttackPlayer();
        else
            ChasePlayer();
    }

    // --- PATROL LOGIC ---
    void Patrol()
    {
        if (!isIdling)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

            if (targetPoint.x > transform.position.x && !facingRight)
                Flip();
            else if (targetPoint.x < transform.position.x && facingRight)
                Flip();

            if (Vector2.Distance(transform.position, targetPoint) < 0.2f)
            {
                isIdling = true;
                idleTimer = idleDuration;
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                isIdling = false;
                targetPoint = (targetPoint == pointA.position) ? pointB.position : pointA.position;
            }
        }
    }

    // --- CHASE LOGIC ---
    void ChasePlayer()
    {
        Vector2 target = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (player.position.x > transform.position.x && !facingRight)
            Flip();
        else if (player.position.x < transform.position.x && facingRight)
            Flip();
    }

    // --- ATTACK LOGIC ---
    void AttackPlayer()
    {
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            Debug.Log($"{gameObject.name} attacked the player!");

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // subtract one heart
            }
            else
            {
                Debug.LogWarning("ClubAI: PlayerHealth reference missing!");
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // --- GIZMOS (for visualization) ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
