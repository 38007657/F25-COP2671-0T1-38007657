using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages shop inventory and transactions
/// </summary>
public class ShopInventory : MonoBehaviour
{
    public static ShopInventory Instance { get; private set; }

    [Header("Available Seeds for Sale")]
    [SerializeField] private List<SeedPacket> availableSeedPackets = new List<SeedPacket>();

    [Header("Crops Shop Will Buy")]
    [SerializeField] private List<InventoryItem> buyableCrops = new List<InventoryItem>();

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Events for UI updates
    public System.Action OnShopInventoryChanged;

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
        if (showDebugLogs)
        {
            Debug.Log($"[ShopInventory] Initialized with {availableSeedPackets.Count} seed types for sale");
            Debug.Log($"[ShopInventory] Shop buys {buyableCrops.Count} crop types");
        }
    }

    // ===== BUYING SEEDS FROM SHOP =====

    /// <summary>
    /// Get all seed packets available for purchase
    /// </summary>
    public List<SeedPacket> GetAvailableSeedPackets()
    {
        // Filter by isAvailableInShop flag
        List<SeedPacket> available = new List<SeedPacket>();
        foreach (SeedPacket packet in availableSeedPackets)
        {
            if (packet != null && packet.isAvailableInShop)
            {
                available.Add(packet);
            }
        }
        return available;
    }

    /// <summary>
    /// Player buys a seed packet from shop
    /// </summary>
    public bool BuySeedPacket(SeedPacket packet)
    {
        if (packet == null)
        {
            Debug.LogWarning("[ShopInventory] Tried to buy null packet!");
            return false;
        }

        if (!packet.isAvailableInShop)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[ShopInventory] {packet.cropName} is not available for purchase");
            }
            return false;
        }

        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[ShopInventory] PlayerInventory not found!");
            return false;
        }

        // Use PlayerInventory's buy method (already handles money and adding seeds)
        bool success = PlayerInventory.Instance.BuySeedPacket(packet);

        if (success)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[ShopInventory] Sold {packet.cropName} seed packet to player for ${packet.packetCost}");
            }
            OnShopInventoryChanged?.Invoke();
        }

        return success;
    }

    // ===== SELLING CROPS TO SHOP =====

    /// <summary>
    /// Get all crops the shop will buy
    /// </summary>
    public List<InventoryItem> GetBuyableCrops()
    {
        // Filter by isAvailableInShop flag
        List<InventoryItem> buyable = new List<InventoryItem>();
        foreach (InventoryItem item in buyableCrops)
        {
            if (item != null && item.isAvailableInShop)
            {
                buyable.Add(item);
            }
        }
        return buyable;
    }

    /// <summary>
    /// Check if shop will buy this crop
    /// </summary>
    public bool WillBuyCrop(InventoryItem crop)
    {
        return crop != null && crop.isAvailableInShop && buyableCrops.Contains(crop);
    }

    /// <summary>
    /// Player sells crops to shop
    /// </summary>
    public bool SellCropToShop(InventoryItem crop, int quantity)
    {
        if (crop == null)
        {
            Debug.LogWarning("[ShopInventory] Tried to sell null crop!");
            return false;
        }

        if (!WillBuyCrop(crop))
        {
            if (showDebugLogs)
            {
                Debug.Log($"[ShopInventory] Shop doesn't buy {crop.itemName}");
            }
            return false;
        }

        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[ShopInventory] PlayerInventory not found!");
            return false;
        }

        // Use PlayerInventory's sell method (already handles removing items and adding money)
        bool success = PlayerInventory.Instance.SellHarvestedItem(crop, quantity);

        if (success)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[ShopInventory] Bought {quantity}x {crop.itemName} from player for ${crop.sellValue * quantity}");
            }
            OnShopInventoryChanged?.Invoke();
        }

        return success;
    }

    // ===== SHOP MANAGEMENT =====

    /// <summary>
    /// Add a seed packet to shop inventory
    /// </summary>
    public void AddSeedPacketToShop(SeedPacket packet)
    {
        if (packet != null && !availableSeedPackets.Contains(packet))
        {
            availableSeedPackets.Add(packet);
            OnShopInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Remove a seed packet from shop inventory
    /// </summary>
    public void RemoveSeedPacketFromShop(SeedPacket packet)
    {
        if (availableSeedPackets.Contains(packet))
        {
            availableSeedPackets.Remove(packet);
            OnShopInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Add a crop type the shop will buy
    /// </summary>
    public void AddBuyableCrop(InventoryItem crop)
    {
        if (crop != null && !buyableCrops.Contains(crop))
        {
            buyableCrops.Add(crop);
            OnShopInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Remove a crop type from shop's buy list
    /// </summary>
    public void RemoveBuyableCrop(InventoryItem crop)
    {
        if (buyableCrops.Contains(crop))
        {
            buyableCrops.Remove(crop);
            OnShopInventoryChanged?.Invoke();
        }
    }
}