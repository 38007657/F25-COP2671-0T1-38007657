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
    }

    private void LateUpdate()
    {
        float yPos = sortingReference.position.y;
        int newOrder = Mathf.RoundToInt(yPos * sortingOrderScale);
        spriteRenderer.sortingOrder = newOrder;
    }
}