using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI display for a single save slot
/// UPDATED: Added selection support for load functionality
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private Button selectButton; // Click slot to select it
    [SerializeField] private Button deleteButton;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectedIndicator; // Border/highlight when selected
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.8f, 1f, 0.8f);

    private SaveSlotInfo slotInfo;
    private SaveLoadMenuUI menuUI;
    private bool isSelected = false;

    // Events
    public System.Action<SaveSlotUI> OnSlotSelected;

    public void Setup(SaveSlotInfo info, SaveLoadMenuUI menu)
    {
        slotInfo = info;
        menuUI = menu;

        // Set text
        if (saveNameText != null)
            saveNameText.text = info.saveName;

        if (saveDateText != null)
            saveDateText.text = info.saveDate;

        if (dayText != null)
            dayText.text = $"Day {info.day}";

        if (moneyText != null)
            moneyText.text = $"${info.money}";

        if (playTimeText != null)
            playTimeText.text = $"Playtime: {info.playTime}";

        // Setup buttons
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);

        // Start unselected
        SetSelected(false);
    }

    /// <summary>
    /// Called when the slot is clicked (to select it for loading)
    /// </summary>
    private void OnSelectClicked()
    {
        OnSlotSelected?.Invoke(this);
    }

    /// <summary>
    /// Called when delete button is clicked
    /// </summary>
    private void OnDeleteClicked()
    {
        if (menuUI != null)
        {
            menuUI.DeleteSave(slotInfo);
        }
    }

    /// <summary>
    /// Set the selected visual state
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // Show/hide selection indicator
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }

        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }

    // Properties
    public SaveSlotInfo SaveSlotInfo => slotInfo;
    public bool IsSelected => isSelected;

    private void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(OnSelectClicked);

        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(OnDeleteClicked);
    }
}