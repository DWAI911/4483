using UnityEngine;

/// <summary>
/// ScriptableObject defining a consumable item (flashbang, decoy, stamina pill).
/// Created via: Assets → Create → Items → Consumable Item
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
                UseStaminaPill(player);
                break;
            case ConsumableType.Flashbang:
                UseFlashbang(player);
                break;
            case ConsumableType.Decoy:
                UseDecoy(player);
                break;
            case ConsumableType.HealthPotion:
                UseHealthPotion(player);
                break;
            case ConsumableType.SpeedBoost:
                UseSpeedBoost(player);
                break;
        }

        Debug.Log($"Used {itemName}");
    }

    private void UseStaminaPill(PlayerController player)
    {
        // Instantly restore stamina
        player.ResetStamina();
        Debug.Log("Stamina fully restored!");
    }

    private void UseFlashbang(PlayerController player)
    {
        // Find nearby AI and stun them
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

        // Visual effect (spawn flash prefab if assigned)
        Debug.Log($"Flashbang stunned {hitColliders.Length} enemies!");
    }

    private void UseDecoy(PlayerController player)
    {
        // Spawn a decoy that attracts AI attention
        // This would be implemented with a DecoyObject prefab
        Debug.Log("Decoy deployed!");
        
        // Example: Instantiate decoy prefab at player position
        // Object.Instantiate(decoyPrefab, player.Position, Quaternion.identity);
    }

    private void UseHealthPotion(PlayerController player)
    {
        // Restore health if health system exists
        // player.Heal(effectValue);
        Debug.Log($"Healed {effectValue} health!");
    }

    private void UseSpeedBoost(PlayerController player)
    {
        // Temporary speed boost would need to be implemented in PlayerController
        // For now, just log it
        Debug.Log($"Speed boost activated for {effectDuration} seconds!");
    }
}
