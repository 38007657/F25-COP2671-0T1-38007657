using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI slot for selling harvested crops to shop
/// </summary>
public class ShopItemSellSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText; // Price per unit
    [SerializeField] private TextMeshProUGUI quantityText; // How many player has
    [SerializeField] private Button sellButton;

    private InventoryItem inventoryItem;
    private int playerQuantity;

    private void Start()
    {
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellButtonClicked);
        }
    }

    /// <summary>
    /// Setup the sell slot
    /// </summary>
    public void Setup(InventoryItem item, int quantity)
    {
        inventoryItem = item;
        playerQuantity = quantity;

        if (itemIcon != null && item.itemIcon != null)
            itemIcon.sprite = item.itemIcon;

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (priceText != null)
            priceText.text = $"${item.sellValue} each";

        if (quantityText != null)
            quantityText.text = $"You have: {quantity}";

        UpdateButtonState();
    }

    /// <summary>
    /// Update button state based on if player has items
    /// </summary>
    private void UpdateButtonState()
    {
        if (sellButton == null) return;

        bool hasItems = playerQuantity > 0;
        sellButton.interactable = hasItems;

        // Update button text
        TextMeshProUGUI buttonText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (hasItems)
            {
                int totalValue = inventoryItem.sellValue * playerQuantity;
                buttonText.text = $"Sell All (${totalValue})";
            }
            else
            {
                buttonText.text = "None to Sell";
                buttonText.color = Color.gray;
            }
        }
    }

    /// <summary>
    /// Called when sell button is clicked
    /// </summary>
    private void OnSellButtonClicked()
    {
        if (inventoryItem == null || playerQuantity <= 0) return;

        if (ShopInventory.Instance != null)
        {
            // Sell ALL of this crop type
            bool success = ShopInventory.Instance.SellCropToShop(inventoryItem, playerQuantity);

            if (success)
            {
                int totalEarned = inventoryItem.sellValue * playerQuantity;
                Debug.Log($"[ShopItemSellSlot] Sold {playerQuantity}x {inventoryItem.itemName} for ${totalEarned}!");

                // Refresh the shop UI
                if (InventoryUIManager.Instance != null)
                {
                    InventoryUIManager.Instance.RefreshCurrentTab();
                }
            }
            else
            {
                Debug.Log($"[ShopItemSellSlot] Failed to sell {inventoryItem.itemName}");
            }
        }
    }

    private void OnDestroy()
    {
        if (sellButton != null)
        {
            sellButton.onClick.RemoveListener(OnSellButtonClicked);
        }
    }
}