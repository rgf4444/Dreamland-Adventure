using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 8f;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool canMove = true;

    [Header("Attack Unlock Status")]
    public bool normalAttackEnabled = false;
    public bool chargedAttackEnabled = false;
    public bool rangedAttackEnabled = false;

    [Header("Attack Settings")]
    public float normalAttackCooldown = 0.5f;
    public float chargedAttackDuration = 2f;
    public float chargedAttackCooldown = 1f;
    public float postChargeMovementDelay = 0.5f;
    public float rangedAttackCooldown = 1.5f;

    private bool isAttacking = false;
    private float nextNormalAttackTime;
    private float nextChargedAttackTime;
    private float nextRangedAttackTime;

    [Header("References")]
    public Rigidbody2D rb;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private float moveInput;
    private bool isRunning;
    private bool isKnockedBack = false;

    [Header("Attack References")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    public int normalAttackDamage = 1;
    public int chargedAttackDamage = 3;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Coroutine chargedAttackCoroutine;
    private bool isCharging = false;

    private void Update()
    {
        HandleInput();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (canMove && !isKnockedBack)
        {
            Move();
        }
        else if (!isKnockedBack)
        {
            // Stop horizontal drift if movement is locked (e.g. during attacks)
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        // Do NOT interfere with rb.velocity when knocked back — physics handles that
    }

    private void HandleInput()
    {
        if (!canMove || isKnockedBack) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    private void Move()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        if (isFacingRight && moveInput < 0)
            Flip();
        else if (!isFacingRight && moveInput > 0)
            Flip();
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void HandleAttack()
    {
        if (isAttacking || isKnockedBack) return;

        // Normal attack (Key 1) - only if enabled
        if (normalAttackEnabled && Input.GetKeyDown(KeyCode.Alpha1) && Time.time >= nextNormalAttackTime)
        {
            StartCoroutine(NormalAttack());
        }

        // Charged attack (Key 2) - only if enabled
        if (chargedAttackEnabled && Input.GetKeyDown(KeyCode.Alpha2) && isGrounded && Time.time >= nextChargedAttackTime)
        {
            chargedAttackCoroutine = StartCoroutine(ChargedAttack());
        }

        // Ranged attack (Key 3) - only if enabled
        if (rangedAttackEnabled && Input.GetKeyDown(KeyCode.Alpha3) && Time.time >= nextRangedAttackTime)
        {
            StartCoroutine(RangedAttack());
        }
    }

    private IEnumerator NormalAttack()
    {
        isAttacking = true;
        canMove = false;
        Debug.Log("Normal Attack!");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(normalAttackDamage);
            enemy.GetComponent<BossHealth>()?.TakeDamage(normalAttackDamage);
        }

        yield return new WaitForSeconds(0.25f);
        canMove = true;
        yield return new WaitForSeconds(0.25f);

        nextNormalAttackTime = Time.time + normalAttackCooldown;
        isAttacking = false;
    }

    private IEnumerator ChargedAttack()
    {
        isAttacking = true;
        canMove = false;
        isCharging = true;
        Debug.Log("Charging...");

        float elapsed = 0f;
        while (elapsed < chargedAttackDuration)
        {
            // If interrupted mid-charge, stop immediately
            if (!isCharging)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Strong Attack Released!");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(chargedAttackDamage);
            enemy.GetComponent<BossHealth>()?.TakeDamage(chargedAttackDamage);

            if (enemy.CompareTag("Enemy"))
            {
                EnemyStunHandler stunHandler = enemy.GetComponent<EnemyStunHandler>();
                if (stunHandler != null)
                {
                    stunHandler.ApplyStun(2f); // 2 seconds stun duration
                }
                else
                {
                    Debug.Log($"{enemy.name} has no EnemyStunHandler attached!");
                }
            }
        }


        yield return new WaitForSeconds(postChargeMovementDelay);
        canMove = true;

        nextChargedAttackTime = Time.time + chargedAttackCooldown;
        isAttacking = false;
    }


    private IEnumerator RangedAttack()
    {
        isAttacking = true;
        canMove = false;
        Debug.Log("Ranged Attack!");

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            PlayerProjectile projectile = bullet.GetComponent<PlayerProjectile>();

            Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
            projectile.Initialize(dir);

            // Flip projectile if facing left
            if (!isFacingRight)
            {
                Vector3 scale = bullet.transform.localScale;
                scale.x *= -1;
                bullet.transform.localScale = scale;
            }
        }

        yield return new WaitForSeconds(0.5f);
        nextRangedAttackTime = Time.time + rangedAttackCooldown;
        canMove = true;
        isAttacking = false;
    }

    public void CancelChargedAttack()
    {
        if (isCharging && chargedAttackCoroutine != null)
        {
            Debug.Log("Charged attack interrupted!");

            // Stop the active coroutine
            StopCoroutine(chargedAttackCoroutine);
            chargedAttackCoroutine = null;

            // Reset states
            isCharging = false;
            isAttacking = false;
            canMove = true;

            // Force cooldown to prevent instant retry
            nextChargedAttackTime = Time.time + chargedAttackCooldown;
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void ApplyKnockbackRecovery(float duration)
    {
        // Prevent overlapping knockback coroutines
        StopCoroutine(nameof(KnockbackRecovery));
        StartCoroutine(KnockbackRecovery(duration));
    }

    private IEnumerator KnockbackRecovery(float duration)
    {
        isKnockedBack = true;
        canMove = false;
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
        canMove = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void EnableNormalAttack()
    {
        normalAttackEnabled = true;
        Debug.Log("Normal Attack Enabled!");
    }

    public void EnableChargedAttack()
    {
        chargedAttackEnabled = true;
        Debug.Log("Charged Attack Enabled!");
    }

    public void EnableRangedAttack()
    {
        rangedAttackEnabled = true;
        Debug.Log("Ranged Attack Enabled!");
    }

    public void DisableNormalAttack()
    {
        normalAttackEnabled = false;
    }

    public void DisableChargedAttack()
    {
        chargedAttackEnabled = false;
    }

    public void DisableRangedAttack()
    {
        rangedAttackEnabled = false;
    }
}
