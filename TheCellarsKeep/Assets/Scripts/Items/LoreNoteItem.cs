using UnityEngine;

/// <summary>
/// Lore Note - reveals story elements.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class LoreNoteItem : ItemPickup
{
    [Header("Lore Note Settings")]
    [SerializeField] [TextArea(5, 10)] private string noteContent;
    [SerializeField] private int noteNumber = 1;
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
        return true;
    }

    protected override bool ShowLoreNote()
    {
        Debug.Log($"=== {noteTitle} ===\n{noteContent}\n==================");
        
        GameStateManager gameState = FindObjectOfType<GameStateManager>();
        if (gameState != null)
        {
            gameState.CollectLoreNote(this);
        }

        return true;
    }
}
