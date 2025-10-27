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
    public Vector3 WorldPosition
    {
        get
        {
            if (plotMarker != null)
            {
                return plotMarker.transform.position; // Use actual marker position if available
            }
            return new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0); // Fallback calculation
        }
    }

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
        // If visual marker doesn't exist, we can't update it
        if (plotSpriteRenderer == null)
        {
            Debug.LogWarning($"[FarmPlot] Plot at {gridPosition} has no sprite renderer! Visual marker may not be created.");
            return;
        }

        // Get appropriate sprite from FarmPlotManager
        if (FarmPlotManager.Instance != null)
        {
            Sprite sprite = FarmPlotManager.Instance.GetPlotSprite(isHoed, isWet, isOccupied);
            plotSpriteRenderer.sprite = sprite;

            // Show/hide based on state
            plotSpriteRenderer.enabled = isHoed; // Only show if hoed

            Debug.Log($"[FarmPlot] Updated visual for plot at {gridPosition}: hoed={isHoed}, wet={isWet}, sprite={sprite?.name}");
        }
        else
        {
            Debug.LogWarning("[FarmPlot] FarmPlotManager.Instance is null!");
        }
    }

    /// <summary>
    /// Create visual marker for this plot
    /// </summary>
    public void CreateVisualMarker(Transform parent)
    {
        if (plotMarker != null)
        {
            Debug.LogWarning($"[FarmPlot] Plot at {gridPosition} already has a visual marker!");
            return;
        }

        plotMarker = new GameObject($"PlotMarker_{gridPosition.x}_{gridPosition.y}");
        plotMarker.transform.SetParent(parent, false); // Important: worldPositionStays = false
        plotMarker.transform.localPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0); // Use localPosition

        plotSpriteRenderer = plotMarker.AddComponent<SpriteRenderer>();

        // Set sorting layer and order from FarmPlotManager
        if (FarmPlotManager.Instance != null)
        {
            plotSpriteRenderer.sortingLayerName = FarmPlotManager.Instance.GetPlotSortingLayer();
            plotSpriteRenderer.sortingOrder = FarmPlotManager.Instance.GetPlotSortingOrder();
        }

        plotSpriteRenderer.enabled = false; // Hidden until hoed

        Debug.Log($"[FarmPlot] Created visual marker for plot at {gridPosition} with world position {plotMarker.transform.position}");
    }

    /// <summary>
    /// Ensure visual marker exists and update it
    /// </summary>
    public void RefreshVisual()
    {
        if (plotSpriteRenderer == null && FarmPlotManager.Instance != null)
        {
            // Try to recreate the visual marker if it's missing
            Transform container = FarmPlotManager.Instance.GetPlotMarkersContainer();
            if (container != null)
            {
                CreateVisualMarker(container);
            }
        }
        UpdatePlotVisual();
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
        if (marker != null)
        {
            plotSpriteRenderer = marker.GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Get visual marker
    /// </summary>
    public GameObject GetMarker()
    {
        return plotMarker;
    }

    /// <summary>
    /// Destroy the visual marker (cleanup)
    /// </summary>
    public void DestroyMarker()
    {
        if (plotMarker != null)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(plotMarker);
            }
            else
            {
                Object.DestroyImmediate(plotMarker);
            }
            plotMarker = null;
            plotSpriteRenderer = null;
        }
    }

    /// <summary>
    /// Debug the sprite renderer state
    /// </summary>
    public void DebugSpriteRenderer()
    {
        if (plotSpriteRenderer != null)
        {
            Debug.Log($"[FarmPlot] Plot {gridPosition} SpriteRenderer Debug:" +
                     $"\n  Enabled: {plotSpriteRenderer.enabled}" +
                     $"\n  Sprite: {plotSpriteRenderer.sprite?.name}" +
                     $"\n  Color: {plotSpriteRenderer.color}" +
                     $"\n  Sorting Layer: {plotSpriteRenderer.sortingLayerName}" +
                     $"\n  Sorting Order: {plotSpriteRenderer.sortingOrder}" +
                     $"\n  Position: {plotSpriteRenderer.transform.position}");
        }
    }
}