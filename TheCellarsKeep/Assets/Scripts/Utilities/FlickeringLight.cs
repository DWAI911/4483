using UnityEngine;

/// <summary>
/// Creates flickering light effects for horror atmosphere.
- Random intensity variation
- Occasional "scare" flickers
- Configurable parameters
/// </summary>
[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    public enum FlickerPattern
    {
        Random,
        SinWave,
        HorrorBurst,
        FaultyBulb
    }

    [Header("Flicker Settings")]
    [SerializeField] private FlickerPattern pattern = FlickerPattern.Random;
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float flickerSpeed = 3f;

    [Header("Horror Burst Settings")]
    [SerializeField] private float burstChance = 0.01f; // Chance per frame
    [SerializeField] private float burstDuration = 0.2f;
    [SerializeField] private int burstFlickerCount = 5;

    [Header("Faulty Bulb Settings")]
    [SerializeField] private float stableTimeMin = 3f;
    [SerializeField] private float stableTimeMax = 10f;
    [SerializeField] private float faultyDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip flickerSound;
    [SerializeField] private float soundPlayChance = 0.1f;

    private Light targetLight;
    private float baseIntensity;
    private float burstTimer = 0f;
    private int burstFlickersRemaining = 0;
    private float stableTimer = 0f;
    private bool isFaulty = false;
    private AudioSource audioSource;

    private void Awake()
    {
        targetLight = GetComponent<Light>();
        baseIntensity = targetLight.intensity;
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        // Initialize stable timer
        stableTimer = Random.Range(stableTimeMin, stableTimeMax);
    }

    private void Update()
    {
        if (!enableFlicker) return;

        switch (pattern)
        {
            case FlickerPattern.Random:
                UpdateRandomFlicker();
                break;
            case FlickerPattern.SinWave:
                UpdateSinWaveFlicker();
                break;
            case FlickerPattern.HorrorBurst:
                UpdateHorrorBurst();
                break;
            case FlickerPattern.FaultyBulb:
                UpdateFaultyBulb();
                break;
        }
    }

    private void UpdateRandomFlicker()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }

    private void UpdateSinWaveFlicker()
    {
        float wave = Mathf.Sin(Time.time * flickerSpeed);
        float normalized = (wave + 1f) / 2f; // Convert from -1~1 to 0~1
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, normalized);
    }

    private void UpdateHorrorBurst()
    {
        // Normal stable light
        targetLight.intensity = baseIntensity;

        // Check for random burst
        if (burstFlickersRemaining <= 0 && Random.value < burstChance)
        {
            burstFlickersRemaining = burstFlickerCount;
            burstTimer = burstDuration / burstFlickerCount;
            PlayFlickerSound();
        }

        // Handle burst flickering
        if (burstFlickersRemaining > 0)
        {
            burstTimer -= Time.deltaTime;
            
            if (burstTimer <= 0f)
            {
                // Toggle light
                targetLight.enabled = !targetLight.enabled;
                targetLight.intensity = Random.Range(minIntensity, maxIntensity);
                
                burstFlickersRemaining--;
                burstTimer = burstDuration / burstFlickerCount;
            }
        }
    }

    private void UpdateFaultyBulb()
    {
        stableTimer -= Time.deltaTime;

        if (stableTimer <= 0f && !isFaulty)
        {
            // Start faulty period
            isFaulty = true;
            stableTimer = faultyDuration;
            PlayFlickerSound();
        }

        if (isFaulty)
        {
            // Rapid flickering
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed * 10f, 0f);
            targetLight.intensity = Mathf.Lerp(0f, maxIntensity, noise);
            targetLight.enabled = noise > 0.3f;

            stableTimer -= Time.deltaTime;
            
            if (stableTimer <= 0f)
            {
                // Return to stable
                isFaulty = false;
                targetLight.enabled = true;
                targetLight.intensity = baseIntensity;
                stableTimer = Random.Range(stableTimeMin, stableTimeMax);
            }
        }
    }

    private void PlayFlickerSound()
    {
        if (flickerSound != null && Random.value < soundPlayChance)
        {
            audioSource.PlayOneShot(flickerSound, 0.5f);
        }
    }

    public void SetFlickerEnabled(bool enabled)
    {
        enableFlicker = enabled;
        
        if (!enabled)
        {
            targetLight.intensity = baseIntensity;
            targetLight.enabled = true;
        }
    }

    public void TriggerBurst()
    {
        burstFlickersRemaining = burstFlickerCount;
        burstTimer = 0f;
    }
}
