using UnityEngine;

/// <summary>
/// Exit door that requires fuses to power and allows the player to escape.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class ExitDoor : MonoBehaviour, IInteractable
{
    [Header("Exit Settings")]
    [SerializeField] private int fusesRequired = 3;
    [SerializeField] private bool isPowered = false;

    [Header("Visual Feedback")]
    [SerializeField] private Light[] powerLights;
    [SerializeField] private ParticleSystem powerParticles;
    [SerializeField] private Renderer doorRenderer;

    [Header("Audio")]
    [SerializeField] private AudioClip fuseInsertSound;
    [SerializeField] private AudioClip powerOnSound;
    [SerializeField] private AudioClip escapeSound;

    [Header("Animation")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";

    private int currentFuses = 0;
    private AudioSource audioSource;
    private bool isOpen = false;

    public string InteractPrompt => GetPrompt();
    public bool IsPowered => isPowered;
    public int FusesRequired => fusesRequired;
    public int CurrentFuses => currentFuses;

    public event System.Action<int, int> OnFuseInserted;
    public event System.Action OnExitActivated;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        UpdateVisuals();
    }

    public void Interact(PlayerInteract player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (inventory == null) return;

        if (isPowered)
        {
            Escape(player);
        }
        else if (inventory.Fuses > 0)
        {
            InsertFuse(inventory);
        }
    }

    private void InsertFuse(PlayerInventory inventory)
    {
        if (currentFuses >= fusesRequired) return;

        inventory.UseFuse();
        currentFuses++;

        PlaySound(fuseInsertSound);
        OnFuseInserted?.Invoke(currentFuses, fusesRequired);

        Debug.Log($"Fuse inserted: {currentFuses}/{fusesRequired}");

        if (currentFuses >= fusesRequired)
        {
            ActivatePower();
        }

        UpdateVisuals();
    }

    private void ActivatePower()
    {
        isPowered = true;

        PlaySound(powerOnSound);

        if (powerParticles != null)
        {
            powerParticles.Play();
        }

        foreach (Light light in powerLights)
        {
            if (light != null)
            {
                light.intensity *= 2f;
                light.color = Color.green;
            }
        }

        OnExitActivated?.Invoke();
        Debug.Log("Exit powered! You can escape!");
    }

    private void Escape(PlayerInteract player)
    {
        if (!isPowered) return;

        PlaySound(escapeSound);

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }

        isOpen = true;

        GameStateManager gameState = GameStateManager.Instance;
        if (gameState != null)
        {
            gameState.HandlePlayerEscape();
        }

        Debug.Log("ESCAPE!");
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < powerLights.Length; i++)
        {
            if (powerLights[i] != null)
            {
                powerLights[i].enabled = (i < currentFuses) || isPowered;
                powerLights[i].color = isPowered ? Color.green : Color.yellow;
            }
        }
    }

    private string GetPrompt()
    {
        if (isPowered)
        {
            return "Press E to ESCAPE";
        }
        else if (currentFuses < fusesRequired)
        {
            return $"Insert Fuse ({currentFuses}/{fusesRequired})";
        }
        else
        {
            return "Exit door is now powered!";
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ResetExit()
    {
        currentFuses = 0;
        isPowered = false;
        isOpen = false;
        UpdateVisuals();
    }
}
