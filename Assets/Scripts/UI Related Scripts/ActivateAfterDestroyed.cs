using UnityEngine;

public class ActivateAfterDestroyed : MonoBehaviour
{
    [Header("Objects to Monitor")]
    public GameObject[] targets;

    [Header("Object to Activate")]
    public GameObject objectToActivate;

    private bool activated = false;

    void Update()
    {
        if (activated) return; // Stop checking once done

        bool allDestroyed = true;

        foreach (GameObject obj in targets)
        {
            if (obj != null) // Still exists
            {
                allDestroyed = false;
                break;
            }
        }

        if (allDestroyed)
        {
            activated = true;
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
                Debug.Log(objectToActivate.name + " has been activated!");
            }
        }
    }
}
