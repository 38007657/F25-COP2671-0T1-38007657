using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Simple floating text for treasure chest - attached as child of chest
/// </summary>
public class TreasureChestFloatingText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("Animation")]
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float floatDistance = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Vector3 startPosition;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Get or add canvas group
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        startPosition = transform.localPosition;

        // Start hidden
        canvasGroup.alpha = 0;
    }

    /// <summary>
    /// Show the floating text with coin amount
    /// </summary>
    public void Show(int coinAmount)
    {
        if (textComponent != null)
        {
            textComponent.text = $"+{coinAmount} Coins!";
        }

        // Reset position
        transform.localPosition = startPosition;

        // Start animation
        StopAllCoroutines();
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float elapsed = 0f;

        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / showDuration;

            // Float upward
            Vector3 newPos = startPosition + Vector3.up * (floatDistance * t);
            transform.localPosition = newPos;

            // Fade in then out
            canvasGroup.alpha = fadeCurve.Evaluate(t);

            yield return null;
        }

        // Hide at the end
        canvasGroup.alpha = 0;
        transform.localPosition = startPosition;
    }
}