using System.Collections;
using UnityEngine;

public class EnemyStunHandler : MonoBehaviour
{
    private bool isStunned = false;
    private MonoBehaviour[] enemyScripts;
    private Animator animator;

    // Animator parameter names
    private const string IS_STUNNED = "isStunned";
    private const string STUN_TRIGGER = "stunTrigger";

    private void Awake()
    {
        // Gather all scripts but we'll exclude certain ones later
        enemyScripts = GetComponents<MonoBehaviour>();
        animator = GetComponent<Animator>();
    }

    public void ApplyStun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;

        // Trigger stun animation
        if (animator != null)
        {
            animator.SetBool(IS_STUNNED, true);
            animator.SetTrigger(STUN_TRIGGER);
        }

        // Disable enemy scripts (excluding essential ones)
        foreach (MonoBehaviour script in enemyScripts)
        {
            // Keep health, stun systems, and animator active
            if (script == this || script is EnemyHealth || script is Animator)
                continue;

            script.enabled = false;
        }

        Debug.Log($"{gameObject.name} stunned for {duration} seconds!");

        yield return new WaitForSeconds(duration);

        // Re-enable scripts
        foreach (MonoBehaviour script in enemyScripts)
        {
            if (script == this || script is EnemyHealth || script is Animator)
                continue;

            script.enabled = true;
        }

        // End stun animation
        if (animator != null)
        {
            animator.SetBool(IS_STUNNED, false);
        }

        isStunned = false;
        Debug.Log($"{gameObject.name} recovered from stun.");
    }

    public bool IsStunned()
    {
        return isStunned;
    }
}