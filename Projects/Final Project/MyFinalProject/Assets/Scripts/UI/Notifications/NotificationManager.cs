using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Notification types for different message styles
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Manages in-game notification popups
/// </summary>
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Notification Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color infoColor = new Color(0.2f, 0.6f, 1f, 0.9f);      // Blue
    [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);  // Green
    [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f, 0.9f);    // Yellow
    [SerializeField] private Color errorColor = new Color(1f, 0.2f, 0.2f, 0.9f);      // Red

    private Coroutine currentNotification;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ensure notification starts hidden
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }

        // Create canvas group if not present
        if (canvasGroup == null && notificationPanel != null)
        {
            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// Show a notification message
    /// </summary>
    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("[NotificationManager] UI references not set");
            return;
        }

        // Cancel any existing notification
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }

        // Start new notification
        currentNotification = StartCoroutine(DisplayNotification(message, type));
    }

    /// <summary>
    /// Coroutine to display notification with fade effects
    /// </summary>
    private IEnumerator DisplayNotification(string message, NotificationType type)
    {
        // Set message text
        notificationText.text = message;

        // Set background color based on type
        if (backgroundImage != null)
        {
            switch (type)
            {
                case NotificationType.Info:
                    backgroundImage.color = infoColor;
                    break;
                case NotificationType.Success:
                    backgroundImage.color = successColor;
                    break;
                case NotificationType.Warning:
                    backgroundImage.color = warningColor;
                    break;
                case NotificationType.Error:
                    backgroundImage.color = errorColor;
                    break;
            }
        }

        // Show panel
        notificationPanel.SetActive(true);

        // Fade in
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Unscaled time so it works when game is paused
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // Wait for display duration
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }

            yield return null;
        }

        // Hide panel
        notificationPanel.SetActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        currentNotification = null;
    }

    /// <summary>
    /// Quick helper methods for common notification types
    /// </summary>
    public void ShowInfo(string message)
    {
        ShowNotification(message, NotificationType.Info);
    }

    public void ShowSuccess(string message)
    {
        ShowNotification(message, NotificationType.Success);
    }

    public void ShowWarning(string message)
    {
        ShowNotification(message, NotificationType.Warning);
    }

    public void ShowError(string message)
    {
        ShowNotification(message, NotificationType.Error);
    }
}