using UnityEngine;

/// <summary>
/// Handles farming actions via event system with proper range-based selection
/// Part 4 Requirements: Use event listeners to call methods
/// </summary>
public class FarmingController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2.5f;
    [Tooltip("Y-axis offset for 64x64 player interacting with 16x16 tiles")]
    [SerializeField] private float playerYOffset = -1.5f; // ← Start with -1.5

    [Header("Test Seed")]
    [SerializeField] private SeedPacket testSeedPacket;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator playerAnimator;

    [Header("Animation Settings")]
    [SerializeField] private string hoeAnimationTrigger = "Hoe";
    [SerializeField] private string waterAnimationTrigger = "Water";
    [SerializeField] private string plantAnimationTrigger = "Plant";
    [SerializeField] private string harvestAnimationTrigger = "Harvest";
    [SerializeField] private bool useDirectionalAnimations = true;
    [SerializeField] private string horizontalParameter = "InputX";
    [SerializeField] private string verticalParameter = "InputY";


    [Header("Auto-Facing")]
    [SerializeField] private float facingLockDuration = 0.3f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private bool showSelectionIndicator = true;
    [SerializeField] private bool showDebugInfo = true;

    private CropManager cropManager;
    private ToolbarController toolbarController;
    private CropBlock selectedBlock;

    // Facing lock
    private Vector2 lockedFacingDirection;
    private float facingLockTimer = 0f;

    // Public property so PlayerController can check if facing is locked
    public bool IsFacingLocked => facingLockTimer > 0f;
    public Vector2 LockedFacingDirection => lockedFacingDirection;

    private void Start()
    {
        // Get references
        cropManager = CropManager.Instance;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // Get animator if not assigned
        if (playerAnimator == null && playerTransform != null)
        {
            playerAnimator = playerTransform.GetComponentInChildren<Animator>();
        }

        // Create selection indicator if needed
        if (showSelectionIndicator && selectionIndicator != null)
        {
            selectionIndicator = Instantiate(selectionIndicator);
            selectionIndicator.SetActive(false);
        }

        // Find and subscribe to toolbar events - Part 4 Requirement
        toolbarController = FindFirstObjectByType<ToolbarController>();

        if (toolbarController != null)
        {
            toolbarController.OnHoe.AddListener(HandleHoeEvent);
            toolbarController.OnSeed.AddListener(HandleSeedEvent);
            toolbarController.OnWater.AddListener(HandleWaterEvent);
            toolbarController.OnGather.AddListener(HandleGatherEvent);

            UnityEngine.Debug.Log("[FarmingController] Subscribed to toolbar events");
        }
        else
        {
            UnityEngine.Debug.LogError("[FarmingController] ToolbarController not found!");
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

        // Find block player is facing
        UpdateSelectedBlock();

        // Update selection indicator visual
        UpdateSelectionIndicator();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (toolbarController != null)
        {
            toolbarController.OnHoe.RemoveListener(HandleHoeEvent);
            toolbarController.OnSeed.RemoveListener(HandleSeedEvent);
            toolbarController.OnWater.RemoveListener(HandleWaterEvent);
            toolbarController.OnGather.RemoveListener(HandleGatherEvent);
        }

        // Destroy selection indicator
        if (selectionIndicator != null)
        {
            Destroy(selectionIndicator);
        }
    }

    // ===== HELPER METHODS =====

    /// <summary>
    /// Find the nearest crop block to player (no facing direction needed)
    /// </summary>
    private void UpdateSelectedBlock()
    {
        if (cropManager == null || playerTransform == null)
        {
            selectedBlock = null;
            return;
        }

        // Add this check
        if (cropManager == null)
        {
            selectedBlock = null;
            return;
        }

        // Just find the absolute closest block - period
        CropBlock closestBlock = null;
        float closestDistance = interactionRange;

        // Check all blocks in the grid (or optimize by checking nearby area)
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector3 checkPos = playerTransform.position + new Vector3(x, y, 0);
                CropBlock block = cropManager.GetBlockAtWorldPosition(checkPos);

                if (block != null)
                {
                    float distance = Vector3.Distance(playerTransform.position, block.worldPosition);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBlock = block;
                    }
                }
            }
        }

        selectedBlock = closestBlock;
    }

    /// <summary>
    /// Get the direction the player is facing from animator
    /// </summary>
    private Vector2 GetPlayerFacingDirection()
    {
        if (playerAnimator == null)
        {
            return Vector2.down;
        }

        float inputX = playerAnimator.GetFloat(horizontalParameter);
        float inputY = playerAnimator.GetFloat(verticalParameter);

        // If not moving, use last input direction
        if (inputX == 0 && inputY == 0)
        {
            inputX = playerAnimator.GetFloat("LastInputX");
            inputY = playerAnimator.GetFloat("LastInputY");
        }

        if (inputX != 0 || inputY != 0)
        {
            return new Vector2(inputX, inputY).normalized;
        }

        return Vector2.down;
    }

    /// <summary>
    /// Snap direction to nearest cardinal direction
    /// </summary>
    private Vector2 SnapToCardinalDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return Vector2.down;

        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absX > absY)
        {
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            return new Vector2(0, Mathf.Sign(direction.y));
        }
    }

    /// <summary>
    /// Update visual indicator to show selected block
    /// </summary>
    private void UpdateSelectionIndicator()
    {
        if (!showSelectionIndicator || selectionIndicator == null)
            return;

        if (selectedBlock != null)
        {
            selectionIndicator.SetActive(true);
            selectionIndicator.transform.position = selectedBlock.worldPosition;
        }
        else
        {
            selectionIndicator.SetActive(false);
        }
    }

    // ===== EVENT HANDLERS =====

    /// <summary>
    /// Handle hoe event from toolbar
    /// </summary>
    public void HandleHoeEvent()
    {
        if (selectedBlock == null)
        {
            UnityEngine.Debug.Log("[FarmingController] No block selected to hoe!");
            return;
        }

        // Use player's CURRENT facing direction (not direction to block)
        Vector3 facingDir = GetPlayerFacingDirection();

        // Only calculate direction to block if player isn't facing anywhere
        if (facingDir == Vector3.zero)
        {
            Vector3 adjustedPlayerPos = playerTransform.position + new Vector3(0, 0.5f, 0);
            facingDir = (selectedBlock.worldPosition - playerTransform.position).normalized;
        }

        // Face that direction
        FaceDirection(facingDir);

        // Perform action
        bool success = selectedBlock.TillSoil();

        // Play animation if successful
        if (success)
        {
            PlayAnimation(hoeAnimationTrigger, facingDir);
        }
    }

    /// <summary>
    /// Handle seed/plant event from toolbar
    /// </summary>
    public void HandleSeedEvent()
    {
        if (selectedBlock == null)
        {
            UnityEngine.Debug.Log("[FarmingController] No block selected to plant!");
            return;
        }

        // Get selected seed from inventory instead of using testSeedPacket
        SeedPacket selectedSeed = SeedInventory.Instance?.SelectedSeed;

        if (selectedSeed == null)
        {
            UnityEngine.Debug.LogWarning("[FarmingController] No seed selected!");
            return;
        }

        // Use player's CURRENT facing direction
        Vector3 facingDir = GetPlayerFacingDirection();

        if (facingDir == Vector3.zero)
        {
            Vector3 adjustedPlayerPos = playerTransform.position + new Vector3(0, 0.5f, 0);
            facingDir = (selectedBlock.worldPosition - playerTransform.position).normalized;
        }

        FaceDirection(facingDir);

        // Perform action with selected seed
        bool success = selectedBlock.PlantSeed(selectedSeed, cropManager.CurrentDay);

        // Play animation if successful
        if (success)
        {
            PlayAnimation(plantAnimationTrigger, facingDir);
        }
    }

    /// <summary>
    /// Handle water event from toolbar
    /// </summary>
    public void HandleWaterEvent()
    {
        if (selectedBlock == null)
        {
            UnityEngine.Debug.Log("[FarmingController] No block selected to water!");
            return;
        }

        // Use player's CURRENT facing direction
        Vector3 facingDir = GetPlayerFacingDirection();

        if (facingDir == Vector3.zero)
        {
            Vector3 adjustedPlayerPos = playerTransform.position + new Vector3(0, 0.5f, 0);
            facingDir = (selectedBlock.worldPosition - playerTransform.position).normalized;
        }

        FaceDirection(facingDir);

        // Perform action
        bool success = selectedBlock.WaterSoil(cropManager.CurrentDay);

        // Play animation if successful
        if (success)
        {
            PlayAnimation(waterAnimationTrigger, facingDir);
        }
    }

    // In FarmingController.HandleGatherEvent() [3]
    public void HandleGatherEvent()
    {
        if (selectedBlock == null) return;

        Vector3 playerPosBefore = playerTransform.position;
        Debug.Log($"[Harvest] Player position BEFORE: {playerPosBefore}");

        Vector3 facingDir = GetPlayerFacingDirection();
        FaceDirection(facingDir);

        GameObject result = selectedBlock.HarvestPlant();

        Debug.Log($"[Harvest] Player position AFTER: {playerTransform.position}");
        Debug.Log($"[Harvest] Position changed by: {playerTransform.position - playerPosBefore}");

        if (result != null)
        {
            PlayAnimation(harvestAnimationTrigger, facingDir);
        }
    }

    // ===== ANIMATION & FACING METHODS =====

    /// <summary>
    /// Make player face a specific direction
    /// </summary>
    private void FaceDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            // If no direction provided, use current facing direction from animator
            if (playerAnimator != null)
            {
                float currentX = playerAnimator.GetFloat(horizontalParameter);
                float currentY = playerAnimator.GetFloat(verticalParameter);

                if (currentX == 0 && currentY == 0)
                {
                    currentX = playerAnimator.GetFloat("LastInputX");
                    currentY = playerAnimator.GetFloat("LastInputY");
                }

                direction = new Vector3(currentX, currentY, 0);
            }

            // Still no direction? Default to down (not up)
            if (direction == Vector3.zero)
            {
                direction = Vector3.down;
            }
        }

        direction.Normalize();

        // Snap to cardinal directions
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        Vector2 snappedDirection;

        // Add threshold to avoid ambiguous directions
        if (Mathf.Approximately(absX, absY))
        {
            // If exactly equal (or very close), prefer horizontal
            snappedDirection = new Vector2(Mathf.Sign(direction.x), 0);
        }
        else if (absX > absY)
        {
            // Horizontal dominates
            snappedDirection = new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            // Vertical dominates
            snappedDirection = new Vector2(0, Mathf.Sign(direction.y));
        }

        lockedFacingDirection = snappedDirection;
        facingLockTimer = facingLockDuration;

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(horizontalParameter, snappedDirection.x);
            playerAnimator.SetFloat(verticalParameter, snappedDirection.y);

            // Also update last input for idle animations
            playerAnimator.SetFloat("LastInputX", snappedDirection.x);
            playerAnimator.SetFloat("LastInputY", snappedDirection.y);
        }
    }

    /// <summary>
    /// Play animation with directional parameters
    /// </summary>
    private void PlayAnimation(string triggerName, Vector3 direction)
    {
        if (playerAnimator != null && !string.IsNullOrEmpty(triggerName))
        {
            if (useDirectionalAnimations)
            {
                playerAnimator.SetFloat(horizontalParameter, direction.x);
                playerAnimator.SetFloat(verticalParameter, direction.y);
            }
            playerAnimator.SetTrigger(triggerName);
        }
    }

    // ===== DEBUG VISUALIZATION =====

    private void OnDrawGizmos()
    {
        if (selectedBlock != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(selectedBlock.worldPosition, Vector3.one * 0.9f);
        }

        // Draw facing direction
        if (playerTransform != null && Application.isPlaying)
        {
            Vector2 facing = GetPlayerFacingDirection();
            facing = SnapToCardinalDirection(facing);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(playerTransform.position, playerTransform.position + (Vector3)(facing * 2f));
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        string debugText = selectedBlock != null
            ? $"Selected: {selectedBlock.gridPosition} | Tilled: {selectedBlock.isTilled} | Planted: {selectedBlock.isPlanted}"
            : "No block selected";

        // Shadow
        GUI.color = Color.black;
        GUI.Label(new Rect(11, 71, 600, 25), debugText, style);

        // Main
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 70, 600, 25), debugText, style);
    }
}