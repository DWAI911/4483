using UnityEngine;

/// <summary>
/// Exit door that requires fuses to power and allows the player to escape.
/// This is the win condition for each run.
/// </summary>
public class ExitDoor : MonoBehaviour, IInteractable
{
    [Header("Exit Settings")]
    [SerializeField] private int fusesRequired = 3;
    [SerializeField] private Transform exitPosition;
    [SerializeField] private bool isPowered = false;

    [Header("Visual Feedback")]
    [SerializeField] private Light[] powerLights;
    [SerializeField] private ParticleSystem powerParticles;
    [SerializeField] private Material poweredMaterial;
    [SerializeField] private Material unpoweredMaterial;
    [SerializeField] private Renderer doorRenderer;

    [Header("Audio")]
    [SerializeField] private AudioClip fuseInsertSound;
    [SerializeField] private AudioClip powerOnSound;
    [SerializeField] private AudioClip doorOpenSound;
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

    public event System.Action<int, int> OnFuseInserted; // (current, required)
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
        GameStateManager gameState = GameStateManager.Instance;

        if (inventory == null) return;

        if (isPowered)
        {
            // Escape!
            Escape(player);
        }
        else if (inventory.Fuses > 0)
        {
            // Insert fuse
            InsertFuse(inventory, gameState);
        }
    }

    private void InsertFuse(PlayerInventory inventory, GameStateManager gameState)
    {
        if (currentFuses >= fusesRequired) return;

        inventory.UseFuse();
        currentFuses++;

        // Play insert sound
        PlaySound(fuseInsertSound);

        // Fire event
        OnFuseInserted?.Invoke(currentFuses, fusesRequired);

        Debug.Log($"Fuse inserted: {currentFuses}/{fusesRequired}");

        // Check if powered
        if (currentFuses >= fusesRequired)
        {
            ActivatePower();
        }

        UpdateVisuals();
    }

    private void ActivatePower()
    {
        isPowered = true;

        // Play power on sound and effects
        PlaySound(powerOnSound);

        if (powerParticles != null)
        {
            powerParticles.Play();
        }

        // Update lights to full power
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

        // Play escape sound
        PlaySound(escapeSound);

        // Trigger animation
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }

        isOpen = true;

        // Notify game state manager
        GameStateManager gameState = GameStateManager.Instance;
        if (gameState != null)
        {
            gameState.HandlePlayerEscape();
        }

        Debug.Log("ESCAPE!");
    }

    private void UpdateVisuals()
    {
        // Update lights based on fuse count
        for (int i = 0; i < powerLights.Length; i++)
        {
            if (powerLights[i] != null)
            {
                powerLights[i].enabled = (i < currentFuses) || isPowered;
                powerLights[i].color = isPowered ? Color.green : Color.yellow;
            }
        }

        // Update material
        if (doorRenderer != null && poweredMaterial != null && unpoweredMaterial != null)
        {
            doorRenderer.material = isPowered ? poweredMaterial : unpoweredMaterial;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isPowered && isOpen)
        {
            // Auto-trigger escape when player walks through open door
            PlayerInteract player = other.GetComponent<PlayerInteract>();
            if (player != null)
            {
                Escape(player);
            }
        }
    }

    public void ResetExit()
    {
        currentFuses = 0;
        isPowered = false;
        isOpen = false;
        UpdateVisuals();
    }

    private void OnDrawGizmosSelected()
    {
        if (exitPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(exitPosition.position, 0.5f);
        }
    }
}
