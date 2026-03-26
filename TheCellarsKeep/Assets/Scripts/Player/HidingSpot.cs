using UnityEngine;

/// <summary>
/// Represents a hiding spot (closet, wardrobe, under bed).
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class HidingSpot : MonoBehaviour
{
    [Header("Hiding Spot Settings")]
    [SerializeField] private Transform hidePosition;
    [SerializeField] private Transform exitPosition;
    [SerializeField] private string spotName = "Closet";

    private bool isOccupied = false;
    private PlayerController hiddenPlayer;

    public bool IsOccupied => isOccupied;

    public void EnterHidingSpot(PlayerInteract player)
    {
        if (isOccupied) return;

        isOccupied = true;
        hiddenPlayer = player.GetComponent<PlayerController>();

        player.transform.position = hidePosition.position;
        player.transform.rotation = hidePosition.rotation;

        SetPlayerVisible(false);
        Debug.Log($"Player hiding in {spotName}");
    }

    public void ExitHidingSpot(PlayerInteract player)
    {
        isOccupied = false;

        player.transform.position = exitPosition.position;
        player.transform.rotation = exitPosition.rotation;

        SetPlayerVisible(true);

        hiddenPlayer = null;
        Debug.Log($"Player exited {spotName}");
    }

    private void SetPlayerVisible(bool visible)
    {
        if (hiddenPlayer == null) return;

        Renderer[] renderers = hiddenPlayer.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hidePosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hidePosition.position, 0.2f);
        }

        if (exitPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(exitPosition.position, 0.2f);
        }
    }
}
