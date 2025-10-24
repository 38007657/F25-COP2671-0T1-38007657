using UnityEngine;

/// <summary>
/// Represents a single farm plot location where crops can be planted
/// </summary>
[System.Serializable]
public class FarmPlot
{
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private bool isOccupied;
    [SerializeField] private CropInstance currentCrop;

    // Visual marker (optional - for debugging/UI)
    private GameObject plotMarker;

    // Properties
    public Vector2Int GridPosition => gridPosition;
    public bool IsOccupied => isOccupied;
    public bool IsEmpty => !isOccupied;
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
    /// Plant a crop at this plot
    /// </summary>
    public bool PlantCrop(CropInstance crop)
    {
        if (isOccupied)
        {
            Debug.LogWarning($"[FarmPlot] Plot at {gridPosition} is already occupied!");
            return false;
        }

        currentCrop = crop;
        isOccupied = true;
        return true;
    }

    /// <summary>
    /// Remove crop from this plot (after harvest or wilt)
    /// </summary>
    public void ClearPlot()
    {
        currentCrop = null;
        isOccupied = false;
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
            Debug.LogWarning($"[FarmPlot] No crop to water at {gridPosition}");
            return false;
        }

        currentCrop.Water(currentDay);
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