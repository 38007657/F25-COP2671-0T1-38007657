using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

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

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement (Optional)")]
    [SerializeField] private bool enableBounce = true;
    [SerializeField] private float bounceForce = 3f;

    private Transform playerTransform;
    private bool canBePickedUp = false;

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Delay pickup slightly so it doesn't instantly get picked up
        Invoke(nameof(EnablePickup), 0.3f);
    }

    private void EnablePickup()
    {
        canBePickedUp = true;
    }

    private void Update()
    {
        if (!canBePickedUp || playerTransform == null) return;

        // Check distance to player
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
        Debug.Log($"[HarvestablePlant] Picked up {quantity}x {plantName}");

        // TODO: Add to inventory here
        // Example: InventoryManager.Instance.AddItem(plantName, quantity);

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