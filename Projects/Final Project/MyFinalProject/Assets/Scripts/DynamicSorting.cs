using UnityEngine;

public class DynamicSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float sortingOrderScale = -100f;
    [SerializeField] private Transform sortingReference;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (sortingReference == null && transform.parent != null)
        {
            sortingReference = transform.parent;
        }
        else if (sortingReference == null)
        {
            sortingReference = transform;
        }

        //Debug.Log($"[DynamicSorting] Setup on {gameObject.name}");
        //Debug.Log($"[DynamicSorting] Using reference: {sortingReference.name}");
        //Debug.Log($"[DynamicSorting] Sprite Renderer Sorting Layer: {spriteRenderer.sortingLayerName}");
    }

    private void LateUpdate()
    {
        float yPos = sortingReference.position.y;
        int newOrder = Mathf.RoundToInt(yPos * sortingOrderScale);
        spriteRenderer.sortingOrder = newOrder;

        // Debug every 30 frames to avoid spam
        if (Time.frameCount % 30 == 0)
        {
            //Debug.Log($"Y: {yPos:F2} | Order: {newOrder} | Layer: {spriteRenderer.sortingLayerName}");
        }
    }
}