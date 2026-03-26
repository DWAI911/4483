using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Meta-shop system for purchasing items between runs.
/// Players spend Fear Essence earned from previous runs.
/// </summary>
public class ShopSystem : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemId;
        public string itemName;
        [TextArea] public string description;
        public int baseCost;
        public int maxPurchaseCount = 1; // 1 = one-time purchase, 0 = unlimited
        public Sprite icon;
        public ConsumableItem consumableRef; // For consumables
        public bool unlockedByDefault = false;
    }

    [System.Serializable]
    public class ShopCategory
    {
        public string categoryName;
        public ShopItem[] items;
    }

    [Header("Shop Categories")]
    [SerializeField] private ShopCategory[] categories;

    [Header("UI References")]
    [SerializeField] private Transform shopPanel;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private UnityEngine.UI.Text essenceText;
    [SerializeField] private UnityEngine.UI.Text selectedItemDescription;
    [SerializeField] private UnityEngine.UI.Button purchaseButton;

    [Header("Audio")]
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip cannotAffordSound;

    private GameStateManager gameState;
    private ShopItem selectedItem;
    private AudioSource audioSource;

    public event System.Action<ShopItem> OnItemPurchased;

    private void Awake()
    {
        gameState = GameStateManager.Instance;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        PopulateShop();
        UpdateEssenceDisplay();
        
        if (gameState != null)
        {
            gameState.OnFearEssenceChanged += UpdateEssenceDisplay;
        }
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.OnFearEssenceChanged -= UpdateEssenceDisplay;
        }
    }

    private void PopulateShop()
    {
        if (shopItemPrefab == null || shopPanel == null) return;

        // Clear existing items
        foreach (Transform child in shopPanel)
        {
            Destroy(child.gameObject);
        }

        // Create items for each category
        foreach (ShopCategory category in categories)
        {
            // Could add category header here
            
            foreach (ShopItem item in category.items)
            {
                CreateShopItemUI(item);
            }
        }
    }

    private void CreateShopItemUI(ShopItem item)
    {
        GameObject itemObj = Instantiate(shopItemPrefab, shopPanel);
        
        // This would reference your specific UI prefab structure
        // For now, we'll set up basic UI elements
        
        ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>();
        if (itemUI == null)
        {
            itemUI = itemObj.AddComponent<ShopItemUI>();
        }

        itemUI.Initialize(item, this);
    }

    public void SelectItem(ShopItem item)
    {
        selectedItem = item;
        
        if (selectedItemDescription != null)
        {
            selectedItemDescription.text = $"{item.itemName}\n\n{item.description}\n\nCost: {GetItemCost(item)} Fear Essence";
        }

        if (purchaseButton != null)
        {
            purchaseButton.interactable = CanAffordItem(item) && !IsItemFullyPurchased(item);
        }
    }

    public void PurchaseSelectedItem()
    {
        if (selectedItem == null) return;
        PurchaseItem(selectedItem);
    }

    public bool PurchaseItem(ShopItem item)
    {
        int cost = GetItemCost(item);
        
        if (!CanAffordItem(item))
        {
            PlaySound(cannotAffordSound);
            Debug.Log($"Cannot afford {item.itemName}");
            return false;
        }

        if (IsItemFullyPurchased(item))
        {
            Debug.Log($"{item.itemName} already fully purchased");
            return false;
        }

        // Process purchase
        if (gameState.PurchaseItem(item.itemId, cost))
        {
            PlaySound(purchaseSound);
            OnItemPurchased?.Invoke(item);
            
            // If it's a consumable, add to player inventory for next run
            // This would be handled by the game state manager
            
            Debug.Log($"Purchased: {item.itemName}");
            return true;
        }

        return false;
    }

    public int GetItemCost(ShopItem item)
    {
        // Could implement dynamic pricing based on purchase count
        int purchaseCount = gameState.GetPurchaseCount(item.itemId);
        
        // Simple scaling: cost increases by 20% per purchase
        if (item.maxPurchaseCount == 0) // Unlimited purchases
        {
            return Mathf.RoundToInt(item.baseCost * (1f + purchaseCount * 0.2f));
        }
        
        return item.baseCost;
    }

    public bool CanAffordItem(ShopItem item)
    {
        return gameState.TotalFearEssence >= GetItemCost(item);
    }

    public bool IsItemFullyPurchased(ShopItem item)
    {
        if (item.maxPurchaseCount == 0) return false; // Unlimited
        if (item.unlockedByDefault) return true;
        
        return gameState.GetPurchaseCount(item.itemId) >= item.maxPurchaseCount;
    }

    public bool IsItemPurchased(ShopItem item)
    {
        return gameState.GetPurchaseCount(item.itemId) > 0 || item.unlockedByDefault;
    }

    private void UpdateEssenceDisplay()
    {
        if (essenceText != null && gameState != null)
        {
            essenceText.text = $"Fear Essence: {gameState.TotalFearEssence}";
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #region Helper Methods for Starting Items
    public List<ShopItem> GetPurchasedStartingItems()
    {
        List<ShopItem> startingItems = new List<ShopItem>();

        foreach (ShopCategory category in categories)
        {
            foreach (ShopItem item in category.items)
            {
                if (IsItemPurchased(item) && item.consumableRef != null)
                {
                    startingItems.Add(item);
                }
            }
        }

        return startingItems;
    }
    #endregion
}

/// <summary>
/// UI component for individual shop items.
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image iconImage;
    [SerializeField] private UnityEngine.UI.Text nameText;
    [SerializeField] private UnityEngine.UI.Text costText;
    [SerializeField] private UnityEngine.UI.Button itemButton;

    private ShopSystem.ShopItem item;
    private ShopSystem shop;

    public void Initialize(ShopSystem.ShopItem shopItem, ShopSystem shopSystem)
    {
        item = shopItem;
        shop = shopSystem;

        if (iconImage != null && shopItem.icon != null)
        {
            iconImage.sprite = shopItem.icon;
        }

        if (nameText != null)
        {
            nameText.text = shopItem.itemName;
        }

        if (costText != null)
        {
            costText.text = shop.GetIsItemFullyPurchased(shopItem) ? "OWNED" : $"${shop.GetItemCost(shopItem)}";
        }

        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OnItemClicked);
        }
    }

    private void OnItemClicked()
    {
        shop.SelectItem(item);
    }
}
