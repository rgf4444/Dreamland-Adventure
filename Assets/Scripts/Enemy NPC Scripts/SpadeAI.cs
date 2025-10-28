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

    [Header("Hit Settings")]
    public float hitStunDuration = 0.5f;

    [Header("References")]
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public PlayerHealth playerHealth;

    [Header("Animation")]
    private Animator animator;
    private EnemyStunHandler stunHandler; // NEW: Reference to stun handler

    // Animator Parameter Names
    private const string IS_MOVING = "isMoving";
    private const string IS_ATTACKING = "isAttacking";
    private const string IS_FRIENDLY = "isFriendly";
    private const string IS_HIT = "isHit";

    private Vector3 targetPoint;
    private bool isIdling = false;
    private bool isDefeated = false;
    private bool isHit = false;
    private bool hasDetectedPlayer = false;
    private bool facingRight = true;
    private float idleTimer;
    private float attackTimer = 0f;
    private float hitTimer = 0f;
    private Coroutine attackCoroutine;
    private Coroutine hitCoroutine;

    void Start()
    {
        targetPoint = pointA.position;
        animator = GetComponent<Animator>();

        // NEW: Get stun handler reference
        stunHandler = GetComponent<EnemyStunHandler>();

        // Initialize animator parameters
        SetAnimatorBool(IS_FRIENDLY, false);
        SetAnimatorBool(IS_MOVING, false);
        SetAnimatorBool(IS_ATTACKING, false);
        SetAnimatorBool(IS_HIT, false);
    }

    void Update()
    {
        // NEW: Check if stunned - don't execute AI logic
        if (stunHandler != null && stunHandler.IsStunned())
            return;

        if (isDefeated) return;

        // Update timers
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (hitTimer > 0)
            hitTimer -= Time.deltaTime;

        if (player == null)
            return;

        // Don't do anything if currently hit
        if (isHit) return;

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

        // Update facing direction every frame when player is detected
        UpdateFacingDirection();

        if (distanceToPlayer <= attackRange)
            AttackPlayer();
        else
            ChasePlayer();
    }

    // --- UPDATED: Separate facing direction logic ---
    void UpdateFacingDirection()
    {
        if (player == null) return;

        bool shouldFaceRight = player.position.x > transform.position.x;

        if (shouldFaceRight && !facingRight)
            Flip();
        else if (!shouldFaceRight && facingRight)
            Flip();
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

        // Fire projectile - direction calculated at firing moment
        if (projectilePrefab != null && firePoint != null && player != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Initialize projectile with your original method (follows player)
            SpadeProjectile spadeProjectile = bullet.GetComponent<SpadeProjectile>();
            if (spadeProjectile != null)
            {
                spadeProjectile.Initialize(player, playerHealth);
            }

            // Flip projectile sprite based on current facing direction
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

    // --- HIT ANIMATION METHOD ---
    public void TriggerHitAnimation()
    {
        if (isDefeated || isHit) return;

        // Stop any current hit coroutine
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        // Start hit coroutine
        hitCoroutine = StartCoroutine(PerformHit());
    }

    private IEnumerator PerformHit()
    {
        isHit = true;
        SetAnimatorBool(IS_HIT, true);

        // Stop movement and attack
        SetAnimatorBool(IS_MOVING, false);

        // Stop any ongoing attack
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            SetAnimatorBool(IS_ATTACKING, false);
        }

        Debug.Log($"{gameObject.name} took hit!");

        // Wait for hit stun duration
        yield return new WaitForSeconds(hitStunDuration);

        // Reset hit state
        SetAnimatorBool(IS_HIT, false);
        isHit = false;
        hitCoroutine = null;
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
        isHit = false;
        hasDetectedPlayer = false;

        // Stop any coroutines
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            SetAnimatorBool(IS_ATTACKING, false);
        }

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        SetAnimatorBool(IS_HIT, false);
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