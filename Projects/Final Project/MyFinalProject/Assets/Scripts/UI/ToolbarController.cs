using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Controls the farming toolbar UI and dispatches farming events
/// </summary>
public class ToolbarController : MonoBehaviour
{
    [Header("UI Buttons - Drag buttons here")]
    [Space(10)]
    [SerializeField] private Button hoeButton;
    [SerializeField] private Button plantButton;
    [SerializeField] private Button waterButton;
    [SerializeField] private Button harvestButton;

    [Header("Farming Events - These auto-populate")]
    [Space(10)]
    public UnityEvent OnHoe = new UnityEvent();
    public UnityEvent OnWater = new UnityEvent();
    public UnityEvent OnSeed = new UnityEvent();
    public UnityEvent OnGather = new UnityEvent();

    private void Start()
    {
        SetupButtons();
        UnityEngine.Debug.Log("[ToolbarController] Initialized with event-driven system");
    }

    private void SetupButtons()
    {
        if (hoeButton != null)
        {
            hoeButton.onClick.AddListener(() => {
                UnityEngine.Debug.Log("[ToolbarController] Hoe button clicked - dispatching event");
                OnHoe?.Invoke();
            });
        }
        else
        {
            UnityEngine.Debug.LogWarning("[ToolbarController] Hoe button not assigned!");
        }

        if (plantButton != null)
        {
            plantButton.onClick.AddListener(() => {
                UnityEngine.Debug.Log("[ToolbarController] Plant button clicked - dispatching event");
                OnSeed?.Invoke();
            });
        }
        else
        {
            UnityEngine.Debug.LogWarning("[ToolbarController] Plant button not assigned!");
        }

        if (waterButton != null)
        {
            waterButton.onClick.AddListener(() => {
                UnityEngine.Debug.Log("[ToolbarController] Water button clicked - dispatching event");
                OnWater?.Invoke();
            });
        }
        else
        {
            UnityEngine.Debug.LogWarning("[ToolbarController] Water button not assigned!");
        }

        if (harvestButton != null)
        {
            harvestButton.onClick.AddListener(() => {
                UnityEngine.Debug.Log("[ToolbarController] Harvest button clicked - dispatching event");
                OnGather?.Invoke();
            });
        }
        else
        {
            UnityEngine.Debug.LogWarning("[ToolbarController] Harvest button not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (hoeButton != null) hoeButton.onClick.RemoveAllListeners();
        if (plantButton != null) plantButton.onClick.RemoveAllListeners();
        if (waterButton != null) waterButton.onClick.RemoveAllListeners();
        if (harvestButton != null) harvestButton.onClick.RemoveAllListeners();
    }
}