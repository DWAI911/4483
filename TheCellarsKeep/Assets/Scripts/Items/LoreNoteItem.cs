using UnityEngine;

/// <summary>
/// Lore Note - reveals story elements about the mansion and the monster.
/// Collecting all notes unlocks the "True Ending".
/// </summary>
public class LoreNoteItem : ItemPickup
{
    [Header("Lore Note Settings")]
    [SerializeField] [TextArea(5, 10)] private string noteContent;
    [SerializeField] private int noteNumber = 1; // 1-10
    [SerializeField] private string noteTitle = "Untitled Note";

    public string NoteContent => noteContent;
    public int NoteNumber => noteNumber;
    public string NoteTitle => noteTitle;

    protected override void Start()
    {
        base.Start();
        itemType = ItemType.LoreNote;
        itemName = $"Note #{noteNumber}: {noteTitle}";
    }

    protected override bool AddToInventory(PlayerInventory inventory)
    {
        // Lore notes don't go in inventory - they're collected globally
        // The game state manager would track which notes have been found
        return true;
    }

    protected override bool ShowLoreNote()
    {
        // In a full implementation, this would show a UI panel with the note
        Debug.Log($"=== {noteTitle} ===\n{noteContent}\n==================");
        
        // Notify game state manager
        GameStateManager gameState = FindObjectOfType<GameStateManager>();
        if (gameState != null)
        {
            gameState.CollectLoreNote(this);
        }

        return true;
    }
}
