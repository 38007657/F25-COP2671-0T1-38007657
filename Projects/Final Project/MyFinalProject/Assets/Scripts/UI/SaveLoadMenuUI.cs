using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the Save/Load menu UI tab
/// </summary>
public class SaveLoadMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Button newSaveButton;
    [SerializeField] private Button refreshButton;

    [Header("New Save Panel")]
    [SerializeField] private GameObject newSavePanel;
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private Button confirmSaveButton;
    [SerializeField] private Button cancelSaveButton;

    [Header("Confirmation Panel")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Info Display")]
    [SerializeField] private TextMeshProUGUI currentPlayTimeText;
    [SerializeField] private TextMeshProUGUI totalSavesText;

    private List<SaveSlotUI> activeSlots = new List<SaveSlotUI>();
    private SaveSlotInfo pendingDeleteSlot;

    private void Start()
    {
        // Setup buttons
        if (newSaveButton != null)
            newSaveButton.onClick.AddListener(ShowNewSavePanel);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshSaveList);

        if (confirmSaveButton != null)
            confirmSaveButton.onClick.AddListener(CreateNewSave);

        if (cancelSaveButton != null)
            cancelSaveButton.onClick.AddListener(HideNewSavePanel);

        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(ConfirmDelete);

        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(CancelDelete);

        // Hide panels initially
        if (newSavePanel != null)
            newSavePanel.SetActive(false);

        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        // Initial refresh
        RefreshSaveList();
    }

    private void OnEnable()
    {
        RefreshSaveList();
        UpdatePlayTimeDisplay();
    }

    /// <summary>
    /// Refresh the list of save slots
    /// </summary>
    public void RefreshSaveList()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("[SaveLoadMenuUI] SaveLoadManager not found!");
            return;
        }

        // Clear existing slots
        ClearSlots();

        // Get all saves
        List<SaveSlotInfo> saves = SaveLoadManager.Instance.GetAllSaves();

        // Sort by date (most recent first)
        saves = saves.OrderByDescending(s => s.saveDate).ToList();

        // Create slot UI for each save
        foreach (SaveSlotInfo save in saves)
        {
            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

            if (slotUI != null)
            {
                slotUI.Setup(save, this);
                activeSlots.Add(slotUI);
            }
        }

        // Update info display
        if (totalSavesText != null)
        {
            totalSavesText.text = $"Total Saves: {saves.Count}";
        }

        UpdatePlayTimeDisplay();

        Debug.Log($"[SaveLoadMenuUI] Refreshed save list - {saves.Count} saves found");
    }

    /// <summary>
    /// Clear all slot UIs
    /// </summary>
    private void ClearSlots()
    {
        foreach (SaveSlotUI slot in activeSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        activeSlots.Clear();
    }

    /// <summary>
    /// Update playtime display
    /// </summary>
    private void UpdatePlayTimeDisplay()
    {
        if (currentPlayTimeText != null && SaveLoadManager.Instance != null)
        {
            currentPlayTimeText.text = $"Current Session: {SaveLoadManager.Instance.GetFormattedPlayTime()}";
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

            // Set default name
            if (saveNameInput != null)
            {
                int saveCount = SaveLoadManager.Instance.GetAllSaves().Count;
                saveNameInput.text = $"Save {saveCount + 1}";
                saveNameInput.Select();
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
    /// Create a new save
    /// </summary>
    private void CreateNewSave()
    {
        if (SaveLoadManager.Instance == null) return;

        string saveName = saveNameInput != null ? saveNameInput.text : "New Save";

        if (string.IsNullOrWhiteSpace(saveName))
        {
            saveName = "Unnamed Save";
        }

        bool success = SaveLoadManager.Instance.SaveGame(saveName);

        if (success)
        {
            Debug.Log($"[SaveLoadMenuUI] Created new save: {saveName}");
            HideNewSavePanel();
            RefreshSaveList();
        }
        else
        {
            Debug.LogError("[SaveLoadMenuUI] Failed to create save!");
        }
    }

    /// <summary>
    /// Load a save by ID
    /// </summary>
    public void LoadSave(string saveId)
    {
        if (SaveLoadManager.Instance == null) return;

        bool success = SaveLoadManager.Instance.LoadGame(saveId);

        if (success)
        {
            Debug.Log($"[SaveLoadMenuUI] Loaded save: {saveId}");

            // Close the menu
            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance.ToggleInventory();
            }

            RefreshSaveList();
        }
        else
        {
            Debug.LogError("[SaveLoadMenuUI] Failed to load save!");
        }
    }

    /// <summary>
    /// Delete a save (with confirmation)
    /// </summary>
    public void DeleteSave(SaveSlotInfo slotInfo)
    {
        pendingDeleteSlot = slotInfo;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);

            if (confirmationText != null)
            {
                confirmationText.text = $"Delete save '{slotInfo.saveName}'?\n\nThis cannot be undone!";
            }
        }
    }

    /// <summary>
    /// Confirm deletion
    /// </summary>
    private void ConfirmDelete()
    {
        if (SaveLoadManager.Instance != null && pendingDeleteSlot != null)
        {
            bool success = SaveLoadManager.Instance.DeleteSave(pendingDeleteSlot.saveId);

            if (success)
            {
                Debug.Log($"[SaveLoadMenuUI] Deleted save: {pendingDeleteSlot.saveName}");
                RefreshSaveList();
            }
        }

        CancelDelete();
    }

    /// <summary>
    /// Cancel deletion
    /// </summary>
    private void CancelDelete()
    {
        pendingDeleteSlot = null;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (newSaveButton != null)
            newSaveButton.onClick.RemoveListener(ShowNewSavePanel);

        if (refreshButton != null)
            refreshButton.onClick.RemoveListener(RefreshSaveList);

        if (confirmSaveButton != null)
            confirmSaveButton.onClick.RemoveListener(CreateNewSave);

        if (cancelSaveButton != null)
            cancelSaveButton.onClick.RemoveListener(HideNewSavePanel);

        if (confirmYesButton != null)
            confirmYesButton.onClick.RemoveListener(ConfirmDelete);

        if (confirmNoButton != null)
            confirmNoButton.onClick.RemoveListener(CancelDelete);
    }
}