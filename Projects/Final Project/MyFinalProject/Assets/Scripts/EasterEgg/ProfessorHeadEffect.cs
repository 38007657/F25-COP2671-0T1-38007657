using UnityEngine;
using System.Collections;

/// <summary>
/// Visual effect that displays professor's head over the player with particles
/// Automatically destroys itself after 3 minutes of game time
/// </summary>
public class ProfessorHeadEffect : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer professorHeadSprite;
    [SerializeField] private ParticleSystem particleEffect;

    [Header("Duration")]
    [SerializeField] private float gameTimeDuration = 3f; // 3 minutes of game time

    [Header("Animation")]
    [SerializeField] private bool rotateSlowly = true;
    [SerializeField] private float rotationSpeed = 30f; // Degrees per second
    [SerializeField] private bool bobUpAndDown = true;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    private float startTime;
    private Vector3 startPosition;
    private TimeManager timeManager;
    private bool isInitialized = false;

    private void Awake()
    {
        startPosition = transform.localPosition;

        // Get TimeManager to track game time
        timeManager = TimeManager.Instance;

        if (timeManager == null)
        {
            Debug.LogError("[ProfessorHead] TimeManager not found! Using real-time instead.");
        }
    }

    /// <summary>
    /// Initialize the effect (called by EasterEggTrigger)
    /// </summary>
    public void Initialize(int coinReward)
    {
        isInitialized = true;
        startTime = GetCurrentGameTime();

        // Start particle effect
        if (particleEffect != null)
        {
            particleEffect.Play();
        }

        // Make sure sprite is visible
        if (professorHeadSprite != null)
        {
            professorHeadSprite.enabled = true;
        }

        Debug.Log($"[ProfessorHead] Effect initialized! Will last {gameTimeDuration} game minutes. Reward: {coinReward} coins");

        // Show a special message
        StartCoroutine(ShowActivationMessage());
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Check if duration has expired (in game time)
        float currentGameTime = GetCurrentGameTime();
        float elapsedGameMinutes = currentGameTime - startTime;

        if (elapsedGameMinutes >= gameTimeDuration)
        {
            Debug.Log("[ProfessorHead] Effect duration expired - destroying");
            Destroy(gameObject);
            return;
        }

        // Rotate slowly
        if (rotateSlowly)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        // Bob up and down
        if (bobUpAndDown)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    /// <summary>
    /// Get current game time in minutes
    /// </summary>
    private float GetCurrentGameTime()
    {
        if (timeManager != null)
        {
            // Convert hours to minutes
            return timeManager.CurrentTime * 60f;
        }
        else
        {
            // Fallback to real time if TimeManager not available
            return Time.time / 60f;
        }
    }

    /// <summary>
    /// Show activation message coroutine
    /// </summary>
    private IEnumerator ShowActivationMessage()
    {
        Debug.Log("════════════════════════════════════════");
        Debug.Log("✨ PROFESSOR'S BLESSING ACTIVATED! ✨");
        Debug.Log("════════════════════════════════════════");
        yield return null;
    }

    private void OnDestroy()
    {
        Debug.Log("[ProfessorHead] Professor's blessing has ended");
    }
}