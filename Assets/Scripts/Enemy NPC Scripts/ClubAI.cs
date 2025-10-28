using UnityEngine;
using System.Collections;

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
    public float damageDelay = 0.2f; // Delay damage to match animation

    [Header("Hit Settings")]
    public float hitStunDuration = 0.5f;

    [Header("References")]
    public Transform player;
    public PlayerHealth playerHealth;

    [Header("Animation")]
    private Animator animator;

    // Animator Parameter Names
    private const string IS_MOVING = "isMoving";
    private const string IS_ATTACKING = "isAttacking";
    private const string IS_FRIENDLY = "isFriendly";
    private const string IS_HIT = "isHit"; // New hit boolean

    private Vector3 targetPoint;
    private bool isIdling = false;
    private bool isDefeated = false;
    private bool isHit = false; // New hit state
    private float idleTimer;
    private bool hasDetectedPlayer = false;
    private bool facingRight = true;
    private float attackTimer = 0f;
    private Coroutine attackCoroutine;
    private Coroutine hitCoroutine;

    void Start()
    {
        targetPoint = pointA.position;
        animator = GetComponent<Animator>();

        SetAnimatorBool(IS_FRIENDLY, false);
        SetAnimatorBool(IS_MOVING, false);
        SetAnimatorBool(IS_ATTACKING, false);
        SetAnimatorBool(IS_HIT, false); // Initialize hit state
    }

    void Update()
    {
        if (isDefeated) return;

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        // Don't do anything if currently hit
        if (isHit) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!hasDetectedPlayer)
        {
            if (distanceToPlayer <= detectionRange)
            {
                hasDetectedPlayer = true;
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

    void AttackPlayer()
    {
        SetAnimatorBool(IS_MOVING, false);

        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;

            // Use coroutine to automatically turn off attack after a short time
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        // Start attack animation
        SetAnimatorBool(IS_ATTACKING, true);

        // Wait for animation to reach the hit frame
        yield return new WaitForSeconds(damageDelay);

        // Apply damage - now synced with animation
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }

        // Wait for rest of animation to play
        yield return new WaitForSeconds(0.5f - damageDelay);

        // Stop attack - this allows transition back to idle
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
        SetAnimatorBool(IS_HIT, true); // Set hit boolean to true

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
        SetAnimatorBool(IS_HIT, false); // Set hit boolean to false
        isHit = false;
        hitCoroutine = null;
    }

    private void SetAnimatorBool(string parameter, bool value)
    {
        if (animator != null)
            animator.SetBool(parameter, value);
    }

    public void TransformToFriendly()
    {
        isDefeated = true;
        isHit = false; // Ensure hit state is cleared
        hasDetectedPlayer = false;

        // Stop any coroutines
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            SetAnimatorBool(IS_ATTACKING, false);
        }

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        // Reset all animation states
        SetAnimatorBool(IS_HIT, false);
        SetAnimatorBool(IS_FRIENDLY, true);
        SetAnimatorBool(IS_MOVING, false);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

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