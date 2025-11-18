using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Pause menu accessible during gameplay
/// Press ESC to open/close
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject pauseButtonContainer; // The container with buttons
    [SerializeField] private GameObject saveLoadPanel; // The SaveLoadPanel from hierarchy
    [SerializeField] private Image darkOverlay;

    [Header("Buttons")]
    [SerializeField] private Button returnToGameButton;
    [SerializeField] private Button saveLoadButton;
    [SerializeField] private Button returnToMenuButton;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.7f);

    private bool isPaused = false;

    private void Awake()
    {
        Debug.Log("[PauseMenu] Awake called");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PauseMenu] Duplicate instance found, destroying");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("[PauseMenu] Instance set successfully");
    }

    private void Start()
    {
        Debug.Log("========================================");
        Debug.Log("[PauseMenu] Start called");
        Debug.Log($"[PauseMenu] pauseMenuPanel assigned: {pauseMenuPanel != null}");
        Debug.Log($"[PauseMenu] pauseButtonContainer assigned: {pauseButtonContainer != null}");
        Debug.Log($"[PauseMenu] saveLoadPanel assigned: {saveLoadPanel != null}");
        Debug.Log($"[PauseMenu] darkOverlay assigned: {darkOverlay != null}");
        Debug.Log($"[PauseMenu] returnToGameButton assigned: {returnToGameButton != null}");
        Debug.Log($"[PauseMenu] saveLoadButton assigned: {saveLoadButton != null}");
        Debug.Log($"[PauseMenu] returnToMenuButton assigned: {returnToMenuButton != null}");
        Debug.Log("========================================");

        // Setup button listeners
        if (returnToGameButton != null)
            returnToGameButton.onClick.AddListener(OnReturnToGame);

        if (saveLoadButton != null)
            saveLoadButton.onClick.AddListener(OnSaveLoadClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenu);

        // Start with pause menu hidden
        HidePauseMenu();

        // Make sure save/load panel is hidden at start
        if (saveLoadPanel != null)
            saveLoadPanel.SetActive(false);

        Debug.Log("[PauseMenu] Start completed");
    }

    private void Update()
    {
        // Toggle pause with ESC key
        if (Input.GetKeyDown(pauseKey))
        {
            Debug.Log($"[PauseMenu] ========== ESC PRESSED ==========");
            Debug.Log($"[PauseMenu] isPaused: {isPaused}");
            Debug.Log($"[PauseMenu] Time.timeScale: {Time.timeScale}");

            // Don't allow pausing if we're in start menu
            if (StartMenuManager.Instance != null)
            {
                bool startMenuShowing = StartMenuManager.Instance.IsStartMenuShowing;
                Debug.Log($"[PauseMenu] StartMenuManager exists, IsStartMenuShowing: {startMenuShowing}");

                if (startMenuShowing)
                {
                    Debug.Log("[PauseMenu] Start menu is showing, ignoring ESC");
                    return;
                }
            }
            else
            {
                Debug.Log("[PauseMenu] No StartMenuManager found");
            }

            // If save/load panel is open, close it instead of unpausing
            if (saveLoadPanel != null && saveLoadPanel.activeSelf)
            {
                Debug.Log("[PauseMenu] Save/load panel open, hiding it");
                HideSaveLoadPanel();
                return;
            }

            Debug.Log("[PauseMenu] Calling TogglePause()");
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
        Debug.Log("[PauseMenu] ShowPauseMenu called");

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Debug.Log("[PauseMenu] PauseMenuPanel activated");
        }
        else
        {
            Debug.LogError("[PauseMenu] PauseMenuPanel is NULL!");
        }

        // Show main buttons, hide save/load panel
        if (pauseButtonContainer != null)
        {
            pauseButtonContainer.SetActive(true);
        }

        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(false);
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.color = overlayColor;
        }

        isPaused = true;
        Time.timeScale = 0f; // Pause game

        Debug.Log("[PauseMenu] Game paused");
    }

    /// <summary>
    /// Hide the pause menu
    /// </summary>
    public void HidePauseMenu()
    {
        Debug.Log("[PauseMenu] HidePauseMenu called");

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
    /// Show the save/load panel
    /// </summary>
    private void OnSaveLoadClicked()
    {
        ShowSaveLoadPanel();
    }

    /// <summary>
    /// Show save/load panel and hide main pause menu buttons
    /// </summary>
    public void ShowSaveLoadPanel()
    {
        Debug.Log("[PauseMenu] === ShowSaveLoadPanel START ===");

        if (pauseButtonContainer != null)
        {
            pauseButtonContainer.SetActive(false);
            Debug.Log("[PauseMenu] Hid pause button container");
        }
        else
        {
            Debug.LogError("[PauseMenu] pauseButtonContainer is NULL!");
        }

        if (saveLoadPanel != null)
        {
            Debug.Log($"[PauseMenu] saveLoadPanel found: {saveLoadPanel.name}");
            Debug.Log($"[PauseMenu] saveLoadPanel was active: {saveLoadPanel.activeSelf}");

            saveLoadPanel.SetActive(true);
            Debug.Log($"[PauseMenu] saveLoadPanel is now active: {saveLoadPanel.activeSelf}");

            // Refresh the save list when opening
            SaveLoadMenuUI saveLoadUI = saveLoadPanel.GetComponent<SaveLoadMenuUI>();
            if (saveLoadUI != null)
            {
                Debug.Log("[PauseMenu] SaveLoadMenuUI component found, calling RefreshSaveList()");
                saveLoadUI.RefreshSaveList();
            }
            else
            {
                Debug.LogError("[PauseMenu] SaveLoadMenuUI component NOT FOUND on saveLoadPanel!");
                Debug.LogError($"[PauseMenu] saveLoadPanel has these components: {string.Join(", ", saveLoadPanel.GetComponents<Component>().Select(c => c.GetType().Name))}");
            }
        }
        else
        {
            Debug.LogError("[PauseMenu] saveLoadPanel is NULL!");
        }

        Debug.Log("[PauseMenu] === ShowSaveLoadPanel END ===");
    }

    /// <summary>
    /// Hide save/load panel and show main pause menu buttons
    /// </summary>
    public void HideSaveLoadPanel()
    {
        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(false);
        }

        if (pauseButtonContainer != null)
        {
            pauseButtonContainer.SetActive(true);
        }

        Debug.Log("[PauseMenu] Save/Load panel closed");
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

    // Property
    public bool IsPaused => isPaused;

    private void OnDestroy()
    {
        // Clean up listeners
        if (returnToGameButton != null)
            returnToGameButton.onClick.RemoveListener(OnReturnToGame);

        if (saveLoadButton != null)
            saveLoadButton.onClick.RemoveListener(OnSaveLoadClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenu);
    }
}