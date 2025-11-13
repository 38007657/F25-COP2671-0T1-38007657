using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private bool autoLoadLatestSave = true;
    [SerializeField] private float loadDelay = 0.5f;

    private void Start()
    {
        if (autoLoadLatestSave && SaveLoadManager.Instance != null)
        {
            Invoke(nameof(TryAutoLoad), loadDelay);
        }
    }

    private void TryAutoLoad()
    {
        if (SaveLoadManager.Instance.AnySavesExist())
        {
            SaveLoadManager.Instance.LoadLatestSave();
            Debug.Log("[GameInitializer] Auto-loaded latest save");
        }
        else
        {
            Debug.Log("[GameInitializer] No saves found, starting new game");
        }
    }
}