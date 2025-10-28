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
        lastWateredDay = currentDay; // Not watered yet
        currentStage = 1; // Start at stage 1 (sprout) so sprite is visible immediately
        daysSinceStageChange = 0;
        isWatered = false; // Must water after planting
        isWilted = false;
        GridPosition = gridPos;

        // Set initial scale to stage 1 start scale (small, visible seed/sprout)
        float startScale = cropData.GetStageStartScale(1);
        transform.localScale = Vector3.one * startScale;

        UpdateVisuals();

        // DO NOT start growth animation when planted - sprite stays at small scale until next sunrise
        Debug.Log($"[CropInstance] Planted {cropData.cropName} at {gridPos} on day {currentDay} - showing at scale {startScale}");
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

        // If already wilted, check if it should die
        if (isWilted)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} is wilted! It will die and show wilted sprite.");
            DieCrop();
            return;
        }

        // Check if crop should wilt
        CheckWiltStatus(currentDay);

        if (isWilted)
        {
            Debug.Log($"[CropInstance] {cropData.cropName} just wilted!");
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
            Debug.Log($"[CropInstance] {cropData.cropName} not watered yesterday, not advancing or growing visually");
            return; // EXIT HERE - no visual growth animation for unwatered crops
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

        // ONLY start growth animation if crop was watered (or doesn't require water)
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
            Debug.LogWarning($"[CropInstance] Cannot water wilted/dead crop {cropData.cropName}!");
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

        // If crop was never watered (lastWateredDay = -1), calculate from planting day
        int daysSinceWater;
        if (lastWateredDay == -1)
        {
            // Crop hasn't been watered since planting
            daysSinceWater = currentDay - dayPlanted;
        }
        else
        {
            // Normal case - calculate from last watered day
            daysSinceWater = currentDay - lastWateredDay;
        }

        Debug.Log($"[CropInstance] {cropData.cropName} - Days since water: {daysSinceWater}, Allowed days without water: {cropData.daysWithoutWater}");

        if (daysSinceWater > cropData.daysWithoutWater)
        {
            isWilted = true;
            OnCropWilted?.Invoke(this);
            UpdateVisuals();

            // Start shrinking animation
            StartWiltShrinkAnimation();

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
    /// Kill this crop and show wilted sprite (stays until player hoes the plot)
    /// </summary>
    private void DieCrop()
    {
        Debug.Log($"[CropInstance] {cropData.cropName} died from wilting. Showing wilted sprite.");

        // Stop any growth animations
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }

        // Show wilted sprite permanently
        isWilted = true;

        // Update visual to show wilted state
        UpdateVisuals();

        // DO NOT destroy the GameObject - keep it visible as wilted
        // Player must hoe the plot to remove it
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
    /// Start shrinking animation for wilted crops
    /// </summary>
    private void StartWiltShrinkAnimation()
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
        }

        growthCoroutine = StartCoroutine(WiltShrinkCoroutine());
    }

    /// <summary>
    /// Coroutine that progressively shrinks wilted crops through stages to smallest size
    /// </summary>
    private IEnumerator WiltShrinkCoroutine()
    {
        if (cropData == null) yield break;

        int startingStage = currentStage;
        float stageShrinkDuration = 4f; // 4 seconds per stage shrink

        Debug.Log($"[CropInstance] Starting progressive wilt shrink from stage {startingStage}");

        // Progressive shrinking through each stage down to stage 1
        for (int stage = startingStage; stage >= 1; stage--)
        {
            // Use wilted sprite if available, otherwise use normal sprite
            if (cropData.wiltedSprite != null)
            {
                spriteRenderer.sprite = cropData.wiltedSprite; // Use dedicated wilted sprite
            }
            else
            {
                spriteRenderer.sprite = cropData.GetStageSprite(stage); // Fallback to normal sprite
                spriteRenderer.color = cropData.wiltedColor; // Apply brown color if no wilted sprite
            }

            if (cropData.wiltedSprite != null)
            {
                spriteRenderer.color = Color.white; // No color tinting needed for dedicated sprite
            }

            // Get scale range for this stage
            float startScale = (stage == startingStage) ? transform.localScale.x : cropData.GetStageStartScale(stage + 1);
            float endScale = cropData.GetStageStartScale(stage);

            Debug.Log($"[CropInstance] Stage {stage} shrink: {startScale:F2} → {endScale:F2}");

            // Shrink from start to end scale over duration
            float elapsedTime = 0f;
            while (elapsedTime < stageShrinkDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / stageShrinkDuration;

                // Smooth shrinking curve
                float currentScale = Mathf.Lerp(startScale, endScale, progress);
                transform.localScale = Vector3.one * currentScale;

                yield return null;
            }

            // Ensure final scale for this stage
            transform.localScale = Vector3.one * endScale;

            // Small pause between stage transitions
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"[CropInstance] Wilt shrink complete - using wilted sprite at smallest scale");
        growthCoroutine = null;
    }

    /// <summary>
    /// Update sprite and visual state
    /// </summary>
    private void UpdateVisuals()
    {
        if (cropData == null || spriteRenderer == null) return;

        // If wilted and shrinking animation is running, don't interfere
        if (isWilted && growthCoroutine != null)
        {
            return; // Let the shrinking coroutine handle visuals
        }

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
            }
            else
            {
                startScale = cropData.GetStageStartScale(currentStage); // Normal scale for healthy crops
            }

            transform.localScale = Vector3.one * startScale;
            targetScale = Vector3.one * cropData.finalScale;
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