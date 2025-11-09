using UnityEngine;

/// <summary>
/// Physical pickup that spawns when crops are harvested
/// </summary>
public class HarvestablePlant : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private string plantName;
    [SerializeField] private int quantity = 1;
    [SerializeField] private float pickupRange = 1.5f;
    [SerializeField] private bool autoPickup = true;

    [Header("Pickup Delay")]
    [Tooltip("Delay in seconds before item can be picked up")]
    [SerializeField] private float pickupDelay = 3f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Inventory")]
    [Tooltip("The inventory item this pickup represents")]
    [SerializeField] private InventoryItem inventoryItem;

    private Transform playerTransform;
    private bool canBePickedUp = false;
    private float pickupTimer = 0f;

    /// <summary>
    /// Initialize the harvestable pickup with icon sprite and inventory item
    /// </summary>
    public void Initialize(Sprite iconSprite, string cropName, InventoryItem item, int amount = 1)
    {
        if (spriteRenderer != null && iconSprite != null)
        {
            spriteRenderer.sprite = iconSprite;
        }

        plantName = cropName;
        quantity = amount;
        inventoryItem = item;

        // Start pickup delay timer
        pickupTimer = pickupDelay;

        Debug.Log($"[HarvestablePlant] Initialized {cropName}, will be pickupable in {pickupDelay}s");
    }

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        // Handle pickup delay timer
        if (!canBePickedUp)
        {
            pickupTimer -= Time.deltaTime;
            if (pickupTimer <= 0f)
            {
                canBePickedUp = true;
                Debug.Log($"[HarvestablePlant] {plantName} can now be picked up!");
            }
            return;
        }

        // Check if player is in range
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= pickupRange && autoPickup)
        {
            Pickup();
        }
    }

    /// <summary>
    /// Called when player picks up the item
    /// </summary>
    public void Pickup()
    {
        if (inventoryItem == null)
        {
            Debug.LogWarning($"[HarvestablePlant] No InventoryItem assigned for {plantName}!");
            Destroy(gameObject);
            return;
        }

        Debug.Log($"[HarvestablePlant] Picked up {quantity}x {plantName}");

        // TODO: Add to player inventory here (we'll do this in the next step)
        // Example: PlayerInventory.Instance.AddItem(inventoryItem, quantity);

        // Destroy pickup
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canBePickedUp) return;

        if (collision.CompareTag("Player"))
        {
            Pickup();
        }
    }

    // Optional: Draw pickup range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}