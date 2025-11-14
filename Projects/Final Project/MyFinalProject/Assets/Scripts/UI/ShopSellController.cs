using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Controls the sell panel with quantity selection and total value
/// </summary>
public class ShopSellController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI totalValueText;
    [SerializeField] private Button sellSelectedButton;
    [SerializeField] private Transform sellContentParent;

    [Header("Button Text")]
    [SerializeField] private string noItemsText = "Sell Items";
    [SerializeField] private string hasItemsText = "Sell ({0} items)";

    private List<ShopItemSellSlot> sellSlots = new List<ShopItemSellSlot>();

    private void Start()
    {
        if (sellSelectedButton != null)
        {
            sellSelectedButton.onClick.AddListener(SellSelectedItems);
        }

        UpdateTotalValue();
    }

    /// <summary>
    /// Register a sell slot (called when slots are created)
    /// </summary>
    public void RegisterSellSlot(ShopItemSellSlot slot)
    {
        if (!sellSlots.Contains(slot))
        {
            sellSlots.Add(slot);
            slot.OnQuantityChanged += UpdateTotalValue;
        }
    }

    /// <summary>
    /// Clear all registered slots
    /// </summary>
    public void ClearSellSlots()
    {
        foreach (var slot in sellSlots)
        {
            if (slot != null)
            {
                slot.OnQuantityChanged -= UpdateTotalValue;
            }
        }
        sellSlots.Clear();
    }

    /// <summary>
    /// Update the total value display
    /// </summary>
    private void UpdateTotalValue()
    {
        int totalValue = 0;
        int totalItems = 0;

        foreach (var slot in sellSlots)
        {
            if (slot != null)
            {
                totalValue += slot.GetTotalValue();
                totalItems += slot.GetSelectedQuantity();
            }
        }

        if (totalValueText != null)
        {
            totalValueText.text = $"Total: ${totalValue}";
        }

        // Enable/disable sell button based on if anything selected
        if (sellSelectedButton != null)
        {
            sellSelectedButton.interactable = totalItems > 0;

            TextMeshProUGUI buttonText = sellSelectedButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                // Show different text based on selection
                if (totalItems > 0)
                {
                    buttonText.text = string.Format(hasItemsText, totalItems);
                }
                else
                {
                    buttonText.text = noItemsText;
                }
            }
        }
    }

    /// <summary>
    /// Sell all selected items
    /// </summary>
    private void SellSelectedItems()
    {
        if (ShopInventory.Instance == null || PlayerInventory.Instance == null)
        {
            Debug.LogError("[ShopSellController] ShopInventory or PlayerInventory not found!");
            return;
        }

        int totalEarned = 0;
        int totalItemsSold = 0;

        // Process each slot
        foreach (var slot in sellSlots)
        {
            if (slot != null)
            {
                int quantity = slot.GetSelectedQuantity();

                if (quantity > 0)
                {
                    InventoryItem item = slot.GetInventoryItem();

                    if (ShopInventory.Instance.SellCropToShop(item, quantity))
                    {
                        totalEarned += item.sellValue * quantity;
                        totalItemsSold += quantity;

                        Debug.Log($"[ShopSellController] Sold {quantity}x {item.itemName} for ${item.sellValue * quantity}");
                    }
                }
            }
        }

        if (totalItemsSold > 0)
        {
            Debug.Log($"[ShopSellController] Total sale: {totalItemsSold} items for ${totalEarned}");

            // Refresh the shop UI
            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance.RefreshCurrentTab();
            }
        }
    }

    private void OnDestroy()
    {
        ClearSellSlots();

        if (sellSelectedButton != null)
        {
            sellSelectedButton.onClick.RemoveListener(SellSelectedItems);
        }
    }
}