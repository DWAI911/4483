using UnityEngine;

/// <summary>
/// Key item used to unlock doors.
/// </summary>
public class KeyItem : ItemPickup
{
    [Header("Key Settings")]
    [SerializeField] private int keyLevel = 1; // Different colored keys
    [SerializeField] private Color keyColor = Color.yellow;

    public int KeyLevel => keyLevel;

    protected override void Start()
    {
        base.Start();
        itemType = ItemType.Key;
        itemName = $"Key (Level {keyLevel})";
    }

    protected override bool AddToInventory(PlayerInventory inventory)
    {
        return inventory.AddKey();
    }
}
