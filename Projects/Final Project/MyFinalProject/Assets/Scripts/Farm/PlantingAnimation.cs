using UnityEngine;
using System.Collections;

/// <summary>
/// Plays a planting animation showing the seed sprite shrinking into the ground
/// </summary>
public class PlantingAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float yOffsetStart = 0.5f; // Starts above ground
    [SerializeField] private float yOffsetEnd = -0.2f; // Ends slightly below ground

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 startScale;

    /// <summary>
    /// Initialize and play the planting animation
    /// </summary>
    public void PlayPlantingAnimation(Sprite seedSprite, Vector3 worldPosition, System.Action onComplete = null)
    {
        // Setup sprite
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = seedSprite;
        spriteRenderer.sortingLayerName = "Default"; // Adjust if needed
        spriteRenderer.sortingOrder = 100; // Above everything

        // Set positions
        startPosition = worldPosition + Vector3.up * yOffsetStart;
        endPosition = worldPosition + Vector3.up * yOffsetEnd;
        transform.position = startPosition;

        // Set scale (make it small)
        startScale = Vector3.one * 0.5f; // Start at 50% size
        transform.localScale = startScale;

        // Start animation
        StartCoroutine(AnimatePlanting(onComplete));
    }

    private IEnumerator AnimatePlanting(System.Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Apply curve to scale
            float scaleMultiplier = scaleCurve.Evaluate(t);
            transform.localScale = startScale * scaleMultiplier;

            // Move down
            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        // Animation complete
        onComplete?.Invoke();

        // Destroy this GameObject
        Destroy(gameObject);
    }

    /// <summary>
    /// Static helper to easily create planting animation
    /// </summary>
    public static void PlayAt(Sprite seedSprite, Vector3 worldPosition, System.Action onComplete = null)
    {
        GameObject animObj = new GameObject("PlantingAnimation");
        PlantingAnimation anim = animObj.AddComponent<PlantingAnimation>();
        anim.PlayPlantingAnimation(seedSprite, worldPosition, onComplete);
    }
}