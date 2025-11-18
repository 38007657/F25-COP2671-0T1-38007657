using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player's inventory - both seed packets and harvested items
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Starting Money")]
    [SerializeField] private int startingMoney = 500;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // === NEW: Testing seeds ===
    [Header("Testing - Starting Seeds")]
    [Tooltip("Drag your SeedPacket scriptable objects here for testing")]
    [SerializeField] private List<SeedPacket> testStartingSeeds = new List<SeedPacket>();

    [Tooltip("How many of each test seed to start with")]
    [SerializeField] private int testSeedsPerPacket = 10;

    // Player's money
    private int currentMoney;

    // Inventory storage
    // Key = InventoryItem, Value = quantity
    private Dictionary<InventoryItem, int> harvestedItems = new Dictionary<InventoryItem, int>();

    // Key = SeedPacket, Value = number of seed packets owned
    private Dictionary<SeedPacket, int> seedPackets = new Dictionary<SeedPacket, int>();

    // Properties
    public int CurrentMoney => currentMoney;
    public Dictionary<InventoryItem, int> HarvestedItems => harvestedItems;
    public Dictionary<SeedPacket, int> SeedPackets => seedPackets;

    // Events for UI to listen to
    public System.Action<int> OnMoneyChanged;
    public System.Action<InventoryItem, int> OnHarvestedItemChanged;
    public System.Action<SeedPacket, int> OnSeedPacketChanged;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentMoney = startingMoney;
        OnMoneyChanged?.Invoke(currentMoney);

        // === TESTING: Give player starting seeds ===
        if (testStartingSeeds.Count > 0)
        {
            foreach (SeedPacket packet in testStartingSeeds)
            {
                if (packet != null)
                {
                    AddSeedPacket(packet, testSeedsPerPacket);
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInventory] Added {testStartingSeeds.Count} test seed types with {testSeedsPerPacket} seeds each");
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] Initialized with ${currentMoney}");
        }
    }

    /// <summary>
    /// Initialize a completely new game (called when New Game is clicked)
    /// </summary>
    public void InitializeNewGame()
    {
        // Clear everything
        ClearInventory();

        // Reset to starting money
        currentMoney = startingMoney;
        OnMoneyChanged?.Invoke(currentMoney);

        // Add starting seeds
        if (testStartingSeeds.Count > 0)
        {
            foreach (SeedPacket packet in testStartingSeeds)
            {
                if (packet != null)
                {
                    AddSeedPacket(packet, testSeedsPerPacket);
                }
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] New game initialized with ${currentMoney} and {testStartingSeeds.Count} seed types");
        }
    }

    // ===== HARVESTED ITEMS =====

    /// <summary>
    /// Add harvested crops to inventory
    /// </summary>
    public void AddHarvestedItem(InventoryItem item, int quantity)
    {
        if (item == null)
        {
            Debug.LogWarning("[PlayerInventory] Tried to add null item!");
            return;
        }

        if (harvestedItems.ContainsKey(item))
        {
            harvestedItems[item] += quantity;
        }
        else
        {
            harvestedItems[item] = quantity;
        }

        OnHarvestedItemChanged?.Invoke(item, harvestedItems[item]);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] Added {quantity}x {item.itemName}. Total: {harvestedItems[item]}");
        }
    }

    /// <summary>
    /// Remove harvested items (when selling)
    /// </summary>
    public bool RemoveHarvestedItem(InventoryItem item, int quantity)
    {
        if (item == null || !harvestedItems.ContainsKey(item))
        {
            return false;
        }

        if (harvestedItems[item] < quantity)
        {
            return false; // Not enough
        }

        harvestedItems[item] -= quantity;

        if (harvestedItems[item] <= 0)
        {
            harvestedItems.Remove(item);
        }

        OnHarvestedItemChanged?.Invoke(item, harvestedItems.ContainsKey(item) ? harvestedItems[item] : 0);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] Removed {quantity}x {item.itemName}");
        }

        return true;
    }

    /// <summary>
    /// Get quantity of a harvested item
    /// </summary>
    public int GetHarvestedItemCount(InventoryItem item)
    {
        return harvestedItems.ContainsKey(item) ? harvestedItems[item] : 0;
    }

    // ===== SEED PACKETS =====

    /// <summary>
    /// Add seed packets to inventory (when buying from shop)
    /// </summary>
    public void AddSeedPacket(SeedPacket packet, int quantity)
    {
        if (packet == null)
        {
            Debug.LogWarning("[PlayerInventory] Tried to add null seed packet!");
            return;
        }

        if (seedPackets.ContainsKey(packet))
        {
            seedPackets[packet] += quantity;
        }
        else
        {
            seedPackets[packet] = quantity;
        }

        OnSeedPacketChanged?.Invoke(packet, seedPackets[packet]);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] Added {quantity}x {packet.cropName} seed packet(s). Total: {seedPackets[packet]}");
        }
    }

    /// <summary>
    /// Remove seed packet (when planting - removes 1 seed from packet)
    /// </summary>
    public bool UseSeedFromPacket(SeedPacket packet)
    {
        if (packet == null || !seedPackets.ContainsKey(packet))
        {
            return false;
        }

        if (seedPackets[packet] <= 0)
        {
            return false;
        }

        // Remove one seed from the packet
        // Note: We're tracking individual seeds, not packets
        // If you want to track packets instead, adjust this logic
        seedPackets[packet]--;

        if (seedPackets[packet] <= 0)
        {
            seedPackets.Remove(packet);
        }

        OnSeedPacketChanged?.Invoke(packet, seedPackets.ContainsKey(packet) ? seedPackets[packet] : 0);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] Used 1 seed from {packet.cropName} packet. Remaining: {(seedPackets.ContainsKey(packet) ? seedPackets[packet] : 0)}");
        }

        return true;
    }

    /// <summary>
    /// Get quantity of seed packets
    /// </summary>
    public int GetSeedPacketCount(SeedPacket packet)
    {
        return seedPackets.ContainsKey(packet) ? seedPackets[packet] : 0;
    }

    // ===== MONEY =====

    /// <summary>
    /// Add money to player's account
    /// </summary>
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] Added ${amount}. New balance: ${currentMoney}");
        }
    }

    /// <summary>
    /// Remove money from player's account
    /// </summary>
    public bool RemoveMoney(int amount)
    {
        if (currentMoney < amount)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInventory] Not enough money! Need ${amount}, have ${currentMoney}");
            }
            return false;
        }

        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] Removed ${amount}. New balance: ${currentMoney}");
        }

        return true;
    }

    /// <summary>
    /// Check if player can afford something
    /// </summary>
    public bool CanAfford(int cost)
    {
        return currentMoney >= cost;
    }

    // ===== SELLING TO SHOP =====

    /// <summary>
    /// Sell harvested items to shop
    /// </summary>
    public bool SellHarvestedItem(InventoryItem item, int quantity)
    {
        if (!item.isAvailableInShop)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInventory] Shop doesn't buy {item.itemName}");
            }
            return false;
        }

        if (RemoveHarvestedItem(item, quantity))
        {
            int totalValue = item.sellValue * quantity;
            AddMoney(totalValue);

            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInventory] Sold {quantity}x {item.itemName} for ${totalValue}");
            }

            return true;
        }

        return false;
    }

    // ===== BUYING FROM SHOP =====

    /// <summary>
    /// Buy seed packet from shop
    /// </summary>
    public bool BuySeedPacket(SeedPacket packet)
    {
        if (!packet.isAvailableInShop)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInventory] {packet.cropName} is not available in shop");
            }
            return false;
        }

        if (!CanAfford(packet.packetCost))
        {
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInventory] Can't afford {packet.cropName} seeds (${packet.packetCost})");
            }
            return false;
        }

        if (RemoveMoney(packet.packetCost))
        {
            // Add the number of seeds in the packet
            AddSeedPacket(packet, packet.seedsPerPacket);

            if (showDebugLogs)
            {
                Debug.Log($"[PlayerInventory] Bought {packet.cropName} seed packet with {packet.seedsPerPacket} seeds for ${packet.packetCost}");
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Set money directly (for loading saves)
    /// </summary>
    public void SetMoney(int amount)
    {
        currentMoney = amount;
        OnMoneyChanged?.Invoke(currentMoney);

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInventory] Set money to ${amount}");
        }
    }

    /// <summary>
    /// Clear all inventory (for loading saves)
    /// </summary>
    public void ClearInventory()
    {
        harvestedItems.Clear();
        seedPackets.Clear();

        if (showDebugLogs)
        {
            Debug.Log("[PlayerInventory] Cleared inventory");
        }
    }

    // ===== DEBUG =====

    [ContextMenu("Debug Inventory")]
    public void DebugInventory()
    {
        Debug.Log("=== PLAYER INVENTORY ===");
        Debug.Log($"Money: ${currentMoney}");
        Debug.Log($"Seed Packets: {seedPackets.Count} types");
        foreach (var kvp in seedPackets)
        {
            Debug.Log($"  - {kvp.Key.cropName}: {kvp.Value} seeds");
        }
        Debug.Log($"Harvested Items: {harvestedItems.Count} types");
        foreach (var kvp in harvestedItems)
        {
            Debug.Log($"  - {kvp.Key.itemName}: {kvp.Value}");
        }
    }
}