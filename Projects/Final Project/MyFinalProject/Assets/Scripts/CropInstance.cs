using UnityEngine;
using System.Collections;

/// <summary>
/// Individual crop instance with coroutine-based growth animations.
/// Advances stages at 6 AM each day if watered, visually grows from 6 AM to 6 PM.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CropInstance : MonoBehaviour
{
    [Header("Crop Configuration")]
    [SerializeField] private CropData cropData;

    [Header("Current State")]
    [SerializeField] private int currentStage = 0; // 0=Seed, 1=Sprout, 2=Growing, 3=Mature, 4=Harvestable
    [SerializeField] private int dayPlanted = 0;
    [SerializeField] private int lastWateredDay = 0;
    [SerializeField] private bool isWatered = false;
    [SerializeField] private bool isWilted = false;
    [SerializeField] private int daysSinceStageChange = 0;

    [Header("Visual State")]
    [SerializeField] private float currentScale = 1f;
    private Vector3 targetScale;

    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private Coroutine growthCoroutine;

    // Properties for save system
    public CropData CropData => cropData;
    public int CropID => cropData != null ? cropData.cropID : -1;
    public int CurrentStage => currentStage;
    public int DayPlanted => dayPlanted;
    public int LastWateredDay => lastWateredDay;
    public bool IsWatered => isWatered;
    public bool IsWilted => isWilted;
    public bool IsHarvestable => currentStage >= 4 && !isWilted;
    public Vector2Int GridPosition { get; private set; }

    // Events
    public System.Action<CropInstance> OnCropHarvested;
    public System.Action<CropInstance> OnCropWilted;
    public System.Action<CropInstance, int> OnStageChanged;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Subscribe to TimeManager sunrise event
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged += OnHourChanged;
            Debug.Log($"[CropInstance] Successfully subscribed to TimeManager OnHourChanged");
        }
        else
        {
            Debug.LogError($"[CropInstance] TimeManager.Instance is null in Awake! Cannot subscribe to events.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from TimeManager
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnHourChanged -= OnHourChanged;
        }

        // Stop any running coroutines
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
        }
    }

    /// <summary>
    /// Initialize a newly planted crop
    /// </summary>
    public void Plant(CropData data, int currentDay, Vector2Int gridPos)
    {
        cropData = data;
        dayPlanted = currentDay;
        lastWateredDay = currentDay;
        currentStage = 0;
        daysSinceStageChange = 0;
        isWatered = true;
        isWilted = false;
        GridPosition = gridPos;

        UpdateVisuals();

        // Start growth animation if it's daytime
        if (TimeManager.Instance != null && TimeManager.Instance.IsDaytime)
        {
            StartGrowthAnimation();
        }

        Debug.Log($"[CropInstance] Planted {cropData.cropName} at {gridPos} on day {currentDay}");
    }

    /// <summary>
    /// Load crop from save data
    /// </summary>
    public void LoadCrop(CropData data, int plantedDay, int wateredDay, int stage, bool watered, Vector2Int gridPos, int currentDay)
    {
        cropData = data;
        dayPlanted = plantedDay;
        lastWateredDay = wateredDay;
        currentStage = stage;
        isWatered = watered;
        GridPosition = gridPos;

        // Calculate days in current stage
        daysSinceStageChange = currentDay - (plantedDay + stage);

        // Check wilted status
        CheckWiltStatus(currentDay);

        UpdateVisuals();

        // Start growth animation if daytime
        if (TimeManager.Instance != null && TimeManager.Instance.IsDaytime)
        {
            StartGrowthAnimation();
        }
    }

    /// <summary>
    /// Called when TimeManager hour changes
    /// </summary>
    private void OnHourChanged(float time)
    {
        if (TimeManager.Instance == null) return;

        int currentHour = Mathf.FloorToInt(time);

        // Check for sunrise (6 AM) - new day begins, advance stage if conditions met
        if (currentHour == 6)
        {
            OnSunrise();
        }
        // Check for sunset (6 PM) - stop growth animation  
        else if (currentHour == 18)
        {
            OnSunset();
        }
    }

    /// <summary>
    /// Called at sunrise (6 AM) - advance growth stage if watered
    /// </summary>
    private void OnSunrise()
    {
        if (CropGrowthManager.Instance == null)
        {
            Debug.LogError("[CropInstance] CropGrowthManager.Instance is null!");
            return;
        }

        int currentDay = CropGrowthManager.Instance.CurrentDay;

        Debug.Log($"[CropInstance] {cropData.cropName} - Sunrise on day {currentDay}, isWatered: {isWatered}, isWilted: {isWilted}, currentStage: {currentStage}");

        // Check if crop should wilt
        CheckWiltStatus(currentDay);

        if (isWilted)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} is wilted!");
            return;
        }

        // Reset watered status for new day
        bool wasWateredYesterday = isWatered;
        isWatered = false;

        Debug.Log($"[CropInstance] Was watered yesterday: {wasWateredYesterday}");

        // Don't advance if already harvestable
        if (currentStage >= 4)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} already harvestable (stage 4)");
            return;
        }

        // Check if crop was watered yesterday
        if (cropData.requiresWater && !wasWateredYesterday)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} not watered yesterday, not advancing");
            return;
        }

        // Calculate days per stage (evenly distribute growth)
        int daysPerStage = Mathf.CeilToInt(cropData.totalGrowthDays / 4f);

        daysSinceStageChange++;

        Debug.Log($"[CropInstance] Days since stage change: {daysSinceStageChange}, Days per stage: {daysPerStage}");

        // Advance stage if enough days passed
        if (daysSinceStageChange >= daysPerStage)
        {
            AdvanceStage();
            daysSinceStageChange = 0;
        }

        // Start daytime growth animation
        StartGrowthAnimation();
    }

    /// <summary>
    /// Called at sunset (6 PM) - stop growth animation
    /// </summary>
    private void OnSunset()
    {
        // Stop growth animation at night
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }

        // Ensure sprite is at final scale
        transform.localScale = targetScale;
    }

    /// <summary>
    /// Advance to next growth stage
    /// </summary>
    private void AdvanceStage()
    {
        if (currentStage < 4)
        {
            currentStage++;
            OnStageChanged?.Invoke(this, currentStage);

            // Spawn particles if available
            if (cropData.stageAdvanceParticles != null)
            {
                Instantiate(cropData.stageAdvanceParticles, transform.position, Quaternion.identity);
            }

            UpdateVisuals();

            Debug.Log($"[CropInstance] {cropData.cropName} advanced to stage {currentStage}");
        }
    }

    /// <summary>
    /// Start growth animation coroutine (6 AM to 6 PM)
    /// </summary>
    private void StartGrowthAnimation()
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
        }

        growthCoroutine = StartCoroutine(GrowthAnimationCoroutine());
    }

    /// <summary>
    /// Coroutine that gradually grows the crop sprite from 6 AM to 6 PM
    /// </summary>
    private IEnumerator GrowthAnimationCoroutine()
    {
        if (TimeManager.Instance == null || cropData == null)
            yield break;

        // No visual growth for seed stage (stage 0 - invisible)
        if (currentStage == 0)
        {
            spriteRenderer.enabled = false;
            yield break;
        }

        // Enable sprite renderer for visible stages
        spriteRenderer.enabled = true;

        // Get stage-specific start scale
        float startScale = cropData.GetStageStartScale(currentStage);
        float endScale = cropData.finalScale;

        // Set initial scale for this stage
        currentScale = startScale;
        targetScale = Vector3.one * endScale;
        transform.localScale = Vector3.one * startScale;

        float sunrise = TimeManager.Instance.SunriseTime;
        float sunset = TimeManager.Instance.SunsetTime;
        float dayDuration = TimeManager.Instance.DayDuration;

        // Calculate real-time duration from sunrise to sunset
        // Daytime is 12 hours of game time (6 AM to 6 PM)
        float daytimeHours = sunset - sunrise; // 12 hours
        float daytimeRealSeconds = (daytimeHours / 24f) * dayDuration; // Real seconds for 12 hours

        Debug.Log($"[CropInstance] Stage {currentStage} growth animation: {startScale:F2} → {endScale:F2} over {daytimeRealSeconds:F1}s (at {TimeManager.Instance.TimeSpeedMultiplier}x speed)");

        float elapsedTime = 0f;

        while (elapsedTime < daytimeRealSeconds)
        {
            // Account for time speed multiplier
            elapsedTime += Time.deltaTime * TimeManager.Instance.TimeSpeedMultiplier;

            float progress = Mathf.Clamp01(elapsedTime / daytimeRealSeconds);

            // Smooth growth curve (ease out)
            float curvedProgress = 1f - Mathf.Pow(1f - progress, 2f);

            currentScale = Mathf.Lerp(startScale, endScale, curvedProgress);
            transform.localScale = Vector3.one * currentScale;

            yield return null;
        }

        // Ensure final scale is set
        transform.localScale = targetScale;
        growthCoroutine = null;
    }

    /// <summary>
    /// Water this crop
    /// </summary>
    public void Water(int currentDay)
    {
        if (isWilted)
        {
            Debug.Log($"[CropInstance] Cannot water wilted crop!");
            return;
        }

        if (isWatered)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} already watered today!");
            return;
        }

        isWatered = true;
        lastWateredDay = currentDay;
        UpdateVisuals();

        Debug.Log($"[CropInstance] Watered {cropData.cropName}");
    }

    /// <summary>
    /// Check if crop should wilt
    /// </summary>
    private void CheckWiltStatus(int currentDay)
    {
        if (!cropData.requiresWater || isWilted) return;

        int daysSinceWater = currentDay - lastWateredDay;

        if (daysSinceWater > cropData.daysWithoutWater)
        {
            isWilted = true;
            OnCropWilted?.Invoke(this);
            UpdateVisuals();

            // Stop growth animation
            if (growthCoroutine != null)
            {
                StopCoroutine(growthCoroutine);
                growthCoroutine = null;
            }

            Debug.Log($"[CropInstance] {cropData.cropName} wilted after {daysSinceWater} days without water!");
        }
    }

    /// <summary>
    /// Harvest this crop
    /// </summary>
    public HarvestResult Harvest()
    {
        if (!IsHarvestable)
        {
            Debug.LogWarning($"[CropInstance] {cropData.cropName} not ready to harvest!");
            return null;
        }

        HarvestResult result = new HarvestResult
        {
            itemID = cropData.harvestItemID,
            quantity = cropData.harvestQuantity,
            cropName = cropData.cropName
        };

        OnCropHarvested?.Invoke(this);

        // Multi-harvest crops regrow, others are destroyed
        if (cropData.multiHarvest)
        {
            currentStage = 2; // Reset to growing stage
            daysSinceStageChange = 0;
            UpdateVisuals();
            StartGrowthAnimation();
            Debug.Log($"[CropInstance] {cropData.cropName} harvested, regrowing...");
        }
        else
        {
            Debug.Log($"[CropInstance] {cropData.cropName} harvested and removed");
            Destroy(gameObject);
        }

        return result;
    }

    /// <summary>
    /// Update sprite and visual state
    /// </summary>
    private void UpdateVisuals()
    {
        if (cropData == null || spriteRenderer == null) return;

        // Get sprite for current stage
        Sprite stageSprite = cropData.GetStageSprite(currentStage);

        // Stage 0 (seed) has no sprite - disable renderer
        if (stageSprite == null)
        {
            spriteRenderer.enabled = false;
            return;
        }

        // Enable renderer and set sprite for visible stages
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = stageSprite;

        // Apply color
        if (isWilted)
        {
            spriteRenderer.color = cropData.wiltedColor;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }

        // Set initial scale based on current stage
        float startScale = cropData.GetStageStartScale(currentStage);
        transform.localScale = Vector3.one * startScale;
        targetScale = Vector3.one * cropData.finalScale;
    }

    // Debug gizmo
    private void OnDrawGizmosSelected()
    {
        if (cropData != null)
        {
            Gizmos.color = isWilted ? Color.red : (IsHarvestable ? Color.green : Color.yellow);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}

/// <summary>
/// Data returned when harvesting
/// </summary>
public class HarvestResult
{
    public int itemID;
    public int quantity;
    public string cropName;
}