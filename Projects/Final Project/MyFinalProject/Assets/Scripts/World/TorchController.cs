using UnityEngine;

/// <summary>
/// Controls torch and fire animations based on day/night cycle
/// </summary>
public class TorchController : MonoBehaviour
{
    [Header("Animation Components")]
    [SerializeField] private Animator torchAnimator;
    [SerializeField] private GameObject fireObject;
    [SerializeField] private Animator fireAnimator;

    [Header("Animation Parameters")]
    [SerializeField] private string nightParameterName = "IsLit";

    [Header("Optional Light (Glow Effect)")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D torchLight;
    [SerializeField] private bool useLightComponent = true;
    [SerializeField] private float lightIntensity = 1.5f;
    [SerializeField] private Color fireColor = new Color(1f, 0.5f, 0.2f); // Orange-red

    [Header("Flicker Effect (Optional)")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float flickerSpeed = 0.1f;
    [SerializeField] private float flickerAmount = 0.3f;

    private bool isLit = false;
    private float baseIntensity;
    private float flickerTimer = 0f;

    private void Start()
    {
        // Auto-find components if not assigned
        if (torchAnimator == null)
        {
            torchAnimator = GetComponent<Animator>();
        }

        if (fireObject == null)
        {
            // Try to find fire child object
            Transform fireTransform = transform.Find("Fire");
            if (fireTransform != null)
            {
                fireObject = fireTransform.gameObject;
                fireAnimator = fireObject.GetComponent<Animator>();
            }
        }

        if (torchLight == null && useLightComponent)
        {
            torchLight = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        }

        baseIntensity = lightIntensity;

        // Subscribe to day/night events
        DayNightEvents events = FindObjectOfType<DayNightEvents>();
        if (events != null)
        {
            events.OnSunset += LightTorch;
            events.OnSunrise += ExtinguishTorch;
        }

        // Set initial state based on current time
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.IsNighttime)
            {
                LightTorch();
            }
            else
            {
                ExtinguishTorch();
            }
        }
        else
        {
            ExtinguishTorch();
        }

        Debug.Log($"[TorchController] Initialized - Torch: {torchAnimator != null}, Fire: {fireObject != null}");
    }

    private void Update()
    {
        // Add flickering effect if enabled and torch is lit
        if (enableFlicker && isLit && torchLight != null && torchLight.enabled)
        {
            flickerTimer += Time.deltaTime;

            if (flickerTimer >= flickerSpeed)
            {
                flickerTimer = 0f;
                float flicker = Random.Range(-flickerAmount, flickerAmount);
                torchLight.intensity = baseIntensity + flicker;
            }
        }
    }

    /// <summary>
    /// Light the torch at night - play fire animation
    /// </summary>
    private void LightTorch()
    {
        isLit = true;

        // Trigger torch animation (if torch itself animates when lit)
        if (torchAnimator != null)
        {
            torchAnimator.SetBool(nightParameterName, true);
        }

        // Enable and play fire animation
        if (fireObject != null)
        {
            fireObject.SetActive(true);

            if (fireAnimator != null)
            {
                fireAnimator.enabled = true;
            }
        }

        // Optional: Enable light glow
        if (useLightComponent && torchLight != null)
        {
            torchLight.enabled = true;
            torchLight.intensity = baseIntensity;
            torchLight.color = fireColor;
        }

        Debug.Log($"[TorchController] 🔥 Torch lit at {transform.position}");
    }

    /// <summary>
    /// Extinguish torch during day - stop fire animation
    /// </summary>
    private void ExtinguishTorch()
    {
        isLit = false;

        // Turn off torch animation
        if (torchAnimator != null)
        {
            torchAnimator.SetBool(nightParameterName, false);
        }

        // Disable fire
        if (fireObject != null)
        {
            fireObject.SetActive(false);
        }

        // Optional: Disable light
        if (torchLight != null)
        {
            torchLight.enabled = false;
        }

        Debug.Log($"[TorchController] 💨 Torch extinguished at {transform.position}");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        DayNightEvents events = FindObjectOfType<DayNightEvents>();
        if (events != null)
        {
            events.OnSunset -= LightTorch;
            events.OnSunrise -= ExtinguishTorch;
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (torchLight != null)
        {
            Gizmos.color = fireColor;
            Gizmos.DrawWireSphere(transform.position, torchLight.pointLightOuterRadius);
        }
    }
}