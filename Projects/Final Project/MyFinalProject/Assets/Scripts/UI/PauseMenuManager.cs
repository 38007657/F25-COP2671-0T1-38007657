using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause menu accessible during gameplay
/// Press ESC to open/close
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Image darkOverlay;

    [Header("Buttons")]
    [SerializeField] private Button returnToGameButton;
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private Button saveLoadGameButton;

    [Header("Panels")]
    [SerializeField] private GameObject saveLoadPanel;
    [SerializeField] private GameObject saveLoadTabContent; // The actual content inside the panel

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.7f);

    private bool isPaused = false;

    private void Awake()
    {
        Debug.Log("[PauseMenuManager] Awake called");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PauseMenuManager] Duplicate instance found, destroying");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("[PauseMenuManager] Instance set successfully");
    }

    private void Start()
    {
        Debug.Log("========================================");
        Debug.Log("[PauseMenuManager] Start called");
        Debug.Log($"[PauseMenuManager] pauseMenuPanel assigned: {pauseMenuPanel != null}");
        Debug.Log($"[PauseMenuManager] darkOverlay assigned: {darkOverlay != null}");
        Debug.Log($"[PauseMenuManager] returnToGameButton assigned: {returnToGameButton != null}");
        Debug.Log($"[PauseMenuManager] returnToMenuButton assigned: {returnToMenuButton != null}");
        Debug.Log($"[PauseMenuManager] saveLoadGameButton assigned: {saveLoadGameButton != null}");
        Debug.Log($"[PauseMenuManager] saveLoadPanel assigned: {saveLoadPanel != null}");
        Debug.Log($"[PauseMenuManager] saveLoadTabContent assigned: {saveLoadTabContent != null}");
        Debug.Log("========================================");

        // Setup button listeners
        if (returnToGameButton != null)
            returnToGameButton.onClick.AddListener(OnReturnToGame);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenu);

        if (saveLoadGameButton != null)
            saveLoadGameButton.onClick.AddListener(OnSaveLoadClicked);

        // Start with pause menu hidden
        HidePauseMenu();

        // Make sure save/load panel AND its content are hidden
        if (saveLoadPanel != null)
            saveLoadPanel.SetActive(false);

        if (saveLoadTabContent != null)
            saveLoadTabContent.SetActive(false);

        Debug.Log("[PauseMenuManager] Start complete - pause menu should be hidden");
    }

    private void Update()
    {
        // Toggle pause with ESC key
        if (Input.GetKeyDown(pauseKey))
        {
            Debug.Log($"[PauseMenuManager] ESC key pressed! isPaused: {isPaused}");

            // Don't allow pausing if we're in start menu
            if (StartMenuManager.Instance != null)
            {
                // Check if start menu canvas is enabled (not just if GameObject is active)
                Canvas startCanvas = StartMenuManager.Instance.GetComponent<Canvas>();
                if (startCanvas != null && startCanvas.enabled)
                {
                    Debug.Log("[PauseMenuManager] Start menu canvas is enabled, ignoring ESC");
                    return;
                }
            }

            TogglePause();
        }
    }

    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            HidePauseMenu();
        }
        else
        {
            ShowPauseMenu();
        }
    }

    /// <summary>
    /// Show the pause menu
    /// </summary>
    public void ShowPauseMenu()
    {
        Debug.Log("[PauseMenuManager] ShowPauseMenu called");

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Debug.Log($"[PauseMenuManager] pauseMenuPanel set to active. Is it active? {pauseMenuPanel.activeSelf}");
        }
        else
        {
            Debug.LogError("[PauseMenuManager] pauseMenuPanel is NULL!");
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.color = overlayColor;
            Debug.Log("[PauseMenuManager] darkOverlay activated");
        }
        else
        {
            Debug.LogWarning("[PauseMenuManager] darkOverlay is NULL!");
        }

        isPaused = true;
        Time.timeScale = 0f; // Pause game

        Debug.Log("[PauseMenuManager] Game paused - Time.timeScale set to 0");
    }

    /// <summary>
    /// Hide the pause menu
    /// </summary>
    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(false);
        }

        isPaused = false;
        Time.timeScale = 1f; // Resume game

        Debug.Log("[PauseMenu] Game resumed");
    }

    /// <summary>
    /// Return to game (resume)
    /// </summary>
    private void OnReturnToGame()
    {
        HidePauseMenu();
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    private void OnReturnToMenu()
    {
        // Hide pause menu
        HidePauseMenu();

        // Show start menu
        if (StartMenuManager.Instance != null)
        {
            StartMenuManager.Instance.ShowStartMenu();
        }

        Debug.Log("[PauseMenu] Returned to main menu");
    }

    /// <summary>
    /// Show save/load panel
    /// </summary>
    private void OnSaveLoadClicked()
    {
        Debug.Log("[PauseMenu] OnSaveLoadClicked called");

        if (saveLoadPanel != null)
        {
            Debug.Log($"[PauseMenu] saveLoadPanel found: {saveLoadPanel.name}");
            Debug.Log($"[PauseMenu] saveLoadPanel active BEFORE: {saveLoadPanel.activeSelf}");

            // Hide pause menu buttons but keep overlay and time paused
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
                Debug.Log("[PauseMenu] pauseMenuPanel hidden");
            }

            // Show save/load panel
            saveLoadPanel.SetActive(true);

            // IMPORTANT: Also activate the content inside (leftover from tabbed system)
            if (saveLoadTabContent != null)
            {
                saveLoadTabContent.SetActive(true);
                Debug.Log("[PauseMenu] saveLoadTabContent activated");
            }

            Debug.Log($"[PauseMenu] saveLoadPanel active AFTER: {saveLoadPanel.activeSelf}");
            Debug.Log("[PauseMenu] Save/Load panel opened");
        }
        else
        {
            Debug.LogError("[PauseMenu] Save/Load panel not assigned!");
        }
    }

    /// <summary>
    /// Return from save/load panel to pause menu
    /// Called by SaveLoadMenuUI when user wants to go back
    /// </summary>
    public void ReturnToPauseMenu()
    {
        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(false);
        }

        // Also hide the tab content
        if (saveLoadTabContent != null)
        {
            saveLoadTabContent.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        Debug.Log("[PauseMenu] Returned to pause menu from save/load");
    }

    // Property
    public bool IsPaused => isPaused;

    private void OnDestroy()
    {
        // Clean up listeners
        if (returnToGameButton != null)
            returnToGameButton.onClick.RemoveListener(OnReturnToGame);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenu);

        if (saveLoadGameButton != null)
            saveLoadGameButton.onClick.RemoveListener(OnSaveLoadClicked);
    }
}