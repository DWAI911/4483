using UnityEngine;
using System.Collections;

/// <summary>
/// Central audio manager handling:
- Background music transitions
- Ambient sounds
- Dynamic audio based on game state
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip explorationMusic;
    [SerializeField] private AudioClip chaseMusic;
    [SerializeField] private AudioClip shopMusic;
    [SerializeField] private AudioClip victoryMusic;

    [Header("Ambient Sounds")]
    [SerializeField] private AudioClip[] ambientClips;
    [SerializeField] private float ambientMinInterval = 10f;
    [SerializeField] private float ambientMaxInterval = 30f;

    [Header("Settings")]
    [SerializeField] private float musicFadeDuration = 2f;
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float ambientVolume = 0.5f;

    private Coroutine ambientCoroutine;
    private AIChaser currentEnemy;
    private PlayerController currentPlayer;
    private bool isChasing = false;

    // Events
    public event System.Action<float> OnMasterVolumeChanged;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = false;
            ambientSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.loop = false;
            footstepSource.playOnAwake = false;
        }

        // Load saved volumes
        LoadVolumeSettings();
    }

    private void Start()
    {
        FindReferences();
        StartAmbientLoop();
    }

    private void FindReferences()
    {
        currentEnemy = FindObjectOfType<AIChaser>();
        currentPlayer = FindObjectOfType<PlayerController>();

        if (currentEnemy != null)
        {
            currentEnemy.OnStateChanged += OnAIStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (currentEnemy != null)
        {
            currentEnemy.OnStateChanged -= OnAIStateChanged;
        }

        if (ambientCoroutine != null)
        {
            StopCoroutine(ambientCoroutine);
        }
    }

    #region Music Control
    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayExplorationMusic()
    {
        if (!isChasing)
        {
            PlayMusic(explorationMusic);
        }
    }

    public void PlayChaseMusic()
    {
        isChasing = true;
        PlayMusic(chaseMusic);
    }

    public void PlayShopMusic()
    {
        PlayMusic(shopMusic);
    }

    public void PlayVictoryMusic()
    {
        isChasing = false;
        PlayMusic(victoryMusic);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeMusic(0f, musicFadeDuration, true));
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        StartCoroutine(CrossfadeMusic(clip, musicFadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        float startVolume = musicSource.volume;
        float targetVolume = musicVolume * masterVolume;

        // Fade out current track
        if (musicSource.isPlaying)
        {
            float timer = 0f;
            while (timer < duration / 2f)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / (duration / 2f));
                yield return null;
            }
        }

        // Switch clip and fade in
        musicSource.clip = newClip;
        musicSource.Play();

        float fadeInTimer = 0f;
        while (fadeInTimer < duration / 2f)
        {
            fadeInTimer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, fadeInTimer / (duration / 2f));
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    private IEnumerator FadeMusic(float targetVolume, float duration, bool stopAfter = false)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;

        if (stopAfter && musicSource != null)
        {
            musicSource.Stop();
        }
    }
    #endregion

    #region Ambient Sounds
    private void StartAmbientLoop()
    {
        if (ambientCoroutine != null)
        {
            StopCoroutine(ambientCoroutine);
        }

        ambientCoroutine = StartCoroutine(AmbientLoopCoroutine());
    }

    private IEnumerator AmbientLoopCoroutine()
    {
        while (true)
        {
            float waitTime = Random.Range(ambientMinInterval, ambientMaxInterval);
            yield return new WaitForSeconds(waitTime);

            PlayRandomAmbient();
        }
    }

    private void PlayRandomAmbient()
    {
        if (ambientClips == null || ambientClips.Length == 0) return;
        if (ambientSource == null) return;

        AudioClip clip = ambientClips[Random.Range(0, ambientClips.Length)];
        ambientSource.volume = ambientVolume * masterVolume;
        ambientSource.PlayOneShot(clip);
    }

    public void PlayAmbientAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, ambientVolume * masterVolume);
    }
    #endregion

    #region Sound Effects
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.volume = sfxVolume * masterVolume;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.volume = volumeScale * sfxVolume * masterVolume;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume);
    }
    #endregion

    #region Footsteps
    public void PlayFootstep(AudioClip clip)
    {
        if (clip == null || footstepSource == null) return;

        footstepSource.volume = sfxVolume * masterVolume * 0.5f;
        footstepSource.PlayOneShot(clip);
    }
    #endregion

    #region AI State Response
    private void OnAIStateChanged(AIChaser.AIState state)
    {
        switch (state)
        {
            case AIChaser.AIState.Chasing:
                PlayChaseMusic();
                break;
            case AIChaser.AIState.Searching:
            case AIChaser.AIState.Investigating:
            case AIChaser.AIState.Patrol:
                if (isChasing)
                {
                    isChasing = false;
                    PlayExplorationMusic();
                }
                break;
        }
    }
    #endregion

    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
        OnMasterVolumeChanged?.Invoke(masterVolume);
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
        SaveVolumeSettings();
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
        SaveVolumeSettings();
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }

        if (ambientSource != null)
        {
            ambientSource.volume = ambientVolume * masterVolume;
        }
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);
        
        ApplyVolumes();
    }
    #endregion
}
