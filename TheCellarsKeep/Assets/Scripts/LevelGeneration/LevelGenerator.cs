using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural level generator.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public class RoomPrefabEntry
    {
        public Room.RoomType type;
        public GameObject prefab;
        public int minCount = 0;
        public int maxCount = 3;
        public float weight = 1f;
    }

    [Header("Generation Settings")]
    [SerializeField] private int minRooms = 10;
    [SerializeField] private int maxRooms = 20;
    [SerializeField] private float roomSpacing = 12f;
    [SerializeField] private int generationSeed = 0;

    [Header("Room Prefabs")]
    [SerializeField] private RoomPrefabEntry[] roomPrefabs;
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private GameObject endRoomPrefab;

    [Header("Item Prefabs")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject fusePrefab;
    [SerializeField] private GameObject fearEssencePrefab;

    [Header("Hiding Spots")]
    [SerializeField] private GameObject[] hidingSpotPrefabs;

    [Header("References")]
    [SerializeField] private AIChaser enemyPrefab;

    // Runtime data
    private List<Room> generatedRooms = new List<Room>();
    private Dictionary<Vector2Int, Room> roomGrid = new Dictionary<Vector2Int, Room>();
    private int currentSeed;
    private Room startRoom;
    private Room endRoom;

    public event System.Action OnGenerationComplete;

    public Room StartRoom => startRoom;
    public Room EndRoom => endRoom;
    public List<Room> GeneratedRooms => generatedRooms;

    private void Awake()
    {
        if (generationSeed != 0)
        {
            currentSeed = generationSeed;
        }
        else
        {
            currentSeed = System.DateTime.Now.Millisecond;
        }
        
        Random.InitState(currentSeed);
        Debug.Log($"Level generation seed: {currentSeed}");
    }

    private void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        ClearLevel();
        
        int targetRoomCount = Random.Range(minRooms, maxRooms + 1);
        
        GenerateRooms(targetRoomCount);
        ConnectRooms();
        DistributeItems();
        DistributeHidingSpots();
        SpawnPlayer();
        SpawnEnemy();

        OnGenerationComplete?.Invoke();
        Debug.Log($"Level generated with {generatedRooms.Count} rooms");
    }

    public void ClearLevel()
    {
        foreach (Room room in generatedRooms)
        {
            if (room != null)
            {
                Destroy(room.gameObject);
            }
        }

        generatedRooms.Clear();
        roomGrid.Clear();
        startRoom = null;
        endRoom = null;
    }

    #region Room Generation
    private void GenerateRooms(int targetCount)
    {
        startRoom = CreateRoom(startRoomPrefab, Vector3.zero, Room.RoomType.StartRoom);
        generatedRooms.Add(startRoom);
        roomGrid[Vector2Int.zero] = startRoom;

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(Vector2Int.zero);

        int roomsPlaced = 1;

        while (roomsPlaced < targetCount && frontier.Count > 0)
        {
            Vector2Int currentGridPos = frontier.Dequeue();

            List<Vector2Int> possibleDirections = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            ShuffleList(possibleDirections);

            foreach (Vector2Int dir in possibleDirections)
            {
                if (roomsPlaced >= targetCount) break;

                Vector2Int newGridPos = currentGridPos + dir;

                if (roomGrid.ContainsKey(newGridPos)) continue;

                if (Random.value > 0.7f && roomsPlaced < targetCount - 2) continue;

                if (roomsPlaced == targetCount - 1)
                {
                    Vector3 worldPos = GridToWorldPos(newGridPos);
                    endRoom = CreateRoom(endRoomPrefab, worldPos, Room.RoomType.EndRoom);
                    generatedRooms.Add(endRoom);
                    roomGrid[newGridPos] = endRoom;
                    roomsPlaced++;
                    break;
                }

                Room.RoomType roomType = SelectRandomRoomType();
                GameObject prefab = GetPrefabForType(roomType);
                
                if (prefab != null)
                {
                    Vector3 worldPos = GridToWorldPos(newGridPos);
                    Room newRoom = CreateRoom(prefab, worldPos, roomType);
                    generatedRooms.Add(newRoom);
                    roomGrid[newGridPos] = newRoom;
                    frontier.Enqueue(newGridPos);
                    roomsPlaced++;
                }
            }
        }

        if (endRoom == null && generatedRooms.Count > 1)
        {
            Debug.Log("No end room placed - need end room prefab");
        }
    }

    private Room CreateRoom(GameObject prefab, Vector3 position, Room.RoomType type)
    {
        GameObject roomObj = Instantiate(prefab, position, Quaternion.identity, transform);
        Room room = roomObj.GetComponent<Room>();
        
        if (room == null)
        {
            room = roomObj.AddComponent<Room>();
        }

        roomObj.name = $"{type}_Room_{generatedRooms.Count}";
        room.Initialize();

        return room;
    }

    private Vector3 GridToWorldPos(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * roomSpacing, 0f, gridPos.y * roomSpacing);
    }

    private Room.RoomType SelectRandomRoomType()
    {
        List<Room.RoomType> weightedTypes = new List<Room.RoomType>();

        foreach (RoomPrefabEntry entry in roomPrefabs)
        {
            int count = Mathf.RoundToInt(entry.weight * 10);
            for (int i = 0; i < count; i++)
            {
                weightedTypes.Add(entry.type);
            }
        }

        if (weightedTypes.Count == 0) return Room.RoomType.Corridor;

        return weightedTypes[Random.Range(0, weightedTypes.Count)];
    }

    private GameObject GetPrefabForType(Room.RoomType type)
    {
        foreach (RoomPrefabEntry entry in roomPrefabs)
        {
            if (entry.type == type) return entry.prefab;
        }
        return null;
    }
    #endregion

    #region Connect Rooms
    private void ConnectRooms()
    {
        // Room connections handled by door prefabs in rooms
        // This is a placeholder for more complex connection logic
    }
    #endregion

    #region Distribute Items
    private void DistributeItems()
    {
        List<Transform> availableSpawnPoints = new List<Transform>();
        
        foreach (Room room in generatedRooms)
        {
            if (room.Type != Room.RoomType.StartRoom)
            {
                foreach (Transform point in room.ItemSpawnPoints)
                {
                    availableSpawnPoints.Add(point);
                }
            }
        }

        ShuffleList(availableSpawnPoints);
        int spawnIndex = 0;

        // Place keys
        for (int i = 0; i < Random.Range(1, 4) && spawnIndex < availableSpawnPoints.Count; i++)
        {
            SpawnItem(keyPrefab, availableSpawnPoints[spawnIndex]);
            spawnIndex++;
        }

        // Place fuses
        for (int i = 0; i < Random.Range(1, 3) && spawnIndex < availableSpawnPoints.Count; i++)
        {
            SpawnItem(fusePrefab, availableSpawnPoints[spawnIndex]);
            spawnIndex++;
        }

        // Place essence
        for (int i = 0; i < Random.Range(5, 15) && spawnIndex < availableSpawnPoints.Count; i++)
        {
            SpawnItem(fearEssencePrefab, availableSpawnPoints[spawnIndex]);
            spawnIndex++;
        }
    }

    private void SpawnItem(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null) return;
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
    #endregion

    #region Hiding Spots
    private void DistributeHidingSpots()
    {
        foreach (Room room in generatedRooms)
        {
            if (room.Type == Room.RoomType.Bedroom || room.Type == Room.RoomType.Library)
            {
                foreach (Transform spawnPoint in room.HidingSpotSpawnPoints)
                {
                    if (Random.value < 0.5f && hidingSpotPrefabs.Length > 0)
                    {
                        GameObject prefab = hidingSpotPrefabs[Random.Range(0, hidingSpotPrefabs.Length)];
                        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                    }
                }
            }
        }
    }
    #endregion

    #region Spawn Player
    private void SpawnPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        
        if (player != null && startRoom != null)
        {
            player.transform.position = startRoom.transform.position + Vector3.up * 1f;
        }
    }
    #endregion

    #region Spawn Enemy
    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Room farthestRoom = null;
        float maxDistance = 0f;

        foreach (Room room in generatedRooms)
        {
            if (room.Type == Room.RoomType.StartRoom) continue;

            float distance = Vector3.Distance(room.transform.position, startRoom.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestRoom = room;
            }
        }

        if (farthestRoom != null)
        {
            Vector3 spawnPos = farthestRoom.transform.position;
            if (farthestRoom.EnemySpawnPoint != null)
            {
                spawnPos = farthestRoom.EnemySpawnPoint.position;
            }

            AIChaser enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.transform.parent = transform;
        }
    }
    #endregion

    #region Utilities
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        foreach (var kvp in roomGrid)
        {
            Vector3 pos = GridToWorldPos(kvp.Key);
            Gizmos.DrawWireCube(pos, new Vector3(roomSpacing - 1f, 4f, roomSpacing - 1f));
        }
    }
}
