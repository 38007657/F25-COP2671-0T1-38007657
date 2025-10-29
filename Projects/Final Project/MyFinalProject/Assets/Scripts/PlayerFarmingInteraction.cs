using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player interactions with farm plots and crops
/// Attach this to your Player GameObject
/// </summary>
public class PlayerFarmingInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private float facingAngleTolerance = 90f; // Increased for easier interaction

    [Header("Test Crop (For Development)")]
    [SerializeField] private CropData testCropData;
    [SerializeField] private bool enableTestMode = true;

    [Header("Key Bindings")]
    [SerializeField] private KeyCode hoeKey = KeyCode.H;
    [SerializeField] private KeyCode plantKey = KeyCode.P;
    [SerializeField] private KeyCode waterKey = KeyCode.O; // Changed from W to avoid movement conflict
    [SerializeField] private KeyCode harvestKey = KeyCode.E; // E for harvest (H is for hoe)
    [SerializeField] private KeyCode clearDeadCropKey = KeyCode.C;

    [Header("Animation")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string hoeAnimationTrigger = "Hoe";
    [SerializeField] private string waterAnimationTrigger = "Water";
    [SerializeField] private bool useDirectionalAnimations = true;
    [SerializeField] private string horizontalParameter = "InputX";
    [SerializeField] private string verticalParameter = "InputY";
    [SerializeField] private string plantAnimationTrigger = "Plant";

    [Header("Visual Feedback")]
    [SerializeField] private bool showInteractionPrompts = true;

    [Header("Auto-Facing")]
    [SerializeField] private float facingLockDuration = 0.3f; // How long to maintain facing after action

    private FarmPlot nearestPlot;
    private FarmPlotManager farmPlotManager;
    private Vector2 lockedFacingDirection;
    private float facingLockTimer = 0f;

    // Public property so PlayerController can check if facing is locked
    public bool IsFacingLocked => facingLockTimer > 0f;
    public Vector2 LockedFacingDirection => lockedFacingDirection;

    private void Start()
    {
        // Cache reference to avoid ambiguity errors
        farmPlotManager = FarmPlotManager.Instance;

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        // Update facing lock timer
        if (facingLockTimer > 0f)
        {
            facingLockTimer -= Time.deltaTime;

            // Apply locked facing direction
            if (playerAnimator != null && lockedFacingDirection != Vector2.zero)
            {
                playerAnimator.SetFloat(horizontalParameter, lockedFacingDirection.x);
                playerAnimator.SetFloat(verticalParameter, lockedFacingDirection.y);
            }
        }

        // Find nearest plot to player
        if (farmPlotManager != null)
        {
            nearestPlot = farmPlotManager.GetNearestPlot(transform.position, interactionRange);
        }

        // Handle input
        HandleInput();
    }

    private void HandleInput()
    {
        if (!enableTestMode) return;

        // Hoe plot (H key) - only if not used for movement
        if (Input.GetKeyDown(hoeKey) && !IsMovementKey(hoeKey))
        {
            TryHoe();
        }

        // Plant crop (P key) - only if not used for movement
        if (Input.GetKeyDown(plantKey) && !IsMovementKey(plantKey))
        {
            TryPlant();
        }

        // Water crop (O key) - only if not used for movement
        if (Input.GetKeyDown(waterKey) && !IsMovementKey(waterKey))
        {
            TryWater();
        }

        // Harvest crop (E key) - only if not used for movement
        if (Input.GetKeyDown(harvestKey) && !IsMovementKey(harvestKey))
        {
            TryHarvest();
        }


    }

    /// <summary>
    /// Check if a key is being used for movement (arrow keys or WASD)
    /// </summary>
    private bool IsMovementKey(KeyCode key)
    {
        // Only arrow keys are used for movement (not WASD)
        // This prevents farming actions from triggering when pressing movement keys
        return false; // Farming keys don't overlap with arrow keys, so always allow
    }

    /// <summary>
    /// Try to hoe a plot at nearest position
    /// </summary>
    public void TryHoe()
    {
        if (farmPlotManager == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] FarmPlotManager not found!");
            return;
        }

        // Get the plot the player is facing (not just nearest)
        FarmPlot plot = GetFacingPlot();
        if (plot == null)
        {
            Debug.Log("[PlayerFarmingInteraction] No plot in front of you to hoe. Face a plot and try again!");
            return;
        }

        // Check if plot can be hoed (unhoed or has dead crop)
        if (!plot.CanHoe)
        {
            Debug.Log("[PlayerFarmingInteraction] Plot is already hoed and has no dead crop to clear!");
            return;
        }

        // Calculate direction to plot and auto-face it
        Vector3 directionToPlot = (plot.WorldPosition - transform.position).normalized;

        // Always face the plot before performing action (even if standing on it)
        if (directionToPlot.magnitude > 0.01f) // Only if there's a meaningful direction
        {
            FaceDirection(directionToPlot);
        }

        // Try to hoe that specific plot
        bool success = plot.Hoe();

        if (success)
        {
            if (plot.HasDeadCrop)
            {
                Debug.Log("[PlayerFarmingInteraction] Cleared dead crop and re-hoed plot");
            }
            else
            {
                Debug.Log("[PlayerFarmingInteraction] Hoed plot");
            }

            // Trigger hoe animation
            if (playerAnimator != null && !string.IsNullOrEmpty(hoeAnimationTrigger))
            {
                // Set direction parameters for blend tree (if using directional animations)
                if (useDirectionalAnimations)
                {
                    // Set animator parameters (InputX, InputY for blend tree)
                    playerAnimator.SetFloat(horizontalParameter, directionToPlot.x);
                    playerAnimator.SetFloat(verticalParameter, directionToPlot.y);
                }
                playerAnimator.SetTrigger(hoeAnimationTrigger);
            }
        }
    }

    /// <summary>
    /// Try to plant a crop at nearest plot
    /// </summary>
    public void TryPlant()
    {
        if (farmPlotManager == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] FarmPlotManager not found!");
            return;
        }

        if (testCropData == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] No test crop data assigned!");
            return;
        }

        // Get the plot the player is facing
        FarmPlot plot = GetFacingPlot();
        if (plot == null || !plot.CanPlant)
        {
            Debug.Log("[PlayerFarmingInteraction] No suitable plot to plant on. Face an empty hoed plot!");
            return;
        }

        // Calculate direction to plot and auto-face it
        Vector3 directionToPlot = (plot.WorldPosition - transform.position).normalized;

        // Always face the plot before performing action (even if standing on it)
        if (directionToPlot.magnitude > 0.01f) // Only if there's a meaningful direction
        {
            FaceDirection(directionToPlot);
        }

        // Try to plant at the specific plot
        bool success = farmPlotManager.PlantCrop(plot.GridPosition, testCropData);

        if (success)
        {
            Debug.Log($"[PlayerFarmingInteraction] Planted {testCropData.cropName}");

            // Trigger plant animation
            if (playerAnimator != null && !string.IsNullOrEmpty(plantAnimationTrigger))
            {
                // Set direction parameters for blend tree (if using directional animations)
                if (useDirectionalAnimations)
                {
                    // Set animator parameters (InputX, InputY for blend tree)
                    playerAnimator.SetFloat(horizontalParameter, directionToPlot.x);
                    playerAnimator.SetFloat(verticalParameter, directionToPlot.y);
                }
                playerAnimator.SetTrigger(plantAnimationTrigger);
            }
        }
        else
        {
            Debug.Log("[PlayerFarmingInteraction] Cannot plant - no valid plot in range or plot not suitable");
        }
    }

    /// <summary>
    /// Try to water crop at nearest plot
    /// </summary>
    public void TryWater()
    {
        if (farmPlotManager == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] FarmPlotManager not found!");
            return;
        }

        // Get the plot the player is facing (not just nearest)
        FarmPlot plot = GetFacingPlot();
        if (plot == null)
        {
            Debug.Log("[PlayerFarmingInteraction] No plot in front of you to water. Face a plot and try again!");
            return;
        }

        // Calculate direction to plot and auto-face it
        Vector3 directionToPlot = (plot.WorldPosition - transform.position).normalized;

        // Always face the plot before performing action (even if standing on it)
        if (directionToPlot.magnitude > 0.01f) // Only if there's a meaningful direction
        {
            FaceDirection(directionToPlot);
        }

        // Water that specific plot
        bool success = plot.WaterCrop(CropGrowthManager.Instance != null ? CropGrowthManager.Instance.CurrentDay : 0);

        if (success)
        {
            Debug.Log("[PlayerFarmingInteraction] Watered crop");

            // Trigger water animation
            if (playerAnimator != null && !string.IsNullOrEmpty(waterAnimationTrigger))
            {
                // Set direction parameters for blend tree (if using directional animations)
                if (useDirectionalAnimations)
                {
                    // Set animator parameters (InputX, InputY for blend tree)
                    playerAnimator.SetFloat(horizontalParameter, directionToPlot.x);
                    playerAnimator.SetFloat(verticalParameter, directionToPlot.y);
                }
                playerAnimator.SetTrigger(waterAnimationTrigger);
            }
        }
        else
        {
            Debug.Log("[PlayerFarmingInteraction] No crop to water in this plot");
        }
    }

    /// <summary>
    /// Try to harvest crop at nearest plot
    /// </summary>
    public void TryHarvest()
    {
        if (farmPlotManager == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] FarmPlotManager not found!");
            return;
        }

        // Get the plot the player is facing
        FarmPlot plot = GetFacingPlot();
        if (plot == null)
        {
            Debug.Log("[PlayerFarmingInteraction] No plot in front of you to harvest. Face a plot and try again!");
            return;
        }

        if (plot.CurrentCrop == null || !plot.CurrentCrop.IsHarvestable)
        {
            Debug.Log("[PlayerFarmingInteraction] No harvestable crop at this plot");
            return;
        }

        // Calculate direction to plot and auto-face it
        Vector3 directionToPlot = (plot.WorldPosition - transform.position).normalized;

        // Always face the plot before performing action (even if standing on it)
        if (directionToPlot.magnitude > 0.01f) // Only if there's a meaningful direction
        {
            FaceDirection(directionToPlot);
        }

        // Harvest the crop
        HarvestResult result = plot.HarvestCrop();

        if (result != null)
        {
            Debug.Log($"[PlayerFarmingInteraction] Harvested {result.quantity}x {result.cropName}!");

            // Trigger harvest animation if set
            if (playerAnimator != null && !string.IsNullOrEmpty("Harvest")) // You can add harvestAnimationTrigger field
            {
                if (useDirectionalAnimations)
                {
                    playerAnimator.SetFloat(horizontalParameter, directionToPlot.x);
                    playerAnimator.SetFloat(verticalParameter, directionToPlot.y);
                }
                playerAnimator.SetTrigger("Harvest");
            }
        }
        else
        {
            Debug.Log("[PlayerFarmingInteraction] Failed to harvest crop");
        }
    }

    /// <summary>
    /// Try to clear dead crop at nearest plot
    /// </summary>
    public void TryClearDeadCrop()
    {
        if (farmPlotManager == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] FarmPlotManager not found!");
            return;
        }

        // Get the plot the player is facing
        FarmPlot plot = GetFacingPlot();
        if (plot == null)
        {
            Debug.Log("[PlayerFarmingInteraction] No plot in front of you to clear.");
            return;
        }

        if (!plot.HasDeadCrop)
        {
            Debug.Log("[PlayerFarmingInteraction] No dead crop to clear at this plot.");
            return;
        }

        // Calculate direction to plot and auto-face it
        Vector3 directionToPlot = (plot.WorldPosition - transform.position).normalized;

        // Always face the plot before performing action (even if standing on it)
        if (directionToPlot.magnitude > 0.01f) // Only if there's a meaningful direction
        {
            FaceDirection(directionToPlot);
        }

        // Clear the dead crop
        if (plot.CurrentCrop != null)
        {
            Destroy(plot.CurrentCrop.gameObject);
        }

        // Reset plot state manually since we're not using the Hoe() method
        // You'll need to add a public method to FarmPlot for this
        plot.ClearDeadCrop();

        Debug.Log("[PlayerFarmingInteraction] Cleared dead crop - plot is now ready for planting");

        // Optional: Trigger hoe animation since it's similar action
        if (playerAnimator != null && !string.IsNullOrEmpty(hoeAnimationTrigger))
        {
            if (useDirectionalAnimations)
            {
                playerAnimator.SetFloat(horizontalParameter, directionToPlot.x);
                playerAnimator.SetFloat(verticalParameter, directionToPlot.y);
            }
            playerAnimator.SetTrigger(hoeAnimationTrigger);
        }
    }

    /// <summary>
    /// Public methods for UI buttons or other systems
    /// </summary>
    public void PlantCrop(CropData cropData)
    {
        if (cropData == null || farmPlotManager == null) return;

        bool success = farmPlotManager.PlantCropNearPosition(
            transform.position,
            cropData,
            interactionRange
        );

        if (!success)
        {
            Debug.Log("Cannot plant crop - no valid plot in range");
        }
    }

    public void WaterCrop()
    {
        TryWater();
    }

    public void HarvestCrop()
    {
        TryHarvest();
    }

    /// <summary>
    /// Get the plot the player is facing or standing on within interaction range
    /// </summary>
    private FarmPlot GetFacingPlot()
    {
        if (farmPlotManager == null) return null;

        // First, check if player is standing directly on a plot (within 0.8 units)
        FarmPlot standingOnPlot = null;
        float closestStandingDistance = 0.8f; // Close enough to be "standing on"

        foreach (FarmPlot plot in farmPlotManager.GetAllPlots())
        {
            Vector3 directionToPlot = (plot.WorldPosition - transform.position);
            float distance = directionToPlot.magnitude;

            // If player is very close to a plot, prioritize it regardless of facing
            if (distance < closestStandingDistance)
            {
                closestStandingDistance = distance;
                standingOnPlot = plot;
            }
        }

        // If standing on a plot, return it immediately
        if (standingOnPlot != null)
        {
            return standingOnPlot;
        }

        // Fall back to original facing logic for plots at a distance
        Vector3 playerForward = transform.up; // Default to transform.up

        // If animator has InputX/InputY, use those for more accurate facing
        if (playerAnimator != null)
        {
            float inputX = playerAnimator.GetFloat(horizontalParameter);
            float inputY = playerAnimator.GetFloat(verticalParameter);
            if (inputX != 0 || inputY != 0)
            {
                playerForward = new Vector3(inputX, inputY, 0).normalized;
            }
        }

        FarmPlot bestPlot = null;
        float bestScore = float.MaxValue; // Lower is better (combination of distance and angle)

        // Check all plots within range using facing logic
        foreach (FarmPlot plot in farmPlotManager.GetAllPlots())
        {
            Vector3 directionToPlot = (plot.WorldPosition - transform.position);
            float distance = directionToPlot.magnitude;

            // Skip if out of range or too close (already handled above)
            if (distance > interactionRange || distance < closestStandingDistance) continue;

            directionToPlot.Normalize();

            // Calculate angle between player forward and plot direction
            float angle = Vector3.Angle(playerForward, directionToPlot);

            // Skip if not within angle tolerance
            if (angle > facingAngleTolerance) continue;

            // Score based on distance (prioritize closest plot in facing direction)
            float score = distance + (angle * 0.1f); // Prioritize closest plot, angle is minor factor

            if (score < bestScore)
            {
                bestScore = score;
                bestPlot = plot;
            }
        }

        return bestPlot;
    }

    /// <summary>
    /// Make player face a specific direction (for farming actions)
    /// </summary>
    private void FaceDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        // Normalize the direction
        direction.Normalize();

        // Snap to cardinal directions for cleaner animations
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        Vector2 snappedDirection;
        if (absX > absY)
        {
            // Horizontal direction dominates
            snappedDirection = new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            // Vertical direction dominates
            snappedDirection = new Vector2(0, Mathf.Sign(direction.y));
        }

        // Store the locked facing direction
        lockedFacingDirection = snappedDirection;
        facingLockTimer = facingLockDuration;

        // For 2D games, update animator parameters immediately
        // The blend tree will handle showing the correct directional animation
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(horizontalParameter, snappedDirection.x);
            playerAnimator.SetFloat(verticalParameter, snappedDirection.y);

            Debug.Log($"[PlayerFarmingInteraction] Auto-facing: Direction=({snappedDirection.x}, {snappedDirection.y}) locked for {facingLockDuration}s");
        }
    }

    // UI feedback
    private void OnGUI()
    {
        if (!showInteractionPrompts || nearestPlot == null) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        string prompt = "";

        if (nearestPlot.HasDeadCrop)
        {
            prompt = $"[{clearDeadCropKey}] Clear Dead Crop"; // CHANGED to use C key
        }
        else if (!nearestPlot.IsHoed)
        {
            prompt = $"[{hoeKey}] Hoe Soil";
        }
        else if (nearestPlot.IsEmpty)
        {
            prompt = $"[{plantKey}] Plant Crop";
        }
        else if (nearestPlot.CurrentCrop != null)
        {
            if (nearestPlot.CurrentCrop.IsHarvestable)
            {
                prompt = $"[{harvestKey}] Harvest {nearestPlot.CurrentCrop.CropData.cropName}";
            }
            else if (!nearestPlot.CurrentCrop.IsWatered && !nearestPlot.CurrentCrop.IsWilted)
            {
                prompt = $"[{waterKey}] Water {nearestPlot.CurrentCrop.CropData.cropName}";
            }
            else if (nearestPlot.CurrentCrop.IsWilted)
            {
                prompt = $"[{hoeKey}] Clear Dead Crop";
            }
            else
            {
                prompt = $"{nearestPlot.CurrentCrop.CropData.cropName} (Stage {nearestPlot.CurrentCrop.CurrentStage})";
            }
        }

        if (!string.IsNullOrEmpty(prompt))
        {
            // Shadow
            GUI.color = Color.black;
            GUI.Label(new Rect(Screen.width / 2 - 149, Screen.height - 81, 300, 30), prompt, style);
            // Main text
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 80, 300, 30), prompt, style);
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw line to nearest plot
        if (nearestPlot != null)
        {
            Gizmos.color = nearestPlot.IsEmpty ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, nearestPlot.WorldPosition);
        }
    }
}