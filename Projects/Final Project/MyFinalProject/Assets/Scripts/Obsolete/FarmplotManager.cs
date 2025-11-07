using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all farm plots and handles crop planting/interaction
/// </summary>
public class FarmPlotManager : MonoBehaviour
{
    public static FarmPlotManager Instance { get; private set; }

    [Header("Plot Configuration")]
    [SerializeField] private List<FarmPlot> farmPlots = new List<FarmPlot>();

    [Header("Plot Sprites")]
    [SerializeField] private Sprite unhoedSprite; // Grass/dirt (not used - plots invisible when unhoed)
    [SerializeField] private Sprite hoedDrySprite; // Tilled soil (dry)
    [SerializeField] private Sprite hoedWetSprite; // Tilled soil (wet/dark)
    [SerializeField] private string plotSortingLayer = "Default"; // Sorting layer for plot sprites
    [SerializeField] private int plotSortingOrder = -100; // Sorting order within layer

    [Header("Crop Prefab")]
    [SerializeField] private GameObject cropPrefab; // Base crop prefab

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 1.5f;
    private float lastDryingCheck = 0f;

    [Header("Debug")]
    [SerializeField] private bool showPlotGizmos = true;
    [SerializeField] private bool showDebugInfo = true;

    // Dictionary for fast lookup
    private Dictionary<Vector2Int, FarmPlot> plotLookup = new Dictionary<Vector2Int, FarmPlot>();

    // Container for plot visual markers
    private Transform plotMarkersContainer;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Create container for plot markers with proper setup
        GameObject markersContainerObj = new GameObject("PlotMarkers");
        plotMarkersContainer = markersContainerObj.transform;
        plotMarkersContainer.SetParent(transform);
        plotMarkersContainer.localPosition = Vector3.zero; // Ensure it's at origin relative to manager
        plotMarkersContainer.localScale = Vector3.one; // Ensure scale is correct

