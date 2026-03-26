using UnityEngine;

/// <summary>
/// ScriptableObject defining a consumable item.
/// Unity 2022.3.62f1 compatible.
/// </summary>
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Items/Consumable Item")]
public class ConsumableItem : ScriptableObject
{
    [Header("Item Info")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] [TextArea] private string description = "Description";
    [SerializeField] private Sprite icon;

    [Header("Shop Settings")]
    [SerializeField] private int cost = 10;
    [SerializeField] private bool unlockedByDefault = false;

    [Header("Effect Settings")]
    [SerializeField] private ConsumableType type;
    [SerializeField] private float effectValue = 50f;
    [SerializeField] private float effectDuration = 5f;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public int Cost => cost;
    public bool UnlockedByDefault => unlockedByDefault;

    public enum ConsumableType
    {
        StaminaPill,
        Flashbang,
        Decoy,
        HealthPotion,
        SpeedBoost
    }

    public void Use(PlayerController player)
    {
        switch (type)
        {
            case ConsumableType.StaminaPill:
                player.ResetStamina();
                Debug.Log("Stamina fully restored!");
                break;
            case ConsumableType.Flashbang:
                UseFlashbang(player);
                break;
            case ConsumableType.Decoy:
                Debug.Log("Decoy deployed!");
                break;
            case ConsumableType.HealthPotion:
                Debug.Log($"Healed {effectValue} health!");
                break;
            case ConsumableType.SpeedBoost:
                Debug.Log($"Speed boost for {effectDuration} seconds!");
                break;
        }

        Debug.Log($"Used {itemName}");
    }

    private void UseFlashbang(PlayerController player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            player.Position,
            effectValue,
            LayerMask.GetMask("Enemy")
        );

        foreach (Collider col in hitColliders)
        {
            AIChaser ai = col.GetComponent<AIChaser>();
            if (ai != null)
            {
                ai.Stun(effectDuration);
            }
        }

        Debug.Log($"Flashbang stunned {hitColliders.Length} enemies!");
    }
}
