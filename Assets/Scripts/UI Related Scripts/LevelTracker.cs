using UnityEngine;

public class LevelTracker : MonoBehaviour
{
    public int levelNumber;

    void Start()
    {
        Debug.Log("LevelTracker Start called for Level " + levelNumber);

        // Save for restart
        PlayerPrefs.SetInt("LastLevel", levelNumber);

        // DON'T unlock levels when manually loading scenes in Editor
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.Log("Editor scene load - skipping unlock");
            return;
        }
#endif

        // Only unlock during actual gameplay
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.UpdateUnlocks(levelNumber);
        }
    }
}