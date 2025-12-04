using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI slot for buying seed packets from shop
/// </summary>
public class ShopItemBuySlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI seedCountText;
    [SerializeField] private Button buyButton;

    private SeedPacket seedPacket;

    private void Start()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    /// <summary>
    /// Setup the buy slot
    /// </summary>
    public void Setup(SeedPacket packet)
    {
        seedPacket = packet;

        if (itemIcon != null && packet.coverImage != null)
            itemIcon.sprite = packet.coverImage;

        if (itemNameText != null)
            itemNameText.text = packet.cropName;

        if (priceText != null)
            priceText.text = $"${packet.packetCost}";

        if (seedCountText != null)
            seedCountText.text = $"{packet.seedsPerPacket} seeds";

        UpdateButtonState();
    }

    /// <summary>
    /// Update button interactable state based on player's money
    /// </summary>
    private void UpdateButtonState()
    {
        if (buyButton == null || seedPacket == null) return;

        bool canAfford = PlayerInventory.Instance != null &&
                        PlayerInventory.Instance.CanAfford(seedPacket.packetCost);

        buyButton.interactable = canAfford;

        // Future Feature Option: Change button color if can't afford
        if (!canAfford && buyButton.GetComponentInChildren<TextMeshProUGUI>() != null)
        {
            buyButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
        }
    }

    /// <summary>
    /// Called when buy button is clicked
    /// </summary>
    private void OnBuyButtonClicked()
    {
        if (seedPacket == null) return;

        if (ShopInventory.Instance != null)
        {
            bool success = ShopInventory.Instance.BuySeedPacket(seedPacket);

            if (success)
            {
                Debug.Log($"[ShopItemBuySlot] Successfully bought {seedPacket.cropName} seeds");

                // Refresh the shop UI
                if (InventoryUIManager.Instance != null)
                {
                    InventoryUIManager.Instance.RefreshCurrentTab();
                }
            }
            else
            {
                Debug.Log($"[ShopItemBuySlot] Failed to buy {seedPacket.cropName} seeds");
            }

            UpdateButtonState();
        }
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyButtonClicked);
        }
    }
}