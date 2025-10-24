using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Farm/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Crop Identity")]
    [Tooltip("Unique ID for save system (0-9 for your 10 crop types)")]
    public int cropID;

    [Tooltip("Display name of the crop")]
    public string cropName;

    [Header("Growth Settings - 4 Stages")]
    [Tooltip("Sprite for Stage 0: Seed (planted state)")]
    public Sprite seedSprite;

    [Tooltip("Sprite for Stage 1: Sprout")]
    public Sprite sproutSprite;

    [Tooltip("Sprite for Stage 2: Growing")]
    public Sprite growingSprite;

    [Tooltip("Sprite for Stage 3: Mature")]
    public Sprite matureSprite;

    [Tooltip("Sprite for Stage 4: Harvestable (final stage)")]
    public Sprite harvestableSprite;

    [Header("Growth Timing")]
    [Tooltip("Total days from seed to harvest (2 for fast crops, 4 for slow crops)")]
    [Range(2, 10)]
    public int totalGrowthDays = 2;

    [Tooltip("Growth speed: Fast (2 days) or Slow (4 days)")]
    public CropSpeed growthSpeed = CropSpeed.Fast;

    [Header("Water Requirements")]
    [Tooltip("Crop must be watered each day to advance to next stage")]
    public bool requiresWater = true;

    [Tooltip("Days crop can survive without water before wilting")]
    [Range(0, 3)]
    public int daysWithoutWater = 1;

    [Header("Harvest Data")]
    [Tooltip("Item ID for inventory system")]
    public int harvestItemID;

    [Tooltip("Quantity produced per harvest")]
    [Range(1, 10)]
    public int harvestQuantity = 1;

    [Tooltip("Can harvest multiple times (regrows after harvest)")]
    public bool multiHarvest = false;

    [Tooltip("Days to regrow if multi-harvest")]
    public int regrowthDays = 1;

    [Header("Visual Settings")]
    [Tooltip("Sprite shown when crop needs water (optional)")]
    public Sprite needsWaterSprite;

    [Tooltip("Color when crop is wilted/dead")]
    public Color wiltedColor = new Color(0.5f, 0.4f, 0.3f, 1f);

    [Tooltip("Scale multiplier for seed sprite (starts small)")]
    [Range(0.1f, 1f)]
    public float seedScale = 0.3f;

    [Tooltip("Final scale at each stage (crops grow to this size during daytime)")]
    [Range(0.8f, 1.5f)]
    public float finalScale = 1f;

    [Header("Animation Settings")]
    [Tooltip("How long the growth animation takes (6 AM to 6 PM = daytime hours)")]
    public bool growDuringDaytime = true;

    [Tooltip("Optional: Particle effect when stage advances")]
    public GameObject stageAdvanceParticles;

    [Header("Seasonal Restrictions (Optional)")]
    [Tooltip("Can only be planted in these seasons (empty = any season)")]
    public Season[] allowedSeasons;

    // Helper methods
    public Sprite GetStageSprite(int stage)
    {
        switch (stage)
        {
            case 0: return seedSprite;
            case 1: return sproutSprite;
            case 2: return growingSprite;
            case 3: return matureSprite;
            case 4: return harvestableSprite;
            default: return seedSprite;
        }
    }

    public int GetTotalStages() => 5; // Seed, Sprout, Growing, Mature, Harvestable

    public bool CanPlantInSeason(Season season)
    {
        if (allowedSeasons == null || allowedSeasons.Length == 0)
            return true;

        foreach (Season s in allowedSeasons)
        {
            if (s == season) return true;
        }
        return false;
    }

    private void OnValidate()
    {
        // Auto-set total growth days based on speed
        if (growthSpeed == CropSpeed.Fast)
        {
            totalGrowthDays = 2;
        }
        else if (growthSpeed == CropSpeed.Slow)
        {
            totalGrowthDays = 4;
        }

        // Validate crop ID
        if (cropID < 0 || cropID > 9)
        {
            Debug.LogWarning($"[{cropName}] Crop ID should be 0-9!");
        }
    }
}

public enum CropSpeed
{
    Fast,   // 2 days
    Slow    // 4 days
}

public enum Season
{
    Spring,
    Summer,
    Fall,
    Winter
}