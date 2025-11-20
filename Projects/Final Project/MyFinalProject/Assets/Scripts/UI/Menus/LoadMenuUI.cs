using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// UI Manager for Load-only menu from start screen
/// Simplified version of SaveLoadMenuUI - no saving, just loading
/// </summary>
public class LoadMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Button backButton;

    [Header("Info Display")]
    [SerializeField] private TextMeshProUGUI totalSavesText;

    private void Start()
    {
        // Setup button listeners
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnEnable()
    {
        UnityEngine.Debug.Log("[LoadMenuUI] OnEnable called");
        RefreshSaveList();
        UpdateInfoDisplay();
    }

    /// <summary>
    /// Refresh the save list
    /// </summary>
    public void RefreshSaveList()
    {
        UnityEngine.Debug.Log("[LoadMenuUI] === RefreshSaveList START ===");

        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[LoadMenuUI] SaveLoadManager.Instance is NULL!");
            return;
        }

        if (saveSlotContainer == null)
        {
            UnityEngine.Debug.LogError("[LoadMenuUI] saveSlotContainer is NULL!");
            return;
        }

        if (saveSlotPrefab == null)
        {
            UnityEngine.Debug.LogError("[LoadMenuUI] saveSlotPrefab is NULL!");
            return;
        }

        // Clear existing slots
        foreach (Transform child in saveSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all saves
        List<SaveSlotInfo> saves = SaveLoadManager.Instance.GetAllSaves();
        UnityEngine.Debug.Log($"[LoadMenuUI] Found {saves.Count} saves");

        // Note: StartMenuManager should prevent opening this panel if no saves exist
        // But we'll handle it gracefully anyway
        if (saves.Count == 0)
        {
            UnityEngine.Debug.LogWarning("[LoadMenuUI] No saves found - this panel shouldn't be open");
            UpdateInfoDisplay();
            return;
        }

        // Sort by date (most recent first)
        saves = saves.OrderByDescending(s => s.saveDate).ToList();

        // Create UI for each save
        for (int i = 0; i < saves.Count; i++)
        {
            SaveSlotInfo save = saves[i];
            UnityEngine.Debug.Log($"[LoadMenuUI] Creating slot {i}: {save.saveName}");

            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            LoadSlotUI slotUI = slotObj.GetComponent<LoadSlotUI>();

            if (slotUI != null)
            {
                slotUI.Setup(save, this);
            }
            else
            {
                // Try using SaveSlotUI if LoadSlotUI doesn't exist
                SaveSlotUI saveSlotUI = slotObj.GetComponent<SaveSlotUI>();
                if (saveSlotUI != null)
                {
                    // We'll need to modify it to work with LoadMenuUI
                    UnityEngine.Debug.LogWarning("[LoadMenuUI] Using SaveSlotUI component - consider creating LoadSlotUI");

                    // We can work around this by manually setting up the slot
                    SetupSaveSlotForLoadOnly(slotObj, save);
                }
                else
                {
                    UnityEngine.Debug.LogError("[LoadMenuUI] No slot UI component found!");
                }
            }
        }

        UpdateInfoDisplay();
        UnityEngine.Debug.Log("[LoadMenuUI] === RefreshSaveList END ===");
    }

    /// <summary>
    /// Setup a save slot for load-only functionality
    /// </summary>
    private void SetupSaveSlotForLoadOnly(GameObject slotObj, SaveSlotInfo save)
    {
        // Find and setup text components
        TextMeshProUGUI[] texts = slotObj.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            if (text.name.Contains("Name")) text.text = save.saveName;
            else if (text.name.Contains("Date")) text.text = save.saveDate;
            else if (text.name.Contains("Day")) text.text = $"Day {save.day}";
            else if (text.name.Contains("Money")) text.text = $"${save.money}";
            else if (text.name.Contains("Time")) text.text = $"Playtime: {save.playTime}";
        }

        // Find and setup load button
        Button[] buttons = slotObj.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            if (button.name.Contains("Load"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => LoadSave(save.saveId));
            }
            else if (button.name.Contains("Delete"))
            {
                // Hide delete button in load-only menu
                button.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Update info text display
    /// </summary>
    private void UpdateInfoDisplay()
    {
        if (SaveLoadManager.Instance == null) return;

        // Update total saves count
        if (totalSavesText != null)
        {
            int saveCount = SaveLoadManager.Instance.GetAllSaves().Count;
            totalSavesText.text = $"Total Saves: {saveCount}";
        }
    }

    /// <summary>
    /// Load a save by ID
    /// </summary>
    public void LoadSave(string saveId)
    {
        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[LoadMenuUI] Cannot load - SaveLoadManager is null!");
            return;
        }

        bool success = SaveLoadManager.Instance.LoadGame(saveId);

        if (success)
        {
            UnityEngine.Debug.Log($"[LoadMenuUI] Loaded save: {saveId}");

            // Hide the start menu after successful load
            if (StartMenuManager.Instance != null)
            {
                StartMenuManager.Instance.HideStartMenu();
            }
        }
        else
        {
            UnityEngine.Debug.LogError($"[LoadMenuUI] Failed to load save: {saveId}");
        }
    }

    /// <summary>
    /// Called when back button is clicked
    /// </summary>
    private void OnBackButtonClicked()
    {
        // Return to start menu
        if (StartMenuManager.Instance != null)
        {
            StartMenuManager.Instance.ReturnToStartMenu();
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);
    }
}