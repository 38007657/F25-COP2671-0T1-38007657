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
    [SerializeField] private int lastWateredDay = -1;
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
        lastWateredDay = -1; // Crop has never been watered
        currentStage = 1;
        daysSinceStageChange = 0;
        isWatered = false; // Must water after planting
        isWilted = false;
        GridPosition = gridPos;

        // Set initial scale to stage 1 start scale (small, visible seed/sprout)
        float startScale = cropData.GetStageStartScale(1);
        transform.localScale = Vector3.one * startScale;
        targetScale = Vector3.one * startScale; // Start with small target - will only grow when watered

        UpdateVisuals();

        // DO NOT start growth animation when planted - sprite stays at small scale until next sunrise
        Debug.Log($"[CropInstance] Planted {cropData.cropName} at {gridPos} on day {currentDay} - showing at scale {startScale}");
    }

    /// <summary>
    /// Load crop from save data
    /// </summary>
    public void LoadCrop(CropData data, int plantedDay, int wateredDay, int stage, bool watered, bool wilted, Vector2Int gridPos, int currentDay)
    {
        cropData = data;
        dayPlanted = plantedDay;
        lastWateredDay = wateredDay;
        currentStage = stage;
        isWatered = watered;
        isWilted = wilted;
        GridPosition = gridPos;

        // Calculate days in current stage
        daysSinceStageChange = currentDay - (plantedDay + stage);

        UpdateVisuals();

        // Start growth animation if daytime AND not wilted
        if (TimeManager.Instance != null && TimeManager.Instance.IsDaytime && !isWilted)
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

        // If already wilted, it stays dead
        if (isWilted)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} is already dead/wilted.");
            return;
        }

        // Store yesterday's watered status before resetting
        bool wasWateredYesterday = isWatered;

        // Reset watered status for new day
        isWatered = false;

        Debug.Log($"[CropInstance] Was watered yesterday: {wasWateredYesterday}");

        // Don't advance if already harvestable
        if (currentStage >= 4)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} already harvestable (stage 4)");
            return;
        }

        // NEW: If crop requires water and wasn't watered yesterday, it dies immediately
        if (cropData.requiresWater && !wasWateredYesterday)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} not watered yesterday - DYING!");
            isWilted = true;
            OnCropWilted?.Invoke(this);

            // Notify the FarmPlot that this crop is dead so it can be hoed
            if (FarmPlotManager.Instance != null)
            {
                FarmPlot plot = FarmPlotManager.Instance.GetPlotAtPosition(GridPosition);
                if (plot != null)
                {
                    plot.SetCropDead();
                    Debug.Log($"[CropInstance] Notified plot at {GridPosition} that crop is dead");
                }
            }

            UpdateVisuals();

            // Set to small wilted size immediately (no shrinking animation)
            float wiltedScale = cropData.GetStageStartScale(1); // Smallest scale
            transform.localScale = Vector3.one * wiltedScale;

            // Stop any growth animations
            if (growthCoroutine != null)
            {
                StopCoroutine(growthCoroutine);
                growthCoroutine = null;
            }

            return; // EXIT - crop is dead
        }

        // If we reach here, crop was watered yesterday and can grow

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

        // Start growth animation since crop was watered
        StartGrowthAnimation();
    }

    /// <summary>
    /// Called at sunset (6 PM) - stop growth animation
    /// </summary>
    private void OnSunset()
    {
        // Stop growth animation at night and ensure final scale is applied
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;

            // Only set to target scale if animation was running
            transform.localScale = targetScale;
        }
        // If no animation was running, leave the scale as-is
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

            // DON'T call UpdateVisuals here - let the growth animation handle it
            // This prevents the crop from shrinking when advancing stages

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

        // Don't animate wilted crops
        if (isWilted)
        {
            yield break;
        }

        // No visual growth for seed stage (stage 0 - invisible)
        if (currentStage == 0)
        {
            spriteRenderer.enabled = false;
            yield break;
        }

        // Enable sprite renderer for visible stages
        spriteRenderer.enabled = true;

        // Update sprite for current stage at the start of animation
        Sprite stageSprite = cropData.GetStageSprite(currentStage);
        if (stageSprite != null)
        {
            spriteRenderer.sprite = stageSprite;
            spriteRenderer.color = Color.white;
        }

        // Start from current scale, not stage start scale
        float startScale = transform.localScale.x; // Use actual current scale
        float endScale = cropData.finalScale;

        // If we're starting a new stage and current scale is too small, use stage start scale
        float stageStartScale = cropData.GetStageStartScale(currentStage);
        if (startScale < stageStartScale)
        {
            startScale = stageStartScale;
        }

        // Set initial scale for this animation
        currentScale = startScale;
        targetScale = Vector3.one * endScale;
        transform.localScale = Vector3.one * startScale;

        float sunrise = TimeManager.Instance.SunriseTime;
        float sunset = TimeManager.Instance.SunsetTime;
        float dayDuration = TimeManager.Instance.DayDuration;

        // Calculate real-time duration from sunrise to sunset
        float daytimeHours = sunset - sunrise; // 12 hours
        float daytimeRealSeconds = (daytimeHours / 24f) * dayDuration; // Real seconds for 12 hours

        Debug.Log($"[CropInstance] Stage {currentStage} growth animation: {startScale:F2} Ã¢â€ â€™ {endScale:F2} over {daytimeRealSeconds:F1}s (at {TimeManager.Instance.TimeSpeedMultiplier}x speed)");

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
            Debug.LogWarning($"[CropInstance] Cannot water wilted/dead crop {cropData.cropName}!");
            return;
        }

        if (IsHarvestable)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} is ready to harvest! Use the Harvest button instead.");
            return;
        }

        if (isWatered)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} already watered today!");
            return;
        }

        isWatered = true;
        lastWateredDay = currentDay;
        // Don't call UpdateVisuals() - watering doesn't change sprite/scale, just sets the watered flag

        Debug.Log($"[CropInstance] Watered {cropData.cropName}");
    }

    /// <summary>
    /// Update sprite and visual state
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

        // Multi-harvest crops regrow, others clear the plot
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
            // Clear the plot before destroying the crop
            if (FarmPlotManager.Instance != null)
            {
                FarmPlot plot = FarmPlotManager.Instance.GetPlotAtPosition(GridPosition);
                if (plot != null)
                {
                    plot.ClearPlot(); // This resets plot to unhoed state
                }
            }

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

        Sprite stageSprite;

        // Use wilted sprite if crop is wilted and wilted sprite is available
        if (isWilted && cropData.wiltedSprite != null)
        {
            stageSprite = cropData.wiltedSprite;
            Debug.Log($"[CropInstance] Using WILTED sprite for {cropData.cropName} at {GridPosition}");
        }
        else
        {
            // Use normal stage sprite
            int displayStage = currentStage;
            stageSprite = cropData.GetStageSprite(displayStage);
            Debug.Log($"[CropInstance] Using NORMAL stage {displayStage} sprite for {cropData.cropName} at {GridPosition}");
        }

        // Stage 0 (seed) has no sprite - disable renderer
        if (stageSprite == null)
        {
            spriteRenderer.enabled = false;
            return;
        }

        // Enable renderer and set sprite
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = stageSprite;

        // Apply color
        if (isWilted && cropData.wiltedSprite == null)
        {
            // Only use color tinting if no dedicated wilted sprite
            spriteRenderer.color = cropData.wiltedColor;
        }
        else
        {
            // Use white color for healthy crops or when using dedicated wilted sprite
            spriteRenderer.color = Color.white;
        }

        // Set scale - only set if not currently animating
        if (growthCoroutine == null)
        {
            float startScale;
            if (isWilted)
            {
                startScale = cropData.GetStageStartScale(1); // Smallest scale for wilted crops
                transform.localScale = Vector3.one * startScale;
                targetScale = Vector3.one * startScale; // Wilted crops stay small, don't grow to finalScale
            }
            else
            {
                startScale = cropData.GetStageStartScale(currentStage); // Normal scale for healthy crops
                transform.localScale = Vector3.one * startScale;
                targetScale = Vector3.one * cropData.finalScale; // Healthy crops can grow to finalScale
            }
        }
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