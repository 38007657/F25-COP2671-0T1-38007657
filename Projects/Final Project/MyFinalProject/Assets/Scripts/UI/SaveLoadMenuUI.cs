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

    [Header("New Save Panel")]
    [SerializeField] private GameObject newSavePanel;
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private Button confirmSaveButton;
    [SerializeField] private Button cancelSaveButton;

    [Header("Info Display")]
    [SerializeField] private TextMeshProUGUI currentPlayTimeText;
    [SerializeField] private TextMeshProUGUI totalSavesText;

    private void Start()
    {
        // Setup button listeners
        if (newSaveButton != null)
            newSaveButton.onClick.AddListener(ShowNewSavePanel);

        if (confirmSaveButton != null)
            confirmSaveButton.onClick.AddListener(CreateNewSave);

        if (cancelSaveButton != null)
            cancelSaveButton.onClick.AddListener(HideNewSavePanel);

        // Make sure new save panel is hidden at start
        if (newSavePanel != null)
            newSavePanel.SetActive(false);
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
        UnityEngine.Debug.Log("[SaveLoadMenuUI] SaveLoadManager found ✓");

        if (saveSlotContainer == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] saveSlotContainer is NULL!");
            return;
        }
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] saveSlotContainer: {saveSlotContainer.name} ✓");

        if (saveSlotPrefab == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] saveSlotPrefab is NULL!");
            return;
        }
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] saveSlotPrefab: {saveSlotPrefab.name} ✓");

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
        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] Cannot load - SaveLoadManager is null!");
            return;
        }

        bool success = SaveLoadManager.Instance.LoadGame(saveId);

        if (success)
        {
            UnityEngine.Debug.Log($"[SaveLoadMenuUI] Loaded save: {saveId}");
            RefreshSaveList();
            UpdateInfoDisplay();
        }
        else
        {
            UnityEngine.Debug.LogError($"[SaveLoadMenuUI] Failed to load save: {saveId}");
        }
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

    private void OnDestroy()
    {
        // Clean up listeners
        if (newSaveButton != null)
            newSaveButton.onClick.RemoveListener(ShowNewSavePanel);

        if (confirmSaveButton != null)
            confirmSaveButton.onClick.RemoveListener(CreateNewSave);

        if (cancelSaveButton != null)
            cancelSaveButton.onClick.RemoveListener(HideNewSavePanel);
    }
}