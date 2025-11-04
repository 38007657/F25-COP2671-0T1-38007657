using UnityEngine;

[CreateAssetMenu(fileName = "New Seed Packet", menuName = "Farm/Seed Packet")]
public class SeedPacket : ScriptableObject
{
    [Header("Crop Identity")]
    public string cropName;

    [Header("Visual")]
    [Tooltip("Icon shown in toolbar/inventory")]
    public Sprite coverImage;

    [Tooltip("4 growth stage sprites (Stage 0, 1, 2, 3)")]
    public Sprite[] growthSprites = new Sprite[4];

    [Tooltip("Sprite shown when crop is wilted/dead")]
    public Sprite wiltedSprite;

    [Header("Harvest")]
    [Tooltip("Prefab to spawn when harvesting (optional)")]
    public GameObject harvestablePrefab;

    [Header("Growth Settings")]
    [Tooltip("Time in seconds for each growth stage")]
    public float[] stageDurations = new float[4] { 2f, 2f, 2f, 2f };

    [Header("Requirements")]
    public bool requiresWater = true;
}