using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Vertical collapsible seed selection bar on the left side of the screen
/// </summary>
public class SeedSelectionBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject seedBarPanel;
    [SerializeField] private Transform seedSlotsContainer;
    [SerializeField] private GameObject seedSlotPrefab;
    [SerializeField] private Button expandCollapseButton;
    [SerializeField] private GameObject expandedContent;
    [SerializeField] private RectTransform collapsedSize;
    [SerializeField] private RectTransform expandedSize;

    [Header("Expand/Collapse")]
    [SerializeField] private Image expandCollapseIcon;
    [SerializeField] private Sprite expandIcon;  // >> arrow
    [SerializeField] private Sprite collapseIcon; // << arrow
    [SerializeField] private float animationSpeed = 5f;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.S;
    [SerializeField] private bool startExpanded = false;
    [SerializeField] private Color selectedSlotColor = new Color(0.8f, 1f, 0.8f);
    [SerializeField] private Color normalSlotColor = Color.white;

    private bool isExpanded;
    private List<SeedSelectionSlot> seedSlots = new List<SeedSelectionSlot>();
    private SeedSelectionSlot selectedSlot;
    private RectTransform rectTransform;
    private Vector2 targetSize;

    private void Awake()
    {
        rectTransform = seedBarPanel.GetComponent<RectTransform>();

        // Set initial state
        isExpanded = startExpanded;
        expandedContent.SetActive(isExpanded);
        UpdateExpandCollapseIcon();

        // Set initial size
        targetSize = isExpanded ? expandedSize.sizeDelta : collapsedSize.sizeDelta;
        rectTransform.sizeDelta = targetSize;
    }

    private void Start()
    {
        // Setup button listener
        if (expandCollapseButton != null)
        {
            expandCollapseButton.onClick.AddListener(ToggleExpanded);
        }

        // Subscribe to player inventory changes
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnSeedPacketChanged += OnSeedInventoryChanged;
        }

        // Subscribe to seed selection changes
        if (SeedInventory.Instance != null)
        {
            SeedInventory.Instance.OnSeedChanged += OnSeedSelectionChanged;
        }

        // Initial population
        RefreshSeedSlots();
    }

    private void Update()
    {
        // Toggle with key
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleExpanded();
        }

        // Smooth size animation
        if (rectTransform.sizeDelta != targetSize)
        {
            rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, targetSize, Time.deltaTime * animationSpeed);
        }

        // Number key selection
        HandleNumberKeySelection();

        // Scroll wheel selection (when collapsed or expanded)
        HandleScrollWheelSelection();
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnSeedPacketChanged -= OnSeedInventoryChanged;
        }

        if (SeedInventory.Instance != null)
        {
            SeedInventory.Instance.OnSeedChanged -= OnSeedSelectionChanged;
        }

        if (expandCollapseButton != null)
        {
            expandCollapseButton.onClick.RemoveListener(ToggleExpanded);
        }
    }

    /// <summary>
    /// Toggle expanded/collapsed state
    /// </summary>
    public void ToggleExpanded()
    {
        isExpanded = !isExpanded;
        expandedContent.SetActive(isExpanded);
        UpdateExpandCollapseIcon();

        // Set target size for animation
        targetSize = isExpanded ? expandedSize.sizeDelta : collapsedSize.sizeDelta;

        Debug.Log($"[SeedSelectionBar] {(isExpanded ? "Expanded" : "Collapsed")}");
    }

    /// <summary>
    /// Refresh the seed slots based on player inventory
    /// </summary>
    private void RefreshSeedSlots()
    {
        // Clear existing slots
        foreach (var slot in seedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        seedSlots.Clear();

        // Get player's seeds
        if (PlayerInventory.Instance == null) return;

        var playerSeeds = PlayerInventory.Instance.SeedPackets
            .Where(kvp => kvp.Value > 0)
            .OrderBy(kvp => kvp.Key.cropName);

        int index = 0;
        foreach (var seedKvp in playerSeeds)
        {
            GameObject slotObj = Instantiate(seedSlotPrefab, seedSlotsContainer);
            SeedSelectionSlot slot = slotObj.GetComponent<SeedSelectionSlot>();

            if (slot != null)
            {
                slot.Setup(seedKvp.Key, seedKvp.Value, index);
                slot.OnSlotClicked += HandleSlotClick;
                seedSlots.Add(slot);

                // Select first slot by default if nothing selected
                if (selectedSlot == null && index == 0)
                {
                    SelectSlot(slot);
                }

                index++;
            }
        }

        // Always ensure the first slot shows in collapsed view
        UpdateCollapsedView();
    }

    /// <summary>
    /// Handle clicking a seed slot
    /// </summary>
    private void HandleSlotClick(SeedSelectionSlot slot)
    {
        SelectSlot(slot);

        // Optionally collapse after selection
        // ToggleExpanded();
    }

    /// <summary>
    /// Select a seed slot
    /// </summary>
    private void SelectSlot(SeedSelectionSlot slot)
    {
        if (slot == null || slot.SeedPacket == null) return;

        // Update visual selection
        if (selectedSlot != null)
            selectedSlot.SetSelected(false);

        selectedSlot = slot;
        selectedSlot.SetSelected(true);

        // Update SeedInventory
        SeedInventory.Instance.SelectSeed(slot.SeedPacket);

        // Update collapsed view to show selected
        UpdateCollapsedView();

        Debug.Log($"[SeedSelectionBar] Selected {slot.SeedPacket.cropName}");
    }

    /// <summary>
    /// Update which slot shows when collapsed
    /// </summary>
    private void UpdateCollapsedView()
    {
        // When collapsed, only the selected slot should be visible at the top
        if (selectedSlot != null)
        {
            // This could be done by reordering, or having a separate display
            // For now, we'll rely on the layout to show the first slot
        }
    }

    /// <summary>
    /// Handle number key selection (1-9)
    /// </summary>
    private void HandleNumberKeySelection()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int slotIndex = i - 1;
                if (slotIndex < seedSlots.Count)
                {
                    SelectSlot(seedSlots[slotIndex]);
                }
            }
        }
    }

    /// <summary>
    /// Handle scroll wheel selection
    /// </summary>
    private void HandleScrollWheelSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            if (seedSlots.Count == 0) return;

            int currentIndex = selectedSlot != null ? seedSlots.IndexOf(selectedSlot) : 0;

            if (scroll > 0) // Scroll up
            {
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = seedSlots.Count - 1;
            }
            else // Scroll down
            {
                currentIndex++;
                if (currentIndex >= seedSlots.Count)
                    currentIndex = 0;
            }

            SelectSlot(seedSlots[currentIndex]);
        }
    }

    /// <summary>
    /// Update expand/collapse button icon
    /// </summary>
    private void UpdateExpandCollapseIcon()
    {
        if (expandCollapseIcon != null)
        {
            expandCollapseIcon.sprite = isExpanded ? collapseIcon : expandIcon;
        }
    }

    /// <summary>
    /// Called when player's seed inventory changes
    /// </summary>
    private void OnSeedInventoryChanged(SeedPacket packet, int quantity)
    {
        RefreshSeedSlots();
    }

    /// <summary>
    /// Called when seed selection changes externally
    /// </summary>
    private void OnSeedSelectionChanged(SeedPacket packet, int index)
    {
        // Find and select the corresponding slot
        var slot = seedSlots.FirstOrDefault(s => s.SeedPacket == packet);
        if (slot != null && slot != selectedSlot)
        {
            SelectSlot(slot);
        }
    }
}