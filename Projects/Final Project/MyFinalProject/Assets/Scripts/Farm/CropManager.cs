using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Manages all farming operations using tilemaps
/// Replaces FarmPlotManager and CropGrowthManager
/// Part 1 Requirements: Grid-based crop management system
/// </summary>
public class CropManager : MonoBehaviour
{
    public static CropManager Instance { get; private set; }

    [Header("Tilemap References")]
    [Tooltip("Ground tilemap for soil states (untilled, dry, wet)")]
    [SerializeField] private Tilemap soilTilemap;

    [Tooltip("FarmingVisuals tilemap for crop sprites")]
    [SerializeField] private Tilemap cropTilemap;

    [Tooltip("Tilemap that defines farmable zones")]
    [SerializeField] private Tilemap farmZoneTilemap;

    [Header("Soil Tiles")]
    [SerializeField] private TileBase untilledTile;
    [SerializeField] private TileBase drySoilTile;
    [SerializeField] private TileBase wetSoilTile;

    [Header("Grid Settings")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(20, 20);
    [SerializeField] private Vector2Int gridOffset = new Vector2Int(-10, -10);

    [Header("Current Day")]
    [SerializeField] private int currentDay = 0;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // 2D array to store crop blocks by tile position
    private CropBlock[,] cropGrid;

    // List of actively planted crops
    private List<CropBlock> plantedCrops = new List<CropBlock>();

    // Properties
    public int CurrentDay => currentDay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Add this debug
        Debug.Log($"[CropManager] Soil Tilemap: {soilTilemap != null}, Crop Tilemap: {cropTilemap != null}");

        CreateGridUsingTilemap();

        // Add this too
        Debug.Log($"[CropManager] Grid created: {cropGrid != null}");
    }

