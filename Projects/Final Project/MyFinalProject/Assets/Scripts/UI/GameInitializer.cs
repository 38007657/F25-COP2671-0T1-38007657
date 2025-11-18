using UnityEngine;

/// <summary>
/// Initializes game state on startup
/// If a start menu exists, lets it handle loading
/// Otherwise, can optionally auto-load the latest save
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private bool autoLoadLatestSave = false;
    [SerializeField] private float loadDelay = 0.5f;

    private void Start()
    {
        // Don't auto-load if there's a start menu - let the player choose
        if (StartMenuManager.Instance != null)
        {
            Debug.Log("[GameInitializer] Start menu found - skipping auto-load");
            return;
        }

        if (autoLoadLatestSave && SaveLoadManager.Instance != null)
        {
            Invoke(nameof(TryAutoLoad), loadDelay);
        }
    }

    private void TryAutoLoad()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("[GameInitializer] SaveLoadManager not found!");
            return;
        }

        if (SaveLoadManager.Instance.AnySavesExist())
        {
            SaveLoadManager.Instance.LoadLatestSave();
            Debug.Log("[GameInitializer] Auto-loaded latest save");

            // Hide start menu if it exists
            if (StartMenuManager.Instance != null)
            {
                StartMenuManager.Instance.HideStartMenu();
            }
        }
        else
        {
            Debug.Log("[GameInitializer] No saves found, starting new game");
        }
    }
}