using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] private float dayDuration = 120f; // Duration of a full day in seconds (2 minutes default)
    [SerializeField][Range(0f, 24f)] private float currentTime = 12f; // Current time in hours (0-24)
    [SerializeField] private bool pauseTime = false;

    [Header("Time Configuration")]
    [SerializeField] private float sunriseTime = 6f; // 6 AM
    [SerializeField] private float sunsetTime = 18f; // 6 PM

    private float timeSpeed;

    // Properties
    public float CurrentTime => currentTime;
    public float CurrentTimeNormalized => currentTime / 24f; // 0-1 value for curves/gradients
    public bool IsDaytime => currentTime >= sunriseTime && currentTime < sunsetTime;
    public bool IsNighttime => !IsDaytime;
    public float SunriseTime => sunriseTime;
    public float SunsetTime => sunsetTime;
    public bool IsPaused => pauseTime;

    // Events
    public delegate void TimeEvent(float time);
    public event TimeEvent OnTimeChanged;
    public event TimeEvent OnHourChanged;

    private float lastHour;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Calculate time speed
        timeSpeed = 24f / dayDuration; // Hours per second
        lastHour = Mathf.Floor(currentTime);
    }

    private void Update()
    {
        if (!pauseTime)
        {
            // Advance time
            float previousTime = currentTime;
            currentTime += timeSpeed * Time.deltaTime;

            // Wrap around 24 hours
            if (currentTime >= 24f)
            {
                currentTime -= 24f;
            }

            // Fire time changed event
            OnTimeChanged?.Invoke(currentTime);

            // Check if hour changed
            float currentHour = Mathf.Floor(currentTime);
            if (currentHour != lastHour)
            {
                lastHour = currentHour;
                OnHourChanged?.Invoke(currentTime);
            }
        }

        // Debug controls
        HandleDebugInput();
    }

    private void HandleDebugInput()
    {
        // Speed up time (hold right arrow)
        if (Input.GetKey(KeyCode.RightArrow))
        {
            currentTime += 10f * Time.deltaTime; // 10x speed
            if (currentTime >= 24f) currentTime -= 24f;
            OnTimeChanged?.Invoke(currentTime);
        }

        // Slow down time (hold left arrow)
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            currentTime -= 10f * Time.deltaTime;
            if (currentTime < 0f) currentTime += 24f;
            OnTimeChanged?.Invoke(currentTime);
        }

        // Pause/unpause (P key)
        if (Input.GetKeyDown(KeyCode.P))
        {
            pauseTime = !pauseTime;
            Debug.Log($"[TimeManager] Time {(pauseTime ? "paused" : "resumed")} at {GetTimeString()}");
        }

        // Jump to specific times
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetTime(6f);   // Sunrise
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetTime(12f);  // Noon
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetTime(18f);  // Sunset
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetTime(0f);   // Midnight

        // Show current time
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"[TimeManager] Current time: {GetTimeString()} - {(IsDaytime ? "Day" : "Night")}");
        }
    }

    // Public methods
    public void SetTime(float hour)
    {
        currentTime = Mathf.Clamp(hour, 0f, 24f);
        lastHour = Mathf.Floor(currentTime);
        OnTimeChanged?.Invoke(currentTime);
        Debug.Log($"[TimeManager] Time set to {GetTimeString()}");
    }

    public void SetDayDuration(float seconds)
    {
        dayDuration = Mathf.Max(1f, seconds);
        timeSpeed = 24f / dayDuration;
    }

    public void PauseTime(bool pause)
    {
        pauseTime = pause;
    }

    public string GetTimeString()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        string period = hours >= 12 ? "PM" : "AM";
        int displayHours = hours == 0 ? 12 : (hours > 12 ? hours - 12 : hours);
        return string.Format("{0}:{1:00} {2}", displayHours, minutes, period);
    }

    public string GetTime24String()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        return string.Format("{0:00}:{1:00}", hours, minutes);
    }

    // For save/load functionality
    public float GetCurrentTime() => currentTime;
    public void LoadTime(float time) => SetTime(time);
}