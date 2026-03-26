using UnityEngine;

/// <summary>
/// Fuse item used to power the exit door/elevator.
/// Multiple fuses may be required to escape.
/// </summary>
public class FuseItem : ItemPickup
{
    [Header("Fuse Settings")]
    [SerializeField] private bool isGoldFuse = false; // Special fuse worth more

    public bool IsGoldFuse => isGoldFuse;

    protected override void Start()
    {
        base.Start();
        itemType = ItemType.Fuse;
        itemName = isGoldFuse ? "Golden Fuse" : "Fuse";
        value = isGoldFuse ? 2 : 1;
    }

    protected override bool AddToInventory(PlayerInventory inventory)
    {
        return inventory.AddFuse();
    }
}
