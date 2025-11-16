using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI display for a single save slot
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private Button deleteButton;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private SaveSlotInfo slotInfo;
    private SaveLoadMenuUI menuUI;

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

        // Setup delete button
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);

        // Make the whole slot clickable to select this save
        Button slotButton = GetComponent<Button>();
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    private void OnSlotClicked()
    {
        UnityEngine.Debug.Log($"[SaveSlotUI] Slot clicked: {slotInfo.saveName}");

        if (menuUI != null)
        {
            menuUI.SelectSave(slotInfo);
        }
    }

    private void OnDeleteClicked()
    {
        if (menuUI != null)
        {
            menuUI.DeleteSave(slotInfo);
        }
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? hoverColor : normalColor;
        }
    }

    private void OnDestroy()
    {
        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(OnDeleteClicked);

        Button slotButton = GetComponent<Button>();
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnSlotClicked);
        }
    }
}