using UnityEngine;

/// <summary>
/// Treasure chest that becomes available every 3 days and rewards coins
/// </summary>
public class TreasureChest : MonoBehaviour
{
    [Header("Reward Settings")]
    [SerializeField] private int minCoins = 3;
    [SerializeField] private int maxCoins = 300;
    [SerializeField] private int daysUntilAvailable = 3;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject availableParticles;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip coinSound;

    [Header("UI Feedback")]
    [SerializeField] private TreasureChestFloatingText floatingText; // Drag the text object here

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // State tracking
    private int lastOpenedDay = -999; // Set very negative so it's available at start
    private bool isAvailable = false;
    private bool playerInRange = false;
    private GameObject activeParticleInstance;
    private Transform playerTransform;

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Chest sprite is already set on the SpriteRenderer in the scene
        // No need to set it here

        // Check if chest should be available at game start
        CheckAvailability();

        if (showDebugInfo)
        {
            Debug.Log($"[TreasureChest] Initialized - Last opened day: {lastOpenedDay}");
        }
    }

    private void Update()
    {
        // Check availability based on current day
        CheckAvailability();

        // Check player distance
        CheckPlayerDistance();

        // Handle interaction input
        if (playerInRange && isAvailable && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    /// <summary>
    /// Check if chest should be available based on days passed
    /// </summary>
    private void CheckAvailability()
    {
        if (CropManager.Instance == null) return;

        int currentDay = CropManager.Instance.CurrentDay;
        int daysSinceOpened = currentDay - lastOpenedDay;

        bool shouldBeAvailable = daysSinceOpened >= daysUntilAvailable;

        // Update availability state
        if (shouldBeAvailable != isAvailable)
        {
            isAvailable = shouldBeAvailable;

            if (isAvailable)
            {
                ShowAvailableState();
            }
            else
            {
                HideAvailableState();
            }

            if (showDebugInfo)
            {
                Debug.Log($"[TreasureChest] Availability changed: {isAvailable} (Days since opened: {daysSinceOpened})");
            }
        }
    }

    /// <summary>
    /// Check if player is within interaction range
    /// </summary>
    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = distance <= interactionRange;
    }

    /// <summary>
    /// Show that chest is available to open
    /// </summary>
    private void ShowAvailableState()
    {
        // Spawn particles
        if (availableParticles != null && activeParticleInstance == null)
        {
            activeParticleInstance = Instantiate(
                availableParticles,
                transform.position + new Vector3(0, 0.5f, 0),
                Quaternion.identity,
                transform // Parent to chest so it moves with it
            );

            if (showDebugInfo)
            {
                Debug.Log("[TreasureChest] Spawned available particles");
            }
        }
    }

    /// <summary>
    /// Hide available state visuals
    /// </summary>
    private void HideAvailableState()
    {
        // Destroy particles
        if (activeParticleInstance != null)
        {
            Destroy(activeParticleInstance);
            activeParticleInstance = null;
        }
    }

    /// <summary>
    /// Open the chest and give rewards
    /// </summary>
    private void OpenChest()
    {
        if (!isAvailable)
        {
            if (showDebugInfo)
            {
                Debug.Log("[TreasureChest] Cannot open - not available yet");
            }
            return;
        }

        // Generate random coin reward
        int coinReward = Random.Range(minCoins, maxCoins + 1);

        // Give coins to player
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddMoney(coinReward);

            if (showDebugInfo)
            {
                Debug.Log($"[TreasureChest] Opened! Gave player {coinReward} coins");
            }
        }

        // Play sounds
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }

        if (coinSound != null)
        {
            AudioSource.PlayClipAtPoint(coinSound, transform.position, 0.7f);
        }

        // Update state
        if (CropManager.Instance != null)
        {
            lastOpenedDay = CropManager.Instance.CurrentDay;
        }

        isAvailable = false;

        // Hide particles
        HideAvailableState();

        // Show floating text with coin amount
        ShowCoinRewardText(coinReward);
    }

    /// <summary>
    /// Show floating text with coin reward
    /// </summary>
    private void ShowCoinRewardText(int amount)
    {
        if (floatingText != null)
        {
            floatingText.Show(amount);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[TreasureChest] Showing floating text: +{amount} Coins!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    private void OnDestroy()
    {
        // Clean up particles
        if (activeParticleInstance != null)
        {
            Destroy(activeParticleInstance);
        }
    }
}