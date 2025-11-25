using UnityEngine;

/// <summary>
/// Easter egg that triggers when player stands in a specific spot and performs a secret input sequence
/// Sequence: Hoe x2, Plant x6, Water x7, Harvest x1
/// </summary>
public class EasterEggTrigger : MonoBehaviour
{
    [Header("Trigger Location")]
    [SerializeField] private Vector2 triggerPosition = new Vector2(0, 0); // Set this in Inspector
    [SerializeField] private float triggerRadius = 1f; // How close player needs to be

    [Header("Easter Egg Settings")]
    [SerializeField] private GameObject professorHeadPrefab; // Assign the ProfessorHeadEffect prefab
    [SerializeField] private int coinReward = 1000;
    [SerializeField] private bool showDebugLogs = true;

    [Header("Floating Text (Optional)")]
    [SerializeField] private EasterEggFloatingText floatingText; // Drag the floating text UI element here

    [Header("Required Sequence")]
    private readonly int[] requiredSequence = { 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3 };
    // 0 = Hoe, 1 = Plant, 2 = Water, 3 = Harvest

    private int currentSequenceIndex = 0;
    private bool isInTriggerZone = false;
    private bool easterEggActivated = false;
    private Transform playerTransform;
    private ToolbarController toolbarController;

    // Input tracking
    private const int HOE = 0;
    private const int PLANT = 1;
    private const int WATER = 2;
    private const int HARVEST = 3;

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Find toolbar controller and subscribe to events
        toolbarController = FindFirstObjectByType<ToolbarController>(FindObjectsInactive.Include);

        if (toolbarController != null)
        {
            toolbarController.OnHoe.AddListener(() => OnToolUsed(HOE));
            toolbarController.OnSeed.AddListener(() => OnToolUsed(PLANT));
            toolbarController.OnWater.AddListener(() => OnToolUsed(WATER));
            toolbarController.OnGather.AddListener(() => OnToolUsed(HARVEST));

            if (showDebugLogs)
            {
                Debug.Log("[EasterEgg] Subscribed to toolbar events");
            }
        }
        else
        {
            Debug.LogError("[EasterEgg] ToolbarController not found!");
        }
    }

    private void Update()
    {
        if (easterEggActivated || playerTransform == null) return;

        // Check if player is in trigger zone
        float distance = Vector2.Distance(playerTransform.position, triggerPosition);
        bool wasInZone = isInTriggerZone;
        isInTriggerZone = distance <= triggerRadius;

        // Reset sequence if player leaves the zone
        if (wasInZone && !isInTriggerZone && currentSequenceIndex > 0)
        {
            if (showDebugLogs)
            {
                Debug.Log("[EasterEgg] Player left trigger zone - resetting sequence");
            }
            ResetSequence();
        }
    }

    /// <summary>
    /// Called when any tool button is pressed
    /// </summary>
    private void OnToolUsed(int toolType)
    {
        if (easterEggActivated || !isInTriggerZone) return;

        if (showDebugLogs)
        {
            Debug.Log($"[EasterEgg] Tool used: {GetToolName(toolType)} (Expected: {GetToolName(requiredSequence[currentSequenceIndex])})");
            Debug.Log($"[EasterEgg] Sequence progress: {currentSequenceIndex + 1}/{requiredSequence.Length}");
        }

        // Check if this matches the required sequence
        if (toolType == requiredSequence[currentSequenceIndex])
        {
            currentSequenceIndex++;

            if (showDebugLogs)
            {
                Debug.Log($"[EasterEgg] ✓ Correct input! Progress: {currentSequenceIndex}/{requiredSequence.Length}");
            }

            // Check if sequence is complete
            if (currentSequenceIndex >= requiredSequence.Length)
            {
                ActivateEasterEgg();
            }
        }
        else
        {
            // Wrong input - reset sequence
            if (showDebugLogs)
            {
                Debug.Log("[EasterEgg] ✗ Wrong input - resetting sequence");
            }
            ResetSequence();
        }
    }

    /// <summary>
    /// Activate the easter egg!
    /// </summary>
    private void ActivateEasterEgg()
    {
        easterEggActivated = true;

        Debug.Log("[EasterEgg] 🎉 EASTER EGG ACTIVATED! 🎉");

        // Show floating text notification
        if (floatingText != null)
        {
            floatingText.ShowWithReward(coinReward);
        }

        // Spawn the professor head effect
        if (professorHeadPrefab != null && playerTransform != null)
        {
            GameObject effectObj = Instantiate(professorHeadPrefab, playerTransform);

            // Position it above the player's head
            effectObj.transform.localPosition = new Vector3(0, 1.5f, 0);

            ProfessorHeadEffect effect = effectObj.GetComponent<ProfessorHeadEffect>();
            if (effect != null)
            {
                effect.Initialize(coinReward);
            }
        }
        else
        {
            Debug.LogError("[EasterEgg] Professor head prefab not assigned or player not found!");
        }

        // Award coins
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddMoney(coinReward);
            Debug.Log($"[EasterEgg] Awarded {coinReward} coins!");
        }

        // Reset after a delay to allow re-activation
        Invoke(nameof(ResetEasterEgg), 180f); // 3 minutes
    }

    /// <summary>
    /// Reset the sequence progress
    /// </summary>
    private void ResetSequence()
    {
        currentSequenceIndex = 0;
    }

    /// <summary>
    /// Reset easter egg state to allow re-activation
    /// </summary>
    private void ResetEasterEgg()
    {
        easterEggActivated = false;
        currentSequenceIndex = 0;

        if (showDebugLogs)
        {
            Debug.Log("[EasterEgg] Easter egg reset - can be activated again");
        }
    }

    /// <summary>
    /// Get tool name for debugging
    /// </summary>
    private string GetToolName(int toolType)
    {
        switch (toolType)
        {
            case HOE: return "Hoe";
            case PLANT: return "Plant";
            case WATER: return "Water";
            case HARVEST: return "Harvest";
            default: return "Unknown";
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (toolbarController != null)
        {
            toolbarController.OnHoe.RemoveListener(() => OnToolUsed(HOE));
            toolbarController.OnSeed.RemoveListener(() => OnToolUsed(PLANT));
            toolbarController.OnWater.RemoveListener(() => OnToolUsed(WATER));
            toolbarController.OnGather.RemoveListener(() => OnToolUsed(HARVEST));
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(triggerPosition, triggerRadius);

        // Draw a marker at the trigger position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(triggerPosition, 0.2f);
    }
}