using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Display script for inventory item slots (read-only)
/// </summary>
public class InventoryItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI valueText; // Optional

    /// <summary>
    /// Setup the slot with all info
    /// </summary>
    public void Setup(Sprite icon, string itemName, int quantity, int value, bool showValue = true)
    {
        if (itemIcon != null)
            itemIcon.sprite = icon;

        if (itemNameText != null)
            itemNameText.text = itemName;

        if (quantityText != null)
            quantityText.text = $"x{quantity}";

        // Only show value if requested
        if (valueText != null)
        {
            if (showValue)
            {
                valueText.text = $"${value}";
                valueText.gameObject.SetActive(true);
            }
            else
            {
                valueText.gameObject.SetActive(false);
            }
        }
    }
}