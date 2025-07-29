using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// We create a class to hold a complete set of tiles. This keeps the Inspector clean.
[System.Serializable]
public class TileSet
{
    [Header("Main Tiles")]
    [Tooltip("The black void outside the dungeon.")]
    public TileBase empty;
    [Tooltip("Floor tiles. Will be chosen randomly from this list.")]
    public TileBase[] floor;

    [Header("Straight Wall Tiles (Will alternate if more than one)")]
    public TileBase[] wallTop;
    public TileBase[] wallTopUnder;
    public TileBase[] wallBottom;
    public TileBase[] wallSideLeft;
    public TileBase[] wallSideRight;

    [Header("Inner Corner Tiles (Intersections)")]
    public TileBase[] wallInnerTopLeft;
    public TileBase[] wallInnerTopRight;
    public TileBase[] wallInnerBottomLeft;
    public TileBase[] wallInnerBottomRight;

    [Header("Outer Corner Tiles (Room Corners)")]
    public TileBase[] wallOuterTopLeft;
    public TileBase[] wallOuterTopRight;
    public TileBase[] wallOuterBottomLeft;
    public TileBase[] wallOuterBottomRight;

    
}

public class DungeonGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 50;
    public int height = 50;

    [Header("Seed Settings")]
    [Tooltip("The seed used for generation. Same seed = same map.")]
    public string seed = "KnightsDescent";
    [Tooltip("If true, a random seed is used every time, ignoring the string above.")]
    public bool useRandomSeed = true;

    [Header("Tilemap Settings")]
    public Tilemap groundTilemap;

    // --- MODIFIED: We now have two TileSet objects to hold all our tile variations ---
    [Header("Tile Sets")]
    public TileSet normalTileSet;
    public TileSet specialRoomTileSet;

    public static int MapWidth { get; private set; }
    public static int MapHeight { get; private set; }

    [Header("Entities & Room Prefabs")]
    public GameObject playerPrefab;
    public GameObject weapon10Prefab, weapon25Prefab, weapon50Prefab;
    public GameObject pickaxePrefab;
    public GameObject exitDoorPrefab;
    public GameObject spawnerPrefab;
    public GameObject bossSpawnerPrefab;

    [Header("Room Settings")]
    public int maxRooms = 8;
    public int roomMinSize = 4;
    public int roomMaxSize = 8;

    public static int[,] grid;
    private List<Room> rooms = new List<Room>();
    private System.Random rng;

    public static DungeonGenerator Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        // Initialize the random number generator based on the seed settings
        if (useRandomSeed)
        {
            // Use a time-based seed for a different result every time
            rng = new System.Random();
        }
        else
        {
            // Use the hash code of the seed string for a consistent result
            rng = new System.Random(seed.GetHashCode());
        }
    }

    void Start()
    {
        // Safety check to ensure you've assigned the tilemaps and tilesets in the inspector
        if (groundTilemap == null || normalTileSet == null || specialRoomTileSet == null)
        {
            Debug.LogError("DungeonGenerator is missing critical Tilemap or TileSet references! Please assign them in the Inspector.");
            return;
        }

        GenerateMapData();
        RenderMapWithAdvancedTiling();
    }

    void GenerateMapData()
    {
        MapWidth = width;
        MapHeight = height;
        grid = new int[width, height];
        rooms.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = 0; // 0 = Wall
            }
        }

        for (int i = 0; i < maxRooms; i++)
        {
            int w = rng.Next(roomMinSize, roomMaxSize + 1);
            int h = rng.Next(roomMinSize, roomMaxSize + 1);
            int x = rng.Next(1, width - w - 1);
            int y = rng.Next(1, height - h - 1);
            Room newRoom = new Room(x, y, w, h);
            bool overlaps = false;
            foreach (Room other in rooms)
            {
                if (newRoom.Overlaps(other)) { overlaps = true; break; }
            }
            if (overlaps) continue;
            CarveRoom(newRoom);
            rooms.Add(newRoom);
        }

        for (int i = rooms.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (rooms[i], rooms[j]) = (rooms[j], rooms[i]);
        }

        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int prevCenter = rooms[i - 1].Center;
            Vector2Int currCenter = rooms[i].Center;
            CarveHTunnel(prevCenter.x, currCenter.x, prevCenter.y);
            CarveVTunnel(prevCenter.y, currCenter.y, currCenter.x);
        }

        PlaceSpecialRoomsAndItems();
    }

    void CarveRoom(Room room)
    {
        for (int x = room.x; x < room.x + room.w; x++)
        {
            for (int y = room.y; y < room.y + room.h; y++)
            {
                grid[x, y] = 1;
            }
        }
    }

    void CarveHTunnel(int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            grid[x, y] = 1; // Bottom row
            if (y + 1 < height)
            {
                grid[x, y + 1] = 1; // Middle row
            }
            if (y + 2 < height) 
            {
                grid[x, y + 2] = 1; // Top row
            }
        }
    }

    void CarveVTunnel(int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            grid[x, y] = 1; // Left column
            if (x + 1 < width)
            {
                grid[x + 1, y] = 1; // Middle column
            }
            if (x + 2 < width)
            {
                grid[x + 2, y] = 1; // Right column
            }
        }
    }

    void RenderMapWithAdvancedTiling()
    {
        groundTilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RenderTileAt(x, y);
            }
        }
    }

    // This is the new "brain" that decides which tile to place
    void RenderTileAt(int x, int y)
    {
        TileSet set = IsInSpecialRoom(x, y) ? specialRoomTileSet : normalTileSet;
        TileBase tile = null;

        if (grid[x, y] == 1) // This is a FLOOR tile
        {
            if (IsWall(x, y + 1))
            {
                tile = SelectTileFromArray(set.wallTopUnder, x); // Alternating under-wall
            }
            else
            {
                tile = SelectTileFromArray(set.floor, -1); // Random floor
            }
        }
        else // This is a WALL tile
        {
            // Check the 8 neighbors
            bool N = IsFloor(x, y + 1);
            bool S = IsFloor(x, y - 1);
            bool W = IsFloor(x - 1, y);
            bool E = IsFloor(x + 1, y);
            bool NW = IsFloor(x - 1, y + 1);
            bool NE = IsFloor(x + 1, y + 1);
            bool SW = IsFloor(x - 1, y - 1);
            bool SE = IsFloor(x + 1, y - 1);


            // Check for Outer Corners first (most specific cases)
            if (IsEmpty(x, y + 1) && IsEmpty(x - 1, y) && IsWallTop(x + 1, y) && IsLeftSideWall(x, y - 1)) tile = SelectTileFromArray(set.wallOuterTopLeft, x);
            else if (IsEmpty(x, y + 1) && IsEmpty(x + 1, y) && IsWallTop(x - 1, y) && IsRightSideWall(x, y - 1)) tile = SelectTileFromArray(set.wallOuterTopRight, x);
            else if (IsEmpty(x - 1, y) && IsEmpty(x, y - 1) && IsLeftSideWall(x, y + 1) && IsWallBottom(x + 1, y)) tile = SelectTileFromArray(set.wallOuterBottomLeft, x);
            else if (IsEmpty(x + 1, y) && IsEmpty(x, y - 1) && IsRightSideWall(x, y + 1) && IsWallBottom(x - 1, y)) tile = SelectTileFromArray(set.wallOuterBottomRight, x);
            // Then check for Inner Corners
            else if (S && W) tile = SelectTileFromArray(set.wallInnerTopRight, x);
            else if (S && E) tile = SelectTileFromArray(set.wallInnerTopLeft, x);
            else if (N && W) tile = SelectTileFromArray(set.wallInnerBottomRight, x);
            else if (N && E) tile = SelectTileFromArray(set.wallInnerBottomLeft, x);
            // Then check for Straight Walls
            else if (S) tile = SelectTileFromArray(set.wallTop, x);
            else if (N) tile = SelectTileFromArray(set.wallBottom, x);
            else if (W) tile = SelectTileFromArray(set.wallSideRight, y);
            else if (E) tile = SelectTileFromArray(set.wallSideLeft, y);
            // Otherwise, it's empty space
            else tile = set.empty;
        }
        groundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    // Helper function to select a tile from an array for variation
    TileBase SelectTileFromArray(TileBase[] array, int coordinate)
    {
        if (array == null || array.Length == 0)
        {
            return null;
        }
        if (array.Length == 1)
        {
            return array[0];
        }

        // If coordinate is -1, pick randomly (for floors). Otherwise, alternate.
        if (coordinate == -1)
        {
            return array[rng.Next(0, array.Length)];
        }
        else
        {
            return array[Mathf.Abs(coordinate) % array.Length];
        }
    }

    public void MineAt(int x, int y)
    {
        if (!IsWithinBounds(x, y) || grid[x, y] != 0)
        {
            return;
        }
        grid[x, y] = 1;
        // Re-render the mined tile and its 8 neighbors to update their appearance
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (IsWithinBounds(i, j))
                {
                    RenderTileAt(i, j);
                }
            }
        }
    }

    // --- Helper Functions to make logic cleaner ---
    bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    bool IsFloor(int x, int y)
    {
        return IsWithinBounds(x, y) && grid[x, y] == 1;
    }

    bool IsEmpty(int x, int y)
    {
        return !IsFloor(x, y);
    }

    bool IsWall(int x, int y)
    {
        return IsWithinBounds(x, y) && grid[x, y] == 0;
    }

    // --- ADD THESE NEW FUNCTIONS HERE ---

    // Returns true if the tile at (x, y) is a wall AND has a floor directly below it.
    bool IsWallTop(int x, int y)
    {
        return IsWall(x, y) && IsFloor(x, y - 1);
    }

    // Returns true if the tile at (x, y) is a wall AND has a floor directly above it.
    bool IsWallBottom(int x, int y)
    {
        return IsWall(x, y) && IsFloor(x, y + 1);
    }

    // Returns true if the tile at (x, y) is a wall AND has a floor to its right.
    bool IsLeftSideWall(int x, int y)
    {
        return IsWall(x, y) && IsFloor(x + 1, y);
    }

    // Returns true if the tile at (x, y) is a wall AND has a floor to its left.
    bool IsRightSideWall(int x, int y)
    {
        return IsWall(x, y) && IsFloor(x - 1, y);
    }

    public bool IsWalkable(int x, int y)
    {
        return IsFloor(x, y);
    }

    bool IsInSpecialRoom(int x, int y)
    {
        foreach (var room in rooms)
        {
            if ((room.type == RoomType.Spawn || room.type == RoomType.BossSpawn) && (x >= room.x && x < room.x + room.w && y >= room.y && y < room.y + room.h))
            {
                return true;
            }
        }
        return false;
    }

    void PlaceSpecialRoomsAndItems()
    {
        if (rooms.Count < 2)
        {
            return;
        }
        rooms[0].type = RoomType.Start;
        rooms[rooms.Count - 1].type = RoomType.Exit;
        int spawnIndex = rng.Next(1, rooms.Count - 1);
        rooms[spawnIndex].type = RoomType.Spawn;
        int bossIndex;
        do
        {
            bossIndex = rng.Next(1, rooms.Count - 1);
        } while (bossIndex == spawnIndex);
        rooms[bossIndex].type = RoomType.BossSpawn;

        foreach (var room in rooms)
        {
            Vector3 worldPos = new Vector3(room.Center.x + 0.5f, room.Center.y + 0.5f, 0);
            switch (room.type)
            {
                case RoomType.Start:
                    Instantiate(playerPrefab, worldPos, Quaternion.identity);
                    break;
                case RoomType.Exit:
                    Instantiate(exitDoorPrefab, worldPos, Quaternion.identity);
                    break;
                case RoomType.Spawn:
                    Instantiate(spawnerPrefab, worldPos, Quaternion.identity);
                    break;
                case RoomType.BossSpawn:
                    Instantiate(bossSpawnerPrefab, worldPos, Quaternion.identity);
                    break;
            }
        }
        SpawnItemRandomly(weapon10Prefab);
        SpawnItemRandomly(weapon25Prefab);
        SpawnItemRandomly(weapon50Prefab);
        SpawnItemRandomly(pickaxePrefab);
    }

    private void SpawnItemRandomly(GameObject itemPrefab)
    {
        if (itemPrefab == null)
        {
            return;
        }
        for (int i = 0; i < 50; i++)
        {
            int x = rng.Next(0, width);
            int y = rng.Next(0, height);
            if (grid[x, y] == 1)
            {
                Instantiate(itemPrefab, new Vector3(x + 0.5f, y + 0.5f, -0.1f), Quaternion.identity);
                return;
            }
        }
        Debug.LogWarning($"Failed to place {itemPrefab.name}");
    }

    private class Room
    {
        public int x, y, w, h;
        public RoomType type = RoomType.Normal;
        public Vector2Int Center => new Vector2Int(x + w / 2, y + h / 2);
        public Room(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }
        public bool Overlaps(Room other)
        {
            return (x < other.x + other.w && x + w > other.x && y < other.y + other.h && y + h > other.y);
        }
    }

    private enum RoomType
    {
        Normal,
        Start,
        Exit,
        Spawn,
        BossSpawn
    }
}
