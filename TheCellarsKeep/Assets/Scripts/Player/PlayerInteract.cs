using UnityEngine;

/// <summary>
/// Handles player interaction with objects (items, doors, hiding spots).
/// Uses raycasting to detect interactables within range.
/// </summary>
public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode hideKey = KeyCode.Q;

    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Text interactPromptText;

    [Header("Hiding Settings")]
    [SerializeField] private Transform playerCamera;

    private IInteractable currentInteractable;
    private HidingSpot currentHidingSpot;
    private bool isHiding = false;

    public bool IsHiding => isHiding;
    public event System.Action<IInteractable> OnInteract;
    public static event System.Action<bool> OnHidingStateChanged;

    private void Update()
    {
        if (isHiding)
        {
            HandleHiding();
            return;
        }

        CheckForInteractable();
        HandleInteraction();
    }

    private void CheckForInteractable()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(
            playerCamera.position,
            playerCamera.forward,
            out hit,
            interactDistance,
            interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            HidingSpot hidingSpot = hit.collider.GetComponent<HidingSpot>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                UpdatePrompt(interactable.InteractPrompt);
            }
            else if (hidingSpot != null)
            {
                currentHidingSpot = hidingSpot;
                UpdatePrompt("Press Q to hide");
            }
            else
            {
                ClearCurrentTarget();
            }
        }
        else
        {
            ClearCurrentTarget();
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            currentInteractable.Interact(this);
            OnInteract?.Invoke(currentInteractable);
            currentInteractable = null;
        }

        if (Input.GetKeyDown(hideKey) && currentHidingSpot != null)
        {
            EnterHidingSpot(currentHidingSpot);
        }
    }

    private void HandleHiding()
    {
        if (Input.GetKeyDown(hideKey) || Input.GetKeyDown(interactKey))
        {
            ExitHidingSpot();
        }
    }

    public void EnterHidingSpot(HidingSpot spot)
    {
        isHiding = true;
        spot.EnterHidingSpot(this);
        UpdatePrompt("Press E or Q to exit");
        
        // Disable player controller
        GetComponent<PlayerController>().enabled = false;
        
        // Notify AI of hiding state change
        OnHidingStateChanged?.Invoke(true);
    }

    public void ExitHidingSpot()
    {
        if (currentHidingSpot != null)
        {
            currentHidingSpot.ExitHidingSpot(this);
        }
        
        isHiding = false;
        currentHidingSpot = null;
        UpdatePrompt("");
        
        // Re-enable player controller
        GetComponent<PlayerController>().enabled = true;
        
        // Notify AI of hiding state change
        OnHidingStateChanged?.Invoke(false);
    }

    private void UpdatePrompt(string text)
    {
        if (interactPromptText != null)
        {
            interactPromptText.text = text;
        }
    }

    private void ClearCurrentTarget()
    {
        currentInteractable = null;
        currentHidingSpot = null;
        UpdatePrompt("");
    }

    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerCamera.position, playerCamera.forward * interactDistance);
        }
    }
}

/// <summary>
/// Interface for all interactable objects (items, doors, switches).
/// </summary>
public interface IInteractable
{
    string InteractPrompt { get; }
    void Interact(PlayerInteract player);
}
