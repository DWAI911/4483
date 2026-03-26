using UnityEngine;

/// <summary>
/// Base class for all collectible items.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class ItemPickup : MonoBehaviour, IInteractable
{
    public enum ItemType
    {
        Key,
        Fuse,
        FearEssence,
        Consumable,
        LoreNote
    }

    [Header("Item Settings")]
    [SerializeField] protected ItemType itemType;
    [SerializeField] protected string itemName = "Item";
    [SerializeField] [TextArea] protected string itemDescription;
    [SerializeField] protected int value = 1;
    [SerializeField] protected Sprite itemIcon;

    [Header("Visual Settings")]
    [SerializeField] protected GameObject visualModel;
    [SerializeField] protected float bobHeight = 0.1f;
    [SerializeField] protected float bobSpeed = 2f;
    [SerializeField] protected float rotationSpeed = 30f;

    [Header("Audio")]
    [SerializeField] protected AudioClip pickupSound;

    protected Vector3 startPosition;
    protected bool hasBeenPickedUp = false;

    public string InteractPrompt => $"Press E to pick up {itemName}";
    public ItemType Type => itemType;
    public string ItemName => itemName;
    public int Value => value;

    protected virtual void Start()
    {
        startPosition = transform.position;
    }

    protected virtual void Update()
    {
        if (!hasBeenPickedUp)
        {
            AnimateItem();
        }
    }

    protected virtual void AnimateItem()
    {
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + Vector3.up * bobOffset;

        if (visualModel != null)
        {
            visualModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    public virtual void Interact(PlayerInteract player)
    {
        if (hasBeenPickedUp) return;

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        
        if (inventory != null)
        {
            bool success = AddToInventory(inventory);
            
            if (success)
            {
                OnPickup();
                hasBeenPickedUp = true;
                Destroy(gameObject);
            }
        }
    }

    protected virtual bool AddToInventory(PlayerInventory inventory)
    {
        switch (itemType)
        {
            case ItemType.Key:
                return inventory.AddKey();
            case ItemType.Fuse:
                return inventory.AddFuse();
            case ItemType.FearEssence:
                inventory.AddFearEssence(value);
                return true;
            case ItemType.LoreNote:
                return ShowLoreNote();
            default:
                return false;
        }
    }

    protected virtual void OnPickup()
    {
        Debug.Log($"Picked up: {itemName}");
        
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    protected virtual bool ShowLoreNote()
    {
        Debug.Log($"Read lore note: {itemDescription}");
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && itemType == ItemType.FearEssence)
        {
            PlayerInteract player = other.GetComponent<PlayerInteract>();
            if (player != null)
            {
                Interact(player);
            }
        }
    }
}
