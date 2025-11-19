using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] private float dayDuration = 600f; // 10 minutes in seconds
    [SerializeField][Range(0f, 24f)] private float currentTime = 6f; // Start at sunrise
    [SerializeField] private bool pauseTime = false;

    [Header("Time Configuration")]
    [SerializeField] private float sunriseTime = 6f; // 6 AM
    [SerializeField] private float sunsetTime = 18f; // 6 PM

    [Header("Speed Control")]
    [SerializeField] private float timeSpeedMultiplier = 1f; // 1x, 2x, 3x, 5x
    [SerializeField] private float[] speedOptions = { 1f, 2f, 3f, 5f };
    private int currentSpeedIndex = 0;

    private float timeSpeed;

    // Properties
    public float CurrentTime => currentTime;
    public float CurrentTimeNormalized => currentTime / 24f;
    public bool IsDaytime => currentTime >= sunriseTime && currentTime < sunsetTime;
    public bool IsNighttime => !IsDaytime;
    public float SunriseTime => sunriseTime;
    public float SunsetTime => sunsetTime;
    public bool IsPaused => pauseTime;
    public float TimeSpeedMultiplier => timeSpeedMultiplier;
    public float DayDuration => dayDuration;

    // Events
    public delegate void TimeEvent(float time);
    public event TimeEvent OnTimeChanged;
    public event TimeEvent OnHourChanged;
    public event System.Action<float> OnSpeedChanged;

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

        // Calculate base time speed
        timeSpeed = 24f / dayDuration; // Hours per second at 1x speed
        lastHour = Mathf.Floor(currentTime);
    }

    private void Update()
    {
        HandleSpeedInput();

        if (!pauseTime)
        {
            // Advance time with speed multiplier
            float previousTime = currentTime;
            currentTime += timeSpeed * timeSpeedMultiplier * Time.deltaTime;

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
    }

    /// <summary>
    /// Handle keyboard input for speed control
    /// </summary>
    private void HandleSpeedInput()
    {
        // Press 1, 2, 3, or 4 keys to set speed
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetSpeed(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetSpeed(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetSpeed(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetSpeed(3);
        }

        // Or cycle through with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleSpeed();
        }
    }

    /// <summary>
    /// Set time speed by index (0=1x, 1=2x, 2=3x, 3=5x)
    /// </summary>
    public void SetSpeed(int index)
    {
        if (index >= 0 && index < speedOptions.Length)
        {
            currentSpeedIndex = index;
            timeSpeedMultiplier = speedOptions[currentSpeedIndex];
            OnSpeedChanged?.Invoke(timeSpeedMultiplier);
            //Debug.Log($"[TimeManager] Speed set to {timeSpeedMultiplier}x");
        }
    }

    /// <summary>
    /// Cycle to next speed option
    /// </summary>
    public void CycleSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % speedOptions.Length;
        timeSpeedMultiplier = speedOptions[currentSpeedIndex];
        OnSpeedChanged?.Invoke(timeSpeedMultiplier);
        //Debug.Log($"[TimeManager] Speed set to {timeSpeedMultiplier}x");
    }

    /// <summary>
    /// Set specific speed multiplier
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        timeSpeedMultiplier = multiplier;
        OnSpeedChanged?.Invoke(timeSpeedMultiplier);
        //Debug.Log($"[TimeManager] Speed set to {timeSpeedMultiplier}x");
    }

    // Public methods
    public void SetTime(float hour)
    {
        currentTime = Mathf.Clamp(hour, 0f, 24f);
        lastHour = Mathf.Floor(currentTime);
        OnTimeChanged?.Invoke(currentTime);
        //Debug.Log($"[TimeManager] Time set to {GetTimeString()}");
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

    // Methods for UI
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

    public string GetSpeedString()
    {
        return $"{timeSpeedMultiplier}x";
    }


    // For save/load functionality
    public float GetCurrentTime() => currentTime;
    public void LoadTime(float time) => SetTime(time);
}
