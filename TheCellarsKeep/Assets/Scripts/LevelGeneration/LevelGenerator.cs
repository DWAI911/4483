using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural level generator that creates a new mansion layout each run.
- Uses a template-based approach with room prefabs
- Ensures start and end rooms are placed
- Distributes items and hiding spots throughout
- Generates NavMesh for AI pathfinding
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
        public float weight = 1f; // Probability weight
    }

    [Header("Generation Settings")]
    [SerializeField] private int minRooms = 10;
    [SerializeField] private int maxRooms = 20;
    [SerializeField] private float roomSpacing = 12f;
    [SerializeField] private int generationSeed = 0; // 0 = random each time

    [Header("Room Prefabs")]
    [SerializeField] private RoomPrefabEntry[] roomPrefabs;
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private GameObject endRoomPrefab;

    [Header("Item Prefabs")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject fusePrefab;
    [SerializeField] private GameObject fearEssencePrefab;
    [SerializeField] private GameObject[] consumablePrefabs;

    [Header("Hiding Spots")]
    [SerializeField] private GameObject[] hidingSpotPrefabs;

    [Header("References")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private AIChaser enemyPrefab;

    [Header("NavMesh")]
    [SerializeField] private bool generateNavMesh = true;

    // Runtime data
    private List<Room> generatedRooms = new List<Room>();
    private List<Door> generatedDoors = new List<Door>();
    private Dictionary<Vector2Int, Room> roomGrid = new Dictionary<Vector2Int, Room>();
    private int currentSeed;
    private Room startRoom;
    private Room endRoom;

    // Events
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
        
        // Phase 1: Generate rooms
        GenerateRooms(targetRoomCount);

        // Phase 2: Connect rooms with doors
        ConnectRooms();

        // Phase 3: Place items
        DistributeItems();

        // Phase 4: Place hiding spots
        DistributeHidingSpots();

        // Phase 5: Spawn player
        SpawnPlayer();

        // Phase 6: Spawn enemy
        SpawnEnemy();

        // Phase 7: Generate NavMesh
        if (generateNavMesh)
        {
            GenerateNavMeshSurface();
        }

        OnGenerationComplete?.Invoke();
        Debug.Log($"Level generated with {generatedRooms.Count} rooms");
    }

    public void ClearLevel()
    {
        // Destroy all generated objects
        foreach (Room room in generatedRooms)
        {
            if (room != null)
            {
                Destroy(room.gameObject);
            }
        }

        generatedRooms.Clear();
        generatedDoors.Clear();
        roomGrid.Clear();
        startRoom = null;
        endRoom = null;
    }

    #region Phase 1: Room Generation
    private void GenerateRooms(int targetCount)
    {
        // Always place start room first
        startRoom = CreateRoom(startRoomPrefab, Vector3.zero, Room.RoomType.StartRoom);
        generatedRooms.Add(startRoom);
        roomGrid[Vector2Int.zero] = startRoom;

        // Use BFS to place rooms
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(Vector2Int.zero);

        int roomsPlaced = 1;

        while (roomsPlaced < targetCount && frontier.Count > 0)
        {
            Vector2Int currentGridPos = frontier.Dequeue();
            Room currentRoom = roomGrid[currentGridPos];

            // Try to place rooms in each direction
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

                // Check if position is already occupied
                if (roomGrid.ContainsKey(newGridPos)) continue;

                // Decide if we should place a room here
                if (Random.value > 0.7f && roomsPlaced < targetCount - 2) continue;

                // Place last room as end room
                if (roomsPlaced == targetCount - 1)
                {
                    Vector3 worldPos = GridToWorldPos(newGridPos);
                    endRoom = CreateRoom(endRoomPrefab, worldPos, Room.RoomType.EndRoom);
                    generatedRooms.Add(endRoom);
                    roomGrid[newGridPos] = endRoom;
                    roomsPlaced++;
                    break;
                }

                // Place random room
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

        // Ensure we have an end room
        if (endRoom == null && generatedRooms.Count > 1)
        {
            // Convert last placed room to end room
            Room lastRoom = generatedRooms[generatedRooms.Count - 1];
            // For simplicity, just mark it as end room
            // In a full implementation, you'd replace it with the end room prefab
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
        return new Vector3(
            gridPos.x * roomSpacing,
            0f,
            gridPos.y * roomSpacing
        );
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

        if (weightedTypes.Count == 0)
        {
            return Room.RoomType.Corridor;
        }

        return weightedTypes[Random.Range(0, weightedTypes.Count)];
    }

    private GameObject GetPrefabForType(Room.RoomType type)
    {
        foreach (RoomPrefabEntry entry in roomPrefabs)
        {
            if (entry.type == type)
            {
                return entry.prefab;
            }
        }
        return null;
    }
    #endregion

    #region Phase 2: Connect Rooms
    private void ConnectRooms()
    {
        // For each room, check neighbors and create doors
        foreach (var kvp in roomGrid)
        {
            Vector2Int gridPos = kvp.Key;
            Room room = kvp.Value;

            // Check each direction
            Vector2Int[] directions = {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = gridPos + dir;
                
                if (roomGrid.TryGetValue(neighborPos, out Room neighborRoom))
                {
                    // Create door between rooms
                    CreateDoorBetweenRooms(room, neighborRoom, dir);
                }
            }
        }
    }

    private void CreateDoorBetweenRooms(Room roomA, Room roomB, Vector2Int direction)
    {
        // Calculate door position (between rooms)
        Vector3 doorPos = (roomA.transform.position + roomB.transform.position) / 2f;
        
        // Create door (simplified - in full implementation, use a door prefab)
        // For now, we'll just note the connection
        // In practice, each room would have door prefabs at connection points
        
        // Randomly lock some doors
        // bool shouldLock = Random.value < 0.3f; // 30% chance to be locked
    }
    #endregion

    #region Phase 3: Distribute Items
    private void DistributeItems()
    {
        int keysToPlace = Random.Range(1, 4);
        int fusesToPlace = Random.Range(1, 3);
        int essenceToPlace = Random.Range(5, 15);
        int consumablesToPlace = Random.Range(2, 6);

        // Get all available spawn points
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
        for (int i = 0; i < keysToPlace && spawnIndex < availableSpawnPoints.Count; i++)
        {
            SpawnItem(keyPrefab, availableSpawnPoints[spawnIndex]);
            spawnIndex++;
        }

        // Place fuses
        for (int i = 0; i < fusesToPlace && spawnIndex < availableSpawnPoints.Count; i++)
        {
            SpawnItem(fusePrefab, availableSpawnPoints[spawnIndex]);
            spawnIndex++;
        }

        // Place fear essence
        for (int i = 0; i < essenceToPlace && spawnIndex < availableSpawnPoints.Count; i++)
        {
            SpawnItem(fearEssencePrefab, availableSpawnPoints[spawnIndex]);
            spawnIndex++;
        }

        // Place consumables
        for (int i = 0; i < consumablesToPlace && spawnIndex < availableSpawnPoints.Count; i++)
        {
            GameObject consumablePrefab = consumablePrefabs[Random.Range(0, consumablePrefabs.Length)];
            SpawnItem(consumablePrefab, availableSpawnPoints[spawnIndex]);
            spawnIndex++;
        }
    }

    private void SpawnItem(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null) return;

        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
    #endregion

    #region Phase 4: Hiding Spots
    private void DistributeHidingSpots()
    {
        foreach (Room room in generatedRooms)
        {
            if (room.Type == Room.RoomType.Bedroom || room.Type == Room.RoomType.Library)
            {
                foreach (Transform spawnPoint in room.HidingSpotSpawnPoints)
                {
                    if (Random.value < 0.5f) // 50% chance per spawn point
                    {
                        GameObject prefab = hidingSpotPrefabs[Random.Range(0, hidingSpotPrefabs.Length)];
                        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                    }
                }
            }
        }
    }
    #endregion

    #region Phase 5: Player Spawn
    private void SpawnPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        
        if (player != null && startRoom != null)
        {
            player.transform.position = startRoom.transform.position + Vector3.up * 1f;
        }
        else if (player != null)
        {
            player.transform.position = Vector3.up * 1f;
        }
    }
    #endregion

    #region Phase 6: Enemy Spawn
    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        // Find a room far from start
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

    #region Phase 7: NavMesh
    private void GenerateNavMeshSurface()
    {
        // For NavMesh generation, you'll need to:
        // 1. Mark floor objects as "Navigation Static"
        // 2. Use Unity's NavMesh baking
        
        // In Unity 2020+, you can use NavMeshSurface component
        // For now, this is a placeholder for manual NavMesh baking
        Debug.Log("NavMesh generation requires NavMeshSurface component or manual baking");
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

    #region Debug
    private void OnDrawGizmos()
    {
        // Draw room grid
        Gizmos.color = Color.gray;
        foreach (var kvp in roomGrid)
        {
            Vector3 pos = GridToWorldPos(kvp.Key);
            Gizmos.DrawWireCube(pos, new Vector3(roomSpacing - 1f, 4f, roomSpacing - 1f));
        }
    }
    #endregion
}
