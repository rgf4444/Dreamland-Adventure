using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;
            Debug.Log("Loading scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // Optional: Use OnTriggerEnter for 3D instead of OnTriggerEnter2D
}
