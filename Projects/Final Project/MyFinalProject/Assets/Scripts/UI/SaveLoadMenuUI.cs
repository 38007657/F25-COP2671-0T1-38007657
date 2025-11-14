using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Manager for Save/Load menu tab
/// UPDATED: Added Load button functionality
/// </summary>
public class SaveLoadMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Button newSaveButton;
    [SerializeField] private Button loadButton; // NEW: Load button

    [Header("New Save Panel")]
    [SerializeField] private GameObject newSavePanel;
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private Button confirmSaveButton;
    [SerializeField] private Button cancelSaveButton;

    [Header("Info Display")]
    [SerializeField] private TextMeshProUGUI currentPlayTimeText;
    [SerializeField] private TextMeshProUGUI totalSavesText;

    private SaveSlotUI selectedSlot = null; // Track which slot is selected
    private List<SaveSlotUI> allSlots = new List<SaveSlotUI>();

    private void Start()
    {
        // Setup button listeners
        if (newSaveButton != null)
            newSaveButton.onClick.AddListener(ShowNewSavePanel);

        if (confirmSaveButton != null)
            confirmSaveButton.onClick.AddListener(CreateNewSave);

        if (cancelSaveButton != null)
            cancelSaveButton.onClick.AddListener(HideNewSavePanel);

        // NEW: Setup load button
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(LoadSelectedSave);
            loadButton.interactable = false; // Disabled by default (nothing selected)
        }

        // Make sure new save panel is hidden at start
        if (newSavePanel != null)
            newSavePanel.SetActive(false);
    }

    private void OnEnable()
    {
        UnityEngine.Debug.Log("[SaveLoadMenuUI] OnEnable called");
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

        if (saveSlotContainer == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] saveSlotContainer is NULL!");
            return;
        }

        if (saveSlotPrefab == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] saveSlotPrefab is NULL!");
            return;
        }

        // Clear existing slots
        foreach (SaveSlotUI slot in allSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        allSlots.Clear();
        selectedSlot = null;

        // Update load button state (nothing selected now)
        UpdateLoadButtonState();

        // Get all saves
        List<SaveSlotInfo> saves = SaveLoadManager.Instance.GetAllSaves();
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] GetAllSaves returned {saves.Count} saves");

        // Create UI for each save
        for (int i = 0; i < saves.Count; i++)
        {
            SaveSlotInfo save = saves[i];
            UnityEngine.Debug.Log($"[SaveLoadMenuUI] Creating slot {i}: {save.saveName}");

            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

            if (slotUI != null)
            {
                slotUI.Setup(save, this);

                // Subscribe to selection events
                slotUI.OnSlotSelected += OnSlotSelected;

                allSlots.Add(slotUI);
            }
            else
            {
                UnityEngine.Debug.LogError($"[SaveLoadMenuUI] SaveSlotUI component missing on slot {i}!");
            }
        }

        UnityEngine.Debug.Log($"[SaveLoadMenuUI] Container now has {saveSlotContainer.childCount} children");
        UpdateInfoDisplay();
    }

    /// <summary>
    /// Called when a save slot is selected/clicked
    /// </summary>
    private void OnSlotSelected(SaveSlotUI slot)
    {
        // Deselect previous slot
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }

        // Select new slot
        selectedSlot = slot;
        selectedSlot.SetSelected(true);

        // Update load button state
        UpdateLoadButtonState();

        UnityEngine.Debug.Log($"[SaveLoadMenuUI] Selected save: {slot.SaveSlotInfo.saveName}");
    }

    /// <summary>
    /// Update the load button's interactable state
    /// </summary>
    private void UpdateLoadButtonState()
    {
        if (loadButton != null)
        {
            // Enable load button only if a slot is selected
            loadButton.interactable = (selectedSlot != null);

            // Optional: Update button text to show what will be loaded
            TextMeshProUGUI buttonText = loadButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (selectedSlot != null)
                {
                    buttonText.text = $"Load: {selectedSlot.SaveSlotInfo.saveName}";
                }
                else
                {
                    buttonText.text = "Load Save";
                }
            }
        }
    }

    /// <summary>
    /// Load the currently selected save
    /// </summary>
    private void LoadSelectedSave()
    {
        if (selectedSlot == null)
        {
            UnityEngine.Debug.LogWarning("[SaveLoadMenuUI] No save selected to load!");
            return;
        }

        if (SaveLoadManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[SaveLoadMenuUI] Cannot load - SaveLoadManager is null!");
            return;
        }

        string saveId = selectedSlot.SaveSlotInfo.saveId;
        UnityEngine.Debug.Log($"[SaveLoadMenuUI] Loading save: {saveId}");

        bool success = SaveLoadManager.Instance.LoadGame(saveId);

        if (success)
        {
            UnityEngine.Debug.Log($"[SaveLoadMenuUI] Successfully loaded save: {selectedSlot.SaveSlotInfo.saveName}");

            // Optional: Close the menu after loading
            // if (InventoryUIManager.Instance != null)
            // {
            //     InventoryUIManager.Instance.ToggleInventory();
            // }

            // Refresh display
            RefreshSaveList();
            UpdateInfoDisplay();
        }
        else
        {
            UnityEngine.Debug.LogError($"[SaveLoadMenuUI] Failed to load save: {saveId}");
        }
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
    /// Delete a save (called by SaveSlotUI)
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

            // Clear selection if we deleted the selected save
            if (selectedSlot != null && selectedSlot.SaveSlotInfo.saveId == slotInfo.saveId)
            {
                selectedSlot = null;
                UpdateLoadButtonState();
            }

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

        if (loadButton != null)
            loadButton.onClick.RemoveListener(LoadSelectedSave);

        // Unsubscribe from all slot events
        foreach (SaveSlotUI slot in allSlots)
        {
            if (slot != null)
            {
                slot.OnSlotSelected -= OnSlotSelected;
            }
        }
    }
}