using System.Collections;
using UnityEngine;

public class JesterAttack : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public Transform player;
    public Animator animator;
    public PlayerHealth playerHealth;
    public BossHealth bossHealth; // assign via Inspector

    [Header("Attack Settings")]
    public float normalAttackInterval = 5f;
    public float phase4AttackInterval = 3f;
    public int flurryTriggerCountPhase2 = 3;
    public int flurryTriggerCountPhase4 = 2;

    private int attackCounter = 0;
    private bool isAttacking = false;

    private void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<BossHealth>();

        StartCoroutine(AttackRoutine());
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            int currentPhase = (bossHealth != null) ? bossHealth.currentPhase : 1;
            float interval = (currentPhase == 4) ? phase4AttackInterval : normalAttackInterval;

            // Wait between attacks
            yield return new WaitForSeconds(interval);

            if (!isAttacking)
            {
                isAttacking = true;
                yield return StartCoroutine(PerformAttack());
                isAttacking = false;
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        int currentPhase = (bossHealth != null) ? bossHealth.currentPhase : 1;
        attackCounter++;

        if (animator != null)
            animator.SetTrigger("Attack");

        // Wait for animation before firing
        yield return new WaitForSeconds(GetAnimationLength("Attack"));

        // Align firepoint to player height
        if (firePoint != null && player != null)
        {
            Vector3 firePos = firePoint.position;
            firePos.y = player.position.y;
            firePoint.position = firePos;
        }

        // Check for flurry
        bool doFlurry = false;

        if (currentPhase == 2 && attackCounter >= flurryTriggerCountPhase2)
        {
            doFlurry = true;
            attackCounter = 0;
        }
        else if (currentPhase == 4 && attackCounter >= flurryTriggerCountPhase4)
        {
            doFlurry = true;
            attackCounter = 0;
        }

        if (doFlurry)
            yield return StartCoroutine(FlurryAttack());
        else
            FireProjectile();
    }

    private IEnumerator FlurryAttack()
    {
        const int shots = 3;
        const float intervalBetweenShots = 1.25f; // tweak for reaction time

        for (int i = 0; i < shots; i++)
        {
            FireProjectile();
            yield return new WaitForSeconds(intervalBetweenShots);
        }
    }

    private void FireProjectile()
    {
        if (firePoint == null || projectilePrefab == null) return;

        Vector3 spawnPos = firePoint.position;
        if (player != null)
            spawnPos.y = player.position.y;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        JesterProjectile jProj = proj.GetComponent<JesterProjectile>();

        if (jProj != null && player != null && playerHealth != null)
            jProj.Initialize(player, playerHealth);
    }

    private float GetAnimationLength(string animName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0.5f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animName)
                return clip.length;
        }

        return 0.5f;
    }
}
