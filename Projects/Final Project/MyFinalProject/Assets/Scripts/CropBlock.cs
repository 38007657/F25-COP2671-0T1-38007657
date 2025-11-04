using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Represents a single farmable tile location
/// Part 3 Requirements: Crop growth cycle with stages
/// </summary>
public class CropBlock
{
    // Grid info
    public Vector2Int gridPosition;
    public Vector3 worldPosition;

    // State
    public bool isTilled = false;
    public bool isWatered = false;
    public bool isPlanted = false;
    public bool isWilted = false;

    // Crop info
    public SeedPacket seedPacket;
    public int currentGrowthStage = 0; // 0-3 for 4 growth stages
    public float growthTimer = 0f;

    // Time tracking
    public int dayPlanted = 0;
    public int lastWateredDay = -1;
    public int daysWithoutWater = 0;

    // References
    private Tilemap soilTilemap;
    private Tilemap cropTilemap;
    private CropManager cropManager;

    // Constructor - Updated to accept both tilemaps
    public CropBlock(Vector2Int gridPos, Vector3 worldPos, Tilemap soilMap, Tilemap cropMap, CropManager manager)
    {
        this.gridPosition = gridPos;
        this.worldPosition = worldPos;
        this.soilTilemap = soilMap;
        this.cropTilemap = cropMap;
        this.cropManager = manager;
    }

    /// <summary>
    /// Till the soil at this block - Part 3 Requirement
    /// </summary>
    public bool TillSoil()
    {
        if (isTilled)
        {
            UnityEngine.Debug.Log($"[CropBlock] Tile at {gridPosition} is already tilled!");
            return false;
        }

        if (isPlanted)
        {
            // Allow hoeing if crop is wilted (to remove it)
            if (isWilted)
            {
                UnityEngine.Debug.Log($"[CropBlock] Removing wilted crop at {gridPosition}");
                ClearCrop();
                // Now till the soil
                isTilled = true;
                isWatered = false;
                UpdateTileVisual();
                return true;
            }

            UnityEngine.Debug.Log($"[CropBlock] Cannot till - crop already planted!");
            return false;
        }

        isTilled = true;
        isWatered = false;

        // Update tilemap visual
        UpdateTileVisual();

        UnityEngine.Debug.Log($"[CropBlock] Tilled soil at {gridPosition}");
        return true;
    }

    /// <summary>
    /// Water the soil/crop at this block - Part 3 Requirement
    /// </summary>
    public bool WaterSoil(int currentDay)
    {
        // Must be tilled
        if (!isTilled)
        {
            UnityEngine.Debug.Log($"[CropBlock] Must till soil before watering!");
            return false;
        }

        // Must have a crop planted to water
        if (!isPlanted)
        {
            UnityEngine.Debug.Log($"[CropBlock] No crop to water! Plant seeds first.");
            return false;
        }

        // Can't water wilted crops
        if (isWilted)
        {
            UnityEngine.Debug.Log($"[CropBlock] Cannot water wilted crop! Use hoe to remove it.");
            return false;
        }

        if (isWatered && lastWateredDay == currentDay)
        {
            UnityEngine.Debug.Log($"[CropBlock] Already watered today!");
            return false;
        }

        isWatered = true;
        lastWateredDay = currentDay;
        daysWithoutWater = 0; // Reset wilting counter

        // Update tilemap visual (shows wet soil)
        UpdateTileVisual();

        UnityEngine.Debug.Log($"[CropBlock] Watered at {gridPosition}");
        return true;
    }

    /// <summary>
    /// Plant a seed at this block - Part 3 Requirement
    /// </summary>
    public bool PlantSeed(SeedPacket seed, int currentDay)
    {
        // Must be tilled before planting
        if (!isTilled)
        {
            UnityEngine.Debug.Log($"[CropBlock] Must till soil before planting!");
            return false;
        }

        if (isPlanted)
        {
            UnityEngine.Debug.Log($"[CropBlock] Crop already planted here!");
            return false;
        }

        seedPacket = seed;
        isPlanted = true;
        isWilted = false;
        currentGrowthStage = 0;
        growthTimer = 0f;
        dayPlanted = currentDay;
        lastWateredDay = -1; // Not watered yet
        isWatered = false;
        daysWithoutWater = 0;

        // Add to planted crops list
        cropManager.AddToPlantedCrops(this);

        // Update tilemap visual - Show both soil AND crop
        UpdateTileVisual();

        UnityEngine.Debug.Log($"[CropBlock] Planted {seed.cropName} at {gridPosition}");
        return true;
    }

    /// <summary>
    /// Harvest the crop at this block - Part 3 Requirement
    /// </summary>
    public GameObject HarvestPlant()
    {
        if (!isPlanted)
        {
            UnityEngine.Debug.Log($"[CropBlock] No crop to harvest!");
            return null;
        }

        if (isWilted)
        {
            UnityEngine.Debug.Log($"[CropBlock] Cannot harvest wilted crop! Use hoe to remove it.");
            return null;
        }

        if (currentGrowthStage < 3)
        {
            UnityEngine.Debug.Log($"[CropBlock] Crop not ready to harvest! Stage: {currentGrowthStage}/3");
            return null;
        }

        // Spawn harvestable (if you want physical drops)
        GameObject harvestable = null;
        if (seedPacket.harvestablePrefab != null)
        {
            harvestable = Object.Instantiate(seedPacket.harvestablePrefab, worldPosition, Quaternion.identity);
        }

        UnityEngine.Debug.Log($"[CropBlock] Harvested {seedPacket.cropName} at {gridPosition}");

        // Clear the crop
        ClearCrop();

        return harvestable;
    }

