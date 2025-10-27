using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 8f;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool canMove = true;

    [Header("Animation")]
    private Animator animator;

    // Animator Parameters
    private const string IS_MOVING = "isMoving";
    private const string IS_JUMPING = "isJumping";
    private const string IS_ATTACKING = "isAttacking";
    private const string IS_CHARGING = "isCharging";
    private const string ATTACK_TYPE = "attackType";

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

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize animator parameters
        SetAnimatorBool(IS_MOVING, false);
        SetAnimatorBool(IS_JUMPING, false);
        SetAnimatorBool(IS_ATTACKING, false);
        SetAnimatorBool(IS_CHARGING, false);
    }

    private void Update()
    {
        HandleInput();
        HandleAttack();
        UpdateAnimation();
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
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
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

        if (moveInput > 0 && !isFacingRight)
            Flip();
        else if (moveInput < 0 && isFacingRight)
            Flip();
    }

    private void UpdateAnimation()
    {
        if (isAttacking || isKnockedBack) return;

        SetAnimatorBool(IS_MOVING, Mathf.Abs(moveInput) > 0.1f);
        SetAnimatorBool(IS_JUMPING, !isGrounded);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        SetAnimatorBool(IS_JUMPING, true);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    private void HandleAttack()
    {
        if (isAttacking || isKnockedBack) return;

        if (normalAttackEnabled && Input.GetKeyDown(KeyCode.Alpha1) && Time.time >= nextNormalAttackTime)
        {
            StartCoroutine(NormalAttack());
        }

        if (chargedAttackEnabled && Input.GetKeyDown(KeyCode.Alpha2) && isGrounded && Time.time >= nextChargedAttackTime)
        {
            chargedAttackCoroutine = StartCoroutine(ChargedAttack());
        }

        if (rangedAttackEnabled && Input.GetKeyDown(KeyCode.Alpha3) && Time.time >= nextRangedAttackTime)
        {
            StartCoroutine(RangedAttack());
        }
    }

    private IEnumerator NormalAttack()
    {
        isAttacking = true;
        canMove = false;

        // Set attack parameters
        SetAnimatorBool(IS_ATTACKING, true);
        SetAnimatorInt(ATTACK_TYPE, 1);
        Debug.Log("Normal Attack!");

        yield return new WaitForSeconds(0.1f);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(normalAttackDamage);
            enemy.GetComponent<BossHealth>()?.TakeDamage(normalAttackDamage);
        }

        yield return new WaitForSeconds(0.4f);

        // End attack (animator will auto-transition back to idle)
        SetAnimatorBool(IS_ATTACKING, false);
        canMove = true;
        nextNormalAttackTime = Time.time + normalAttackCooldown;
        isAttacking = false;
    }

    private IEnumerator ChargedAttack()
    {
        isAttacking = true;
        canMove = false;
        isCharging = true;

        // Start charging animation
        SetAnimatorBool(IS_CHARGING, true);
        Debug.Log("Charging...");

        float elapsed = 0f;
        while (elapsed < chargedAttackDuration)
        {
            if (!isCharging)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // End charging, start attack
        SetAnimatorBool(IS_CHARGING, false);
        SetAnimatorBool(IS_ATTACKING, true);
        SetAnimatorInt(ATTACK_TYPE, 2);
        Debug.Log("Strong Attack Released!");

        yield return new WaitForSeconds(0.1f);

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
                    stunHandler.ApplyStun(2f);
                }
            }
        }

        yield return new WaitForSeconds(postChargeMovementDelay);

        // End attack
        SetAnimatorBool(IS_ATTACKING, false);
        canMove = true;
        nextChargedAttackTime = Time.time + chargedAttackCooldown;
        isAttacking = false;
        isCharging = false;
    }

    private IEnumerator RangedAttack()
    {
        isAttacking = true;
        canMove = false;

        // Set ranged attack parameters
        SetAnimatorBool(IS_ATTACKING, true);
        SetAnimatorInt(ATTACK_TYPE, 3);
        Debug.Log("Ranged Attack!");

        yield return new WaitForSeconds(0.2f);

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            PlayerProjectile projectile = bullet.GetComponent<PlayerProjectile>();

            Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
            projectile.Initialize(dir);

            if (!isFacingRight)
            {
                Vector3 scale = bullet.transform.localScale;
                scale.x *= -1;
                bullet.transform.localScale = scale;
            }
        }

        yield return new WaitForSeconds(0.3f);

        // End attack
        SetAnimatorBool(IS_ATTACKING, false);
        nextRangedAttackTime = Time.time + rangedAttackCooldown;
        canMove = true;
        isAttacking = false;
    }

    // Animation Helper Methods
    private void SetAnimatorBool(string parameter, bool value)
    {
        if (animator != null)
            animator.SetBool(parameter, value);
    }

    private void SetAnimatorInt(string parameter, int value)
    {
        if (animator != null)
            animator.SetInteger(parameter, value);
    }

    public void CancelChargedAttack()
    {
        if (isCharging && chargedAttackCoroutine != null)
        {
            Debug.Log("Charged attack interrupted!");
            StopCoroutine(chargedAttackCoroutine);
            chargedAttackCoroutine = null;

            // Reset animation states
            SetAnimatorBool(IS_CHARGING, false);
            SetAnimatorBool(IS_ATTACKING, false);

            isCharging = false;
            isAttacking = false;
            canMove = true;
            nextChargedAttackTime = Time.time + chargedAttackCooldown;
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void ApplyKnockbackRecovery(float duration)
    {
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

    public void EnableNormalAttack() { normalAttackEnabled = true; Debug.Log("Normal Attack Enabled!"); }
    public void EnableChargedAttack() { chargedAttackEnabled = true; Debug.Log("Charged Attack Enabled!"); }
    public void EnableRangedAttack() { rangedAttackEnabled = true; Debug.Log("Ranged Attack Enabled!"); }
    public void DisableNormalAttack() { normalAttackEnabled = false; }
    public void DisableChargedAttack() { chargedAttackEnabled = false; }
    public void DisableRangedAttack() { rangedAttackEnabled = false; }
}