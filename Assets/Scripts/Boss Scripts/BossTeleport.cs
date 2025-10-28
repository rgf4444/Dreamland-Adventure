using UnityEngine;
using System.Collections;

public class BossTeleport : MonoBehaviour
{
    [Header("References")]
    public BossHealth bossHealth;
    public Animator animator;
    public Transform pointA;
    public Transform pointB;
    public Transform firePoint;
    public SpriteRenderer spriteRenderer;

    [Header("Detection Settings")]
    public float detectionRange = 3f;
    public LayerMask playerLayer;
    public float stayDurationRequired = 3f;

    [Header("Timing Settings")]
    public float teleportCooldown = 15f;
    public float phase4TeleportInterval = 5f;
    public float teleportAnimDelay = 0.5f;
    public float postTeleportInvincibilityDuration = 5f; // Default duration for phases 1–3
    public float phase4InvincibilityDuration = 2f;        // Shorter invincibility during phase 4

    [Header("Visual Settings")]
    public Color invincibleColor = new Color(1f, 0.6f, 0.6f, 1f); // Slight reddish tint while invincible
    private Color originalColor;

    private bool isTeleporting = false;
    private bool isAtPointA = false;
    private float stayTimer = 0f;
    private float cooldownTimer = 0f;
    private float phase4Timer = 0f;

    private void Start()
    {
        // Save original color for later restoration
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Start at point B by default
        if (pointB != null)
        {
            transform.position = pointB.position;
            isAtPointA = false;
            UpdateOrientation();
        }
    }

    private void Update()
    {
        if (bossHealth == null || bossHealth.isDead)
            return;

        int currentPhase = bossHealth.currentPhase;

        // Update cooldown
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        // Phase 4: automatic teleport (with shorter invincibility)
        if (currentPhase >= 4)
        {
            phase4Timer += Time.deltaTime;

            if (phase4Timer >= phase4TeleportInterval && !isTeleporting)
            {
                phase4Timer = 0f;
                StartCoroutine(TeleportRoutine());
            }
            return;
        }

        // Phases 1–3: teleport if player stays close
        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (playerInRange != null)
        {
            stayTimer += Time.deltaTime;

            if (stayTimer >= stayDurationRequired && cooldownTimer <= 0f && !isTeleporting)
            {
                StartCoroutine(TeleportRoutine());
            }
        }
        else
        {
            stayTimer = 0f;
        }
    }

    private IEnumerator TeleportRoutine()
    {
        isTeleporting = true;
        stayTimer = 0f;
        cooldownTimer = teleportCooldown;

        if (animator != null)
            animator.SetTrigger("Teleport");

        Debug.Log("Teleport animation triggered!");

        // Wait for teleport animation to finish (8 frames @12fps  0.67s)
        yield return new WaitForSeconds(8f / 12f);

        // Switch teleport position
        Transform targetPoint = (isAtPointA) ? pointB : pointA;
        transform.position = targetPoint.position;
        isAtPointA = !isAtPointA;

        // Update facing and fire point orientation
        UpdateOrientation();

        // Short pause before idle
        yield return new WaitForSeconds(0.1f);

        if (animator != null)
            animator.SetTrigger("Idle");

        // Give temporary invincibility (duration depends on phase)
        float duration = bossHealth.currentPhase >= 4 ? phase4InvincibilityDuration : postTeleportInvincibilityDuration;
        StartCoroutine(GrantTemporaryInvincibility(duration));

        isTeleporting = false;
    }

    private IEnumerator GrantTemporaryInvincibility(float duration)
    {
        bossHealth.isInvincible = true;
        Debug.Log($"Boss is now invincible for {duration} seconds!");

        // Change color to indicate invincibility
        if (spriteRenderer != null)
            spriteRenderer.color = invincibleColor;

        yield return new WaitForSeconds(duration);

        // Restore normal color and remove invincibility
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        bossHealth.isInvincible = false;
        Debug.Log("Boss invincibility expired.");
    }

    private void UpdateOrientation()
    {
        // Flip sprite depending on side
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isAtPointA;
        }

        // Flip firePoint position & scale
        if (firePoint != null)
        {
            Vector3 localPos = firePoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (isAtPointA ? 1 : -1);
            firePoint.localPosition = localPos;

            Vector3 localScale = firePoint.localScale;
            localScale.x = Mathf.Abs(localScale.x) * (isAtPointA ? 1 : -1);
            firePoint.localScale = localScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (pointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.15f);
        }

        if (pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointB.position, 0.15f);
        }
    }
}
