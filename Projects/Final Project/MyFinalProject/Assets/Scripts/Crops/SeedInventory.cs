using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player's seed inventory and selection
/// </summary>
public class SeedInventory : MonoBehaviour
{
    public static SeedInventory Instance { get; private set; }

    [Header("Current Selection")]
    private SeedPacket selectedSeed = null;

    // Properties
    public SeedPacket SelectedSeed => selectedSeed;

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
    /// Select seed by SeedPacket reference
    /// </summary>
    public void SelectSeed(SeedPacket seed)
    {
        if (seed == null)
        {
            Debug.LogWarning("[SeedInventory] Tried to select null seed!");
            return;
        }

        selectedSeed = seed;

        OnSeedChanged?.Invoke(selectedSeed, -1);

        Debug.Log($"[SeedInventory] Selected: {selectedSeed.cropName}");
    }

    /// <summary>
    /// Clear the current selection
    /// </summary>
    public void ClearSelection()
    {
        selectedSeed = null;
        OnSeedChanged?.Invoke(null, -1);
        Debug.Log("[SeedInventory] Selection cleared");
    }

    /// <summary>
    /// Check if a specific seed is currently selected
    /// </summary>
    public bool IsSelected(SeedPacket seed)
    {
        return selectedSeed == seed;
    }
}