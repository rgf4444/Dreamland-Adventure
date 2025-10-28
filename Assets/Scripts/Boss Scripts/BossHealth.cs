using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 200;
    public int currentHealth;

    [Header("UI References")]
    public UnityEngine.UI.Slider healthBar;
    public TMPro.TMP_Text healthText;

    [Header("Phase Thresholds")]
    public int phase2Threshold = 150;
    public int phase3Threshold = 100;
    public int phase4Threshold = 50;

    [Header("Defeat Sequence")]
    public CanvasGroup defeatDialogGroup;
    public float deathAnimDuration = 2f;   //  how long the death animation lasts
    public float fadeDuration = 1f;
    public float postDialogDelay = 2f;
    public string nextSceneName = "GameComplete";

    [Header("Animation References")]
    public Animator animator;              //  assign boss animator in Inspector
    public string deathTriggerName = "Death"; //  animation trigger name

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public int currentPhase = 1;
    [HideInInspector] public bool isInvincible = false;

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        // Prevent damage if boss is dead or invincible
        if (isDead || isInvincible)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDeath());
            return;
        }

        CheckPhaseChange();
    }

    private void CheckPhaseChange()
    {
        if (currentHealth <= phase4Threshold && currentPhase < 4)
        {
            currentPhase = 4;
            Debug.Log("Boss Phase 4 triggered! (<= 50 HP)");
        }
        else if (currentHealth <= phase3Threshold && currentPhase < 3)
        {
            currentPhase = 3;
            Debug.Log("Boss Phase 3 triggered! (<= 100 HP)");
        }
        else if (currentHealth <= phase2Threshold && currentPhase < 2)
        {
            currentPhase = 2;
            Debug.Log("Boss Phase 2 triggered! (<= 150 HP)");
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
            healthBar.value = currentHealth;

        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }

    private IEnumerator HandleDeath()
    {
        isDead = true;
        Debug.Log("Boss defeated! Playing death animation...");

        // Stop any movement or attacks if needed
        isInvincible = true;

        // Trigger death animation if animator exists
        if (animator != null && !string.IsNullOrEmpty(deathTriggerName))
            animator.ResetTrigger("Teleport");
            animator.ResetTrigger("Idle");
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("JackAttack");
            animator.SetTrigger(deathTriggerName);

        // Wait for animation to finish
        yield return new WaitForSeconds(deathAnimDuration);

        // Proceed to defeat sequence (dialog + scene transition)
        StartCoroutine(DefeatSequence());
    }

    private IEnumerator DefeatSequence()
    {
        Debug.Log("Starting end sequence...");

        // Freeze gameplay
        Time.timeScale = 0f;

        if (defeatDialogGroup == null)
        {
            Debug.LogWarning("Defeat dialog group not assigned in inspector!");
            yield break;
        }

        defeatDialogGroup.gameObject.SetActive(false);

        // No more dialogDelay — play immediately after animation
        defeatDialogGroup.gameObject.SetActive(true);
        defeatDialogGroup.alpha = 0f;
        yield return StartCoroutine(FadeIn(defeatDialogGroup));

        // Wait until the player closes the dialog (deactivated externally)
        yield return new WaitUntil(() => defeatDialogGroup == null || !defeatDialogGroup.gameObject.activeSelf);

        // Wait before loading next scene
        yield return new WaitForSecondsRealtime(postDialogDelay);

        // Resume time before loading
        Time.timeScale = 1f;

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
}
