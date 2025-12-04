using UnityEngine;
using TMPro;

/// <summary>
/// Displays current day and time to the player
/// </summary>
public class DayTimeDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI speedText; // Show time speed

    [Header("Display Settings")]
    [SerializeField] private bool show24HourFormat = false;
    [SerializeField] private bool showSpeedIndicator = true;
    [SerializeField] private string dayPrefix = "Day ";

    [Header("Optional: Hide during menus")]
    [SerializeField] private GameObject displayPanel;

    private TimeManager timeManager;
    private CropManager cropManager;

    private void Start()
    {
        // Get references
        timeManager = TimeManager.Instance;
        cropManager = CropManager.Instance;

        if (timeManager == null)
        {
            Debug.LogError("[DayTimeDisplay] TimeManager not found");
            return;
        }

        if (cropManager == null)
        {
            Debug.LogError("[DayTimeDisplay] CropManager not found");
            return;
        }

        // Subscribe to time changes
        timeManager.OnTimeChanged += UpdateTimeDisplay;

        if (showSpeedIndicator && timeManager != null)
        {
            timeManager.OnSpeedChanged += UpdateSpeedDisplay;
        }

        // Initial update
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnTimeChanged -= UpdateTimeDisplay;

            if (showSpeedIndicator)
            {
                timeManager.OnSpeedChanged -= UpdateSpeedDisplay;
            }
        }
    }

    private void Update()
    {
        // Update day display each frame
        UpdateDayDisplay();

        // Hide during menus
        CheckMenuState();
    }

    /// <summary>
    /// Update the entire display
    /// </summary>
    private void UpdateDisplay()
    {
        UpdateDayDisplay();
        UpdateTimeDisplay(timeManager.CurrentTime);

        if (showSpeedIndicator)
        {
            UpdateSpeedDisplay(timeManager.TimeSpeedMultiplier);
        }
    }

    /// <summary>
    /// Update day number display
    /// </summary>
    private void UpdateDayDisplay()
    {
        if (dayText != null && cropManager != null)
        {
            dayText.text = $"{dayPrefix}{cropManager.CurrentDay}";
        }
    }

    /// <summary>
    /// Update time display
    /// </summary>
    private void UpdateTimeDisplay(float currentTime)
    {
        if (timeText != null && timeManager != null)
        {
            if (show24HourFormat)
            {
                timeText.text = timeManager.GetTime24String();
            }
            else
            {
                timeText.text = timeManager.GetTimeString();
            }
        }
    }

    /// <summary>
    /// Update speed indicator
    /// </summary>
    private void UpdateSpeedDisplay(float speed)
    {
        if (speedText != null)
        {
            speedText.text = $"Speed: {speed}x";
        }
    }

    /// <summary>
    /// Hide display when menus are open
    /// </summary>
    private void CheckMenuState()
    {
        if (displayPanel == null) return;

        bool shouldShow = true;

        // Hide during start menu
        if (StartMenuManager.Instance != null && StartMenuManager.Instance.IsStartMenuShowing)
        {
            shouldShow = false;
        }

        // Hide during pause menu
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused)
        {
            shouldShow = false;
        }

        // Hide when inventory is open
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen)
        {
            shouldShow = false;
        }

        displayPanel.SetActive(shouldShow);
    }

    /// <summary>
    /// Toggle 12/24 hour format
    /// </summary>
    public void Toggle24HourFormat()
    {
        show24HourFormat = !show24HourFormat;
        UpdateDisplay();
    }
}