using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI slot for selling harvested crops to shop (with quantity selector)
/// </summary>
public class ShopItemSellSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText; // Price per unit
    [SerializeField] private TextMeshProUGUI availableText; // "You have: X"
    [SerializeField] private TextMeshProUGUI quantityText; // Shows selected amount
    [SerializeField] private Button decreaseButton; // ← arrow
    [SerializeField] private Button increaseButton; // → arrow

    private InventoryItem inventoryItem;
    private int availableQuantity;
    private int selectedQuantity = 0;

    // Event to notify when quantity changes
    public System.Action OnQuantityChanged;

    private void Start()
    {
        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(DecreaseQuantity);

        if (increaseButton != null)
            increaseButton.onClick.AddListener(IncreaseQuantity);
    }

    /// <summary>
    /// Setup the sell slot
    /// </summary>
    public void Setup(InventoryItem item, int quantity)
    {
        Debug.Log($"[ShopItemSellSlot] Setup called for {item?.itemName ?? "NULL"} with quantity {quantity}");

        inventoryItem = item;
        availableQuantity = quantity;
        selectedQuantity = 0; // Start at 0

        if (itemIcon != null && item.itemIcon != null)
            itemIcon.sprite = item.itemIcon;

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (priceText != null)
            priceText.text = $"${item.sellValue} each";

        if (availableText != null)
        {
            availableText.text = $"You have: {quantity}";
            Debug.Log($"[ShopItemSellSlot] Set availableText to: You have: {quantity}");
        }
        else
        {
            Debug.LogError("[ShopItemSellSlot] availableText is NULL!");
        }

        UpdateQuantityDisplay();
    }

    /// <summary>
    /// Decrease selected quantity
    /// </summary>
    private void DecreaseQuantity()
    {
        if (selectedQuantity > 0)
        {
            selectedQuantity--;
            UpdateQuantityDisplay();
            OnQuantityChanged?.Invoke();
        }
    }

    /// <summary>
    /// Increase selected quantity
    /// </summary>
    private void IncreaseQuantity()
    {
        if (selectedQuantity < availableQuantity)
        {
            selectedQuantity++;
            UpdateQuantityDisplay();
            OnQuantityChanged?.Invoke();
        }
    }

    /// <summary>
    /// Update the quantity display and button states
    /// </summary>
    private void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            quantityText.text = selectedQuantity.ToString();
            Debug.Log($"[ShopItemSellSlot] Updated quantityText to: {selectedQuantity}");
        }
        else
        {
            Debug.LogError("[ShopItemSellSlot] quantityText is NULL!");
        }

        // Update button interactability
        if (decreaseButton != null)
            decreaseButton.interactable = selectedQuantity > 0;

        if (increaseButton != null)
            increaseButton.interactable = selectedQuantity < availableQuantity;

        Debug.Log($"[ShopItemSellSlot] availableQuantity = {availableQuantity}, selectedQuantity = {selectedQuantity}");
    }

    /// <summary>
    /// Get the selected quantity for this item
    /// </summary>
    public int GetSelectedQuantity()
    {
        return selectedQuantity;
    }

    /// <summary>
    /// Get the inventory item
    /// </summary>
    public InventoryItem GetInventoryItem()
    {
        return inventoryItem;
    }

    /// <summary>
    /// Get total value of selected quantity
    /// </summary>
    public int GetTotalValue()
    {
        return selectedQuantity * inventoryItem.sellValue;
    }

    /// <summary>
    /// Reset quantity to 0
    /// </summary>
    public void ResetQuantity()
    {
        selectedQuantity = 0;
        UpdateQuantityDisplay();
    }

    private void OnDestroy()
    {
        if (decreaseButton != null)
            decreaseButton.onClick.RemoveListener(DecreaseQuantity);

        if (increaseButton != null)
            increaseButton.onClick.RemoveListener(IncreaseQuantity);
    }
}