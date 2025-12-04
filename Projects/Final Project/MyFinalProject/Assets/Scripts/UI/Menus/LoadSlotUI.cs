using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI display for a single save slot in the load-only menu
/// Similar to SaveSlotUI but without delete functionality
/// </summary>
public class LoadSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private Button loadButton;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.8f, 1f, 0.8f);

    private SaveSlotInfo slotInfo;
    private LoadMenuUI menuUI;
    private bool isHovered = false;

    public void Setup(SaveSlotInfo info, LoadMenuUI menu)
    {
        slotInfo = info;
        menuUI = menu;

        UnityEngine.Debug.Log($"[LoadSlotUI] Setting up load slot: {info.saveName} (ID: {info.saveId})");

        // Set text
        if (saveNameText != null)
            saveNameText.text = info.saveName;

        if (saveDateText != null)
            saveDateText.text = info.saveDate;

        if (dayText != null)
            dayText.text = $"Day {info.day}";

        if (moneyText != null)
            moneyText.text = CurrencyFormatter.FormatCoins(info.money);

        if (playTimeText != null)
            playTimeText.text = info.playTime;

        // Setup load button
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadClicked);

            // Add button text if it has a text component
            TextMeshProUGUI buttonText = loadButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Load Game";
            }
        }
        else
        {
            UnityEngine.Debug.LogError($"[LoadSlotUI] Load button is NULL");
        }

        // Set initial visual state
        UpdateVisualState();
    }

    private void OnLoadClicked()
    {
        UnityEngine.Debug.Log($"[LoadSlotUI] Load button clicked for: {slotInfo?.saveName}");

        if (menuUI != null && slotInfo != null)
        {
            menuUI.LoadSave(slotInfo.saveId);
        }
        else
        {
            UnityEngine.Debug.LogError("[LoadSlotUI] menuUI or slotInfo is NULL");
        }
    }

    /// <summary>
    /// Mouse hover effects
    /// </summary>
    public void OnPointerEnter()
    {
        isHovered = true;
        UpdateVisualState();
    }

    public void OnPointerExit()
    {
        isHovered = false;
        UpdateVisualState();
    }

    /// <summary>
    /// Update visual state based on hover
    /// </summary>
    private void UpdateVisualState()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isHovered ? hoverColor : normalColor;
        }
    }

    /// <summary>
    /// Make entire slot clickable
    /// </summary>
    public void OnSlotClicked()
    {
        OnLoadClicked();
    }

    private void OnDestroy()
    {
        if (loadButton != null)
            loadButton.onClick.RemoveListener(OnLoadClicked);
    }
}