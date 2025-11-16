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

    [Header("Shop Sell Controller")]
    [SerializeField] private ShopSellController shopSellController;

    [Header("Main Panel")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    [Header("Tab Buttons")]
    [SerializeField] private Button inventoryTabButton;
    [SerializeField] private Button moneyTabButton;
    [SerializeField] private Button shopTabButton;
    [SerializeField] private Button saveLoadTabButton;

    [Header("Tab Contents")]
    [SerializeField] private GameObject inventoryTabContent;
    [SerializeField] private GameObject moneyTabContent;
    [SerializeField] private GameObject shopTabContent;
    [SerializeField] private GameObject saveLoadTabContent;

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

    // Public property to check if inventory is open
    public bool IsOpen => isOpen;

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
        saveLoadTabButton.onClick.AddListener(() => SwitchTab(3));

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
            // Pause the game when inventory opens
            Time.timeScale = 0f;

            // Always refresh when opening (don't rely on Start)
            RefreshAllTabs();
        }
        else
        {
            // Resume the game when inventory closes (only if not in pause menu)
            if (PauseMenuManager.Instance == null || !PauseMenuManager.Instance.IsPaused)
            {
                Time.timeScale = 1f;
            }
        }
    }

    public void SwitchTab(int tabIndex)
    {
        UnityEngine.Debug.Log($"[InventoryUI] Switching to tab {tabIndex}");

        // Hide all tabs
        inventoryTabContent.SetActive(false);
        moneyTabContent.SetActive(false);
        shopTabContent.SetActive(false);
        saveLoadTabContent.SetActive(false); // â† MAKE SURE THIS LINE EXISTS

        // Reset all button colors
        SetButtonColor(inventoryTabButton, inactiveTabColor);
        SetButtonColor(moneyTabButton, inactiveTabColor);
        SetButtonColor(shopTabButton, inactiveTabColor);
        SetButtonColor(saveLoadTabButton, inactiveTabColor); // â† MAKE SURE THIS LINE EXISTS

        // Show selected tab and highlight button
        switch (tabIndex)
        {
            case 0: // Inventory
                inventoryTabContent.SetActive(true);
                SetButtonColor(inventoryTabButton, activeTabColor);
                RefreshInventoryTab();
                break;

            case 1: // Money
                moneyTabContent.SetActive(true);
                SetButtonColor(moneyTabButton, activeTabColor);
                RefreshMoneyTab();
                break;

            case 2: // Shop
                shopTabContent.SetActive(true);
                SetButtonColor(shopTabButton, activeTabColor);
                RefreshShopTab();
                break;

            case 3: // Save/Load â† MAKE SURE THIS CASE EXISTS
                UnityEngine.Debug.Log("[InventoryUI] Activating SaveLoadTabContent");
                saveLoadTabContent.SetActive(true);
                UnityEngine.Debug.Log($"[InventoryUI] SaveLoadTabContent.activeSelf = {saveLoadTabContent.activeSelf}");
                SetButtonColor(saveLoadTabButton, activeTabColor);
                break;

            default:
                UnityEngine.Debug.LogWarning($"[InventoryUI] Invalid tab index: {tabIndex}");
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
    /// Refresh whichever tab is currently active
    /// </summary>
    public void RefreshCurrentTab()
    {
        // Determine which tab is active and refresh it
        if (inventoryTabContent.activeSelf)
        {
            RefreshInventoryTab();
        }
        else if (moneyTabContent.activeSelf)
        {
            RefreshMoneyTab();
        }
        else if (shopTabContent.activeSelf)
        {
            RefreshShopTab();
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

        // Populate seeds - DON'T show value (we'll buy them in shop)
        foreach (var kvp in PlayerInventory.Instance.SeedPackets)
        {
            SeedPacket packet = kvp.Key;
            int quantity = kvp.Value;

            Debug.Log($"[InventoryUI] Creating slot for {packet.cropName} x{quantity}");

            GameObject slot = Instantiate(inventoryItemSlotPrefab, seedsContentParent);
            InventoryItemSlot slotScript = slot.GetComponent<InventoryItemSlot>();
            if (slotScript != null)
            {
                // Last parameter 'false' = don't show value
                slotScript.Setup(packet.coverImage, packet.cropName, quantity, packet.packetCost, showValue: false);
            }
            else
            {
                Debug.LogError("[InventoryUI] InventoryItemSlot script not found on prefab!");
            }
        }

        // Populate harvested crops - SHOW value (what we can sell them for)
        foreach (var kvp in PlayerInventory.Instance.HarvestedItems)
        {
            InventoryItem item = kvp.Key;
            int quantity = kvp.Value;

            Debug.Log($"[InventoryUI] Creating slot for {item.itemName} x{quantity}");

            GameObject slot = Instantiate(inventoryItemSlotPrefab, cropsContentParent);
            InventoryItemSlot slotScript = slot.GetComponent<InventoryItemSlot>();
            if (slotScript != null)
            {
                // Last parameter 'true' = show value (sell price)
                slotScript.Setup(item.itemIcon, item.itemName, quantity, item.sellValue, showValue: true);
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
        if (ShopInventory.Instance == null)
        {
            Debug.LogWarning("[InventoryUI] ShopInventory not found!");
            return;
        }

        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("[InventoryUI] PlayerInventory not found!");
            return;
        }

        Debug.Log("[InventoryUI] Refreshing Shop Tab");

        // Clear existing items
        ClearContainer(buyContentParent);
        ClearContainer(sellContentParent);

        // Clear sell controller slots FIRST
        if (shopSellController != null)
        {
            shopSellController.ClearSellSlots();
        }

        // === POPULATE BUY SECTION (Seeds for sale) ===
        List<SeedPacket> availableSeeds = ShopInventory.Instance.GetAvailableSeedPackets();

        Debug.Log($"[InventoryUI] Shop has {availableSeeds.Count} seed types for sale");

        foreach (SeedPacket packet in availableSeeds)
        {
            GameObject slot = Instantiate(shopItemBuySlotPrefab, buyContentParent);
            ShopItemBuySlot slotScript = slot.GetComponent<ShopItemBuySlot>();

            if (slotScript != null)
            {
                slotScript.Setup(packet);
                Debug.Log($"[InventoryUI] Created buy slot for {packet.cropName}");
            }
            else
            {
                Debug.LogError("[InventoryUI] ShopItemBuySlot script not found on prefab!");
            }
        }

        // === POPULATE SELL SECTION (Player's harvested crops) ===
        // Only show crops that the shop will buy AND player actually has
        List<InventoryItem> buyableCrops = ShopInventory.Instance.GetBuyableCrops();

        Debug.Log($"[InventoryUI] Shop buys {buyableCrops.Count} crop types");

        foreach (InventoryItem crop in buyableCrops)
        {
            // Check if player has any of this crop
            int playerQuantity = PlayerInventory.Instance.GetHarvestedItemCount(crop);

            // Only show if player has at least 1
            if (playerQuantity > 0)
            {
                GameObject slot = Instantiate(shopItemSellSlotPrefab, sellContentParent);
                ShopItemSellSlot slotScript = slot.GetComponent<ShopItemSellSlot>();

                if (slotScript != null)
                {
                    slotScript.Setup(crop, playerQuantity);

                    // â† THIS IS THE KEY FIX - Register the slot with the controller
                    if (shopSellController != null)
                    {
                        shopSellController.RegisterSellSlot(slotScript);
                    }

                    Debug.Log($"[InventoryUI] Created sell slot for {crop.itemName} (player has {playerQuantity})");
                }
                else
                {
                    Debug.LogError("[InventoryUI] ShopItemSellSlot script not found on prefab!");
                }
            }
        }

        // Update money display
        UpdateMoneyDisplay(PlayerInventory.Instance.CurrentMoney);
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
}