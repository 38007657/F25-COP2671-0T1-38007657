using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Manager for Save/Load menu tab
/// Simplified version - no refresh button, no delete confirmation
/// </summary>
public class SaveLoadMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Button newSaveButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button loadButton; // Main load button
    [SerializeField] private TextMeshProUGUI loadButtonText; // Text component on load button

    [Header("New Save Panel")]
    [SerializeField] private GameObject newSavePanel;
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private Button confirmSaveButton;
    [SerializeField] private Button cancelSaveButton;

    [Header("Info Display")]
    [SerializeField] private TextMeshProUGUI currentPlayTimeText;
    [SerializeField] private TextMeshProUGUI totalSavesText;

    private SaveSlotInfo selectedSave = null; // Track which save is selected

    private void Start()
    {
        // Setup button listeners
        if (newSaveButton != null)
            newSaveButton.onClick.AddListener(ShowNewSavePanel);

        if (confirmSaveButton != null)
            confirmSaveButton.onClick.AddListener(CreateNewSave);

        if (cancelSaveButton != null)
            cancelSaveButton.onClick.AddListener(HideNewSavePanel);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadButtonClicked);

        // Make sure new save panel is hidden at start
        if (newSavePanel != null)
            newSavePanel.SetActive(false);

        // Disable load button initially (no save selected)
        UpdateLoadButton();
    }

    private void OnEnable()
    {
        UnityEngine.Debug.Log("========================================");
        UnityEngine.Debug.Log("[SaveLoadMenuUI] OnEnable called");
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] SaveLoadManager.Instance exists: {SaveLoadManager.Instance != null}");
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] saveSlotContainer assigned: {saveSlotContainer != null}");
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] saveSlotPrefab assigned: {saveSlotPrefab != null}");
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] currentPlayTimeText assigned: {currentPlayTimeText != null}");
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] totalSavesText assigned: {totalSavesText != null}");
        UnityEngine.Debug.Log("========================================");

        RefreshSaveList();
        UpdateInfoDisplay();
    }

    public void RefreshSaveList()
    {
        UnityEngine.Debug.Log("[SaveLoadMenuUI] === RefreshSaveList START ===");

        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] SaveLoadManager.Instance is NULL!");
            return;
        }
        UnityEngine.Debug.Log("[SaveLoadMenuUI] SaveLoadManager found âœ“");

        if (saveSlotContainer == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] saveSlotContainer is NULL!");
            return;
        }
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] saveSlotContainer: {saveSlotContainer.name} âœ“");

        if (saveSlotPrefab == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] saveSlotPrefab is NULL!");
            return;
        }
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] saveSlotPrefab: {saveSlotPrefab.name} âœ“");

        // Clear selection when refreshing
        selectedSave = null;
        UpdateLoadButton();

        // Clear existing slots
        int childCount = saveSlotContainer.childCount;
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] Clearing {childCount} existing children");
        foreach (Transform child in saveSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all saves
        List<SaveSlotInfo> saves = SaveLoadManager.Instance.GetAllSaves();
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] GetAllSaves returned {saves.Count} saves");

        if (saves.Count == 0)
        {
            UnityEngine.Debug.LogWarning("[SaveLoadMenuUI] No saves found - list will be empty");
        }

        // Create UI for each save
        for (int i = 0; i < saves.Count; i++)
        {
            SaveSlotInfo save = saves[i];
            UnityEngine.Debug.Log($"[SaveLoadMenuUI] Creating slot {i}: {save.saveName}");

            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            UnityEngine.Debug.Log($"[SaveLoadMenuUI] Instantiated slot object: {slotObj.name}");

            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

            if (slotUI != null)
            {
                UnityEngine.Debug.Log($"[SaveLoadMenuUI] SaveSlotUI component found, calling Setup");
                slotUI.Setup(save, this);
            }
            else
            {
                UnityEngine.Debug.LogError($"[SaveLoadMenuUI] SaveSlotUI component missing on slot {i}!");
            }
        }

        UnityEngine.Debug.Log($"[SaveLoadMenuUI] Container now has {saveSlotContainer.childCount} children");
        UnityEngine.Debug.Log("[SaveLoadMenuUI] === RefreshSaveList END ===");

        UpdateInfoDisplay();
    }

    /// <summary>
    /// Update info text display
    /// </summary>
    private void UpdateInfoDisplay()
    {
        if (SaveLoadManager.Instance == null) return;

        // Update playtime
        if (currentPlayTimeText != null)
        {
            currentPlayTimeText.text = $"Playtime: {SaveLoadManager.Instance.GetFormattedPlayTime()}";
        }

        // Update total saves count
        if (totalSavesText != null)
        {
            int saveCount = SaveLoadManager.Instance.GetAllSaves().Count;
            totalSavesText.text = $"Total Saves: {saveCount}";
        }
    }

    /// <summary>
    /// Show the new save panel
    /// </summary>
    private void ShowNewSavePanel()
    {
        if (newSavePanel != null)
        {
            newSavePanel.SetActive(true);

            // Clear previous input and focus
            if (saveNameInput != null)
            {
                saveNameInput.text = $"Save {System.DateTime.Now:MMdd_HHmm}";
                saveNameInput.Select();
                saveNameInput.ActivateInputField();
            }
        }
    }

    /// <summary>
    /// Hide the new save panel
    /// </summary>
    private void HideNewSavePanel()
    {
        if (newSavePanel != null)
        {
            newSavePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Create a new save with the entered name
    /// </summary>
    private void CreateNewSave()
    {
        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] Cannot create save - SaveLoadManager is null!");
            return;
        }

        string saveName = "New Save";

        if (saveNameInput != null && !string.IsNullOrWhiteSpace(saveNameInput.text))
        {
            saveName = saveNameInput.text.Trim();
        }

        // Create the save
        bool success = SaveLoadManager.Instance.SaveGame(saveName);

        if (success)
        {
            UnityEngine.Debug.Log($"[SaveLoadMenuUI] Created new save: {saveName}");
            HideNewSavePanel();
            RefreshSaveList();
        }
        else
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] Failed to create save!");
        }
    }

    /// <summary>
    /// Load a save by ID
    /// </summary>
    public void LoadSave(string saveId)
    {
        UnityEngine.Debug.Log("========================================");
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] LoadSave called with saveId: {saveId}");

        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] Cannot load - SaveLoadManager is null!");
            return;
        }

        UnityEngine.Debug.Log("[SaveLoadMenuUI] SaveLoadManager found, calling LoadGame...");

        bool success = SaveLoadManager.Instance.LoadGame(saveId);

        UnityEngine.Debug.Log($"[SaveLoadMenuUI] LoadGame returned: {success}");

        if (success)
        {
            UnityEngine.Debug.Log($"[SaveLoadMenuUI] ✅ Loaded save: {saveId}");

            // Close the save/load panel and pause menu, resume game
            if (PauseMenuManager.Instance != null)
            {
                UnityEngine.Debug.Log("[SaveLoadMenuUI] PauseMenuManager found, calling HidePauseMenu...");
                PauseMenuManager.Instance.HidePauseMenu();
                UnityEngine.Debug.Log("[SaveLoadMenuUI] ✅ Closed pause menu after loading");
            }
            else
            {
                UnityEngine.Debug.LogError("[SaveLoadMenuUI] ❌ PauseMenuManager.Instance is NULL!");
            }
        }
        else
        {
            UnityEngine.Debug.LogError($"[SaveLoadMenuUI] ❌ Failed to load save: {saveId}");
        }

        UnityEngine.Debug.Log("========================================");
    }

    /// <summary>
    /// Delete a save immediately (no confirmation)
    /// </summary>
    public void DeleteSave(SaveSlotInfo slotInfo)
    {
        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] Cannot delete - SaveLoadManager is null!");
            return;
        }

        bool success = SaveLoadManager.Instance.DeleteSave(slotInfo.saveId);

        if (success)
        {
            UnityEngine.Debug.Log($"[SaveLoadMenuUI] Deleted save: {slotInfo.saveName}");
            RefreshSaveList();
        }
        else
        {
            UnityEngine.Debug.LogError($"[SaveLoadMenuUI] Failed to delete save: {slotInfo.saveName}");
        }
    }

    /// <summary>
    /// Handle back button - return to pause menu
    /// </summary>
    private void OnBackClicked()
    {
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.ReturnToPauseMenu();
        }
        else
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] PauseMenuManager not found!");
        }
    }

    /// <summary>
    /// Handle main Load button click
    /// </summary>
    private void OnLoadButtonClicked()
    {
        UnityEngine.Debug.Log("[SaveLoadMenuUI] Main Load button clicked");

        if (selectedSave == null)
        {
            UnityEngine.Debug.LogWarning("[SaveLoadMenuUI] No save selected!");
            return;
        }

        UnityEngine.Debug.Log($"[SaveLoadMenuUI] Loading selected save: {selectedSave.saveName}");
        LoadSave(selectedSave.saveId);
    }

    /// <summary>
    /// Called when a save slot is clicked - selects that save
    /// </summary>
    public void SelectSave(SaveSlotInfo saveInfo)
    {
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] Save selected: {saveInfo.saveName}");
        selectedSave = saveInfo;
        UpdateLoadButton();
    }

    /// <summary>
    /// Update the load button text and state
    /// </summary>
    private void UpdateLoadButton()
    {
        if (loadButton == null) return;

        if (selectedSave == null)
        {
            // No save selected - disable button
            loadButton.interactable = false;

            if (loadButtonText != null)
            {
                loadButtonText.text = "Select a Save";
            }
        }
        else
        {
            // Save selected - enable button and show save name
            loadButton.interactable = true;

            if (loadButtonText != null)
            {
                loadButtonText.text = $"Load {selectedSave.saveName}";
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (newSaveButton != null)
            newSaveButton.onClick.RemoveListener(ShowNewSavePanel);

        if (confirmSaveButton != null)
            confirmSaveButton.onClick.RemoveListener(CreateNewSave);

        if (cancelSaveButton != null)
            cancelSaveButton.onClick.RemoveListener(HideNewSavePanel);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);

        if (loadButton != null)
            loadButton.onClick.RemoveListener(OnLoadButtonClicked);
    }
}