        InitializePlots();
    }

    private void Update()
    {
        // Update drying state once per second instead of every frame
        if (Time.time - lastDryingCheck > 1f)
        {
            foreach (FarmPlot plot in farmPlots)
            {
                plot.UpdateDryingState();
            }
            lastDryingCheck = Time.time;
        }
    }

    /// <summary>
    /// Initialize all farm plot positions (called from Awake or via Inspector button)
    /// </summary>
    [ContextMenu("Initialize Farm Plots")]
    private void InitializePlots()
    {
        farmPlots.Clear();
        plotLookup.Clear();

        // All 66 plot positions with your updated coordinates
        Vector2Int[] plotPositions = new Vector2Int[]
        {
            // Top-left section (3 rows of 6)
            new Vector2Int(-21, 9), new Vector2Int(-20, 9), new Vector2Int(-19, 9),
            new Vector2Int(-18, 9), new Vector2Int(-17, 9), new Vector2Int(-16, 9),

            new Vector2Int(-21, 6), new Vector2Int(-20, 6), new Vector2Int(-19, 6),
            new Vector2Int(-18, 6), new Vector2Int(-17, 6), new Vector2Int(-16, 6),

            new Vector2Int(-21, 3), new Vector2Int(-20, 3), new Vector2Int(-19, 3),
            new Vector2Int(-18, 3), new Vector2Int(-17, 3), new Vector2Int(-16, 3),
            
            // Left vertical strips (2 columns of 5 + 2 new)
            new Vector2Int(-19, 0), new Vector2Int(-19, -1), new Vector2Int(-19, -2),
            new Vector2Int(-19, -3), new Vector2Int(-19, -4),

            new Vector2Int(-16, 0), new Vector2Int(-16, -1), new Vector2Int(-16, -2),
            new Vector2Int(-16, -3), new Vector2Int(-16, -4),
            
            // NEW: Additional left column plots
            new Vector2Int(-22, 0), new Vector2Int(-22, -3),
            
            // Top-right section UPDATED (2 rows of 11 - changed from -10 to -9 start)
            new Vector2Int(-9, 9), new Vector2Int(-8, 9), new Vector2Int(-7, 9),
            new Vector2Int(-6, 9), new Vector2Int(-5, 9), new Vector2Int(-4, 9),
            new Vector2Int(-3, 9), new Vector2Int(-2, 9), new Vector2Int(-1, 9),
            new Vector2Int(0, 9), new Vector2Int(1, 9),

            new Vector2Int(-9, 6), new Vector2Int(-8, 6), new Vector2Int(-7, 6),
            new Vector2Int(-6, 6), new Vector2Int(-5, 6), new Vector2Int(-4, 6),
            new Vector2Int(-3, 6), new Vector2Int(-2, 6), new Vector2Int(-1, 6),
            new Vector2Int(0, 6), new Vector2Int(1, 6),
            
            // Bottom-middle section (2 rows of 6)
            new Vector2Int(-5, 1), new Vector2Int(-4, 1), new Vector2Int(-3, 1),
            new Vector2Int(-2, 1), new Vector2Int(-1, 1), new Vector2Int(0, 1),

            new Vector2Int(-5, -2), new Vector2Int(-4, -2), new Vector2Int(-3, -2),
            new Vector2Int(-2, -2), new Vector2Int(-1, -2), new Vector2Int(0, -2),
        };

        // Create FarmPlot objects with 0.5 offset to center in cells
        foreach (Vector2Int pos in plotPositions)
        {
            FarmPlot plot = new FarmPlot(pos);
            farmPlots.Add(plot);
            plotLookup[pos] = plot;

            // Create visual marker for this plot
            plot.CreateVisualMarker(plotMarkersContainer);
        }

        Debug.Log($"[FarmPlotManager] Initialized {farmPlots.Count} farm plots");
    }

    /// <summary>
    /// Get appropriate sprite for plot state
    /// </summary>
    public Sprite GetPlotSprite(bool isHoed, bool isWet, bool isOccupied)
    {
        if (!isHoed) return null; // Unhoed plots are invisible

        return isWet ? hoedWetSprite : hoedDrySprite;
    }

    /// <summary>
    /// Get sorting layer name for plot sprites
    /// </summary>
    public string GetPlotSortingLayer()
    {
        return plotSortingLayer;
    }

    /// <summary>
    /// Get sorting order for plot sprites
    /// </summary>
    public int GetPlotSortingOrder()
    {
        return plotSortingOrder;
    }

    /// <summary>
    /// Get plot markers container
    /// </summary>
    public Transform GetPlotMarkersContainer()
    {
        return plotMarkersContainer;
    }

    /// <summary>
    /// Get all plots (for iteration/filtering)
    /// </summary>
    public List<FarmPlot> GetAllPlots()
    {
        return farmPlots;
    }

    /// <summary>
    /// Get plot at specific grid position
    /// </summary>
    public FarmPlot GetPlotAtPosition(Vector2Int gridPos)
    {
        plotLookup.TryGetValue(gridPos, out FarmPlot plot);
        return plot;
    }

    /// <summary>
    /// Get nearest empty plot to a world position
    /// </summary>
    public FarmPlot GetNearestEmptyPlot(Vector3 worldPos)
    {
        FarmPlot nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (FarmPlot plot in farmPlots)
        {
            if (plot.IsEmpty)
            {
                float distance = Vector3.Distance(worldPos, plot.WorldPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = plot;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// Get nearest plot to a world position (occupied or not)
    /// </summary>
    public FarmPlot GetNearestPlot(Vector3 worldPos, float maxRange = -1f)
    {
        FarmPlot nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (FarmPlot plot in farmPlots)
        {
            float distance = Vector3.Distance(worldPos, plot.WorldPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = plot;
            }
        }

        // Check if within range
        if (maxRange > 0 && nearestDistance > maxRange)
        {
            return null;
        }

        return nearest;
    }

    /// <summary>
    /// Plant a crop at specified plot
    /// </summary>
    public bool PlantCrop(Vector2Int gridPos, CropData cropData)
    {
        FarmPlot plot = GetPlotAtPosition(gridPos);

        if (plot == null)
        {
            Debug.LogWarning($"[FarmPlotManager] No plot exists at {gridPos}");
            return false;
        }

        if (!plot.IsHoed)
        {
            Debug.LogWarning($"[FarmPlotManager] Plot at {gridPos} must be hoed before planting!");
            return false;
        }

        if (plot.IsOccupied)
        {
            Debug.LogWarning($"[FarmPlotManager] Plot at {gridPos} is already occupied");
            return false;
        }

        return PlantCropAtPlot(plot, cropData);
    }

    /// <summary>
    /// Plant a crop at the nearest empty plot to world position
    /// </summary>
    public bool PlantCropNearPosition(Vector3 worldPos, CropData cropData, float maxRange = -1f)
    {
        FarmPlot plot = GetNearestEmptyPlot(worldPos);

        if (plot == null)
        {
            Debug.LogWarning("[FarmPlotManager] No empty plots available");
            return false;
        }

        if (!plot.IsHoed)
        {
            Debug.LogWarning("[FarmPlotManager] Nearest plot is not hoed! Press H to hoe it first.");
            return false;
        }

        if (maxRange > 0 && Vector3.Distance(worldPos, plot.WorldPosition) > maxRange)
        {
            Debug.LogWarning("[FarmPlotManager] Nearest empty plot is out of range");
            return false;
        }

        return PlantCropAtPlot(plot, cropData);
    }

    /// <summary>
    /// Internal method to instantiate and plant crop at plot
    /// </summary>
    private bool PlantCropAtPlot(FarmPlot plot, CropData cropData)
    {
        if (cropData == null)
        {
            Debug.LogError("[FarmPlotManager] CropData is null!");
            return false;
        }

        if (cropPrefab == null)
        {
            Debug.LogError("[FarmPlotManager] Crop prefab is not assigned!");
            return false;
        }

        // Instantiate crop prefab at plot position
        GameObject cropObj = Instantiate(cropPrefab, plot.WorldPosition, Quaternion.identity);
        cropObj.name = $"Crop_{cropData.cropName}_{plot.GridPosition}";

        // Get CropInstance component
        CropInstance cropInstance = cropObj.GetComponent<CropInstance>();

        if (cropInstance == null)
        {
            Debug.LogError("[FarmPlotManager] Crop prefab is missing CropInstance component!");
            Destroy(cropObj);
            return false;
        }

        // Initialize the crop
        int currentDay = CropGrowthManager.Instance != null ? CropGrowthManager.Instance.CurrentDay : 0;
        cropInstance.Plant(cropData, currentDay, plot.GridPosition);

        // Register with managers
        if (CropGrowthManager.Instance != null)
        {
            CropGrowthManager.Instance.RegisterCrop(cropInstance);
        }

        // Update plot
        plot.PlantCrop(cropInstance);

        if (showDebugInfo)
        {
            Debug.Log($"[FarmPlotManager] Planted {cropData.cropName} at {plot.GridPosition}");
        }

        return true;
    }

    /// <summary>
    /// Hoe the nearest unhoed plot to world position
    /// </summary>
    public bool HoeNearestPlot(Vector3 worldPos)
    {
        FarmPlot plot = GetNearestPlot(worldPos, interactionRange);

        if (plot == null)
        {
            if (showDebugInfo)
            {
                Debug.Log("[FarmPlotManager] No plot in range to hoe");
            }
            return false;
        }

        bool success = plot.Hoe();

        if (success && showDebugInfo)
        {
            Debug.Log($"[FarmPlotManager] Hoed plot at {plot.GridPosition}");
        }

        return success;
    }

    /// <summary>
    /// Water crop at nearest plot to position
    /// </summary>
    public bool WaterNearestCrop(Vector3 worldPos)
    {
        FarmPlot plot = GetNearestPlot(worldPos, interactionRange);

        if (plot == null || plot.IsEmpty)
        {
            if (showDebugInfo)
            {
                Debug.Log("[FarmPlotManager] No crop to water in range");
            }
            return false;
        }

        int currentDay = CropGrowthManager.Instance != null ? CropGrowthManager.Instance.CurrentDay : 0;
        return plot.WaterCrop(currentDay);
    }

    /// <summary>
    /// Harvest crop at nearest plot to position
    /// </summary>
    public HarvestResult HarvestNearestCrop(Vector3 worldPos)
    {
        FarmPlot plot = GetNearestPlot(worldPos, interactionRange);

        if (plot == null || plot.IsEmpty)
        {
            if (showDebugInfo)
            {
                Debug.Log("[FarmPlotManager] No crop to harvest in range");
            }
            return null;
        }

        HarvestResult result = plot.HarvestCrop();

        if (result != null && showDebugInfo)
        {
            Debug.Log($"[FarmPlotManager] Harvested {result.quantity}x {result.cropName}");
        }

        return result;
    }

    /// <summary>
    /// Get all plots within range of position
    /// </summary>
    public List<FarmPlot> GetPlotsInRange(Vector3 worldPos, float range)
    {
        List<FarmPlot> plotsInRange = new List<FarmPlot>();

        foreach (FarmPlot plot in farmPlots)
        {
            if (plot.IsInRange(worldPos, range))
            {
                plotsInRange.Add(plot);
            }
        }

        return plotsInRange;
    }

    /// <summary>
    /// Get statistics about plots
    /// </summary>
    public string GetPlotStats()
    {
        int occupied = 0;
        int harvestable = 0;
        int needWater = 0;

        foreach (FarmPlot plot in farmPlots)
        {
            if (plot.IsOccupied)
            {
                occupied++;

                if (plot.CurrentCrop != null)
                {
                    if (plot.CurrentCrop.IsHarvestable)
                        harvestable++;

                    if (!plot.CurrentCrop.IsWatered)
                        needWater++;
                }
            }
        }

        return $"Plots: {occupied}/{farmPlots.Count} | Harvestable: {harvestable} | Need Water: {needWater}";
    }

    /// <summary>
    /// Debug method to check all plot visual markers
    /// </summary>
    [ContextMenu("Debug Plot Visuals")]
    public void DebugPlotVisuals()
    {
        Debug.Log($"[FarmPlotManager] Checking {farmPlots.Count} plots...");

        int missingVisuals = 0;
        int workingVisuals = 0;

        foreach (FarmPlot plot in farmPlots)
        {
            GameObject marker = plot.GetMarker();
            if (marker == null)
            {
                Debug.LogError($"[FarmPlotManager] Plot at {plot.GridPosition} has NO visual marker!");
                missingVisuals++;

                // Recreate it
                plot.CreateVisualMarker(plotMarkersContainer);
            }
            else
            {
                Debug.Log($"[FarmPlotManager] Plot at {plot.GridPosition} has visual at world position {marker.transform.position}");
                workingVisuals++;

                // Force update the visual
                plot.RefreshVisual();
            }
        }

        Debug.Log($"[FarmPlotManager] Visual check complete: {workingVisuals} working, {missingVisuals} missing (now fixed)");
    }

    /// <summary>
    /// Force refresh all plot visuals - useful for debugging visual issues
    /// </summary>
    [ContextMenu("Force Refresh All Visuals")]
    public void ForceRefreshAllVisuals()
    {
        // Ensure container exists
        if (plotMarkersContainer == null)
        {
            GameObject markersContainerObj = new GameObject("PlotMarkers");
            plotMarkersContainer = markersContainerObj.transform;
            plotMarkersContainer.SetParent(transform);
            plotMarkersContainer.localPosition = Vector3.zero;
            plotMarkersContainer.localScale = Vector3.one;
        }

        foreach (FarmPlot plot in farmPlots)
        {
            // Destroy existing marker if it exists
            plot.DestroyMarker();

            // Recreate the visual marker
            plot.CreateVisualMarker(plotMarkersContainer);

            // If plot is hoed, make sure it shows
            if (plot.IsHoed)
            {
                plot.RefreshVisual();
            }
        }

        Debug.Log("[FarmPlotManager] Force refreshed all plot visuals");
    }

    /// <summary>
    /// Refresh all plot visuals (call this if some plots aren't showing correctly)
    /// </summary>
    [ContextMenu("Refresh All Plot Visuals")]
    public void RefreshAllPlotVisuals()
    {
        foreach (FarmPlot plot in farmPlots)
        {
            plot.RefreshVisual();
        }
        Debug.Log("[FarmPlotManager] Refreshed all plot visuals");
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!showPlotGizmos || farmPlots == null) return;

        foreach (FarmPlot plot in farmPlots)
        {
            if (plot == null) continue;

            // Color based on state
            if (plot.IsOccupied)
            {
                if (plot.CurrentCrop != null && plot.CurrentCrop.IsHarvestable)
                {
                    Gizmos.color = Color.green; // Ready to harvest
                }
                else if (plot.CurrentCrop != null && plot.CurrentCrop.IsWilted)
                {
                    Gizmos.color = Color.red; // Wilted
                }
                else
                {
                    Gizmos.color = Color.yellow; // Growing
                }
            }
            else if (plot.IsHoed)
            {
                Gizmos.color = new Color(0.8f, 0.6f, 0.4f, 0.8f); // Hoed (light brown)
            }
            else
            {
                Gizmos.color = new Color(0.5f, 0.3f, 0.1f, 0.5f); // Unhoed (dark brown)
            }

            // Draw plot marker
            Gizmos.DrawWireCube(plot.WorldPosition, Vector3.one * 0.8f);
        }
    }

    [ContextMenu("Debug Y=9 Plots Only")]
    public void DebugY9Plots()
    {
        Debug.Log("[FarmPlotManager] === DEBUG Y=9 PLOTS ===");

        foreach (FarmPlot plot in farmPlots)
        {
            if (plot.GridPosition.y == 9)
            {
                GameObject marker = plot.GetMarker();

                Debug.Log($"Plot {plot.GridPosition}: " +
                         $"Hoed={plot.IsHoed}, " +
                         $"Marker={(marker != null ? "EXISTS" : "NULL")}, " +
                         $"SpriteRenderer={(marker?.GetComponent<SpriteRenderer>() != null ? "EXISTS" : "NULL")}, " +
                         $"Sprite={(marker?.GetComponent<SpriteRenderer>()?.sprite?.name ?? "NULL")}");

                // Force refresh this specific plot
                plot.RefreshVisual();
            }
        }
    }

    [ContextMenu("Debug Y6 and Y9 Sprite Renderers")]
    public void DebugProblematicPlots()
    {
        foreach (FarmPlot plot in farmPlots)
        {
            if (plot.GridPosition.y == 6 || plot.GridPosition.y == 9)
            {
                plot.DebugSpriteRenderer();
            }
        }
    }
}