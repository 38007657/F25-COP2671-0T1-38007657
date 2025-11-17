using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Represents a single farmable tile location
/// Part 3 Requirements: Crop growth cycle with stages
/// </summary>
public class CropBlock
{

    [Header("Visual Effects")]
    [SerializeField] private GameObject harvestReadyParticles; // Assign in inspector or via CropManager

    private GameObject activeParticleInstance;

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

    public bool TillSoil()
    {
        // NEW: Check if this position is farmable
        if (!cropManager.IsGridPositionFarmable(gridPosition))
        {
            UnityEngine.Debug.Log($"[CropBlock] âŒ Cannot till - not in farmable area at {gridPosition}!");
            return false;
        }

        // CHECK FOR PLANTED CROPS FIRST (before checking isTilled)
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

        // NOW check if already tilled (moved this down)
        if (isTilled)
        {
            UnityEngine.Debug.Log($"[CropBlock] Tile at {gridPosition} is already tilled!");
            return false;
        }

        isTilled = true;
        isWatered = false;

        // Update tilemap visual
        UpdateTileVisual();

        UnityEngine.Debug.Log($"[CropBlock] âœ… Tilled soil at {gridPosition}");
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

        if (currentGrowthStage >= 3)
        {
            UnityEngine.Debug.Log($"[CropBlock] Crop is ready to harvest! No need to water.");
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

        // === NEW: Check if player has seeds ===
        if (PlayerInventory.Instance != null)
        {
            if (PlayerInventory.Instance.GetSeedPacketCount(seed) <= 0)
            {
                UnityEngine.Debug.Log($"[CropBlock] No {seed.cropName} seeds in inventory!");
                return false;
            }

            // Consume one seed
            if (!PlayerInventory.Instance.UseSeedFromPacket(seed))
            {
                UnityEngine.Debug.Log($"[CropBlock] Failed to use seed from inventory!");
                return false;
            }
        }

        seedPacket = seed;
        isPlanted = true;
        isWilted = false;
        currentGrowthStage = 0;
        growthTimer = 0f;
        dayPlanted = currentDay;
        lastWateredDay = -1;
        isWatered = false;
        daysWithoutWater = 0;

        // Add to planted crops list
        cropManager.AddToPlantedCrops(this);

        // Update tilemap visual - Show both soil AND crop
        UpdateTileVisual();

        UnityEngine.Debug.Log($"[CropBlock] Planted {seed.cropName} at {gridPosition}");
        return true;
    }

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

        HideHarvestReadyParticles();

        // Spawn harvestable (if you want physical drops)
        GameObject harvestable = null;
        if (seedPacket.harvestablePrefab != null)
        {
            Vector3 spawnPos = worldPosition;
            harvestable = Object.Instantiate(seedPacket.harvestablePrefab, spawnPos, Quaternion.identity);

            // Initialize the harvestable with the InventoryItem
            HarvestablePlant harvestScript = harvestable.GetComponent<HarvestablePlant>();
            if (harvestScript != null)
            {
                // Pass the InventoryItem from the SeedPacket
                harvestScript.Initialize(
                    seedPacket.harvestIcon,
                    seedPacket.cropName,
                    seedPacket.harvestedItem,  // â† Now passing the actual InventoryItem
                    seedPacket.harvestYield
                );
            }
        }

        UnityEngine.Debug.Log($"[CropBlock] Harvested {seedPacket.cropName} at {gridPosition}");

        // Clear the crop
        ClearCrop();

        return harvestable;
    }

    /// <summary>
    /// Update growth progress - Part 3 Requirement
    /// Growth now happens at sunrise (day-based), not frame-based
    /// This method kept for compatibility but no longer advances stages
    /// </summary>
    public void UpdateGrowth(float deltaTime, int currentDay)
    {
        // Day-based growth - stages advance at sunrise via OnSunrise()
        // This method intentionally does nothing now
    }