    /// <summary>
    /// Update growth progress - Part 3 Requirement: Advance growth stages over time
    /// Only grow if watered
    /// </summary>
    public void UpdateGrowth(float deltaTime, int currentDay)
    {
        if (!isPlanted || seedPacket == null) return;
        if (isWilted) return; // Wilted crops don't grow
        if (currentGrowthStage >= 3) return; // Already fully grown

        // Check if watered (required to grow) - seeds don't grow without water
        if (seedPacket.requiresWater && !isWatered)
        {
            // Crop doesn't grow without water - stays at current stage
            return;
        }

        // Advance growth timer
        growthTimer += deltaTime;

        // Check if ready for next stage
        float stageDuration = seedPacket.stageDurations[currentGrowthStage];

        if (growthTimer >= stageDuration)
        {
            growthTimer = 0f;
            AdvanceStage();
        }
    }

    /// <summary>
    /// Advance to next growth stage - Part 3 Requirement
    /// </summary>
    private void AdvanceStage()
    {
        if (currentGrowthStage >= 3) return;

        currentGrowthStage++;

        // Update tile sprite based on growth stage - Part 3 Requirement
        UpdateTileVisual();

        UnityEngine.Debug.Log($"[CropBlock] {seedPacket.cropName} advanced to stage {currentGrowthStage}");
    }

    /// <summary>
    /// Called at sunrise (6 AM) - reset watered status and check for wilting
    /// </summary>
    public void OnSunrise(int currentDay)
    {
        if (!isPlanted) return;
        if (isWilted) return; // Already wilted

        // Check if was watered yesterday
        bool wasWatered = isWatered;

        // Reset watered status for new day
        isWatered = false;

        if (!wasWatered && seedPacket != null && seedPacket.requiresWater)
        {
            daysWithoutWater++;

            UnityEngine.Debug.Log($"[CropBlock] {seedPacket.cropName} not watered - {daysWithoutWater} days without water");

            // Wilt if past seed stage (stage 0) and not watered
            if (currentGrowthStage > 0 && daysWithoutWater >= 1)
            {
                // Crop wilts after 1 day without water (only if past seed stage)
                isWilted = true;
                UnityEngine.Debug.Log($"[CropBlock] {seedPacket.cropName} has wilted at {gridPosition}!");
                UpdateTileVisual();
            }
            else if (currentGrowthStage == 0)
            {
                // Seeds (stage 0) just stay as seeds, don't wilt
                UnityEngine.Debug.Log($"[CropBlock] Seed at {gridPosition} remains planted (waiting for water)");
            }
        }
        else if (wasWatered)
        {
            daysWithoutWater = 0; // Reset counter
        }

        // Update soil to show dry
        UpdateTileVisual();
    }

    /// <summary>
    /// Update the tilemap tiles based on current state - Part 3 Requirement: Update tile sprite
    /// SOIL LAYER: Always shows soil state (untilled/dry/wet)
    /// CROP LAYER: Shows crop sprite or nothing
    /// </summary>
    private void UpdateTileVisual()
    {
        if (soilTilemap == null) return;

        Vector3Int tilePos = soilTilemap.WorldToCell(worldPosition);

        // ===== UPDATE SOIL LAYER (always visible) =====
        if (isTilled)
        {
            // Show tilled soil (watered or dry)
            if (isWatered)
            {
                cropManager.SetWetSoilTile(tilePos);
            }
            else
            {
                cropManager.SetDrySoilTile(tilePos);
            }
        }
        else
        {
            // Show untilled grass/dirt
            cropManager.SetUntilledTile(tilePos);
        }

        // ===== UPDATE CROP LAYER (only if planted) =====
        if (cropTilemap != null)
        {
            Vector3Int cropTilePos = cropTilemap.WorldToCell(worldPosition);

            if (isPlanted && seedPacket != null)
            {
                // Show wilted sprite if wilted
                if (isWilted && seedPacket.wiltedSprite != null)
                {
                    cropManager.SetCropTile(cropTilePos, seedPacket.wiltedSprite);
                }
                // Show current growth stage sprite
                else if (!isWilted && currentGrowthStage < seedPacket.growthSprites.Length)
                {
                    Sprite cropSprite = seedPacket.growthSprites[currentGrowthStage];
                    cropManager.SetCropTile(cropTilePos, cropSprite);
                }
            }
            else
            {
                // No crop planted - clear crop tile (soil will be visible underneath)
                cropManager.SetCropTile(cropTilePos, null);
            }
        }
    }

    /// <summary>
    /// Clear the crop and reset to untilled state
    /// </summary>
    private void ClearCrop()
    {
        isPlanted = false;
        isWilted = false;
        seedPacket = null;
        currentGrowthStage = 0;
        growthTimer = 0f;
        isWatered = false;
        daysWithoutWater = 0;
        isTilled = false; // Reset to untilled after clearing crop

        // Remove from planted crops
        cropManager.RemoveFromPlantedCrops(this);

        // Reset to untilled
        UpdateTileVisual();
    }

    /// <summary>
    /// Check if this block is ready to harvest
    /// </summary>
    public bool IsHarvestable()
    {
        return isPlanted && !isWilted && currentGrowthStage >= 3;
    }
}