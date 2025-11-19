using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Displays temporary notification messages to the player
/// </summary>
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Image notificationBackground;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color infoColor = new Color(0.2f, 0.5f, 0.8f, 0.9f);
    [SerializeField] private Color warningColor = new Color(0.9f, 0.7f, 0.2f, 0.9f);
    [SerializeField] private Color errorColor = new Color(0.9f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.3f, 0.9f);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip infoSound;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private bool playSounds = true;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 0.7f;

    private Coroutine currentNotification;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Get or add CanvasGroup for fading
        canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        // Get or add AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        // Start hidden
        notificationPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Show an info notification (blue)
    /// </summary>
    public void ShowInfo(string message)
    {
        ShowNotification(message, infoColor, infoSound);
    }

    /// <summary>
    /// Show a warning notification (yellow/orange)
    /// </summary>
    public void ShowWarning(string message)
    {
        Debug.Log($"[NotificationManager] ShowWarning called with message: '{message}'");
        ShowNotification(message, warningColor, warningSound);
    }

    /// <summary>
    /// Show an error notification (red)
    /// </summary>
    public void ShowError(string message)
    {
        ShowNotification(message, errorColor, errorSound);
    }

    /// <summary>
    /// Show a success notification (green)
    /// </summary>
    public void ShowSuccess(string message)
    {
        ShowNotification(message, successColor, successSound);
    }

    /// <summary>
    /// Show a notification with a custom color and sound
    /// </summary>
    public void ShowNotification(string message, Color backgroundColor, AudioClip sound = null)
    {
        Debug.Log($"[NotificationManager] ShowNotification called");
        Debug.Log($"[NotificationManager] - Message: '{message}'");
        Debug.Log($"[NotificationManager] - Panel active: {notificationPanel != null && notificationPanel.activeSelf}");
        Debug.Log($"[NotificationManager] - CanvasGroup: {canvasGroup != null}");

        // Cancel any existing notification
        if (currentNotification != null)
        {
            Debug.Log("[NotificationManager] Stopping previous notification");
            StopCoroutine(currentNotification);
        }

        // Play sound if enabled
        if (playSounds && audioSource != null && sound != null)
        {
            Debug.Log("[NotificationManager] Playing sound");
            audioSource.PlayOneShot(sound, soundVolume);
        }
        else if (playSounds && sound == null)
        {
            Debug.Log("[NotificationManager] No sound assigned for this notification type");
        }

        Debug.Log("[NotificationManager] Starting DisplayNotification coroutine");
        currentNotification = StartCoroutine(DisplayNotification(message, backgroundColor));
    }

    private IEnumerator DisplayNotification(string message, Color backgroundColor)
    {
        // Set text and color
        notificationText.text = message;
        notificationBackground.color = backgroundColor;

        // Show panel
        notificationPanel.SetActive(true);

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so it works when paused
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Wait for display duration
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Hide panel
        notificationPanel.SetActive(false);
        currentNotification = null;
    }

    /// <summary>
    /// Immediately hide any active notification
    /// </summary>
    public void HideNotification()
    {
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
            currentNotification = null;
        }

        notificationPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }
}