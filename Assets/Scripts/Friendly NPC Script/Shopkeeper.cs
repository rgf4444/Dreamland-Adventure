using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Shopkeeper : MonoBehaviour
{
    [Header("Shopkeeper Components")]
    public Animator shopkeeperAnimator;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 2f;
    public GameObject interactPrompt;

    [Header("Shop UI")]
    public GameObject shopWindow;
    public Button closeButton;

    [Header("Item Buttons")]
    public Button healthPotionButton;
    public Button damageUpgradeButton;

    [Header("Health Potion Settings")]
    public int healthHealAmount = 1;

    [Header("Damage Upgrade Settings ")]
    public bool upgradeNormalAttack = false;
    public bool upgradeChargedAttack = false;
    public bool upgradeRangedAttack = false;

    [Header("Damage Increase Amounts")]
    public int normalAttackDamageIncrease = 1;
    public int chargedAttackDamageIncrease = 2;
    public int rangedAttackDamageIncrease = 1;

    [Header("Button Behavior")]
    public bool disableButtonsAfterPurchase = true;

    [Header("Thank You Message")]
    public GameObject thankYouMessage;
    public float thankYouDuration = 5f;
    public string thankYouText = "May you save our kingdom!";
    public Vector3 thankYouOffset = new Vector3(0, 2f, 0);
    public Vector3 interactPromptOffset = new Vector3(0, 1.5f, 0);

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private bool isPlayerNearby = false;
    private bool isShopOpen = false;
    private bool isShowingThankYou = false;
    private Transform player;
    private Camera mainCamera;

    private bool healthPotionPurchased = false;
    private bool damageUpgradePurchased = false;

    void Start()
    {
        FindPlayerComponents();
        mainCamera = Camera.main;

        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (shopWindow != null) shopWindow.SetActive(false);
        if (thankYouMessage != null) thankYouMessage.SetActive(false);

        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        if (healthPotionButton != null) healthPotionButton.onClick.AddListener(BuyHealthPotion);
        if (damageUpgradeButton != null) damageUpgradeButton.onClick.AddListener(BuyDamageUpgrade);

        UpdateButtonStates();
        UpdateUpgradeButtonText();
    }

    void FindPlayerComponents()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }

        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        isPlayerNearby = distance <= interactionRange;

        if (shopkeeperAnimator != null)
        {
            shopkeeperAnimator.SetBool("IsPlayerNearby", isPlayerNearby);
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(isPlayerNearby && !isShopOpen && !isShowingThankYou);
        }

        UpdateInteractPromptPosition();
        UpdateThankYouMessagePosition();

        if (isPlayerNearby && Input.GetKeyDown(interactKey) && !isShopOpen && !isShowingThankYou)
        {
            OpenShop();
        }

        if (isShopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }

        if (isShopOpen)
        {
            UpdateButtonStates();
        }
    }

    void UpdateInteractPromptPosition()
    {
        if (mainCamera == null || interactPrompt == null || !interactPrompt.activeInHierarchy)
            return;

        Vector3 worldPosition = transform.position + interactPromptOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        interactPrompt.transform.position = screenPosition;
    }

    void UpdateThankYouMessagePosition()
    {
        if (mainCamera == null || thankYouMessage == null || !thankYouMessage.activeInHierarchy)
            return;

        Vector3 worldPosition = transform.position + thankYouOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        thankYouMessage.transform.position = screenPosition;
    }

    void OpenShop()
    {
        isShopOpen = true;

        if (shopWindow != null)
        {
            shopWindow.SetActive(true);
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        if (thankYouMessage != null)
        {
            thankYouMessage.SetActive(false);
            isShowingThankYou = false;
        }

        UpdateButtonStates();
        Debug.Log("Welcome! Take what you need!");
    }

    void CloseShop()
    {
        isShopOpen = false;

        if (shopWindow != null)
        {
            shopWindow.SetActive(false);
        }

        Debug.Log("Thank you for visiting!");
        ShowThankYouMessage();
    }

    void ShowThankYouMessage()
    {
        if (thankYouMessage != null)
        {
            TextMeshProUGUI textComponent = thankYouMessage.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = thankYouText;
            }

            thankYouMessage.SetActive(true);
            isShowingThankYou = true;
            StartCoroutine(HideThankYouAfterDelay());
        }
    }

    IEnumerator HideThankYouAfterDelay()
    {
        yield return new WaitForSeconds(thankYouDuration);

        if (thankYouMessage != null)
        {
            thankYouMessage.SetActive(false);
            isShowingThankYou = false;
        }
    }

    void BuyHealthPotion()
    {
        if (playerHealth != null && !healthPotionPurchased && !IsHealthFull())
        {
            playerHealth.TakeDamage(-healthHealAmount);
            Debug.Log($"Health Potion purchased! Healed {healthHealAmount} heart(s).");

            healthPotionPurchased = true;
            UpdateButtonStates();
        }
        else if (IsHealthFull())
        {
            Debug.Log("Health is already full! No need for a potion.");
        }
    }

    void BuyDamageUpgrade()
    {
        if (playerMovement != null && !damageUpgradePurchased)
        {
            string upgradedAttacks = "";

            if (upgradeNormalAttack)
            {
                playerMovement.normalAttackDamage += normalAttackDamageIncrease;
                upgradedAttacks += $"Normal: +{normalAttackDamageIncrease} = {playerMovement.normalAttackDamage}, ";
            }

            if (upgradeChargedAttack)
            {
                playerMovement.chargedAttackDamage += chargedAttackDamageIncrease;
                upgradedAttacks += $"Charged: +{chargedAttackDamageIncrease} = {playerMovement.chargedAttackDamage}, ";
            }

            if (upgradeRangedAttack && playerMovement.rangedAttackEnabled)
            {
                upgradedAttacks += "Ranged Attack (needs implementation), ";
            }

            Debug.Log($"Damage Upgrade purchased! {upgradedAttacks.TrimEnd(',', ' ')}");

            damageUpgradePurchased = true;
            UpdateButtonStates();
            UpdateUpgradeButtonText();
        }
    }

    void UpdateButtonStates()
    {
        // Health Potion Button
        if (healthPotionButton != null)
        {
            bool canBuyHealth = !healthPotionPurchased && !IsHealthFull();
            healthPotionButton.interactable = canBuyHealth;

            TextMeshProUGUI buttonText = healthPotionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (healthPotionPurchased)
                {
                    buttonText.text = "SOLD OUT";
                }
                else if (IsHealthFull())
                {
                    buttonText.text = "HEALTH FULL";
                }
                else
                {
                    buttonText.text = "Health Potion";
                }
            }
        }

        // Damage Upgrade Button
        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.interactable = !damageUpgradePurchased;
        }
    }

    void UpdateUpgradeButtonText()
    {
        if (damageUpgradeButton != null)
        {
            TextMeshProUGUI buttonText = damageUpgradeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (damageUpgradePurchased)
                {
                    buttonText.text = "SOLD OUT";
                }
                else
                {
                    string upgradeInfo = "Damage Upgrade: ";

                    if (upgradeNormalAttack) upgradeInfo += $"Normal +{normalAttackDamageIncrease} ";
                    if (upgradeChargedAttack) upgradeInfo += $"Charged +{chargedAttackDamageIncrease} ";
                    if (upgradeRangedAttack) upgradeInfo += $"Ranged +{rangedAttackDamageIncrease} ";

                    buttonText.text = upgradeInfo.Trim();
                }
            }
        }
    }

    bool IsHealthFull()
    {
        if (playerHealth == null) return false;
        return playerHealth.health >= playerHealth.hearts.Length;
    }
}
