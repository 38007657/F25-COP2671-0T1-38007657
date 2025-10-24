using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightLighting : MonoBehaviour
{
    [Header("2D Light Reference")]
    [SerializeField] private Light2D globalLight2D;

    // These will be set by DayNightEvents
    private AnimationCurve lightIntensityCurve;
    private Gradient lightColorGradient;
    private Gradient ambientColorGradient;

    private TimeManager timeManager;

    private void Awake()
    {
        // Find 2D light if not assigned
        if (globalLight2D == null)
        {
            globalLight2D = FindAnyObjectByType<Light2D>();

            if (globalLight2D == null)
            {
                //Debug.LogError("[DayNightLighting] No Light2D found! Please create a Global Light 2D.");
            }
        }
    }

    private void Start()
    {
        // Get TimeManager
        timeManager = GetComponent<TimeManager>();
        if (timeManager == null)
        {
            timeManager = TimeManager.Instance;
        }

        if (timeManager == null)
        {
            //Debug.LogError("[DayNightLighting] TimeManager not found!");
            return;
        }

        // Subscribe to time changes
        timeManager.OnTimeChanged += UpdateLighting;

        // Initial lighting update
        UpdateLighting(timeManager.CurrentTime);

        //Debug.Log("[DayNightLighting] 2D Lighting system initialized");
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnTimeChanged -= UpdateLighting;
        }
    }

    // Called by DayNightEvents to set the curves
    public void SetLightingCurves(AnimationCurve intensityCurve, Gradient colorGradient, Gradient ambientGradient)
    {
        lightIntensityCurve = intensityCurve;
        lightColorGradient = colorGradient;
        ambientColorGradient = ambientGradient;

        //Debug.Log("[DayNightLighting] 2D lighting curves set");
    }

    private void UpdateLighting(float currentTime)
    {
        if (globalLight2D == null)
        {
            //Debug.LogWarning("[DayNightLighting] Global Light 2D is null!");
            return;
        }

        // Get normalized time (0-1)
        float normalizedTime = timeManager.CurrentTimeNormalized;

        //Debug.Log($"[DayNightLighting] Updating 2D lighting - Time: {currentTime:F2}, Normalized: {normalizedTime:F2}");

        // Update light intensity
        if (lightIntensityCurve != null && lightIntensityCurve.length > 0)
        {
            float intensity = lightIntensityCurve.Evaluate(normalizedTime);
            globalLight2D.intensity = intensity;
            //Debug.Log($"[DayNightLighting] Light intensity: {intensity:F2}");
        }

        // Update light color
        if (lightColorGradient != null)
        {
            Color color = lightColorGradient.Evaluate(normalizedTime);
            globalLight2D.color = color;
            //Debug.Log($"[DayNightLighting] Light color: {color}");
        }

        // Update ambient lighting (still works with 2D)
        if (ambientColorGradient != null)
        {
            Color ambientColor = ambientColorGradient.Evaluate(normalizedTime);
            RenderSettings.ambientLight = ambientColor;
        }
    }
}