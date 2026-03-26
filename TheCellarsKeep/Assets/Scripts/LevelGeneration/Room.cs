using UnityEngine;

/// <summary>
/// Defines a room for procedural generation.
/// Each room has connection points (doors) that link to other rooms.
/// </summary>
public class Room : MonoBehaviour
{
    public enum RoomType
    {
        StartRoom,      // Player spawn
        EndRoom,        // Exit door
        Corridor,       // Connecting passage
        Library,        // Lore notes, items
        Kitchen,        // Items
        Bedroom,        // Hiding spots, items
        Basement,       // Dark, difficult
        Attic,          // High value items
        SafeRoom        // Meta-shop access
    }

    [Header("Room Settings")]
    [SerializeField] private RoomType roomType = RoomType.Corridor;
    [SerializeField] private Vector3 roomSize = new Vector3(10f, 4f, 10f);

    [Header("Connection Points")]
    [SerializeField] private Transform[] connectionPoints; // Door positions
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] itemSpawnPoints;
    [SerializeField] private Transform[] hidingSpotSpawnPoints;
    [SerializeField] private Transform enemySpawnPoint;

    [Header("Doors")]
    [SerializeField] private Door[] doors;

    [Header("Lighting")]
    [SerializeField] private Light[] roomLights;
    [SerializeField] private bool lightsOnByDefault = true;

    [Header("Fog")]
    [SerializeField] private bool hasFog = true;
    [SerializeField] private float fogDensity = 0.02f;

    // Runtime data
    private bool hasBeenVisited = false;
    private int connectedDoors = 0;

    public RoomType Type => roomType;
    public Vector3 Size => roomSize;
    public Transform[] ConnectionPoints => connectionPoints;
    public Transform[] ItemSpawnPoints => itemSpawnPoints;
    public Transform[] HidingSpotSpawnPoints => hidingSpotSpawnPoints;
    public Transform EnemySpawnPoint => enemySpawnPoint;
    public Door[] Doors => doors;
    public bool HasBeenVisited => hasBeenVisited;

    public void Initialize()
    {
        hasBeenVisited = false;
        
        // Set up lighting
        foreach (Light light in roomLights)
        {
            light.enabled = lightsOnByDefault;
        }
    }

    public void OnPlayerEnter()
    {
        if (!hasBeenVisited)
        {
            hasBeenVisited = true;
            // Could trigger events here (enemy spawn, etc.)
        }
    }

    public void ToggleLights(bool on)
    {
        foreach (Light light in roomLights)
        {
            if (light != null)
            {
                light.enabled = on;
            }
        }
    }

    public Transform GetRandomConnectionPoint()
    {
        if (connectionPoints == null || connectionPoints.Length == 0) return null;
        return connectionPoints[Random.Range(0, connectionPoints.Length)];
    }

    public Transform GetRandomItemSpawnPoint()
    {
        if (itemSpawnPoints == null || itemSpawnPoints.Length == 0) return null;
        return itemSpawnPoints[Random.Range(0, itemSpawnPoints.Length)];
    }

    public void CloseAllDoors()
    {
        foreach (Door door in doors)
        {
            if (door != null)
            {
                door.CloseDoor();
            }
        }
    }

    public void OpenAllDoors()
    {
        foreach (Door door in doors)
        {
            if (door != null)
            {
                door.OpenDoor();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw room bounds
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, roomSize);

        // Draw connection points
        if (connectionPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in connectionPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.3f);
                    Gizmos.DrawRay(point.position, point.forward * 2f);
                }
            }
        }

        // Draw item spawn points
        if (itemSpawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform point in itemSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawCube(point.position, Vector3.one * 0.2f);
                }
            }
        }

        // Draw hiding spot spawn points
        if (hidingSpotSpawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in hidingSpotSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.3f);
                }
            }
        }

        // Draw enemy spawn point
        if (enemySpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(enemySpawnPoint.position, 0.5f);
        }
    }
}
