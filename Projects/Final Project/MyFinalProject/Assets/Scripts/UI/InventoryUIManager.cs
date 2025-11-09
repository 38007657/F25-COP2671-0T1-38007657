using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the tabbed inventory/shop UI
/// </summary>
public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("Main Panel")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    [Header("Tab Buttons")]
    [SerializeField] private Button inventoryTabButton;
    [SerializeField] private Button moneyTabButton;
    [SerializeField] private Button shopTabButton;

    [Header("Tab Contents")]
    [SerializeField] private GameObject inventoryTabContent;
    [SerializeField] private GameObject moneyTabContent;
    [SerializeField] private GameObject shopTabContent;

    [Header("Inventory Tab - Scroll View Contents")]
    [SerializeField] private Transform seedsContentParent;
    [SerializeField] private Transform cropsContentParent;

    [Header("Shop Tab - Scroll View Contents")]
    [SerializeField] private Transform buyContentParent;
    [SerializeField] private Transform sellContentParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject inventoryItemSlotPrefab;
    [SerializeField] private GameObject shopItemBuySlotPrefab;
    [SerializeField] private GameObject shopItemSellSlotPrefab;

    [Header("Money Display")]
    [SerializeField] private TextMeshProUGUI currentMoneyText;

    [Header("Tab Button Colors")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = Color.gray;

    private bool isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Setup tab button listeners
        inventoryTabButton.onClick.AddListener(() => SwitchTab(0));
        moneyTabButton.onClick.AddListener(() => SwitchTab(1));
        shopTabButton.onClick.AddListener(() => SwitchTab(2));

        // Subscribe to inventory events
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            PlayerInventory.Instance.OnHarvestedItemChanged += OnInventoryChanged;
            PlayerInventory.Instance.OnSeedPacketChanged += OnInventoryChanged;
        }

        // Start with inventory tab open, panel closed
        SwitchTab(0);
        inventoryPanel.SetActive(false);
        isOpen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
            PlayerInventory.Instance.OnHarvestedItemChanged -= OnInventoryChanged;
            PlayerInventory.Instance.OnSeedPacketChanged -= OnInventoryChanged;
        }
    }

    /// <summary>
    /// Toggle inventory panel open/closed
    /// </summary>
    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            // Always refresh when opening (don't rely on Start)
            RefreshAllTabs();
        }
    }

    /// <summary>
    /// Switch between tabs (0=Inventory, 1=Money, 2=Shop)
    /// </summary>
    public void SwitchTab(int tabIndex)
    {
        Debug.Log($"[InventoryUI] Switching to tab {tabIndex}");

        // Hide all tabs
        inventoryTabContent.SetActive(false);
        moneyTabContent.SetActive(false);
        shopTabContent.SetActive(false);

        // Reset all button colors
        SetButtonColor(inventoryTabButton, inactiveTabColor);
        SetButtonColor(moneyTabButton, inactiveTabColor);
        SetButtonColor(shopTabButton, inactiveTabColor);

        // Show selected tab and highlight button
        switch (tabIndex)
        {
            case 0: // Inventory
                Debug.Log("[InventoryUI] Activating Inventory Tab");
                inventoryTabContent.SetActive(true);
                SetButtonColor(inventoryTabButton, activeTabColor);
                RefreshInventoryTab();
                break;
            case 1: // Money
                Debug.Log("[InventoryUI] Activating Money Tab");
                moneyTabContent.SetActive(true);
                SetButtonColor(moneyTabButton, activeTabColor);
                RefreshMoneyTab();
                break;
            case 2: // Shop
                Debug.Log("[InventoryUI] Activating Shop Tab");
                shopTabContent.SetActive(true);
                SetButtonColor(shopTabButton, activeTabColor);
                RefreshShopTab();
                break;
        }
    }

    /// <summary>
    /// Set button color
    /// </summary>
    private void SetButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.selectedColor = color;
        button.colors = colors;
    }

    /// <summary>
    /// Refresh all tabs
    /// </summary>
    private void RefreshAllTabs()
    {
        RefreshInventoryTab();
        RefreshMoneyTab();
        RefreshShopTab();
    }

    /// <summary>
    /// Refresh Inventory Tab
    /// </summary>
    private void RefreshInventoryTab()
    {
        Debug.Log("[InventoryUI] Refreshing Inventory Tab");

        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[InventoryUI] PlayerInventory.Instance is NULL!");
            return;
        }

        Debug.Log($"[InventoryUI] Player has {PlayerInventory.Instance.SeedPackets.Count} seed types");
        Debug.Log($"[InventoryUI] Player has {PlayerInventory.Instance.HarvestedItems.Count} crop types");

        // Clear existing items
        ClearContainer(seedsContentParent);
        ClearContainer(cropsContentParent);

        // Populate seeds
        foreach (var kvp in PlayerInventory.Instance.SeedPackets)
        {
            SeedPacket packet = kvp.Key;
            int quantity = kvp.Value;

            Debug.Log($"[InventoryUI] Creating slot for {packet.cropName} x{quantity}");

            GameObject slot = Instantiate(inventoryItemSlotPrefab, seedsContentParent);
            InventoryItemSlot slotScript = slot.GetComponent<InventoryItemSlot>();
            if (slotScript != null)
            {
                slotScript.Setup(packet.coverImage, packet.cropName, quantity, packet.packetCost);
            }
            else
            {
                Debug.LogError("[InventoryUI] InventoryItemSlot script not found on prefab!");
            }
        }

        // Populate harvested crops
        foreach (var kvp in PlayerInventory.Instance.HarvestedItems)
        {
            InventoryItem item = kvp.Key;
            int quantity = kvp.Value;

            Debug.Log($"[InventoryUI] Creating slot for {item.itemName} x{quantity}");

            GameObject slot = Instantiate(inventoryItemSlotPrefab, cropsContentParent);
            InventoryItemSlot slotScript = slot.GetComponent<InventoryItemSlot>();
            if (slotScript != null)
            {
                slotScript.Setup(item.itemIcon, item.itemName, quantity, item.sellValue);
            }
            else
            {
                Debug.LogError("[InventoryUI] InventoryItemSlot script not found on prefab!");
            }
        }
    }

    /// <summary>
    /// Refresh Money Tab
    /// </summary>
    private void RefreshMoneyTab()
    {
        UpdateMoneyDisplay(PlayerInventory.Instance.CurrentMoney);

        // TODO: Add stats display here later if you want
    }

    /// <summary>
    /// Refresh Shop Tab
    /// </summary>
    private void RefreshShopTab()
    {
        if (PlayerInventory.Instance == null) return;

        // Clear existing items
        ClearContainer(buyContentParent);
        ClearContainer(sellContentParent);

        // TODO: We'll populate shop items in the next step when we create ShopInventory
        // For now, we can show all available seed packets and harvested items

        // Temporary: Show all seed packets that are available in shop
        // You'll replace this with ShopInventory data later
    }

    /// <summary>
    /// Update money display text
    /// </summary>
    private void UpdateMoneyDisplay(int amount)
    {
        if (currentMoneyText != null)
        {
            currentMoneyText.text = $"Money: ${amount}";
        }
    }

    /// <summary>
    /// Called when inventory changes
    /// </summary>
    private void OnInventoryChanged(InventoryItem item, int quantity)
    {
        if (isOpen && inventoryTabContent.activeSelf)
        {
            RefreshInventoryTab();
        }
    }

    /// <summary>
    /// Called when seed packets change
    /// </summary>
    private void OnInventoryChanged(SeedPacket packet, int quantity)
    {
        if (isOpen && inventoryTabContent.activeSelf)
        {
            RefreshInventoryTab();
        }
    }

    /// <summary>
    /// Clear all children from a container
    /// </summary>
    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}