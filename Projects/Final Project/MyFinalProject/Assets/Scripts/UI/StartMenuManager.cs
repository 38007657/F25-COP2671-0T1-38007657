using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu shown when game starts
/// Covers the game world with a dark overlay
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    public static StartMenuManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private Image darkOverlay;
    [SerializeField] private Canvas startMenuCanvas; // Reference to the Canvas component

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;

    [Header("Other Panels")]
    [SerializeField] private GameObject loadGamePanel; // Your existing SaveLoadTab

    [Header("Settings")]
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.85f); // Dark overlay
    [SerializeField] private float fadeSpeed = 2f;

    private bool isInStartMenu = true;

    // Public property for other scripts to check
    public bool IsInStartMenu => isInStartMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Get Canvas component if not assigned
        if (startMenuCanvas == null)
        {
            startMenuCanvas = GetComponent<Canvas>();
        }
    }

    private void Start()
    {
        // Setup button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(OnLoadGameClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Show start menu on game start
        ShowStartMenu();

        // Pause the game while in menu
        Time.timeScale = 0f;

        // Check if Continue button should be enabled
        UpdateContinueButton();
    }

    /// <summary>
    /// Show the start menu
    /// </summary>
    public void ShowStartMenu()
    {
        // Enable the canvas to make everything visible
        if (startMenuCanvas != null)
        {
            startMenuCanvas.enabled = true;
        }

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.color = overlayColor;
        }

        isInStartMenu = true;
        Time.timeScale = 0f; // Pause game

        Debug.Log("[StartMenu] Start menu shown");
    }

    /// <summary>
    /// Hide the start menu and start playing
    /// </summary>
    public void HideStartMenu()
    {
        Debug.Log("[StartMenu] ===== HideStartMenu called =====");

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
            Debug.Log("[StartMenu] startMenuPanel deactivated");
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(false);
            Debug.Log("[StartMenu] darkOverlay deactivated");
        }

        isInStartMenu = false;
        Time.timeScale = 1f; // Resume game
        Debug.Log("[StartMenu] isInStartMenu = false, Time.timeScale = 1");

        // Disable the canvas to make it completely inactive
        if (startMenuCanvas != null)
        {
            startMenuCanvas.enabled = false;
            Debug.Log($"[StartMenu] Canvas disabled. Canvas.enabled is now: {startMenuCanvas.enabled}");
        }
        else
        {
            Debug.LogError("[StartMenu] startMenuCanvas is NULL!");
        }

        Debug.Log("[StartMenu] ===== HideStartMenu complete =====");
    }

    /// <summary>
    /// Check if we have a save to continue from
    /// </summary>
    private void UpdateContinueButton()
    {
        if (continueButton == null) return;

        bool hasSave = SaveLoadManager.Instance != null &&
                       SaveLoadManager.Instance.AnySavesExist();

        continueButton.interactable = hasSave;

        Debug.Log($"[StartMenu] Continue button: {(hasSave ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// Continue from most recent save
    /// </summary>
    private void OnContinueClicked()
    {
        if (SaveLoadManager.Instance != null)
        {
            // Load the most recent save
            SaveLoadManager.Instance.LoadLatestSave();
            HideStartMenu();
        }
        else
        {
            Debug.LogError("[StartMenu] SaveLoadManager not found!");
        }
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    private void OnNewGameClicked()
    {
        // Just hide menu and start playing
        // The game should already be initialized
        HideStartMenu();

        Debug.Log("[StartMenu] New game started");
    }

    /// <summary>
    /// Show the load game panel
    /// </summary>
    private void OnLoadGameClicked()
    {
        if (loadGamePanel != null)
        {
            // Hide start menu buttons but keep overlay
            if (startMenuPanel != null)
            {
                startMenuPanel.SetActive(false);
            }

            // Show load panel
            loadGamePanel.SetActive(true);

            Debug.Log("[StartMenu] Load game panel opened");
        }
    }

    /// <summary>
    /// Return from load panel to start menu
    /// </summary>
    public void ReturnToStartMenu()
    {
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(false);
        }

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
        }

        Debug.Log("[StartMenu] Returned to start menu");
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    private void OnQuitClicked()
    {
        Debug.Log("[StartMenu] Quitting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(OnNewGameClicked);

        if (loadGameButton != null)
            loadGameButton.onClick.RemoveListener(OnLoadGameClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }
}