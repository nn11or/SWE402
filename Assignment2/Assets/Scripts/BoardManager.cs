using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;

        public CellData(bool passable)
        {
            Passable = passable;
            ContainedObject = null;
        }
    }

    [SerializeField, Tooltip("Width of the game board")]
    private int boardWidth = 8;
    [SerializeField, Tooltip("Height of the game board")]
    private int boardHeight = 8;

    [SerializeField, Tooltip("Ground tiles to randomly pick from")]
    private Tile[] groundTiles;
    [SerializeField, Tooltip("Wall tiles for edges and obstacles")]
    private Tile[] wallTiles;

    [SerializeField, Tooltip("Tilemap to paint ground on")]
    private Tilemap groundTilemap;
    [SerializeField, Tooltip("Tilemap to paint walls on")]
    private Tilemap wallTilemap;

    [SerializeField, Tooltip("Food prefabs with different AmountGranted values")]
    private FoodObject[] foodPrefabs;
    [SerializeField, Range(1, 10), Tooltip("Min food items per level")]
    private int minFoodCount = 1;
    [SerializeField, Range(1, 10), Tooltip("Max food items per level")]
    private int maxFoodCount = 5;

    [SerializeField, Tooltip("Wall obstacle prefab")]
    private WallObject wallPrefab;
    [SerializeField, Range(1, 15), Tooltip("Min wall obstacles per level")]
    private int minWallCount = 3;
    [SerializeField, Range(1, 15), Tooltip("Max wall obstacles per level")]
    private int maxWallCount = 8;

    [SerializeField, Tooltip("Enemy prefab")]
    private Enemy enemyPrefab;
    [SerializeField, Range(1, 10), Tooltip("Min enemies per level")]
    private int minEnemyCount = 1;
    [SerializeField, Range(1, 10), Tooltip("Max enemies per level")]
    private int maxEnemyCount = 3;

    [SerializeField, Tooltip("Exit cell prefab")]
    private ExitCellObject exitPrefab;

    private CellData[,] boardData;
    private ObjectPool objectPool;

    public int BoardWidth => boardWidth;
    public int BoardHeight => boardHeight;

    public void Init()
    {
        objectPool = FindFirstObjectByType<ObjectPool>();
        GenerateBoard();
    }

    public void Clean()
    {
        if (groundTilemap != null) groundTilemap.ClearAllTiles();
        if (wallTilemap != null) wallTilemap.ClearAllTiles();

        if (boardData == null) return;

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (boardData[x, y] != null && boardData[x, y].ContainedObject != null)
                {
                    var obj = boardData[x, y].ContainedObject;
                    boardData[x, y].ContainedObject = null;

                    if (objectPool != null)
                        objectPool.ReturnToPool(obj);
                    else
                        Destroy(obj.gameObject);
                }
            }
        }
    }

    private void GenerateBoard()
    {
        boardData = new CellData[boardWidth, boardHeight];

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                bool isEdge = (x == 0 || x == boardWidth - 1 || y == 0 || y == boardHeight - 1);

                if (isEdge)
                {
                    boardData[x, y] = new CellData(false);
                    if (wallTiles != null && wallTiles.Length > 0 && wallTilemap != null)
                    {
                        Tile tile = wallTiles[Random.Range(0, wallTiles.Length)];
                        wallTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }
                else
                {
                    boardData[x, y] = new CellData(true);
                }

                if (groundTiles != null && groundTiles.Length > 0 && groundTilemap != null)
                {
                    Tile tile = groundTiles[Random.Range(0, groundTiles.Length)];
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        SpawnExit();
        SpawnObjects();
    }

    private void SpawnExit()
    {
        int exitX = boardWidth - 2;
        int exitY = boardHeight - 2;

        if (exitPrefab != null)
        {
            ExitCellObject exit;
            if (objectPool != null)
                exit = objectPool.GetFromPool(exitPrefab) as ExitCellObject;
            else
                exit = Instantiate(exitPrefab);

            exit.transform.position = CellToWorld(exitX, exitY);
            exit.gameObject.SetActive(true);
            exit.Init(exitX, exitY);
            boardData[exitX, exitY].ContainedObject = exit;
        }
    }

    private void SpawnObjects()
    {
        int foodCount = Random.Range(minFoodCount, maxFoodCount + 1);
        int wallCount = Random.Range(minWallCount, maxWallCount + 1);
        int enemyCount = Random.Range(minEnemyCount, maxEnemyCount + 1);

        SpawnCellObjects(foodPrefabs, foodCount);

        if (wallPrefab != null)
            SpawnCellObjects(new CellObject[] { wallPrefab }, wallCount);

        if (enemyPrefab != null)
            SpawnCellObjects(new CellObject[] { enemyPrefab }, enemyCount);
    }

    private void SpawnCellObjects(CellObject[] prefabs, int count)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = 100;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;
            int x = Random.Range(1, boardWidth - 1);
            int y = Random.Range(1, boardHeight - 1);

            // Don't spawn on player start (1,1) or exit cell or occupied cells
            if ((x == 1 && y == 1) || boardData[x, y].ContainedObject != null || !boardData[x, y].Passable)
                continue;

            CellObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            CellObject obj;

            if (objectPool != null)
                obj = objectPool.GetFromPool(prefab);
            else
                obj = Instantiate(prefab);

            obj.transform.position = CellToWorld(x, y);
            obj.gameObject.SetActive(true);
            obj.Init(x, y);
            boardData[x, y].ContainedObject = obj;
            spawned++;
        }
    }

    public CellData GetCellData(int x, int y)
    {
        if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight)
            return null;
        return boardData[x, y];
    }

    public void SetCellObject(int x, int y, CellObject obj)
    {
        if (x >= 0 && x < boardWidth && y >= 0 && y < boardHeight)
            boardData[x, y].ContainedObject = obj;
    }

    public Vector3 CellToWorld(int x, int y)
    {
        return new Vector3(x + 0.5f, y + 0.5f, 0f);
    }
}
