using UnityEngine;

/// <summary>
/// Fear Essence - the currency earned during runs.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class FearEssenceItem : ItemPickup
{
    [Header("Essence Settings")]
    [SerializeField] private EssenceSize size = EssenceSize.Small;

    public enum EssenceSize
    {
        Small,
        Medium,
        Large,
        Rare
    }

    protected override void Start()
    {
        base.Start();
        itemType = ItemType.FearEssence;
        
        value = size switch
        {
            EssenceSize.Small => 5,
            EssenceSize.Medium => 15,
            EssenceSize.Large => 30,
            EssenceSize.Rare => 50,
            _ => 5
        };

        itemName = $"Fear Essence ({value})";
    }

    protected override bool AddToInventory(PlayerInventory inventory)
    {
        inventory.AddFearEssence(value);
        return true;
    }

    protected override void AnimateItem()
    {
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
