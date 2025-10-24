using UnityEngine;

/// <summary>
/// Handles player interactions with farm plots and crops
/// Attach this to your Player GameObject
/// </summary>
public class PlayerFarmingInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 1.5f;

    [Header("Test Crop (For Development)")]
    [SerializeField] private CropData testCropData; // Assign a CropData for testing
    [SerializeField] private bool enableTestMode = true;

    [Header("Key Bindings")]
    [SerializeField] private KeyCode plantKey = KeyCode.P;
    [SerializeField] private KeyCode waterKey = KeyCode.W;
    [SerializeField] private KeyCode harvestKey = KeyCode.H;

    [Header("Visual Feedback")]
    [SerializeField] private bool showInteractionPrompts = true;

    private FarmPlot nearestPlot;

    private void Update()
    {
        // Find nearest plot to player
        if (FarmPlotManager.Instance != null)
        {
            nearestPlot = FarmPlotManager.Instance.GetNearestPlot(transform.position, interactionRange);
        }

        // Handle input
        HandleInput();
    }

    private void HandleInput()
    {
        if (!enableTestMode) return;

        // Plant crop (P key)
        if (Input.GetKeyDown(plantKey))
        {
            TryPlant();
        }

        // Water crop (W key)
        if (Input.GetKeyDown(waterKey))
        {
            TryWater();
        }

        // Harvest crop (H key)
        if (Input.GetKeyDown(harvestKey))
        {
            TryHarvest();
        }
    }

    /// <summary>
    /// Try to plant a crop at nearest plot
    /// </summary>
    public void TryPlant()
    {
        if (FarmPlotManager.Instance == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] FarmPlotManager not found!");
            return;
        }

        if (testCropData == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] No test crop data assigned!");
            return;
        }

        // Try to plant at nearest empty plot
        bool success = FarmPlotManager.Instance.PlantCropNearPosition(
            transform.position,
            testCropData,
            interactionRange
        );

        if (success)
        {
            Debug.Log($"[PlayerFarmingInteraction] Planted {testCropData.cropName}");

            // TODO: Play planting animation/sound
            // TODO: Consume seed from inventory
        }
        else
        {
            Debug.Log("[PlayerFarmingInteraction] Cannot plant - no valid plot in range");
        }
    }

    /// <summary>
    /// Try to water crop at nearest plot
    /// </summary>
    public void TryWater()
    {
        if (FarmPlotManager.Instance == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] FarmPlotManager not found!");
            return;
        }

        bool success = FarmPlotManager.Instance.WaterNearestCrop(transform.position);

        if (success)
        {
            Debug.Log("[PlayerFarmingInteraction] Watered crop");

            // TODO: Play watering animation/sound
            // TODO: Show water particle effect
        }
        else
        {
            Debug.Log("[PlayerFarmingInteraction] No crop to water in range");
        }
    }

    /// <summary>
    /// Try to harvest crop at nearest plot
    /// </summary>
    public void TryHarvest()
    {
        if (FarmPlotManager.Instance == null)
        {
            Debug.LogWarning("[PlayerFarmingInteraction] FarmPlotManager not found!");
            return;
        }

        HarvestResult result = FarmPlotManager.Instance.HarvestNearestCrop(transform.position);

        if (result != null)
        {
            Debug.Log($"[PlayerFarmingInteraction] Harvested {result.quantity}x {result.cropName}!");

            // TODO: Add to inventory
            // InventoryManager.Instance?.AddItem(result.itemID, result.quantity);

            // TODO: Play harvest animation/sound
            // TODO: Show harvest particles
        }
        else
        {
            Debug.Log("[PlayerFarmingInteraction] No harvestable crop in range");
        }
    }

    /// <summary>
    /// Public methods for UI buttons or other systems
    /// </summary>
    public void PlantCrop(CropData cropData)
    {
        if (cropData == null || FarmPlotManager.Instance == null) return;

        bool success = FarmPlotManager.Instance.PlantCropNearPosition(
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

    // UI feedback
    private void OnGUI()
    {
        if (!showInteractionPrompts || nearestPlot == null) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        string prompt = "";

        if (nearestPlot.IsEmpty)
        {
            prompt = $"[{plantKey}] Plant Crop";
        }
        else if (nearestPlot.CurrentCrop != null)
        {
            if (nearestPlot.CurrentCrop.IsHarvestable)
            {
                prompt = $"[{harvestKey}] Harvest {nearestPlot.CurrentCrop.CropData.cropName}";
            }
            else if (!nearestPlot.CurrentCrop.IsWatered)
            {
                prompt = $"[{waterKey}] Water {nearestPlot.CurrentCrop.CropData.cropName}";
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