    /// <summary>
    /// Advance to next growth stage - Part 3 Requirement
    /// </summary>
    private void AdvanceStage()
    {
        if (currentGrowthStage >= 3) return;

        currentGrowthStage++;
        UpdateTileVisual();

        // NEW: Show harvest particles when reaching stage 3
        if (currentGrowthStage >= 3)
        {
            ShowHarvestReadyParticles();
        }

        UnityEngine.Debug.Log($"[CropBlock] {seedPacket.cropName} advanced to stage {currentGrowthStage}");
    }

    // NEW METHOD
    private void ShowHarvestReadyParticles()
    {
        UnityEngine.Debug.Log($"[CropBlock] === ShowHarvestReadyParticles called ===");
        UnityEngine.Debug.Log($"[CropBlock] seedPacket: {seedPacket?.cropName ?? "NULL"}");
        UnityEngine.Debug.Log($"[CropBlock] harvestReadyParticles prefab: {seedPacket?.harvestReadyParticles?.name ?? "NULL"}");

        if (seedPacket == null || seedPacket.harvestReadyParticles == null)
        {
            UnityEngine.Debug.LogWarning($"[CropBlock] ⚠️ No particle prefab assigned for {seedPacket?.cropName ?? "null crop"}");
            return;
        }

        // Don't create duplicate particles
        if (activeParticleInstance != null)
        {
            UnityEngine.Debug.Log($"[CropBlock] Particles already active for {seedPacket.cropName}");
            return;
        }

        Vector3 spawnPos = worldPosition + new Vector3(0, 0.5f, 0);
        spawnPos.z = 0; // Force to camera's view plane
        activeParticleInstance = Object.Instantiate(
            seedPacket.harvestReadyParticles,
            spawnPos,
            Quaternion.identity
        );

        UnityEngine.Debug.Log($"[CropBlock] ✨ Spawned harvest particles for {seedPacket.cropName} at {spawnPos}");
        UnityEngine.Debug.Log($"[CropBlock] Particle GameObject name: {activeParticleInstance.name}");
        UnityEngine.Debug.Log($"[CropBlock] Particle active: {activeParticleInstance.activeSelf}");

        // Check if it has a ParticleSystem component
        ParticleSystem ps = activeParticleInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            UnityEngine.Debug.Log($"[CropBlock] ParticleSystem found - Playing: {ps.isPlaying}, Particle Count: {ps.particleCount}");
        }
        else
        {
            UnityEngine.Debug.LogError($"[CropBlock] ⚠️ NO ParticleSystem component on instantiated prefab!");
        }
    }

    // NEW METHOD
    private void HideHarvestReadyParticles()
    {
        if (activeParticleInstance != null)
        {
            UnityEngine.Debug.Log($"[CropBlock] 💨 Destroying harvest particles for {seedPacket?.cropName}");
            Object.Destroy(activeParticleInstance);
            activeParticleInstance = null;
        }
    }

    /// <summary>
    /// Called at sunrise (6 AM) - advance stage if watered yesterday
    /// Day-based growth: One stage per day if watered
    /// </summary>
    public void OnSunrise(int currentDay)
    {
        if (!isPlanted) return;
        if (isWilted) return; // Already wilted, can't grow

        // Store yesterday's watered status BEFORE resetting
        bool wasWateredYesterday = isWatered;

        // Reset watered status for the new day
        isWatered = false;

        // Update soil visual to show dry
        UpdateTileVisual();

        UnityEngine.Debug.Log($"[CropBlock] === SUNRISE Day {currentDay} === {seedPacket.cropName} at {gridPosition}");
        UnityEngine.Debug.Log($"[CropBlock] Was Watered Yesterday: {wasWateredYesterday} | Current Stage: {currentGrowthStage}/3");

        // Check if crop requires water
        if (seedPacket.requiresWater && !wasWateredYesterday)
        {
            daysWithoutWater++;

            UnityEngine.Debug.Log($"[CropBlock] NOT watered - {daysWithoutWater} day(s) without water");

            // NEW: Harvestable crops (stage 3) never wilt - they stay harvestable forever
            if (currentGrowthStage >= 3)
            {
                UnityEngine.Debug.Log($"[CropBlock] Crop is harvestable - no wilting!");
                return; // Exit - harvestable crops don't wilt
            }

            // Wilt if past seed stage (stage > 0) and not watered
            if (currentGrowthStage > 0)
            {
                isWilted = true;
                UnityEngine.Debug.Log($"[CropBlock] â˜ ï¸ {seedPacket.cropName} has WILTED!");
                UpdateTileVisual();
                return; // Exit - crop is dead
            }
            else
            {
                // Seeds (stage 0) don't wilt, just don't grow
                UnityEngine.Debug.Log($"[CropBlock] ðŸŒ± Seed remains at stage 0 (needs water to grow)");
                return; // Exit - no growth
            }
        }

        // Reset wilting counter if watered
        if (wasWateredYesterday)
        {
            daysWithoutWater = 0;
        }

        // === ADVANCE STAGE if watered and not fully grown ===
        if (wasWateredYesterday && currentGrowthStage < 3)
        {
            currentGrowthStage++;
            growthTimer = 0f; // Reset timer (not used but kept for compatibility)

            UnityEngine.Debug.Log($"[CropBlock] âœ… {seedPacket.cropName} ADVANCED to stage {currentGrowthStage}!");

            // Update visual to show new stage
            UpdateTileVisual();

            if (currentGrowthStage >= 3)
            {
                UnityEngine.Debug.Log($"[CropBlock] ðŸŒ¾ {seedPacket.cropName} is now HARVESTABLE!");
                UnityEngine.Debug.Log($"[CropBlock] About to call ShowHarvestReadyParticles()...");
                ShowHarvestReadyParticles();
            }
        }
        else if (currentGrowthStage >= 3)
        {
            UnityEngine.Debug.Log($"[CropBlock] {seedPacket.cropName} is already harvestable (stage 3)");
        }
        else if (!wasWateredYesterday)
        {
            UnityEngine.Debug.Log($"[CropBlock] No growth - crop wasn't watered yesterday");
        }
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

    /// <summary>
    /// Clear this block for loading a save
    /// </summary>
    public void ClearForLoad()
    {
        isPlanted = false;
        isWilted = false;
        isTilled = false;
        isWatered = false;
        seedPacket = null;
        currentGrowthStage = 0;
        growthTimer = 0f;
        dayPlanted = 0;
        lastWateredDay = -1;
        daysWithoutWater = 0;

        HideHarvestReadyParticles();
        UpdateTileVisual();
    }

    public void RestoreState(SavedCropBlock savedBlock, SeedPacket packet)
    {
        if (savedBlock == null)
        {
            UnityEngine.Debug.LogError("[CropBlock] RestoreState called with null savedBlock!");
            return;
        }

        if (packet == null)
        {
            UnityEngine.Debug.LogError("[CropBlock] RestoreState called with null packet!");
            return;
        }

        UnityEngine.Debug.Log($"[CropBlock] Restoring {packet.cropName} at grid {gridPosition}");

        // Restore all state
        isTilled = savedBlock.isTilled;
        isWatered = savedBlock.isWatered;
        isPlanted = savedBlock.isPlanted;
        isWilted = savedBlock.isWilted;
        seedPacket = packet;
        currentGrowthStage = savedBlock.currentGrowthStage;
        growthTimer = savedBlock.growthTimer;
        dayPlanted = savedBlock.dayPlanted;
        lastWateredDay = savedBlock.lastWateredDay;
        daysWithoutWater = savedBlock.daysWithoutWater;

        // Update visual BEFORE adding to planted crops
        UpdateTileVisual();

        // Add to planted crops if needed
        if (isPlanted && cropManager != null)
        {
            cropManager.AddToPlantedCrops(this);
        }

        // Show particles if harvestable
        if (currentGrowthStage >= 3 && !isWilted)
        {
            ShowHarvestReadyParticles();
        }

        UnityEngine.Debug.Log($"[CropBlock] Successfully restored {packet.cropName} at stage {currentGrowthStage}");
    }
}