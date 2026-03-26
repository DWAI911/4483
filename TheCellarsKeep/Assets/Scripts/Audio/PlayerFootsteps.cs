using UnityEngine;

/// <summary>
/// Handles player footstep audio.
/// Unity 2022.3.62f1 compatible.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerFootsteps : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceSounds
    {
        public string surfaceName;
        public AudioClip[] footstepClips;
        public float volume = 0.5f;
        public float pitchVariation = 0.1f;
    }

    [Header("Footstep Settings")]
    [SerializeField] private SurfaceSounds[] surfaceTypes;
    [SerializeField] private string defaultSurface = "Default";

    [Header("Timing")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    [Header("Audio Source")]
    [SerializeField] private AudioSource footstepSource;

    private PlayerController playerController;
    private float stepTimer;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 1f;
        }
    }

    private void Update()
    {
        if (playerController == null) return;

        bool isMoving = playerController.IsMoving;

        if (!isMoving)
        {
            stepTimer = 0f;
            return;
        }

        float stepInterval = playerController.IsRunning ? runStepInterval : walkStepInterval;

        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            PlayFootstep();
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        SurfaceSounds surface = GetSurfaceSounds(defaultSurface);

        if (surface == null || surface.footstepClips.Length == 0) return;

        AudioClip clip = surface.footstepClips[Random.Range(0, surface.footstepClips.Length)];

        footstepSource.volume = surface.volume;
        footstepSource.pitch = 1f + Random.Range(-surface.pitchVariation, surface.pitchVariation);

        footstepSource.PlayOneShot(clip);
    }

    private SurfaceSounds GetSurfaceSounds(string surfaceName)
    {
        foreach (SurfaceSounds surface in surfaceTypes)
        {
            if (surface.surfaceName == surfaceName)
            {
                return surface;
            }
        }

        if (surfaceTypes.Length > 0)
        {
            return surfaceTypes[0];
        }

        return null;
    }

    public void SetCurrentSurface(string surfaceName)
    {
        defaultSurface = surfaceName;
    }
}
