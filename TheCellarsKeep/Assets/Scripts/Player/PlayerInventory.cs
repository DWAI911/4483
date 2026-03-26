using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player inventory - keys, fuses, consumables, and currency.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Starting Items")]
    [SerializeField] private int startingFearEssence = 0;

    [Header("Inventory Limits")]
    [SerializeField] private int maxKeys = 3;
    [SerializeField] private int maxFuses = 3;
    [SerializeField] private int maxConsumables = 5;

    // Current inventory state
    private int currentKeys;
    private int currentFuses;
    private int fearEssence; // Currency earned this run
    private int totalFearEssence; // Currency across all runs
    private List<ConsumableItem> consumables = new List<ConsumableItem>();

    // Public properties
    public int Keys => currentKeys;
    public int Fuses => currentFuses;
    public int FearEssence => fearEssence;
    public int TotalFearEssence => totalFearEssence;
    public int ConsumableCount => consumables.Count;

    // Events
    public event System.Action<int> OnKeysChanged;
    public event System.Action<int> OnFusesChanged;
    public event System.Action<int> OnFearEssenceChanged;
    public event System.Action<ConsumableItem> OnConsumableAdded;
    public event System.Action<ConsumableItem> OnConsumableUsed;

    private void Awake()
    {
        fearEssence = 0;
        currentKeys = 0;
        currentFuses = 0;
        consumables.Clear();
    }

    #region Keys
    public bool AddKey()
    {
        if (currentKeys >= maxKeys) return false;
        
        currentKeys++;
        OnKeysChanged?.Invoke(currentKeys);
        Debug.Log($"Key added. Total keys: {currentKeys}");
        return true;
    }

    public bool UseKey()
    {
        if (currentKeys <= 0) return false;
        
        currentKeys--;
        OnKeysChanged?.Invoke(currentKeys);
        Debug.Log($"Key used. Remaining keys: {currentKeys}");
        return true;
    }
    #endregion

    #region Fuses
    public bool AddFuse()
    {
        if (currentFuses >= maxFuses) return false;
        
        currentFuses++;
        OnFusesChanged?.Invoke(currentFuses);
        Debug.Log($"Fuse added. Total fuses: {currentFuses}");
        return true;
    }

    public bool UseFuse()
    {
        if (currentFuses <= 0) return false;
        
        currentFuses--;
        OnFusesChanged?.Invoke(currentFuses);
        Debug.Log($"Fuse used. Remaining fuses: {currentFuses}");
        return true;
    }
    #endregion

    #region Fear Essence (Currency)
    public void AddFearEssence(int amount)
    {
        fearEssence += amount;
        OnFearEssenceChanged?.Invoke(fearEssence);
        Debug.Log($"Fear Essence +{amount}. Current: {fearEssence}");
    }

    public void BankFearEssence()
    {
        // Called when player dies - saves earned essence for shop
        totalFearEssence += fearEssence;
        Debug.Log($"Banked {fearEssence} Fear Essence. Total: {totalFearEssence}");
        fearEssence = 0;
    }

    public bool SpendFearEssence(int amount)
    {
        if (totalFearEssence < amount) return false;
        
        totalFearEssence -= amount;
        OnFearEssenceChanged?.Invoke(totalFearEssence);
        return true;
    }

    public void ResetRunEssence()
    {
        fearEssence = 0;
        OnFearEssenceChanged?.Invoke(fearEssence);
    }
    #endregion

    #region Consumables
    public bool AddConsumable(ConsumableItem item)
    {
        if (consumables.Count >= maxConsumables) return false;

        consumables.Add(item);
        OnConsumableAdded?.Invoke(item);
        Debug.Log($"Consumable added: {item.ItemName}");
        return true;
    }

    public bool UseConsumable(int index, PlayerController player)
    {
        if (index < 0 || index >= consumables.Count) return false;

        ConsumableItem item = consumables[index];
        item.Use(player);
        consumables.RemoveAt(index);
        OnConsumableUsed?.Invoke(item);
        Debug.Log($"Used consumable: {item.ItemName}");
        return true;
    }

    public ConsumableItem GetConsumable(int index)
    {
        if (index < 0 || index >= consumables.Count) return null;
        return consumables[index];
    }

    public List<ConsumableItem> GetAllConsumables()
    {
        return new List<ConsumableItem>(consumables);
    }
    #endregion

    #region Save/Load
    public void ResetInventory()
    {
        currentKeys = 0;
        currentFuses = 0;
        fearEssence = 0;
        consumables.Clear();
        
        OnKeysChanged?.Invoke(currentKeys);
        OnFusesChanged?.Invoke(currentFuses);
        OnFearEssenceChanged?.Invoke(fearEssence);
    }
    #endregion
}
