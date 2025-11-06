using UnityEngine;

[CreateAssetMenu(fileName = "New Seed Packet", menuName = "Farm/Seed Packet")]
public class SeedPacket : ScriptableObject
{
    [Header("Crop Identity")]
    public string cropName;

    [Header("Visual")]
    [Tooltip("Icon shown in toolbar/inventory")]
    public Sprite coverImage;

    [Tooltip("4 growth stage sprites (Stage 0=Seed, 1=Sprout, 2=Growing, 3=Harvestable)")]
    public Sprite[] growthSprites = new Sprite[4];

    [Tooltip("Sprite shown when crop is wilted/dead")]
    public Sprite wiltedSprite;

    [Header("Harvest")]
    [Tooltip("Prefab to spawn when harvesting (optional)")]
    public GameObject harvestablePrefab;

    [Tooltip("Particle effect shown when crop is ready to harvest")]
    public GameObject harvestReadyParticles;

    [Header("Growth Settings")]
    [Tooltip("Total days from seed to harvest (minimum 3 days if watered each day)")]
    [Range(3, 10)]
    public int totalGrowthDays = 3;

    [Header("Requirements")]
    [Tooltip("Crop must be watered each day to advance to next stage")]
    public bool requiresWater = true;
}