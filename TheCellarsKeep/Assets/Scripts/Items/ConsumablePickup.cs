using UnityEngine;

/// <summary>
/// Pickup for consumable items.
/// Unity 2022.3.62f1 compatible.
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
