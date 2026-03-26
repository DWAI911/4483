using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Central game state manager.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Win Condition")]
    [SerializeField] private int fusesRequiredToEscape = 3;

    [Header("Scene References")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip escapeMusic;

    // Persistent data
    private int totalFearEssence = 0;
    private int currentRunEssence = 0;
    private int totalRuns = 0;
    private int successfulEscapes = 0;
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
    public int TotalNotesCollected => collectedLoreNotes.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPersistentData();
    }

    private void Start()
    {
        FindReferences();
        StartNewRun();
        
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

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnRunCountChanged?.Invoke(totalRuns);
        Debug.Log($"Starting run #{totalRuns}");
    }

    public void RestartRun()
    {
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
        
        if (survivalTime > longestSurvivalTime)
        {
            longestSurvivalTime = survivalTime;
        }

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
        Debug.Log($"Player died. Earned {currentRunEssence} Fear Essence. Total: {totalFearEssence}");

        SavePersistentData();

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, Vector3.zero);
        }

        ShowDeathScreen();
    }

    public void HandlePlayerEscape()
    {
        hasWon = true;
        successfulEscapes++;

        float survivalTime = Time.time - currentRunStartTime;
        int escapeBonus = 50 + Mathf.FloorToInt(survivalTime);
        totalFearEssence += currentRunEssence + escapeBonus;

        bool trueEnding = collectedLoreNotes.Count >= 10;

        OnPlayerEscape?.Invoke();
        Debug.Log($"ESCAPED! Bonus: +{escapeBonus} Essence. Total: {totalFearEssence}");

        if (escapeMusic != null)
        {
            AudioSource.PlayClipAtPoint(escapeMusic, Vector3.zero);
        }

        SavePersistentData();
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
            Debug.Log($"Collected lore note {note.NoteNumber}/10");
            SavePersistentData();
        }
    }

    public bool HasCollectedNote(int noteNumber)
    {
        return collectedLoreNotes.Contains(noteNumber);
    }
    #endregion

    #region Shop
    public void OpenShop()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
            
            if (!shopItemPurchaseCount.ContainsKey(itemId))
            {
                shopItemPurchaseCount[itemId] = 0;
            }
            shopItemPurchaseCount[itemId]++;

            OnFearEssenceChanged?.Invoke(totalFearEssence);
            SavePersistentData();
            
            return true;
        }
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
    }
    #endregion

    #region UI Callbacks
    private void ShowDeathScreen()
    {
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
        Debug.Log(trueEnding ? "=== TRUE ENDING ===" : "=== YOU ESCAPED ===");
    }
    #endregion

    #region Save/Load
    private void SavePersistentData()
    {
        PlayerPrefs.SetInt("TotalFearEssence", totalFearEssence);
        PlayerPrefs.SetInt("TotalRuns", totalRuns);
        PlayerPrefs.SetInt("SuccessfulEscapes", successfulEscapes);
        PlayerPrefs.SetFloat("LongestSurvivalTime", longestSurvivalTime);

        string notesString = string.Join(",", collectedLoreNotes);
        PlayerPrefs.SetString("CollectedLoreNotes", notesString);

        foreach (var kvp in shopItemPurchaseCount)
        {
            PlayerPrefs.SetInt($"Shop_{kvp.Key}", kvp.Value);
        }

        PlayerPrefs.Save();
    }

    private void LoadPersistentData()
    {
        totalFearEssence = PlayerPrefs.GetInt("TotalFearEssence", 0);
        totalRuns = PlayerPrefs.GetInt("TotalRuns", 0);
        successfulEscapes = PlayerPrefs.GetInt("SuccessfulEscapes", 0);
        longestSurvivalTime = PlayerPrefs.GetFloat("LongestSurvivalTime", 0f);

        string notesString = PlayerPrefs.GetString("CollectedLoreNotes", "");
        if (!string.IsNullOrEmpty(notesString))
        {
            string[] noteStrings = notesString.Split(',');
            foreach (string s in noteStrings)
            {
                if (int.TryParse(s, out int noteNum))
                {
                    collectedLoreNotes.Add(noteNum);
                }
            }
        }

        Debug.Log($"Loaded: {totalFearEssence} Essence, {totalRuns} runs");
    }

    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        totalFearEssence = 0;
        currentRunEssence = 0;
        totalRuns = 0;
        successfulEscapes = 0;
        collectedLoreNotes.Clear();
        shopItemPurchaseCount.Clear();
    }
    #endregion
}
