using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Simple adapter to use Tilemap visuals with your existing FarmPlot system
/// Just attach this to your scene and assign the tilemap + tiles
/// Your existing farming logic stays exactly the same!
/// </summary>
public class FarmPlotTilemapVisuals : MonoBehaviour
{
    [Header("Tilemap Reference")]
    [SerializeField] private Tilemap farmingTilemap;

    [Header("Tile Visuals")]
    [Tooltip("Leave null to hide unhoed plots (recommended)")]
    [SerializeField] private TileBase unhoedTile = null;

    [Tooltip("Brown dirt tile for hoed soil")]
    [SerializeField] private TileBase hoedDryTile;

    [Tooltip("Dark/wet dirt tile for watered soil")]
    [SerializeField] private TileBase hoedWetTile;

    [Header("Settings")]
    [SerializeField] private bool syncOnStart = true;
    [SerializeField] private float updateInterval = 0.5f; // Update every 0.5 seconds

    private FarmPlotManager plotManager;
    private float updateTimer = 0f;

    private void Start()
    {
        plotManager = FarmPlotManager.Instance;

        if (plotManager == null)
        {
            Debug.LogError("[TilemapVisuals] FarmPlotManager not found!");
            enabled = false;
            return;
        }

        if (farmingTilemap == null)
        {
            Debug.LogError("[TilemapVisuals] Farming Tilemap not assigned!");
            enabled = false;
            return;
        }

        if (syncOnStart)
        {
            SyncAllPlots();
        }
    }

    private void Update()
    {
        // Periodic sync to catch any changes
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            SyncAllPlots();
        }
    }

    /// <summary>
    /// Sync all plots with tilemap visuals
    /// </summary>
    [ContextMenu("Sync All Plots Now")]
    public void SyncAllPlots()
    {
        if (plotManager == null || farmingTilemap == null) return;

        foreach (FarmPlot plot in plotManager.GetAllPlots())
        {
            UpdateTilemapForPlot(plot);
        }
    }

    /// <summary>
    /// Update tilemap visual for a specific plot
    /// Call this from FarmPlot when state changes (optional optimization)
    /// </summary>
    public void UpdateTilemapForPlot(FarmPlot plot)
    {
        if (farmingTilemap == null || plot == null) return;

        // Convert world position to tile position
        Vector3Int tilePos = farmingTilemap.WorldToCell(plot.WorldPosition);

        // Determine which tile to show based on plot state
        TileBase tileToShow = null;

        if (!plot.IsHoed)
        {
            // Unhoed - use unhoed tile or null (to show base terrain)
            tileToShow = unhoedTile;
        }
        else if (plot.IsWet)
        {
            // Watered soil - dark/wet tile
            tileToShow = hoedWetTile;
        }
        else
        {
            // Hoed but dry - brown dirt tile
            tileToShow = hoedDryTile;
        }

        // Update the tilemap
        farmingTilemap.SetTile(tilePos, tileToShow);
    }

    /// <summary>
    /// Manual sync for a specific plot (call this from your FarmPlot if you want instant updates)
    /// </summary>
    public void SyncPlot(Vector2Int gridPosition)
    {
        if (plotManager == null) return;

        FarmPlot plot = plotManager.GetPlotAtPosition(gridPosition);
        if (plot != null)
        {
            UpdateTilemapForPlot(plot);
        }
    }

    /// <summary>
    /// Clear all tilemap visuals (useful for testing)
    /// </summary>
    [ContextMenu("Clear All Tiles")]
    public void ClearAllTiles()
    {
        if (farmingTilemap == null) return;

        farmingTilemap.ClearAllTiles();
        Debug.Log("[TilemapVisuals] Cleared all tiles");
    }
}