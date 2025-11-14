using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player's seed inventory and selection
/// UPDATED: Stores selected seed directly instead of by index
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
    /// Select seed by SeedPacket reference (called from UI)
    /// This is the PRIMARY method now - no more index confusion!
    /// </summary>
    public void SelectSeed(SeedPacket seed)
    {
        if (seed == null)
        {
            Debug.LogWarning("[SeedInventory] Tried to select null seed!");
            return;
        }

        selectedSeed = seed;

        // Fire event with -1 index since we're not using indexes anymore
        OnSeedChanged?.Invoke(selectedSeed, -1);

        Debug.Log($"[SeedInventory] Selected: {selectedSeed.cropName}");
    }

    /// <summary>
    /// Select seed by index (DEPRECATED - kept for backwards compatibility)
    /// </summary>
    public void SelectSeed(int index)
    {
        Debug.LogWarning("[SeedInventory] SelectSeed(int) is deprecated - use SelectSeed(SeedPacket) instead");

        // This method is no longer reliable since we don't maintain a master list
        // If you need to use it, you must pass the actual SeedPacket reference
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