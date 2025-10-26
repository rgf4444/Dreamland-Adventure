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
    public CanvasGroup defeatDialogGroup;  // assign your dialog UI canvas group here
    public float dialogDelay = 1f;         // wait before showing dialog
    public float fadeDuration = 1f;        // fade-in speed
    public float postDialogDelay = 2f;     // delay before scene load
    public string nextSceneName = "GameComplete";

    private bool isDead = false;
    public int currentPhase = 1;

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
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            StartCoroutine(DefeatSequence());
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

    private IEnumerator DefeatSequence()
    {
        isDead = true;
        Debug.Log("Boss defeated! Starting end sequence...");

        // Freeze gameplay
        Time.timeScale = 0f;

        // Safety check
        if (defeatDialogGroup == null)
        {
            Debug.LogWarning("Defeat dialog group not assigned in inspector!");
            yield break;
        }

        // Make sure it’s off first, then turn it on after the delay
        defeatDialogGroup.gameObject.SetActive(false);

        // Wait a bit before showing the dialog
        yield return new WaitForSecondsRealtime(dialogDelay);

        // Activate and fade it in
        defeatDialogGroup.gameObject.SetActive(true);
        defeatDialogGroup.alpha = 0f;
        yield return StartCoroutine(FadeIn(defeatDialogGroup));

        // Wait until the player closes the dialog (deactivated externally)
        yield return new WaitUntil(() => defeatDialogGroup == null || !defeatDialogGroup.gameObject.activeSelf);

        // Give a little time before loading next scene
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
