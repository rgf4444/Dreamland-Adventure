using UnityEngine;

public class SpadeAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    public float idleDuration = 3f;

    [Header("Detection & Attack Settings")]
    public float detectionRange = 7f;
    public float attackRange = 5f;
    public float attackCooldown = 4f;
    public float projectileSpeed = 6f;

    [Header("References")]
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public PlayerHealth playerHealth; // optional (in case projectiles handle this)

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

        if (player == null)
            return;

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

        // Once detected, chase forever until player or enemy dies
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

    // --- ATTACK LOGIC (RANGED) ---
    void AttackPlayer()
    {
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            Debug.Log($"{gameObject.name} fired at the player!");

            if (projectilePrefab != null && firePoint != null)
            {
                GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

                // initialize projectile with references
                SpadeProjectile spadeProjectile = bullet.GetComponent<SpadeProjectile>();
                if (spadeProjectile != null && player != null)
                {
                    spadeProjectile.Initialize(player, playerHealth);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: Missing SpadeProjectile or Player reference!");
                }

                // flip projectile direction based on facing direction
                if (!facingRight)
                {
                    Vector3 scale = bullet.transform.localScale;
                    scale.x *= -1;
                    bullet.transform.localScale = scale;
                }
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: ProjectilePrefab or FirePoint not assigned!");
            }
        }
    }


    // --- FLIP LOGIC ---
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

        if (firePoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        }
    }
}
