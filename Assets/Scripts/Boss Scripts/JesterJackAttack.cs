using UnityEngine;
using System.Collections;

public class JesterJackAttack : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public GameObject jackPrefab;
    public Transform[] spawnPoints;  // 5 spawn points
    public Transform[] popPoints;    // 5 pop points
    public PlayerHealth playerHealth;
    public Transform player;
    public BossHealth bossHealth;
    public JesterAttack jesterAttack; // so we can pause card attacks

    [Header("Settings")]
    public float spawnInterval = 1.5f; // delay between jacks
    public float phase3Cooldown = 10f;
    public float phase4Cooldown = 6f;
    public string jackAttackAnim = "JackAttack"; // name of the animation

    private bool isAttacking = false;
    private bool isOnCooldown = false;

    private void Update()
    {
        if (bossHealth == null) return;

        // Trigger this pattern only on phase 3 or higher
        if (bossHealth.currentPhase < 3) return;

        if (!isAttacking && !isOnCooldown)
        {
            StartCoroutine(PerformJackAttack());
        }
    }

    private IEnumerator PerformJackAttack()
    {
        isAttacking = true;
        isOnCooldown = true;

        // Stop card attacks during this sequence (optional)
        if (jesterAttack != null)
            jesterAttack.enabled = false;

        // Trigger animation
        if (animator != null)
            animator.SetTrigger(jackAttackAnim);

        // Wait until animation finishes before spawning
        float animLength = GetAnimationLength(jackAttackAnim);
        yield return new WaitForSeconds(animLength);

        // Begin dropping jacks sequentially
        for (int i = 0; i < Mathf.Min(spawnPoints.Length, popPoints.Length); i++)
        {
            GameObject jack = Instantiate(jackPrefab, spawnPoints[i].position, Quaternion.identity);
            JackInTheBox jackScript = jack.GetComponent<JackInTheBox>();

            if (jackScript != null)
                jackScript.Initialize(playerHealth, player, popPoints[i].position);

            yield return new WaitForSeconds(spawnInterval);
        }

        // Re-enable card attacks (after all boxes deployed)
        yield return new WaitForSeconds(5f); // optional short buffer before resuming cards
        if (jesterAttack != null)
            jesterAttack.enabled = true;

        isAttacking = false;

        // Cooldown before next jack attack
        float cooldown = (bossHealth.currentPhase >= 4) ? phase4Cooldown : phase3Cooldown;
        yield return new WaitForSeconds(cooldown);

        isOnCooldown = false;
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
