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

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.7f);

    private bool isPaused = false;

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
        if (returnToGameButton != null)
            returnToGameButton.onClick.AddListener(OnReturnToGame);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenu);

        // Start with pause menu hidden
        HidePauseMenu();
    }

    private void Update()
    {
        // Toggle pause with ESC key
        if (Input.GetKeyDown(pauseKey))
        {
            // Don't allow pausing if we're in start menu
            if (StartMenuManager.Instance != null &&
                StartMenuManager.Instance.gameObject.activeInHierarchy)
            {
                return;
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
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
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

    // Property
    public bool IsPaused => isPaused;

    private void OnDestroy()
    {
        // Clean up listeners
        if (returnToGameButton != null)
            returnToGameButton.onClick.RemoveListener(OnReturnToGame);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenu);
    }
}