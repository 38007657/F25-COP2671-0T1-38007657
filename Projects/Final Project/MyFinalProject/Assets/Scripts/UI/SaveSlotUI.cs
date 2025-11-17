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
    [SerializeField] private Button loadButton;
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

        UnityEngine.Debug.Log($"[SaveSlotUI] Setting up save slot: {info.saveName} (ID: {info.saveId})");
        UnityEngine.Debug.Log($"[SaveSlotUI] menuUI is null: {menuUI == null}");
        UnityEngine.Debug.Log($"[SaveSlotUI] loadButton is null: {loadButton == null}");

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

        // Setup load button
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadClicked);
            UnityEngine.Debug.Log($"[SaveSlotUI] Load button listener added successfully");
        }
        else
        {
            UnityEngine.Debug.LogError($"[SaveSlotUI] Load button is NULL! Cannot add listener.");
        }

        // Setup delete button
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    private void OnLoadClicked()
    {
        UnityEngine.Debug.Log($"[SaveSlotUI] === LOAD BUTTON CLICKED ===");
        UnityEngine.Debug.Log($"[SaveSlotUI] slotInfo is null: {slotInfo == null}");
        UnityEngine.Debug.Log($"[SaveSlotUI] menuUI is null: {menuUI == null}");

        if (slotInfo != null)
        {
            UnityEngine.Debug.Log($"[SaveSlotUI] Attempting to load save: {slotInfo.saveName} (ID: {slotInfo.saveId})");
        }

        if (menuUI != null)
        {
            menuUI.LoadSave(slotInfo.saveId);
        }
        else
        {
            UnityEngine.Debug.LogError("[SaveSlotUI] menuUI is NULL! Cannot load save.");
        }
    }

    private void OnDeleteClicked()
    {
        UnityEngine.Debug.Log($"[SaveSlotUI] Delete button clicked for: {slotInfo?.saveName}");

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
        if (loadButton != null)
            loadButton.onClick.RemoveListener(OnLoadClicked);

        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(OnDeleteClicked);
    }
}