using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Farm/Inventory Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Item Identity")]
    [Tooltip("Name of the item (e.g., 'Tomato', 'Carrot')")]
    public string itemName;

    [Header("Visual")]
    [Tooltip("Icon shown in inventory UI")]
    public Sprite itemIcon;

    [Header("Economy")]
    [Tooltip("Sell value per unit when selling to shop")]
    public int sellValue = 10;

    [Tooltip("Can this item be sold to the shop?")]
    public bool isAvailableInShop = true;

    [Header("References")]
    [Tooltip("Optional: Link back to the seed packet that grows this crop")]
    public SeedPacket sourceSeedPacket;
}