using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all crops and tracks game days via TimeManager integration.
/// Handles day progression and crop coordination.
/// </summary>
public class CropGrowthManager : MonoBehaviour
{
    public static CropGrowthManager Instance { get; private set; }

    [Header("Day Tracking")]
    [SerializeField] private int currentDay = 0;
    [SerializeField] private int currentSeason = 0; // 0=Spring, 1=Summer, 2=Fall, 3=Winter
    [SerializeField] private int daysPerSeason = 28;

    [Header("Crop Management")]
    [SerializeField] private List<CropInstance> activeCrops = new List<CropInstance>();

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Properties
    public int CurrentDay => currentDay;
    public Season CurrentSeason => (Season)currentSeason;

    // Events
    public System.Action<int> OnDayChanged;
    public System.Action<int> OnSeasonChanged;

    private float lastHour = -1f;
    private bool hasAdvancedToday = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to TimeManager (moved to Start to ensure TimeManager is initialized first)
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged += OnHourChanged;
            Debug.Log("[CropGrowthManager] Subscribed to TimeManager");
        }
        else
        {
            Debug.LogError("[CropGrowthManager] TimeManager not found! Make sure TimeManager GameObject exists in the scene.");
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged -= OnHourChanged;
        }
    }

    /// <summary>
    /// Called when hour changes in TimeManager
    /// </summary>
    private void OnHourChanged(float time)
    {
        if (TimeManager.Instance == null) return;

        float sunrise = TimeManager.Instance.SunriseTime;

        // Detect when we cross sunrise (6 AM) - advance day
        // Handle both normal progression and time wrapping (23 -> 0)
        bool crossedSunrise = false;

        if (lastHour >= 0)
        {
            // Time wrapped from 23 to 0+
            if (lastHour > time)
            {
                crossedSunrise = (time >= sunrise || lastHour < sunrise);
            }
            // Normal time progression
            else
            {
                crossedSunrise = (lastHour < sunrise && time >= sunrise);
            }
        }

        lastHour = time;

        // Advance day only once per sunrise
        if (crossedSunrise && !hasAdvancedToday)
        {
            AdvanceDay();
            hasAdvancedToday = true;
        }

        // Reset flag when time moves past sunrise
        if (time > sunrise + 1f)
        {
            hasAdvancedToday = false;
        }
    }

    /// <summary>
    /// Advance to next day
    /// </summary>
    private void AdvanceDay()
    {
        currentDay++;

        if (showDebugInfo)
        {
            Debug.Log($"[CropGrowthManager] === DAY {currentDay} ({GetDayString()}) ===");
            Debug.Log($"[CropGrowthManager] Active crops: {activeCrops.Count}");
        }

        OnDayChanged?.Invoke(currentDay);

        // Check for season change
        if (currentDay % daysPerSeason == 0)
        {
            currentSeason = (currentSeason + 1) % 4;
            OnSeasonChanged?.Invoke(currentSeason);

            if (showDebugInfo)
            {
                Debug.Log($"[CropGrowthManager] Season changed to {CurrentSeason}");
            }
        }

        // Clean up null crop references
        UpdateCropList();
    }

    /// <summary>
    /// Register a crop with the manager
    /// </summary>
    public void RegisterCrop(CropInstance crop)
    {
        if (!activeCrops.Contains(crop))
        {
            activeCrops.Add(crop);

            // Subscribe to crop events
            crop.OnCropHarvested += OnCropHarvested;
            crop.OnCropWilted += OnCropWilted;

            if (showDebugInfo)
            {
                Debug.Log($"[CropGrowthManager] Registered {crop.CropData.cropName} at {crop.GridPosition}");
            }
        }
    }

    /// <summary>
    /// Unregister a crop
    /// </summary>
    public void UnregisterCrop(CropInstance crop)
    {
        if (activeCrops.Contains(crop))
        {
            activeCrops.Remove(crop);

            crop.OnCropHarvested -= OnCropHarvested;
            crop.OnCropWilted -= OnCropWilted;

            if (showDebugInfo)
            {
                Debug.Log($"[CropGrowthManager] Unregistered {crop.CropData.cropName}");
            }
        }
    }

    /// <summary>
    /// Clean up null references
    /// </summary>
    private void UpdateCropList()
    {
        activeCrops.RemoveAll(crop => crop == null);
    }

    /// <summary>
    /// Get crop at specific grid position
    /// </summary>
    public CropInstance GetCropAtPosition(Vector2Int gridPos)
    {
        foreach (CropInstance crop in activeCrops)
        {
            if (crop != null && crop.GridPosition == gridPos)
            {
                return crop;
            }
        }
        return null;
    }

    /// <summary>
    /// Check if position has a crop
    /// </summary>
    public bool HasCropAtPosition(Vector2Int gridPos)
    {
        return GetCropAtPosition(gridPos) != null;
    }

    /// <summary>
    /// Water all crops in radius
    /// </summary>
    public void WaterCropsInRadius(Vector3 worldPos, float radius)
    {
        int wateredCount = 0;

        foreach (CropInstance crop in activeCrops)
        {
            if (crop != null)
            {
                float distance = Vector3.Distance(worldPos, crop.transform.position);
                if (distance <= radius)
                {
                    crop.Water(currentDay);
                    wateredCount++;
                }
            }
        }

        if (showDebugInfo && wateredCount > 0)
        {
            Debug.Log($"[CropGrowthManager] Watered {wateredCount} crops");
        }
    }

    /// <summary>
    /// Get all harvestable crops
    /// </summary>
    public List<CropInstance> GetHarvestableCrops()
    {
        List<CropInstance> harvestable = new List<CropInstance>();

        foreach (CropInstance crop in activeCrops)
        {
            if (crop != null && crop.IsHarvestable)
            {
                harvestable.Add(crop);
            }
        }

        return harvestable;
    }

    /// <summary>
    /// Get crops that need water
    /// </summary>
    public List<CropInstance> GetCropsNeedingWater()
    {
        List<CropInstance> needWater = new List<CropInstance>();

        foreach (CropInstance crop in activeCrops)
        {
            if (crop != null && !crop.IsWatered && !crop.IsWilted)
            {
                needWater.Add(crop);
            }
        }

        return needWater;
    }

    // Event handlers
    private void OnCropHarvested(CropInstance crop)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[CropGrowthManager] {crop.CropData.cropName} harvested");
        }

        // Hook for future inventory system
        // InventoryManager.Instance?.AddItem(result.itemID, result.quantity);
    }

    private void OnCropWilted(CropInstance crop)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[CropGrowthManager] {crop.CropData.cropName} wilted at {crop.GridPosition}");
        }
    }

    // Utility methods
    public string GetDayString()
    {
        int dayOfSeason = (currentDay % daysPerSeason);
        if (dayOfSeason == 0) dayOfSeason = daysPerSeason;
        return $"{CurrentSeason} Day {dayOfSeason}";
    }

    public void SetDay(int day)
    {
        currentDay = day;
        currentSeason = (day / daysPerSeason) % 4;
        OnDayChanged?.Invoke(currentDay);
    }

    // For save/load
    public int GetCurrentDay() => currentDay;
    public void LoadDay(int day) => SetDay(day);

    // Debug display
    private void OnGUI()
    {
        if (showDebugInfo)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;

            // Shadow effect
            GUI.color = Color.black;
            GUI.Label(new Rect(11, 11, 400, 25), $"Day: {currentDay} ({GetDayString()})", style);
            GUI.Label(new Rect(11, 31, 400, 25), $"Time: {TimeManager.Instance?.GetTimeString()} ({TimeManager.Instance?.GetSpeedString()})", style);
            GUI.Label(new Rect(11, 51, 400, 25), $"Crops: {activeCrops.Count} | Harvestable: {GetHarvestableCrops().Count} | Need Water: {GetCropsNeedingWater().Count}", style);

            // Main text
            GUI.color = Color.white;
            GUI.Label(new Rect(10, 10, 400, 25), $"Day: {currentDay} ({GetDayString()})", style);
            GUI.Label(new Rect(10, 30, 400, 25), $"Time: {TimeManager.Instance?.GetTimeString()} ({TimeManager.Instance?.GetSpeedString()})", style);
            GUI.Label(new Rect(10, 50, 400, 25), $"Crops: {activeCrops.Count} | Harvestable: {GetHarvestableCrops().Count} | Need Water: {GetCropsNeedingWater().Count}", style);

            GUI.color = Color.white;
        }
    }
}