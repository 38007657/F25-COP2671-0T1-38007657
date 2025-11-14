using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu shown when game starts
/// Appears over the inactive game scene
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    public static StartMenuManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject errorMessagePanel; // Panel with error text

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float errorMessageDuration = 2f; // How long to show error

    private bool isInStartMenu = true;
    private float errorMessageTimer = 0f;

    // Public property to check if we're in the start menu
    public bool IsInStartMenu => isInStartMenu;

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

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Hide error message initially
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(false);

        // Show start menu on game start
        ShowStartMenu();

        // Pause the game while in menu
        Time.timeScale = 0f;

        // Check if Continue button should be enabled
        UpdateContinueButton();
    }

    private void Update()
    {
        // Handle error message timer
        if (errorMessageTimer > 0f)
        {
            errorMessageTimer -= Time.unscaledDeltaTime; // Use unscaled time since game is paused

            if (errorMessageTimer <= 0f)
            {
                HideErrorMessage();
            }
        }
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

        UnityEngine.Debug.Log("[StartMenu] Start menu shown");
    }

    /// <summary>
    /// Hide the start menu and start playing
    /// </summary>
    public void HideStartMenu()
    {
        UnityEngine.Debug.Log("[StartMenu] HideStartMenu() called");

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }

        isInStartMenu = false;
        Time.timeScale = 1f; // Resume game

        UnityEngine.Debug.Log($"[StartMenu] Start menu hidden - isInStartMenu is now: {isInStartMenu}");
    }

    /// <summary>
    /// Check if we have a save to continue from
    /// </summary>
    private void UpdateContinueButton()
    {
        if (continueButton == null)
        {
            UnityEngine.Debug.LogWarning("[StartMenu] Continue button is not assigned!");
            return;
        }

        bool hasSave = false;

        if (SaveLoadManager.Instance != null)
        {
            hasSave = SaveLoadManager.Instance.AnySavesExist();
            UnityEngine.Debug.Log($"[StartMenu] SaveLoadManager found. AnySavesExist() = {hasSave}");

            // Also log how many saves exist
            var saves = SaveLoadManager.Instance.GetAllSaves();
            UnityEngine.Debug.Log($"[StartMenu] Number of saves found: {saves.Count}");
        }
        else
        {
            UnityEngine.Debug.LogError("[StartMenu] SaveLoadManager.Instance is NULL!");
        }

        continueButton.interactable = hasSave;

        UnityEngine.Debug.Log($"[StartMenu] Continue button: {(hasSave ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// Continue from most recent save
    /// </summary>
    private void OnContinueClicked()
    {
        UnityEngine.Debug.Log("[StartMenu] Continue button clicked");

        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[StartMenu] SaveLoadManager not found!");
            ShowErrorMessage("Error: Save system not found!");
            return;
        }

        // Check if saves exist
        if (!SaveLoadManager.Instance.AnySavesExist())
        {
            UnityEngine.Debug.Log("[StartMenu] No saves exist!");
            ShowErrorMessage("No save files found!");
            return;
        }

        // Load the most recent save
        bool success = SaveLoadManager.Instance.LoadLatestSave();

        if (success)
        {
            HideStartMenu();
            UnityEngine.Debug.Log("[StartMenu] Successfully loaded latest save");
        }
        else
        {
            UnityEngine.Debug.LogError("[StartMenu] Failed to load latest save!");
            ShowErrorMessage("Failed to load save file!");
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

        UnityEngine.Debug.Log("[StartMenu] New game started");
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    private void OnQuitClicked()
    {
        UnityEngine.Debug.Log("[StartMenu] Quitting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    /// <summary>
    /// Show error message for a duration
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        if (errorMessagePanel != null)
        {
            errorMessagePanel.SetActive(true);
            errorMessageTimer = errorMessageDuration;

            // Optionally update the text if you have a TextMeshProUGUI component
            var textComponent = errorMessagePanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = message;
            }

            UnityEngine.Debug.Log($"[StartMenu] Showing error: {message}");
        }
    }

    /// <summary>
    /// Hide error message
    /// </summary>
    private void HideErrorMessage()
    {
        if (errorMessagePanel != null)
        {
            errorMessagePanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(OnNewGameClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }
}