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

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;

    [Header("Other Panels")]
    [SerializeField] private GameObject loadGamePanel; // Dedicated load-only panel

    private bool isInStartMenu = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
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
        Debug.Log("[StartMenu] HideStartMenu called");

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }

        // Hide the load game panel too if it's open
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(false);
        }

        isInStartMenu = false;
        Time.timeScale = 1f; // Resume game

        Debug.Log("[StartMenu] Start menu hidden - game started");
    }

    /// <summary>
    /// Check if we have saves and update button states
    /// </summary>
    private void UpdateContinueButton()
    {
        bool hasSave = SaveLoadManager.Instance != null &&
                       SaveLoadManager.Instance.AnySavesExist();

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;
            Debug.Log($"[StartMenu] Continue button: {(hasSave ? "Enabled" : "Disabled")}");
        }

        if (loadGameButton != null)
        {
            loadGameButton.interactable = hasSave;
            Debug.Log($"[StartMenu] Load Game button: {(hasSave ? "Enabled" : "Disabled")}");
        }
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
    /// Show the load game panel or display error if no saves
    /// </summary>
    private void OnLoadGameClicked()
    {
        // Check if there are any saves first
        if (SaveLoadManager.Instance == null || !SaveLoadManager.Instance.AnySavesExist())
        {
            // Show error notification instead of opening load panel
            NotificationManager.Instance?.ShowNotification(
                "No saved games found! Start a New Game to begin.",
                NotificationType.Error
            );

            Debug.Log("[StartMenu] No saves found - showing notification");
            return;
        }

        if (loadGamePanel != null)
        {
            // Hide start menu panel (since they're siblings in the same canvas)
            if (startMenuPanel != null)
            {
                startMenuPanel.SetActive(false);
            }

            // Show load panel
            loadGamePanel.SetActive(true);

            // Refresh the save list when opening
            LoadMenuUI loadMenuUI = loadGamePanel.GetComponent<LoadMenuUI>();
            if (loadMenuUI != null)
            {
                Debug.Log("[StartMenu] LoadMenuUI component found, refreshing save list");
                loadMenuUI.RefreshSaveList();
            }
            else
            {
                Debug.LogError("[StartMenu] LoadMenuUI component NOT FOUND on loadGamePanel!");
            }

            Debug.Log("[StartMenu] Load game panel opened");
        }
        else
        {
            Debug.LogError("[StartMenu] loadGamePanel is not assigned!");
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

        // Update button states in case saves changed
        UpdateContinueButton();

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

    // Public property to check if start menu is actually showing
    public bool IsStartMenuShowing => isInStartMenu;
}