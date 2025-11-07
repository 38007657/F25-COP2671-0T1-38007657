using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player's seed inventory and selection
/// </summary>
public class SeedInventory : MonoBehaviour
{
    public static SeedInventory Instance { get; private set; }

    [Header("Available Seeds")]
    [SerializeField] private List<SeedPacket> allSeeds = new List<SeedPacket>();

    [Header("Current Selection")]
    private int selectedSeedIndex = 0;

    // Properties
    public SeedPacket SelectedSeed => allSeeds.Count > 0 && selectedSeedIndex < allSeeds.Count
        ? allSeeds[selectedSeedIndex]
        : null;

    public int SelectedIndex => selectedSeedIndex;
    public List<SeedPacket> AllSeeds => allSeeds;

    // Events
    public delegate void SeedSelectionChanged(SeedPacket newSeed, int index);
    public event SeedSelectionChanged OnSeedChanged;

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

    /// <summary>
    /// Select seed by index (called from UI)
    /// </summary>
    public void SelectSeed(int index)
    {
        if (index < 0 || index >= allSeeds.Count) return;

        selectedSeedIndex = index;
        OnSeedChanged?.Invoke(SelectedSeed, selectedSeedIndex);

        Debug.Log($"[SeedInventory] Selected: {SelectedSeed.cropName}");
    }

    /// <summary>
    /// Select seed by SeedPacket reference (called from UI)
    /// </summary>
    public void SelectSeed(SeedPacket seed)
    {
        int index = allSeeds.IndexOf(seed);
        if (index >= 0)
        {
            SelectSeed(index);
        }
    }

    /// <summary>
    /// Add a new seed type to inventory
    /// </summary>
    public void AddSeedType(SeedPacket seed)
    {
        if (!allSeeds.Contains(seed))
        {
            allSeeds.Add(seed);
            Debug.Log($"[SeedInventory] Added seed type: {seed.cropName}");
        }
    }

    /// <summary>
    /// Remove a seed type from inventory
    /// </summary>
    public void RemoveSeedType(SeedPacket seed)
    {
        allSeeds.Remove(seed);
    }
}