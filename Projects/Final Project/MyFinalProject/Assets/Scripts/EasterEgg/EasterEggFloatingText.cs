using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Floating text that appears when the professor easter egg is activated
/// Similar to TreasureChestFloatingText but with more dramatic effects
/// </summary>
public class EasterEggFloatingText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("Animation")]
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private float floatDistance = 2f;
    [SerializeField] private AnimationCurve fadeCurve; // Will be set in Awake if null
    [SerializeField] private AnimationCurve scaleCurve; // Optional: make text pulse

    [Header("Colors")]
    [SerializeField] private Gradient colorGradient; // Rainbow or gold gradient

    private Vector3 startPosition;
    private CanvasGroup canvasGroup;
    private Vector3 startScale;

    private void Awake()
    {
        // Get or add canvas group for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        startPosition = transform.localPosition;
        startScale = transform.localScale;

        // Start hidden
        canvasGroup.alpha = 0;

        // Setup default fade curve if not assigned - ensures full visibility
        if (fadeCurve == null || fadeCurve.length == 0)
        {
            fadeCurve = new AnimationCurve(
                new Keyframe(0f, 0f),      // Start invisible
                new Keyframe(0.2f, 1f),    // Quickly fade in to FULL opacity
                new Keyframe(0.7f, 1f),    // Stay fully visible for most of duration
                new Keyframe(1f, 0f)       // Fade out at end
            );
        }

        // Setup default scale curve if not assigned
        if (scaleCurve == null || scaleCurve.length == 0)
        {
            // Creates a "pop in" effect
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 1.2f),
                new Keyframe(0.4f, 1f),
                new Keyframe(1f, 1f)
            );
        }

        // Setup default color gradient if not assigned
        if (colorGradient == null || colorGradient.colorKeys.Length == 0)
        {
            colorGradient = new Gradient();
            colorGradient.colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(new Color(1f, 0.84f, 0f), 0.5f), // Gold
                new GradientColorKey(Color.white, 1f)
            };
            // Make sure alpha is FULL
            colorGradient.alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };
        }
    }

    /// <summary>
    /// Show the floating text with a message
    /// </summary>
    public void Show(string message)
    {
        if (textComponent != null)
        {
            textComponent.text = message;
        }

        // Reset position
        transform.localPosition = startPosition;
        transform.localScale = startScale;

        // Start animation
        StopAllCoroutines();
        StartCoroutine(AnimateText());
    }

    /// <summary>
    /// Show with coin reward display
    /// </summary>
    public void ShowWithReward(int coinAmount)
    {
        Show($"COP2671 Rocks!\n+{coinAmount} Coins!");
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

            // Scale animation (pop effect)
            float scaleMultiplier = scaleCurve.Evaluate(t);
            transform.localScale = startScale * scaleMultiplier;

            // Color change over time (rainbow/gold effect)
            if (textComponent != null)
            {
                textComponent.color = colorGradient.Evaluate(t);
            }

            yield return null;
        }

        // Make sure it's hidden at the end
        canvasGroup.alpha = 0;
        transform.localPosition = startPosition;
        transform.localScale = startScale;
    }

    /// <summary>
    /// Manual test from inspector
    /// </summary>
    [ContextMenu("Test Animation")]
    private void TestAnimation()
    {
        Show("🎉 TEST MESSAGE! 🎉");
    }
}