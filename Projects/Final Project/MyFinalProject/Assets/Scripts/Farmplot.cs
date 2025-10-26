using UnityEngine;

/// <summary>
/// Represents a single farm plot location where crops can be planted
/// </summary>
[System.Serializable]
public class FarmPlot
{
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private bool isHoed;
    [SerializeField] private bool isWet;
    [SerializeField] private bool isOccupied;
    [SerializeField] private CropInstance currentCrop;

    // Visual marker (sprite that shows plot state)
    private GameObject plotMarker;
    private SpriteRenderer plotSpriteRenderer;

    // Time tracking for wet soil drying
    private float lastWateredTime;
    private const float WET_DURATION = 120f; // 2 minutes (or adjust as needed)

    // Properties
    public Vector2Int GridPosition => gridPosition;
    public bool IsHoed => isHoed;
    public bool IsWet => isWet;
    public bool IsOccupied => isOccupied;
    public bool IsEmpty => !isOccupied;
    public bool CanPlant => isHoed && !isOccupied; // Must be hoed to plant
    public CropInstance CurrentCrop => currentCrop;
    public Vector3 WorldPosition => new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0); // Center of cell

    // Constructor
    public FarmPlot(Vector2Int position)
    {
        gridPosition = position;
        isOccupied = false;
        currentCrop = null;
    }

    public FarmPlot(int x, int y) : this(new Vector2Int(x, y)) { }

    /// <summary>
    /// Hoe this plot to prepare it for planting
    /// </summary>
    public bool Hoe()
    {
        if (isHoed)
        {
            Debug.LogWarning($"[FarmPlot] Plot at {gridPosition} is already hoed!");
            return false;
        }

        if (isOccupied)
        {
            Debug.LogWarning($"[FarmPlot] Cannot hoe - plot at {gridPosition} has a crop!");
            return false;
        }

        isHoed = true;
        isWet = false;
        UpdatePlotVisual();
        return true;
    }

    /// <summary>
    /// Water this plot (makes soil wet)
    /// </summary>
    public void WaterSoil()
    {
        if (!isHoed) return;

        isWet = true;
        lastWateredTime = Time.time;
        UpdatePlotVisual();
    }

    /// <summary>
    /// Check if wet soil should dry out
    /// </summary>
    public void UpdateDryingState()
    {
        if (!isWet) return;

        if (Time.time - lastWateredTime > WET_DURATION)
        {
            isWet = false;
            UpdatePlotVisual();
        }
    }

    /// <summary>
    /// Reset plot to unhoed state (after harvest)
    /// </summary>
    public void ResetToUnhoed()
    {
        isHoed = false;
        isWet = false;
        isOccupied = false;
        currentCrop = null;
        UpdatePlotVisual();
    }

    /// <summary>
    /// Update the visual sprite based on plot state
    /// </summary>
    private void UpdatePlotVisual()
    {
        if (plotSpriteRenderer == null) return;

        // Get appropriate sprite from FarmPlotManager
        if (FarmPlotManager.Instance != null)
        {
            Sprite sprite = FarmPlotManager.Instance.GetPlotSprite(isHoed, isWet, isOccupied);
            plotSpriteRenderer.sprite = sprite;

            // Show/hide based on state
            plotSpriteRenderer.enabled = isHoed; // Only show if hoed
        }
    }

    /// <summary>
    /// Create visual marker for this plot
    /// </summary>
    public void CreateVisualMarker(Transform parent)
    {
        if (plotMarker != null) return;

        plotMarker = new GameObject($"PlotMarker_{gridPosition.x}_{gridPosition.y}");
        plotMarker.transform.SetParent(parent);
        plotMarker.transform.position = WorldPosition;

        plotSpriteRenderer = plotMarker.AddComponent<SpriteRenderer>();
        plotSpriteRenderer.sortingOrder = -10; // Below crops
        plotSpriteRenderer.enabled = false; // Hidden until hoed
    }

    /// <summary>
    /// Plant a crop at this plot
    /// </summary>
    public bool PlantCrop(CropInstance crop)
    {
        if (!isHoed)
        {
            Debug.LogWarning($"[FarmPlot] Plot at {gridPosition} must be hoed before planting!");
            return false;
        }

        if (isOccupied)
        {
            Debug.LogWarning($"[FarmPlot] Plot at {gridPosition} is already occupied!");
            return false;
        }

        currentCrop = crop;
        isOccupied = true;
        UpdatePlotVisual();
        return true;
    }

    /// <summary>
    /// Remove crop from this plot (after harvest or wilt)
    /// </summary>
    public void ClearPlot()
    {
        currentCrop = null;
        isOccupied = false;

        // Reset to unhoed state after harvest
        ResetToUnhoed();
    }

    /// <summary>
    /// Check if position is within interaction range of this plot
    /// </summary>
    public bool IsInRange(Vector3 position, float range)
    {
        return Vector3.Distance(WorldPosition, position) <= range;
    }

    /// <summary>
    /// Water the crop at this plot
    /// </summary>
    public bool WaterCrop(int currentDay)
    {
        if (!isOccupied || currentCrop == null)
        {
            // If plot is hoed but empty, just water the soil
            if (isHoed)
            {
                WaterSoil();
                Debug.Log($"[FarmPlot] Watered empty soil at {gridPosition}");
                return true;
            }

            Debug.LogWarning($"[FarmPlot] No crop to water at {gridPosition}");
            return false;
        }

        // Water both the crop and the soil
        currentCrop.Water(currentDay);
        WaterSoil();
        return true;
    }

    /// <summary>
    /// Harvest the crop at this plot
    /// </summary>
    public HarvestResult HarvestCrop()
    {
        if (!isOccupied || currentCrop == null)
        {
            Debug.LogWarning($"[FarmPlot] No crop to harvest at {gridPosition}");
            return null;
        }

        if (!currentCrop.IsHarvestable)
        {
            Debug.LogWarning($"[FarmPlot] Crop at {gridPosition} is not ready to harvest!");
            return null;
        }

        HarvestResult result = currentCrop.Harvest();

        // If crop was destroyed (not multi-harvest), clear the plot
        if (currentCrop == null || currentCrop.gameObject == null)
        {
            ClearPlot();
        }

        return result;
    }

    /// <summary>
    /// Set visual marker for this plot (optional - for debugging)
    /// </summary>
    public void SetMarker(GameObject marker)
    {
        plotMarker = marker;
    }

    /// <summary>
    /// Get visual marker
    /// </summary>
    public GameObject GetMarker()
    {
        return plotMarker;
    }
}