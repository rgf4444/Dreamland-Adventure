using System.Collections;
using UnityEngine;

public class EnemyStunHandler : MonoBehaviour
{
    private bool isStunned = false;
    private MonoBehaviour[] enemyScripts;

    private void Awake()
    {
        // Gather all scripts but we’ll exclude certain ones later
        enemyScripts = GetComponents<MonoBehaviour>();
    }

    public void ApplyStun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;

        foreach (MonoBehaviour script in enemyScripts)
        {
            // Keep health and stun systems active
            if (script == this || script is EnemyHealth)
                continue;

            script.enabled = false;
        }

        Debug.Log($"{gameObject.name} stunned for {duration} seconds!");

        yield return new WaitForSeconds(duration);

        foreach (MonoBehaviour script in enemyScripts)
        {
            if (script == this || script is EnemyHealth)
                continue;

            script.enabled = true;
        }

        isStunned = false;
        Debug.Log($"{gameObject.name} recovered from stun.");
    }
}
