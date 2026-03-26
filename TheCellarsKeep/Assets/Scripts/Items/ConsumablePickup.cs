using UnityEngine;

/// <summary>
/// Pickup for consumable items (flashbang, decoy, stamina pill).
/// Adds the consumable to player inventory.
/// </summary>
public class ConsumablePickup : ItemPickup
{
    [Header("Consumable Settings")]
    [SerializeField] private ConsumableItem consumableItem;

    public ConsumableItem ConsumableItem => consumableItem;

    protected override void Start()
    {
        base.Start();
        itemType = ItemType.Consumable;

        if (consumableItem != null)
        {
            itemName = consumableItem.ItemName;
            itemDescription = consumableItem.Description;
            itemIcon = consumableItem.Icon;
        }
    }

    protected override bool AddToInventory(PlayerInventory inventory)
    {
        if (consumableItem == null)
        {
            Debug.LogError("ConsumablePickup has no consumable item assigned!");
            return false;
        }

        return inventory.AddConsumable(consumableItem);
    }
}
