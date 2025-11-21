using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Individual seed slot in the vertical selection bar
/// </summary>
public class SeedSelectionSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image seedIcon;
    [SerializeField] private TextMeshProUGUI seedNameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image selectionBorder;
    [SerializeField] private GameObject numberKeyHint;
    [SerializeField] private TextMeshProUGUI numberKeyText;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color selectedColor = new Color(0.8f, 1f, 0.8f);

    // Data
    private SeedPacket seedPacket;
    private int quantity;
    private int slotIndex;
    private bool isSelected;
    private bool isHovered;

    // Events
    public System.Action<SeedSelectionSlot> OnSlotClicked;

    // Properties
    public SeedPacket SeedPacket => seedPacket;
    public int SlotIndex => slotIndex;

    /// <summary>
    /// Setup the slot with seed data
    /// </summary>
    public void Setup(SeedPacket packet, int qty, int index)
    {
        seedPacket = packet;
        quantity = qty;
        slotIndex = index;

        // Update visuals
        if (seedIcon != null && packet.coverImage != null)
        {
            seedIcon.sprite = packet.coverImage;
            seedIcon.enabled = true;
        }

        if (seedNameText != null)
        {
            seedNameText.text = packet.cropName;
        }

        if (quantityText != null)
        {
            quantityText.text = qty.ToString();
        }

        // REMOVED: Number key hints - Click only selection!
        // Hide number key hints (optional UI elements)
        if (numberKeyHint != null)
        {
            numberKeyHint.SetActive(false);
        }

        UpdateVisualState();
    }

    /// <summary>
    /// Update quantity display
    /// </summary>
    public void UpdateQuantity(int qty)
    {
        quantity = qty;
        if (quantityText != null)
        {
            quantityText.text = qty.ToString();
        }
    }

    /// <summary>
    /// Set selected state
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();
    }

    /// <summary>
    /// Update visual state based on selection/hover
    /// </summary>
    private void UpdateVisualState()
    {
        if (backgroundImage != null)
        {
            if (isSelected)
            {
                backgroundImage.color = selectedColor;
            }
            else if (isHovered)
            {
                backgroundImage.color = hoverColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }

        if (selectionBorder != null)
        {
            selectionBorder.enabled = isSelected;
        }
    }

    // UI Event Handlers
    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisualState();

        // Optional: Show tooltip with seed name
        // TooltipManager.Show(seedPacket.cropName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisualState();

        // Optional: Hide tooltip
        // TooltipManager.Hide();
    }
}