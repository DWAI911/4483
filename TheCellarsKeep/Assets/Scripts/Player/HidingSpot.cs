using UnityEngine;

/// <summary>
/// Represents a hiding spot (closet, wardrobe, under bed).
/// Player becomes invisible to AI while hiding.
/// </summary>
public class HidingSpot : MonoBehaviour
{
    [Header("Hiding Spot Settings")]
    [SerializeField] private Transform hidePosition;
    [SerializeField] private Transform exitPosition;
    [SerializeField] private string spotName = "Closet";

    private bool isOccupied = false;
    private PlayerController hiddenPlayer;
    private PlayerInteract playerInteract;

    public bool IsOccupied => isOccupied;

    public void EnterHidingSpot(PlayerInteract player)
    {
        if (isOccupied) return;

        isOccupied = true;
        playerInteract = player;
        hiddenPlayer = player.GetComponent<PlayerController>();

        // Teleport player to hide position
        player.transform.position = hidePosition.position;
        player.transform.rotation = hidePosition.rotation;

        // Disable player renderer (optional - makes player invisible)
        // Or you could use a layer system for AI detection
        SetPlayerVisible(false);

        Debug.Log($"Player hiding in {spotName}");
    }

    public void ExitHidingSpot(PlayerInteract player)
    {
        isOccupied = false;

        // Teleport player out
        player.transform.position = exitPosition.position;
        player.transform.rotation = exitPosition.rotation;

        SetPlayerVisible(true);

        hiddenPlayer = null;
        playerInteract = null;

        Debug.Log($"Player exited {spotName}");
    }

    private void SetPlayerVisible(bool visible)
    {
        // Disable all renderers on player
        Renderer[] renderers = hiddenPlayer.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }

        // You could also set a custom layer that AI ignores
        // hiddenPlayer.gameObject.layer = visible ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("Hidden");
    }

    private void OnDrawGizmosSelected()
    {
        if (hidePosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hidePosition.position, 0.2f);
            Gizmos.Label(hidePosition.position + Vector3.up * 0.5f, "Hide Position");
        }

        if (exitPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(exitPosition.position, 0.2f);
            Gizmos.Label(exitPosition.position + Vector3.up * 0.5f, "Exit Position");
        }
    }
}
