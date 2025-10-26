using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponsmithHostage : MonoBehaviour
{
    [Header("Weapon Smith Components")]
    public Animator WeaponSmith;

    [Header("Manual Attack Unlocks")]
    public bool givesNormalAttack = false;
    public bool givesChargedAttack = false;
    public bool givesRangedAttack = false;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 2f;
    public GameObject interactPrompt; // Use the same type as shopkeeper

    [Header("Player Reference")]
    public PlayerMovement playerMovement;

    [Header("UI Popup")]
    public GameObject weaponPopupCanvas;

    [Header("Next Button")]
    public Button nextButton;

    [Header("Thanks Popup Settings")]
    public GameObject thanksPopup;

    private bool isBound = true;
    private bool hasBeenRescued = false;
    private bool isPlayerNearby = false;
    private Transform player;
    private Camera mainCamera;

    void Start()
    {
        // Find player exactly like shopkeeper
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }

        mainCamera = Camera.main;

        if (WeaponSmith != null)
        {
            WeaponSmith.SetBool("IsBound", true);
        }

        // Hide UI initially - EXACTLY like shopkeeper
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (weaponPopupCanvas != null) weaponPopupCanvas.SetActive(false);
        if (thanksPopup != null) thanksPopup.SetActive(false);

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    void Update()
    {
        if (player == null) return;

        // SIMPLE distance check like shopkeeper
        float distance = Vector2.Distance(transform.position, player.position);
        isPlayerNearby = distance <= interactionRange;

        if (isBound)
        {
            // Show interact prompt EXACTLY like shopkeeper
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(isPlayerNearby);
            }

            // Interaction EXACTLY like shopkeeper
            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                FreeWeaponSmith();
            }
        }
        else if (hasBeenRescued)
        {
            // Thanks popup like shopkeeper's simple logic
            if (thanksPopup != null)
            {
                thanksPopup.SetActive(isPlayerNearby);
            }
        }

        // Update positions
        UpdateUIPositions();
    }

    void UpdateUIPositions()
    {
        if (mainCamera == null) return;

        // Update interact prompt position
        if (interactPrompt != null && interactPrompt.activeInHierarchy)
        {
            Vector3 worldPosition = transform.position + Vector3.up * 1.5f;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition.y += 50f;
            interactPrompt.transform.position = screenPosition;
        }

        // Update thanks popup position
        if (thanksPopup != null && thanksPopup.activeInHierarchy)
        {
            Vector3 worldPosition = transform.position + Vector3.up * 1.5f;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition.y += 50f;
            thanksPopup.transform.position = screenPosition;
        }
    }

    void FreeWeaponSmith()
    {
        if (!isBound) return;

        isBound = false;

        if (WeaponSmith != null)
        {
            WeaponSmith.SetBool("IsBound", false);
        }

        // Hide interact prompt like shopkeeper hides when opening shop
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

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
        }

        if (givesChargedAttack)
        {
            playerMovement.EnableChargedAttack();
        }

        if (givesRangedAttack)
        {
            playerMovement.EnableRangedAttack();
        }
    }
}