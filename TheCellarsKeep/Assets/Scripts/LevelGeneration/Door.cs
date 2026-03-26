using UnityEngine;

/// <summary>
/// Door that can be locked (requires key) or open freely.
/// Connects two rooms together.
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
    public enum DoorState
    {
        Open,
        Closed,
        Locked,
        Blocked  // Used during generation when not connected
    }

    [Header("Door Settings")]
    [SerializeField] private DoorState initialState = DoorState.Closed;
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private int keyLevel = 1; // Different colored keys

    [Header("Animation")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound;

    [Header("Connections")]
    [SerializeField] private Room connectedRoomA;
    [SerializeField] private Room connectedRoomB;

    private DoorState currentState;
    private bool isAnimating = false;
    private float currentAngle = 0f;
    private AudioSource audioSource;

    public bool IsOpen => currentState == DoorState.Open;
    public bool IsLocked => currentState == DoorState.Locked;
    public bool RequiresKey => requiresKey;
    public int KeyLevel => keyLevel;
    public string InteractPrompt => GetPrompt();

    public Room ConnectedRoomA => connectedRoomA;
    public Room ConnectedRoomB => connectedRoomB;

    public event System.Action OnDoorOpened;
    public event System.Action OnDoorClosed;

    private void Awake()
    {
        currentState = initialState;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        UpdateDoorVisual();
    }

    public void Interact(PlayerInteract player)
    {
        if (isAnimating) return;

        switch (currentState)
        {
            case DoorState.Open:
                CloseDoor();
                break;
            case DoorState.Closed:
                OpenDoor(player);
                break;
            case DoorState.Locked:
                TryUnlock(player);
                break;
            case DoorState.Blocked:
                // Can't interact with blocked doors
                break;
        }
    }

    private void OpenDoor(PlayerInteract player)
    {
        if (requiresKey)
        {
            // Check if player has key
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.Keys > 0)
            {
                inventory.UseKey();
                requiresKey = false;
            }
            else
            {
                // Play locked sound
                PlaySound(lockedSound);
                Debug.Log("Door is locked! Need a key.");
                return;
            }
        }

        currentState = DoorState.Open;
        isAnimating = true;
        PlaySound(openSound);
        OnDoorOpened?.Invoke();
    }

    public void CloseDoor()
    {
        if (currentState == DoorState.Locked || currentState == DoorState.Blocked) return;

        currentState = DoorState.Closed;
        isAnimating = true;
        PlaySound(openSound);
        OnDoorClosed?.Invoke();
    }

    public void LockDoor()
    {
        currentState = DoorState.Locked;
        requiresKey = true;
        UpdateDoorVisual();
    }

    public void UnlockDoor()
    {
        requiresKey = false;
        currentState = DoorState.Closed;
        UpdateDoorVisual();
    }

    public void BlockDoor()
    {
        // Used during generation - this door doesn't connect anywhere
        currentState = DoorState.Blocked;
        if (doorPivot != null)
        {
            doorPivot.gameObject.SetActive(false);
        }
    }

    private void TryUnlock(PlayerInteract player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        
        if (inventory != null && inventory.Keys > 0)
        {
            inventory.UseKey();
            UnlockDoor();
            OpenDoor(player);
        }
        else
        {
            PlaySound(lockedSound);
            Debug.Log("Need a key to unlock this door!");
        }
    }

    private void UpdateDoorVisual()
    {
        if (doorPivot == null) return;

        float targetAngle = 0f;
        
        switch (currentState)
        {
            case DoorState.Open:
                targetAngle = openAngle;
                break;
            case DoorState.Closed:
            case DoorState.Locked:
                targetAngle = 0f;
                break;
            case DoorState.Blocked:
                doorPivot.gameObject.SetActive(false);
                return;
        }

        doorPivot.localRotation = Quaternion.Euler(0f, targetAngle, 0f);
    }

    private void Update()
    {
        if (!isAnimating) return;

        float targetAngle = currentState == DoorState.Open ? openAngle : 0f;
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, animationSpeed * Time.deltaTime);
        
        doorPivot.localRotation = Quaternion.Euler(0f, currentAngle, 0f);

        if (Mathf.Abs(currentAngle - targetAngle) < 0.5f)
        {
            currentAngle = targetAngle;
            isAnimating = false;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private string GetPrompt()
    {
        switch (currentState)
        {
            case DoorState.Open:
                return "Press E to close";
            case DoorState.Closed:
                return requiresKey ? "Press E to unlock (Requires Key)" : "Press E to open";
            case DoorState.Locked:
                return "Locked (Requires Key)";
            case DoorState.Blocked:
                return "";
            default:
                return "";
        }
    }

    public Room GetOtherRoom(Room currentRoom)
    {
        if (connectedRoomA == currentRoom) return connectedRoomB;
        if (connectedRoomB == currentRoom) return connectedRoomA;
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentState == DoorState.Locked ? Color.red : 
                       currentState == DoorState.Blocked ? Color.black : Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
    }
}