    private void Start()
    {
        // Subscribe to TimeManager if it exists
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged += OnHourChanged;
            UnityEngine.Debug.Log("[CropManager] Subscribed to TimeManager");
        }
        else
        {
            UnityEngine.Debug.LogWarning("[CropManager] TimeManager not found!");
        }
    }

    private void Update()
    {
        // Growth is handled by OnSunrise() at 6 AM each day
        // No need to update crops every frame

        // Optional: Add visual effects here in the future
    }

    /// <summary>
    /// Check if a world position is in a farmable zone
    /// </summary>
    public bool IsPositionFarmable(Vector3 worldPosition)
    {
        if (farmZoneTilemap == null)
        {
            // If no farm zone tilemap, all areas are farmable (legacy behavior)
            Debug.LogWarning("[CropManager] Farm Zone Tilemap not assigned! Allowing all positions.");
            return true;
        }

        Vector3Int cellPosition = farmZoneTilemap.WorldToCell(worldPosition);
        TileBase tile = farmZoneTilemap.GetTile(cellPosition);

        bool isFarmable = tile != null;
        Debug.Log($"[CropManager] Checking world position {worldPosition}: Farmable = {isFarmable}");

        return isFarmable;
    }

    /// <summary>
    /// Check if a grid position is in a farmable zone
    /// </summary>
    public bool IsGridPositionFarmable(Vector2Int gridPosition)
    {
        if (farmZoneTilemap == null)
        {
            Debug.LogWarning("[CropManager] Farm Zone Tilemap not assigned! Allowing all positions.");
            return true;
        }

        Vector3Int cellPosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
        TileBase tile = farmZoneTilemap.GetTile(cellPosition);

        bool isFarmable = tile != null;
        Debug.Log($"[CropManager] Checking grid position {gridPosition}: Farmable = {isFarmable}, Tile = {tile}");

        return isFarmable;
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged -= OnHourChanged;
        }
    }

    /// <summary>
    /// Handle time changes - advance day at sunrise
    /// </summary>
    private void OnHourChanged(float time)
    {
        if (TimeManager.Instance == null) return;

        int currentHour = Mathf.FloorToInt(time);

        // Advance day at 6 AM (sunrise)
        if (currentHour == 6)
        {
            AdvanceDay();
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
            UnityEngine.Debug.Log($"[CropManager] === DAY {currentDay} ===");
            UnityEngine.Debug.Log($"[CropManager] Planted crops: {plantedCrops.Count}");
        }

        // Call OnSunrise for each planted crop
        foreach (CropBlock block in plantedCrops)
        {
            if (block != null)
            {
                block.OnSunrise(currentDay);
            }
        }
    }

    /// <summary>
    /// Create grid using tilemap - Part 1 Requirement
    /// </summary>
    public void CreateGridUsingTilemap()
    {
        if (soilTilemap == null || cropTilemap == null)
        {
            UnityEngine.Debug.LogError("[CropManager] Both soil and crop tilemaps must be assigned!");
            return;
        }

        // Initialize 2D array to store crop data by tile position
        cropGrid = new CropBlock[gridSize.x, gridSize.y];

        // Create crop blocks for each grid position
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int gridPos = new Vector2Int(x + gridOffset.x, y + gridOffset.y);
                Vector3 worldPos = soilTilemap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));

                CreateGridBlock(gridPos, worldPos, x, y);
            }
        }

        UnityEngine.Debug.Log($"[CropManager] Created {gridSize.x}x{gridSize.y} farming grid");
    }

    /// <summary>
    /// Create a single grid block - Part 1 Requirement
    /// </summary>
    private void CreateGridBlock(Vector2Int location, Vector3 position, int arrayX, int arrayY)
    {
        CropBlock block = new CropBlock(location, position, soilTilemap, cropTilemap, this);
        cropGrid[arrayX, arrayY] = block;
    }

    /// <summary>
    /// Add crop block to planted crops list - Part 1 Requirement
    /// </summary>
    public void AddToPlantedCrops(CropBlock cropBlock)
    {
        if (!plantedCrops.Contains(cropBlock))
        {
            plantedCrops.Add(cropBlock);

            if (showDebugInfo)
            {
                UnityEngine.Debug.Log($"[CropManager] Added {cropBlock.seedPacket?.cropName ?? "crop"} to planted crops list");
            }
        }
    }

    /// <summary>
    /// Remove crop block from planted crops list - Part 1 Requirement
    /// </summary>
    public void RemoveFromPlantedCrops(CropBlock cropBlock)
    {
        plantedCrops.Remove(cropBlock);

        if (showDebugInfo)
        {
            UnityEngine.Debug.Log($"[CropManager] Removed {cropBlock.seedPacket?.cropName ?? "crop"} from planted crops list");
        }
    }

    /// <summary>
    /// Get crop block at specific grid position
    /// </summary>
    public CropBlock GetBlockAtPosition(Vector2Int gridPos)
    {
        // Add null check for grid
        if (cropGrid == null)
        {
            Debug.LogWarning("[CropManager] cropGrid is null!");
            return null;
        }

        int x = gridPos.x - gridOffset.x;
        int y = gridPos.y - gridOffset.y;

        // Check bounds
        if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
        {
            return null;
        }

        return cropGrid[x, y];
    }

    /// <summary>
    /// Get crop block at world position
    /// </summary>
    public CropBlock GetBlockAtWorldPosition(Vector3 worldPos)
    {
        if (soilTilemap == null) return null;

        Vector3Int cellPos = soilTilemap.WorldToCell(worldPos);
        return GetBlockAtPosition(new Vector2Int(cellPos.x, cellPos.y));
    }

    /// <summary>
    /// Check if currently daytime (for growth logic)
    /// </summary>
    public bool IsDaytime()
    {
        if (TimeManager.Instance != null)
        {
            return TimeManager.Instance.IsDaytime;
        }
        return true; // Default to daytime if no TimeManager
    }

    // Tile setting methods for soil tilemap
    public void SetUntilledTile(Vector3Int tilePos)
    {
        if (soilTilemap != null && untilledTile != null)
        {
            soilTilemap.SetTile(tilePos, untilledTile);
            Debug.Log($"[CropManager] âœ… Set untilled tile at {tilePos}");
        }
        else
        {
            Debug.LogError($"[CropManager] âŒ Missing soilTilemap ({soilTilemap != null}) or untilledTile ({untilledTile != null})!");
        }
    }

    public void SetDrySoilTile(Vector3Int tilePos)
    {
        if (soilTilemap != null && drySoilTile != null)
        {
            soilTilemap.SetTile(tilePos, drySoilTile);
            Debug.Log($"[CropManager] âœ… Set dry soil tile at {tilePos}");
        }
        else
        {
            Debug.LogError($"[CropManager] âŒ Missing soilTilemap ({soilTilemap != null}) or drySoilTile ({drySoilTile != null})!");
        }
    }

    public void SetWetSoilTile(Vector3Int tilePos)
    {
        if (soilTilemap != null && wetSoilTile != null)
        {
            soilTilemap.SetTile(tilePos, wetSoilTile);
            Debug.Log($"[CropManager] âœ… Set wet soil tile at {tilePos}");
        }
        else
        {
            Debug.LogError($"[CropManager] âŒ Missing soilTilemap ({soilTilemap != null}) or wetSoilTile ({wetSoilTile != null})!");
        }
    }

    /// <summary>
    /// Set crop sprite on crop tilemap layer
    /// </summary>
    public void SetCropTile(Vector3Int tilePos, Sprite sprite)
    {
        if (cropTilemap == null) return;

        if (sprite != null)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            cropTilemap.SetTile(tilePos, tile);
        }
        else
        {
            // Clear crop tile (soil will be visible underneath)
            cropTilemap.SetTile(tilePos, null);
        }
    }

    /// <summary>
    /// Get all harvestable crops
    /// </summary>
    public List<CropBlock> GetHarvestableCrops()
    {
        List<CropBlock> harvestable = new List<CropBlock>();

        foreach (CropBlock block in plantedCrops)
        {
            if (block != null && block.IsHarvestable())
            {
                harvestable.Add(block);
            }
        }

        return harvestable;
    }

    /// <summary>
    /// Get crops that need water
    /// </summary>
    public List<CropBlock> GetCropsNeedingWater()
    {
        List<CropBlock> needWater = new List<CropBlock>();

        foreach (CropBlock block in plantedCrops)
        {
            if (block != null && block.isPlanted && !block.isWatered && !block.isWilted)
            {
                needWater.Add(block);
            }
        }

        return needWater;
    }

    /// <summary>
    /// Get all planted crops (for saving)
    /// </summary>
    public List<CropBlock> GetAllPlantedCrops()
    {
        return new List<CropBlock>(plantedCrops);
    }

    /// <summary>
    /// Get all tilled blocks that don't have crops planted (for saving)
    /// </summary>
    public List<CropBlock> GetAllTilledBlocks()
    {
        List<CropBlock> tilledBlocks = new List<CropBlock>();

        // Iterate through entire grid
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                CropBlock block = cropGrid[x, y];

                // Only add if tilled but NOT planted
                if (block != null && block.isTilled && !block.isPlanted)
                {
                    tilledBlocks.Add(block);
                }
            }
        }

        if (showDebugInfo)
        {
            UnityEngine.Debug.Log($"[CropManager] Found {tilledBlocks.Count} tilled (not planted) blocks");
        }

        return tilledBlocks;
    }

    /// <summary>
    /// Clear all crops (for loading)
    /// </summary>
    public void ClearAllCrops()
    {
        UnityEngine.Debug.Log("========================================");
        UnityEngine.Debug.Log($"[CropManager] ClearAllCrops called");
        UnityEngine.Debug.Log($"[CropManager] Planted crops: {plantedCrops.Count}");
        UnityEngine.Debug.Log($"[CropManager] Grid size: {gridSize.x} x {gridSize.y} = {gridSize.x * gridSize.y} total blocks");
        UnityEngine.Debug.Log("========================================");

        // IMPORTANT: Only clear the blocks that were actually planted!
        List<CropBlock> blocksToClean = new List<CropBlock>(plantedCrops);

        UnityEngine.Debug.Log($"[CropManager] Clearing {blocksToClean.Count} planted blocks only");

        foreach (CropBlock block in blocksToClean)
        {
            if (block != null)
            {
                block.ClearForLoad();
            }
        }

        // Clear the planted crops list
        plantedCrops.Clear();

        // DON'T iterate through the entire grid!
        // The untilled blocks should stay invisible/empty

        if (showDebugInfo)
        {
            UnityEngine.Debug.Log("[CropManager] Cleared all planted crops for loading");
        }
    }

    /// <summary>
    /// Set the current day (for loading)
    /// </summary>
    public void SetCurrentDay(int day)
    {
        currentDay = day;

        if (showDebugInfo)
        {
            Debug.Log($"[CropManager] Set current day to {day}");
        }
    }

    /// <summary>
    /// Debug: Show grid info
    /// </summary>
    [ContextMenu("Debug Grid Info")]
    public void DebugGridInfo()
    {
        UnityEngine.Debug.Log($"=== CROP MANAGER DEBUG ===");
        UnityEngine.Debug.Log($"Grid Size: {gridSize}");
        UnityEngine.Debug.Log($"Grid Offset: {gridOffset}");
        UnityEngine.Debug.Log($"Current Day: {currentDay}");
        UnityEngine.Debug.Log($"Planted Crops: {plantedCrops.Count}");
        UnityEngine.Debug.Log($"Harvestable Crops: {GetHarvestableCrops().Count}");
        UnityEngine.Debug.Log($"Crops Needing Water: {GetCropsNeedingWater().Count}");
    }

    // Debug display
    //private void OnGUI()
    //{
    //    if (showDebugInfo)
    //    {
    //        GUIStyle style = new GUIStyle(GUI.skin.label);
    //        style.fontSize = 14;
    //        style.normal.textColor = Color.white;
    //        style.fontStyle = FontStyle.Bold;

    //        // Shadow effect
    //        GUI.color = Color.black;
    //        GUI.Label(new Rect(11, 11, 400, 25), $"Day: {currentDay}", style);
    //        GUI.Label(new Rect(11, 31, 400, 25), $"Planted: {plantedCrops.Count} | Harvestable: {GetHarvestableCrops().Count} | Need Water: {GetCropsNeedingWater().Count}", style);

    //        // Main text
    //        GUI.color = Color.white;
    //        GUI.Label(new Rect(10, 10, 400, 25), $"Day: {currentDay}", style);
    //        GUI.Label(new Rect(10, 30, 400, 25), $"Planted: {plantedCrops.Count} | Harvestable: {GetHarvestableCrops().Count} | Need Water: {GetCropsNeedingWater().Count}", style);

    //        GUI.color = Color.white;
    //    }
    //}
}