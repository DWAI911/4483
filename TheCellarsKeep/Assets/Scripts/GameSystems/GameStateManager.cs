using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Central game state manager handling:
- Run tracking (attempt count)
- Player death and respawn
- Win condition (escape)
- Persistence across runs (Fear Essence, unlocks)
- Lore note collection
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Run Settings")]
    [SerializeField] private int maxRunsPerSession = 50;

    [Header("Win Condition")]
    [SerializeField] private int fusesRequiredToEscape = 3;

    [Header("Scene References")]
    [SerializeField] private string shopSceneName = "Shop";
    [SerializeField] private string gameSceneName = "Game";

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip escapeMusic;

    // Persistent data (survives between runs)
    private int totalFearEssence = 0;
    private int currentRunEssence = 0;
    private int totalRuns = 0;
    private int successfulEscapes = 0;
    private List<string> unlockedItems = new List<string>();
    private List<int> collectedLoreNotes = new List<int>();
    private float longestSurvivalTime = 0f;
    private Dictionary<string, int> shopItemPurchaseCount = new Dictionary<string, int>();

    // Current run data
    private float currentRunStartTime;
    private bool isPaused = false;
    private bool hasWon = false;

    // References
    private PlayerController player;
    private PlayerInventory playerInventory;
    private LevelGenerator levelGenerator;

    // Events
    public event System.Action<int> OnFearEssenceChanged;
    public event System.Action<int> OnRunCountChanged;
    public event System.Action OnPlayerDeath;
    public event System.Action OnPlayerEscape;
    public event System.Action<int> OnLoreNoteCollected;

    // Properties
    public int TotalFearEssence => totalFearEssence;
    public int CurrentRunEssence => currentRunEssence;
    public int TotalRuns => totalRuns;
    public int SuccessfulEscapes => successfulEscapes;
    public int FusesRequired => fusesRequiredToEscape;
    public bool IsPaused => isPaused;
    public bool HasWon => hasWon;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scene loads

        LoadPersistentData();
    }

    private void Start()
    {
        FindReferences();
        StartNewRun();
        
        // Subscribe to AI caught event
        AIChaser.OnPlayerCaught += OnPlayerCaught;
    }

    private void OnDestroy()
    {
        AIChaser.OnPlayerCaught -= OnPlayerCaught;
    }

    private void FindReferences()
    {
        player = FindObjectOfType<PlayerController>();
        playerInventory = FindObjectOfType<PlayerInventory>();
        levelGenerator = FindObjectOfType<LevelGenerator>();

        if (player != null)
        {
            player.OnDeath += HandlePlayerDeath;
        }

        if (playerInventory != null)
        {
            playerInventory.OnFearEssenceChanged += (amount) => currentRunEssence = amount;
        }
    }

    #region Run Management
    public void StartNewRun()
    {
        totalRuns++;
        currentRunStartTime = Time.time;
        currentRunEssence = 0;
        hasWon = false;
        isPaused = false;

        // Reset time scale
        Time.timeScale = 1f;

        // Unlock cursor for shop, lock for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnRunCountChanged?.Invoke(totalRuns);
        Debug.Log($"Starting run #{totalRuns}");

        // Regenerate level if needed
        if (levelGenerator != null)
        {
            levelGenerator.GenerateLevel();
            FindReferences(); // Re-find references after generation
        }
    }

    public void RestartRun()
    {
        // Reload the game scene
        SceneManager.LoadScene(gameSceneName);
        StartNewRun();
    }
    #endregion

    #region Death & Win
    private void OnPlayerCaught()
    {
        HandlePlayerDeath();
    }

    public void HandlePlayerDeath()
    {
        float survivalTime = Time.time - currentRunStartTime;
        
        // Update longest survival time
        if (survivalTime > longestSurvivalTime)
        {
            longestSurvivalTime = survivalTime;
        }

        // Bank the Fear Essence earned this run
        if (playerInventory != null)
        {
            playerInventory.BankFearEssence();
            totalFearEssence = playerInventory.TotalFearEssence;
        }
        else
        {
            totalFearEssence += currentRunEssence;
        }

        OnPlayerDeath?.Invoke();
        Debug.Log($"Player died after {survivalTime:F1} seconds. Earned {currentRunEssence} Fear Essence. Total: {totalFearEssence}");

        // Save data
        SavePersistentData();

        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, Vector3.zero);
        }

        // Show death UI or load shop
        ShowDeathScreen();
    }

    public void HandlePlayerEscape()
    {
        hasWon = true;
        successfulEscapes++;

        float survivalTime = Time.time - currentRunStartTime;
        
        // Bonus essence for escaping
        int escapeBonus = 50 + Mathf.FloorToInt(survivalTime);
        totalFearEssence += currentRunEssence + escapeBonus;

        // Check for true ending (all lore notes collected)
        bool trueEnding = collectedLoreNotes.Count >= 10;

        OnPlayerEscape?.Invoke();
        Debug.Log($"ESCAPED! Survival time: {survivalTime:F1}s. Bonus: +{escapeBonus} Essence. Total: {totalFearEssence}");

        // Play escape music
        if (escapeMusic != null)
        {
            AudioSource.PlayClipAtPoint(escapeMusic, Vector3.zero);
        }

        // Save data
        SavePersistentData();

        // Show win screen
        ShowWinScreen(trueEnding);
    }

    public bool CheckWinCondition()
    {
        if (playerInventory == null) return false;
        return playerInventory.Fuses >= fusesRequiredToEscape;
    }
    #endregion

    #region Lore Notes
    public void CollectLoreNote(LoreNoteItem note)
    {
        if (!collectedLoreNotes.Contains(note.NoteNumber))
        {
            collectedLoreNotes.Add(note.NoteNumber);
            OnLoreNoteCollected?.Invoke(note.NoteNumber);
            Debug.Log($"Collected lore note {note.NoteNumber}/{10}");

            SavePersistentData();
        }
    }

    public bool HasCollectedNote(int noteNumber)
    {
        return collectedLoreNotes.Contains(noteNumber);
    }

    public int TotalNotesCollected => collectedLoreNotes.Count;
    #endregion

    #region Shop Integration
    public void OpenShop()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load shop scene additively or show shop UI
        // SceneManager.LoadScene(shopSceneName, LoadSceneMode.Additive);
        
        Debug.Log("Shop opened");
    }

    public void CloseShop()
    {
        isPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Shop closed");
    }

    public bool PurchaseItem(string itemId, int cost)
    {
        if (totalFearEssence >= cost)
        {
            totalFearEssence -= cost;
            
            // Track purchase count
            if (!shopItemPurchaseCount.ContainsKey(itemId))
            {
                shopItemPurchaseCount[itemId] = 0;
            }
            shopItemPurchaseCount[itemId]++;

            OnFearEssenceChanged?.Invoke(totalFearEssence);
            SavePersistentData();
            
            Debug.Log($"Purchased {itemId} for {cost} Essence. Remaining: {totalFearEssence}");
            return true;
        }

        Debug.Log($"Cannot afford {itemId}. Need {cost}, have {totalFearEssence}");
        return false;
    }

    public int GetPurchaseCount(string itemId)
    {
        return shopItemPurchaseCount.TryGetValue(itemId, out int count) ? count : 0;
    }
    #endregion

    #region Pause
    public void TogglePause()
    {
        if (hasWon) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        Debug.Log(isPaused ? "Game paused" : "Game resumed");
    }
    #endregion

    #region UI Callbacks
    private void ShowDeathScreen()
    {
        // This would be handled by a UI manager
        // For now, just pause and log
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("=== YOU DIED ===");
    }

    private void ShowWinScreen(bool trueEnding)
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (trueEnding)
        {
            Debug.Log("=== TRUE ENDING ACHIEVED ===");
        }
        else
        {
            Debug.Log("=== YOU ESCAPED ===");
        }
    }
    #endregion

    #region Save/Load
    private void SavePersistentData()
    {
        PlayerPrefs.SetInt("TotalFearEssence", totalFearEssence);
        PlayerPrefs.SetInt("TotalRuns", totalRuns);
        PlayerPrefs.SetInt("SuccessfulEscapes", successfulEscapes);
        PlayerPrefs.SetFloat("LongestSurvivalTime", longestSurvivalTime);

        // Save unlocked items
        string unlockedItemsString = string.Join(",", unlockedItems);
        PlayerPrefs.SetString("UnlockedItems", unlockedItemsString);

        // Save collected lore notes
        string notesString = string.Join(",", collectedLoreNotes);
        PlayerPrefs.SetString("CollectedLoreNotes", notesString);

        // Save shop purchase counts
        foreach (var kvp in shopItemPurchaseCount)
        {
            PlayerPrefs.SetInt($"Shop_{kvp.Key}", kvp.Value);
        }

        PlayerPrefs.Save();
        Debug.Log("Game data saved");
    }

    private void LoadPersistentData()
    {
        totalFearEssence = PlayerPrefs.GetInt("TotalFearEssence", 0);
        totalRuns = PlayerPrefs.GetInt("TotalRuns", 0);
        successfulEscapes = PlayerPrefs.GetInt("SuccessfulEscapes", 0);
        longestSurvivalTime = PlayerPrefs.GetFloat("LongestSurvivalTime", 0f);

        // Load unlocked items
        string unlockedItemsString = PlayerPrefs.GetString("UnlockedItems", "");
        if (!string.IsNullOrEmpty(unlockedItemsString))
        {
            unlockedItems = new List<string>(unlockedItemsString.Split(','));
        }

        // Load collected lore notes
        string notesString = PlayerPrefs.GetString("CollectedLoreNotes", "");
        if (!string.IsNullOrEmpty(notesString))
        {
            string[] noteStrings = notesString.Split(',');
            collectedLoreNotes = new List<int>();
            foreach (string s in noteStrings)
            {
                if (int.TryParse(s, out int noteNum))
                {
                    collectedLoreNotes.Add(noteNum);
                }
            }
        }

        Debug.Log($"Loaded game data: {totalFearEssence} Essence, {totalRuns} runs, {successfulEscapes} escapes");
    }

    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        totalFearEssence = 0;
        currentRunEssence = 0;
        totalRuns = 0;
        successfulEscapes = 0;
        longestSurvivalTime = 0f;
        unlockedItems.Clear();
        collectedLoreNotes.Clear();
        shopItemPurchaseCount.Clear();
        
        Debug.Log("All game data reset");
    }
    #endregion

    #region Cheats/Debug
    [ContextMenu("Add 100 Essence")]
    public void AddDebugEssence()
    {
        totalFearEssence += 100;
        OnFearEssenceChanged?.Invoke(totalFearEssence);
        Debug.Log($"Added 100 Essence. Total: {totalFearEssence}");
    }

    [ContextMenu("Unlock All Items")]
    public void DebugUnlockAllItems()
    {
        // Would unlock all shop items
        Debug.Log("All items unlocked (debug)");
    }
    #endregion
}
