using UnityEngine;

/// <summary>
/// Fuse item used to power the exit door.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class FuseItem : ItemPickup
{
    [Header("Fuse Settings")]
    [SerializeField] private bool isGoldFuse = false;

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
