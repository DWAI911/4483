using UnityEngine;

/// <summary>
/// Fear Essence - the currency earned during runs.
/// Used in the meta-shop to buy upgrades and items.
/// </summary>
public class FearEssenceItem : ItemPickup
{
    [Header("Essence Settings")]
    [SerializeField] private int essenceAmount = 5;
    [SerializeField] private EssenceSize size = EssenceSize.Small;

    public enum EssenceSize
    {
        Small,  // 5 essence
        Medium, // 15 essence
        Large,  // 30 essence
        Rare    // 50 essence
    }

    protected override void Start()
    {
        base.Start();
        itemType = ItemType.FearEssence;
        
        // Set value based on size
        value = size switch
        {
            EssenceSize.Small => 5,
            EssenceSize.Medium => 15,
            EssenceSize.Large => 30,
            EssenceSize.Rare => 50,
            _ => 5
        };

        essenceAmount = value;
        itemName = $"Fear Essence ({essenceAmount})";
    }

    protected override bool AddToInventory(PlayerInventory inventory)
    {
        inventory.AddFearEssence(essenceAmount);
        return true;
    }

    protected override void AnimateItem()
    {
        // More dramatic animation for rare essences
        float bobMultiplier = size == EssenceSize.Rare ? 1.5f : 1f;
        float rotationMultiplier = size == EssenceSize.Rare ? 2f : 1f;

        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight * bobMultiplier;
        transform.position = startPosition + Vector3.up * bobOffset;

        if (visualModel != null)
        {
            visualModel.transform.Rotate(Vector3.up, rotationSpeed * rotationMultiplier * Time.deltaTime);
        }
    }
}
