using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls UI buttons for farming actions
/// Connect your Canvas buttons to this script
/// </summary>
public class FarmingUIController : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button hoeButton;
    [SerializeField] private Button plantButton;
    [SerializeField] private Button waterButton;
    [SerializeField] private Button harvestButton;

    [Header("Player Reference")]
    [SerializeField] private PlayerFarmingInteraction playerFarming;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = new Color(0.8f, 1f, 0.8f);
    [SerializeField] private float flashDuration = 0.2f;

    private void Start()
    {
        // Auto-find player if not assigned
        if (playerFarming == null)
        {
            playerFarming = FindObjectOfType<PlayerFarmingInteraction>();
        }

        if (playerFarming == null)
        {
            Debug.LogError("[FarmingUIController] PlayerFarmingInteraction not found!");
            enabled = false;
            return;
        }

        // Setup button listeners
        if (hoeButton != null)
            hoeButton.onClick.AddListener(OnHoeButtonClicked);

        if (plantButton != null)
            plantButton.onClick.AddListener(OnPlantButtonClicked);

        if (waterButton != null)
            waterButton.onClick.AddListener(OnWaterButtonClicked);

        if (harvestButton != null)
            harvestButton.onClick.AddListener(OnHarvestButtonClicked);

        Debug.Log("[FarmingUIController] Farming UI buttons initialized");
    }

    // Button click handlers
    private void OnHoeButtonClicked()
    {
        if (playerFarming != null)
        {
            playerFarming.TryHoe();
            FlashButton(hoeButton);
        }
    }

    private void OnPlantButtonClicked()
    {
        if (playerFarming != null)
        {
            playerFarming.TryPlant();
            FlashButton(plantButton);
        }
    }

    private void OnWaterButtonClicked()
    {
        if (playerFarming != null)
        {
            playerFarming.TryWater();
            FlashButton(waterButton);
        }
    }

    private void OnHarvestButtonClicked()
    {
        if (playerFarming != null)
        {
            playerFarming.TryHarvest();
            FlashButton(harvestButton);
        }
    }

    // Optional: Visual feedback when button is pressed
    private void FlashButton(Button button)
    {
        if (button == null) return;
        StartCoroutine(FlashButtonCoroutine(button));
    }

    private System.Collections.IEnumerator FlashButtonCoroutine(Button button)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage == null) yield break;

        Color originalColor = buttonImage.color;
        buttonImage.color = pressedColor;

        yield return new WaitForSeconds(flashDuration);

        buttonImage.color = originalColor;
    }
}