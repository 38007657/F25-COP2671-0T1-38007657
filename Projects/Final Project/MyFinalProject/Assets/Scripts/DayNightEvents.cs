using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public enum LightingPreset
{
    Realistic
}

public class DayNightEvents : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField] private bool enableDayNightEvents = true;

    [Header("Preset Selection")]
    [SerializeField] private LightingPreset lightingPreset = LightingPreset.Realistic;

    [Header("Lighting Curves (for DayNightLighting script)")]
    [SerializeField] private AnimationCurve lightIntensityCurve;
    [SerializeField] private Gradient lightColorGradient;
    [SerializeField] private Gradient ambientColorGradient;

    private TimeManager timeManager;
    private DayNightLighting lightingController;
    private bool wasDay = true;

    // Events for other systems to subscribe to
    public System.Action OnSunrise;
    public System.Action OnSunset;
    public System.Action OnNoon;
    public System.Action OnMidnight;

    private void Awake()
    {
        ValidateAndInitializeCurves();
    }

    private void Start()
    {
        // Get components
        timeManager = GetComponent<TimeManager>();
        lightingController = GetComponent<DayNightLighting>();

        if (timeManager == null)
        {
            timeManager = TimeManager.Instance;
        }

        if (timeManager == null)
        {
            Debug.LogError("[DayNightEvents] TimeManager not found!");
            return;
        }

        if (lightingController == null)
        {
            Debug.LogError("[DayNightEvents] DayNightLighting component not found!");
            return;
        }

        //Make sure curves are initialized
        if (lightIntensityCurve == null || lightIntensityCurve.length == 0)
        {
            Debug.Log("[DayNightEvents] Applying default preset for 2D lighting...");
            ApplyPreset(lightingPreset);
        }

        // Pass curves to lighting controller
        lightingController.SetLightingCurves(lightIntensityCurve, lightColorGradient, ambientColorGradient);
        Debug.Log("[DayNightEvents] 2D lighting curves passed to controller");

        // Subscribe to time changes for events only
        timeManager.OnTimeChanged += CheckForDayNightEvents;
        timeManager.OnHourChanged += CheckForSpecificTimeEvents;

        wasDay = timeManager.IsDaytime;

        Debug.Log($"[DayNightEvents] 2D system initialized - Current time: {timeManager.CurrentTime:F2}");
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnTimeChanged -= CheckForDayNightEvents;
            timeManager.OnHourChanged -= CheckForSpecificTimeEvents;
        }
    }

    private void CheckForDayNightEvents(float currentTime)
    {
        if (!enableDayNightEvents) return;

        bool isCurrentlyDay = timeManager.IsDaytime;

        // Check for day/night transitions
        if (wasDay != isCurrentlyDay)
        {
            if (isCurrentlyDay)
            {
                Debug.Log("[DayNightEvents] Sunrise event triggered!");
                OnSunrise?.Invoke();
            }
            else
            {
                Debug.Log("[DayNightEvents] Sunset event triggered!");
                OnSunset?.Invoke();
            }

            wasDay = isCurrentlyDay;
        }
    }

    private void CheckForSpecificTimeEvents(float currentTime)
    {
        if (!enableDayNightEvents) return;

        int hour = Mathf.FloorToInt(currentTime);

        switch (hour)
        {
            case 12: // Noon
                Debug.Log("[DayNightEvents] Noon event triggered!");
                OnNoon?.Invoke();
                break;
            case 0: // Midnight
                Debug.Log("[DayNightEvents] Midnight event triggered!");
                OnMidnight?.Invoke();
                break;
        }
    }

    private void ValidateAndInitializeCurves()
    {
        // Check if curves need initialization
        if (lightIntensityCurve == null || lightIntensityCurve.length == 0 ||
            lightColorGradient == null || lightColorGradient.colorKeys.Length == 0 ||
            ambientColorGradient == null || ambientColorGradient.colorKeys.Length == 0)
        {
            Debug.Log("[DayNightEvents] Initializing 2D lighting curves...");
            ApplyPreset(lightingPreset);
        }
    }

    private void ApplyPreset(LightingPreset preset)
    {
        switch (preset)
        {
            case LightingPreset.Realistic:
                SetRealisticPreset();
                break;
        }

        // Update lighting controller if it exists
        if (lightingController != null)
        {
            lightingController.SetLightingCurves(lightIntensityCurve, lightColorGradient, ambientColorGradient);
        }
    }

    private void SetRealisticPreset()
    {
        lightIntensityCurve = new AnimationCurve(new Keyframe[]
        {
            new Keyframe(0f, 0.1f),    // Night
            new Keyframe(0.25f, 0.2f), // Sunrise
            new Keyframe(0.3f, 0.7f),  // Morning
            new Keyframe(0.5f, 1f),    // Noon
            new Keyframe(0.7f, 0.7f),  // Evening
            new Keyframe(0.75f, 0.2f), // Sunset
            new Keyframe(1f, 0.1f)     // Night
        });

        lightColorGradient = new Gradient();
        lightColorGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.4f, 0.4f, 0.8f), 0f),     // Night - blue
            new GradientColorKey(new Color(1f, 0.6f, 0.4f), 0.25f),    // Sunrise - orange
            new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0.35f),   // Morning - warm white
            new GradientColorKey(Color.white, 0.5f),                    // Noon - white
            new GradientColorKey(new Color(1f, 0.7f, 0.5f), 0.75f),    // Sunset - warm orange
            new GradientColorKey(new Color(0.4f, 0.4f, 0.8f), 1f)      // Night - blue
        };
        lightColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

        ambientColorGradient = new Gradient();
        ambientColorGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0f),   // Night
            new GradientColorKey(new Color(0.5f, 0.4f, 0.4f), 0.23f), // Dawn
            new GradientColorKey(new Color(0.7f, 0.7f, 0.7f), 0.5f),  // Day
            new GradientColorKey(new Color(0.5f, 0.4f, 0.4f), 0.77f), // Dusk
            new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 1f)    // Night
        };
        ambientColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };        
    }

    [ContextMenu("Apply Current Preset")]
    public void ApplyCurrentPreset()
    {
        ApplyPreset(lightingPreset);
    }
}