using UnityEngine;
using System.Collections;

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
    public float attackDuration = 1f;

    [Header("References")]
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public PlayerHealth playerHealth;

    [Header("Animation")]
    private Animator animator;

    // Animator Parameter Names
    private const string IS_MOVING = "isMoving";
    private const string IS_ATTACKING = "isAttacking";
    private const string IS_FRIENDLY = "isFriendly";

    private Vector3 targetPoint;
    private bool isIdling = false;
    private bool isDefeated = false;
    private float idleTimer;
    private bool hasDetectedPlayer = false;
    private bool facingRight = true;
    private float attackTimer = 0f;
    private Coroutine attackCoroutine;

    void Start()
    {
        targetPoint = pointA.position;
        animator = GetComponent<Animator>();

        // Initialize animator parameters
        SetAnimatorBool(IS_FRIENDLY, false);
        SetAnimatorBool(IS_MOVING, false);
        SetAnimatorBool(IS_ATTACKING, false);
    }

    void Update()
    {
        if (isDefeated) return;

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

            SetAnimatorBool(IS_MOVING, true);

            if (targetPoint.x > transform.position.x && !facingRight)
                Flip();
            else if (targetPoint.x < transform.position.x && facingRight)
                Flip();

            if (Vector2.Distance(transform.position, targetPoint) < 0.2f)
            {
                isIdling = true;
                idleTimer = idleDuration;
                SetAnimatorBool(IS_MOVING, false);
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

        SetAnimatorBool(IS_MOVING, true);

        if (player.position.x > transform.position.x && !facingRight)
            Flip();
        else if (player.position.x < transform.position.x && facingRight)
            Flip();
    }

    // --- ATTACK LOGIC (RANGED) ---
    void AttackPlayer()
    {
        SetAnimatorBool(IS_MOVING, false);

        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;

            // Start attack coroutine
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        // Start attack animation
        SetAnimatorBool(IS_ATTACKING, true);
        Debug.Log($"{gameObject.name} fired at the player!");

        // Wait a bit for attack animation to sync
        yield return new WaitForSeconds(0.3f);

        // Fire projectile
        if (projectilePrefab != null && firePoint != null && player != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Initialize projectile with your existing method
            SpadeProjectile spadeProjectile = bullet.GetComponent<SpadeProjectile>();
            if (spadeProjectile != null)
            {
                spadeProjectile.Initialize(player, playerHealth);
            }

            // Flip projectile based on facing direction
            if (!facingRight)
            {
                Vector3 scale = bullet.transform.localScale;
                scale.x *= -1;
                bullet.transform.localScale = scale;
            }
        }

        // Wait for attack animation to complete
        yield return new WaitForSeconds(attackDuration - 0.3f);

        // End attack animation
        SetAnimatorBool(IS_ATTACKING, false);
    }

    // --- ANIMATION METHODS ---
    private void SetAnimatorBool(string parameter, bool value)
    {
        if (animator != null)
            animator.SetBool(parameter, value);
    }

    // --- DEFEAT/TRANSFORM METHOD ---
    public void TransformToFriendly()
    {
        isDefeated = true;
        hasDetectedPlayer = false;

        // Stop any attack
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            SetAnimatorBool(IS_ATTACKING, false);
        }

        SetAnimatorBool(IS_FRIENDLY, true);
        SetAnimatorBool(IS_MOVING, false);
        Debug.Log($"{gameObject.name} transformed to friendly!");
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