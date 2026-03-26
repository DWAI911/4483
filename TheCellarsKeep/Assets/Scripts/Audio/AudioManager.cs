using UnityEngine;
using System.Collections;

/// <summary>
/// Central audio manager handling music and sound effects.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip explorationMusic;
    [SerializeField] private AudioClip chaseMusic;
    [SerializeField] private AudioClip shopMusic;
    [SerializeField] private AudioClip victoryMusic;

    [Header("Settings")]
    [SerializeField] private float musicFadeDuration = 2f;
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;

    private AIChaser currentEnemy;
    private bool isChasing = false;

    public float MasterVolume => masterVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        LoadVolumeSettings();
    }

    private void Start()
    {
        FindReferences();
    }

    private void FindReferences()
    {
        currentEnemy = FindObjectOfType<AIChaser>();

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
    }

    #region Music Control
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

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        StartCoroutine(CrossfadeMusic(clip, musicFadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        float startVolume = musicSource.volume;
        float targetVolume = musicVolume * masterVolume;

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
    #endregion

    #region Sound Effects
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.volume = sfxVolume * masterVolume;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume);
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
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        ApplyVolumes();
    }
    #endregion
}
