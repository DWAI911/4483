using UnityEngine;

/// <summary>
/// Defines a room for procedural generation.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class Room : MonoBehaviour
{
    public enum RoomType
    {
        StartRoom,
        EndRoom,
        Corridor,
        Library,
        Kitchen,
        Bedroom,
        Basement,
        Attic,
        SafeRoom
    }

    [Header("Room Settings")]
    [SerializeField] private RoomType roomType = RoomType.Corridor;
    [SerializeField] private Vector3 roomSize = new Vector3(10f, 4f, 10f);

    [Header("Connection Points")]
    [SerializeField] private Transform[] connectionPoints;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] itemSpawnPoints;
    [SerializeField] private Transform[] hidingSpotSpawnPoints;
    [SerializeField] private Transform enemySpawnPoint;

    [Header("Doors")]
    [SerializeField] private Door[] doors;

    [Header("Lighting")]
    [SerializeField] private Light[] roomLights;
    [SerializeField] private bool lightsOnByDefault = true;

    // Runtime data
    private bool hasBeenVisited = false;

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
        
        foreach (Light light in roomLights)
        {
            if (light != null)
                light.enabled = lightsOnByDefault;
        }
    }

    public void OnPlayerEnter()
    {
        if (!hasBeenVisited)
        {
            hasBeenVisited = true;
        }
    }

    public void ToggleLights(bool on)
    {
        foreach (Light light in roomLights)
        {
            if (light != null)
                light.enabled = on;
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
                door.CloseDoor();
        }
    }

    public void OpenAllDoors()
    {
        foreach (Door door in doors)
        {
            if (door != null)
                door.OpenDoor();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, roomSize);

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

        if (enemySpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(enemySpawnPoint.position, 0.5f);
        }
    }
}
