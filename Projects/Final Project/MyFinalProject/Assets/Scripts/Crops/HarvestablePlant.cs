using System;
using System.Collections;
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

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Bounce Animation")]
    [SerializeField] private float bounceHeight = 0.8f;
    [SerializeField] private float bounceDuration = 0.5f;
    [SerializeField] private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Transform playerTransform;
    private bool canBePickedUp = false;
    private Vector3 groundPosition;

    /// <summary>
    /// Initialize the harvestable pickup with icon sprite
    /// </summary>
    public void Initialize(Sprite iconSprite, string cropName, int amount = 1)
    {
        if (spriteRenderer != null && iconSprite != null)
        {
            spriteRenderer.sprite = iconSprite;
        }

        plantName = cropName;
        quantity = amount;

        // Store ground position
        groundPosition = transform.position;

        // Start bounce animation
        StartCoroutine(BounceAnimation());
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

    /// <summary>
    /// Bounce animation when spawned
    /// </summary>
    private IEnumerator BounceAnimation()
    {
        float elapsed = 0f;
        Vector3 startPos = groundPosition + Vector3.up * bounceHeight;
        transform.position = startPos;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            float curveValue = bounceCurve.Evaluate(t);

            // Move from high position down to ground
            transform.position = Vector3.Lerp(startPos, groundPosition, curveValue);

            yield return null;
        }

        // Ensure we end exactly at ground position
        transform.position = groundPosition;

        // Enable pickup after bounce completes
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