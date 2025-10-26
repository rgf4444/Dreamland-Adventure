using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FreezeTimeWithDialogFade : MonoBehaviour
{
    [Header("Settings")]
    public bool freezeOnStart = true;
    public float dialogDelay = 1f;     // Delay before dialog appears
    public float fadeDuration = 1f;    // Fade-in duration

    [Header("References")]
    public CanvasGroup dialogCanvasGroup; // Attach the dialog CanvasGroup here

    private bool isFrozen = false;

    private void Start()
    {
        if (freezeOnStart)
            StartCoroutine(FreezeAndShowDialog());
    }

    private IEnumerator FreezeAndShowDialog()
    {
        Freeze();

        if (dialogCanvasGroup != null)
        {
            dialogCanvasGroup.gameObject.SetActive(false);
            dialogCanvasGroup.alpha = 0f;
        }

        // Wait for the pre-dialog delay (in real time)
        yield return new WaitForSecondsRealtime(dialogDelay);

        if (dialogCanvasGroup != null)
        {
            dialogCanvasGroup.gameObject.SetActive(true);
            yield return StartCoroutine(FadeIn(dialogCanvasGroup));
        }
    }

    private IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unaffected by Time.timeScale
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public void Freeze()
    {
        Time.timeScale = 0f;
        isFrozen = true;
    }

    public void Unfreeze()
    {
        Time.timeScale = 1f;
        isFrozen = false;
    }

    public void ToggleFreeze()
    {
        if (isFrozen)
            Unfreeze();
        else
            Freeze();
    }
}
