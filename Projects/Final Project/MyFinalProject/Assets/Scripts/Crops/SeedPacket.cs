using UnityEngine;

/// <summary>
/// Growth type for crops - determines how they advance through stages
/// </summary>
public enum GrowthType
{
    DayBased,   // Advances 1 stage per day at sunrise (if watered)
    HourBased   // Advances based on hours elapsed (fast crops)
}

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
    [Tooltip("Icon shown when crop becomes a collectible pickup")]
    public Sprite harvestIcon;

    [Tooltip("Prefab to spawn when harvesting (optional)")]
    public GameObject harvestablePrefab;

    [Tooltip("Particle effect shown when crop is ready to harvest")]
    public GameObject harvestReadyParticles;

    [Header("Growth Settings")]
    [Tooltip("How this crop grows: DayBased (traditional) or HourBased (fast crops)")]
    public GrowthType growthType = GrowthType.DayBased;

    [Tooltip("FOR DAY-BASED: Total days from seed to harvest (minimum 1 day if watered each day)")]
    [Range(1, 10)]
    public int totalGrowthDays = 3;

    [Tooltip("FOR HOUR-BASED: Hours needed to advance ONE stage (4 stages total, so 8 hours/stage = 32 hours total)")]
    [Range(1f, 24f)]
    public float hoursPerStage = 8f;

    [Header("Requirements")]
    [Tooltip("Crop must be watered each day to advance to next stage")]
    public bool requiresWater = true;

    // === ECONOMY FIELDS ===
    [Header("Economy")]
    [Tooltip("Number of seeds in one packet")]
    public int seedsPerPacket = 5;

    [Tooltip("Cost to purchase this seed packet")]
    public int packetCost = 50;

    [Tooltip("Is this seed packet available for purchase in the shop?")]
    public bool isAvailableInShop = true;

    [Tooltip("How many crops does one seed yield when harvested?")]
    [Range(1, 10)]
    public int harvestYield = 1;

    [Tooltip("The inventory item created when this crop is harvested")]
    public InventoryItem harvestedItem;
}