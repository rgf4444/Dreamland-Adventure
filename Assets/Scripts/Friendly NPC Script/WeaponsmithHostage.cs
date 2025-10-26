using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponSmithHostage : MonoBehaviour
{
    [Header("Weapon Smith Components")]
    public Animator WeaponSmith;

    [Header("Manual Attack Unlocks")]
    public bool givesNormalAttack = false;
    public bool givesChargedAttack = false;
    public bool givesRangedAttack = false;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 1.5f;

    [Header("Player Reference")]
    public PlayerMovement playerMovement;

    [Header("UI Popup")]
    public GameObject weaponPopupCanvas;

    [Header("Next Button")]
    public Button nextButton;

    [Header("Top Popup Settings")]
    public GameObject topPopup; // The popup that appears above the weaponsmith's head (Overlay Canvas)
    public float popupHeightOffset = 50f; // Screen space offset above the weaponsmith

    [Header("Thanks Popup Settings")]
    public GameObject thanksPopup; // The "Thanks again!" popup after receiving attacks

    private bool isBound = true;
    private bool canInteract = false;
    private bool hasBeenRescued = false;
    private Transform player;
    private Camera mainCamera;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;

        if (WeaponSmith != null)
        {
            WeaponSmith.SetBool("IsBound", true);
        }

        if (weaponPopupCanvas != null) weaponPopupCanvas.SetActive(false);
        if (topPopup != null) topPopup.SetActive(false);
        if (thanksPopup != null) thanksPopup.SetActive(false);

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    void Update()
    {
        if (isBound)
        {
            CheckPlayerDistance();

            if (canInteract && Input.GetKeyDown(interactKey))
            {
                FreeWeaponSmith();
            }
        }
        else if (hasBeenRescued)
        {
            // After rescue, check distance for thanks popup
            CheckThanksPopupDistance();
        }

        // Update the popup positions to follow the weaponsmith
        UpdatePopupPositions();
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInteractable = canInteract;
        canInteract = distance <= interactionRange;

        // Show/hide top popup based on interaction range
        if (topPopup != null && isBound) // Only show if still bound
        {
            topPopup.SetActive(canInteract);
        }

        // If player moved out of range, hide top popup
        if (wasInteractable && !canInteract && topPopup != null)
        {
            topPopup.SetActive(false);
        }
    }

    void CheckThanksPopupDistance()
    {
        if (player == null || thanksPopup == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRange = distance <= interactionRange;

        // Show/hide thanks popup based on interaction range
        thanksPopup.SetActive(inRange);
    }

    void UpdatePopupPositions()
    {
        if (mainCamera == null) return;

        // Update top popup position (for bound state)
        if (topPopup != null && topPopup.activeInHierarchy)
        {
            Vector3 worldPosition = transform.position + Vector3.up * 1.5f;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition.y += popupHeightOffset;
            topPopup.transform.position = screenPosition;
        }

        // Update thanks popup position (for rescued state)
        if (thanksPopup != null && thanksPopup.activeInHierarchy)
        {
            Vector3 worldPosition = transform.position + Vector3.up * 1.5f;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition.y += popupHeightOffset;
            thanksPopup.transform.position = screenPosition;
        }
    }

    void FreeWeaponSmith()
    {
        if (!isBound) return;

        isBound = false;
        canInteract = false;

        if (WeaponSmith != null)
        {
            WeaponSmith.SetBool("IsBound", false);
        }

        // Hide the top popup when rescued
        if (topPopup != null) topPopup.SetActive(false);

        Debug.Log("Weapon Smith rescued!");

        StartCoroutine(ShowWeaponPopupAfterAnimation());
    }

    IEnumerator ShowWeaponPopupAfterAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        ShowWeaponPopup();
    }

    void ShowWeaponPopup()
    {
        if (weaponPopupCanvas != null)
        {
            weaponPopupCanvas.SetActive(true);
        }
    }

    void OnNextButtonClicked()
    {
        if (weaponPopupCanvas != null)
        {
            weaponPopupCanvas.SetActive(false);
        }

        if (!hasBeenRescued)
        {
            UnlockAttacks();
            hasBeenRescued = true;
        }
    }

    void UnlockAttacks()
    {
        if (playerMovement == null) return;

        if (givesNormalAttack)
        {
            playerMovement.EnableNormalAttack();
            Debug.Log("Normal Attack Unlocked!");
        }

        if (givesChargedAttack)
        {
            playerMovement.EnableChargedAttack();
            Debug.Log("Charged Attack Unlocked!");
        }

        if (givesRangedAttack)
        {
            playerMovement.EnableRangedAttack();
            Debug.Log("Ranged Attack Unlocked!");
        }
    }
}