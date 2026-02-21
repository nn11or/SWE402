using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;

    [Header("Wave Settings")]
    public int waveNumber = 1;

    [Header("Arena Reference")]
    public Transform arena;
    public float edgePadding = 1.5f;

    private bool gameOver;

    void OnEnable()
    {
        GameManager.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
    }

    void Start()
    {
        SpawnEnemyWave(waveNumber);
        SpawnPowerup();
    }

    void Update()
    {
        if (gameOver) return;

        int enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        if (enemyCount == 0)
        {
            waveNumber++;
            SpawnEnemyWave(waveNumber);
            SpawnPowerup();
        }
    }

    public void SpawnEnemyWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        }
    }

    public void SpawnPowerup()
    {
        Debug.Log("Spawning powerup at " + GenerateSpawnPosition());
        Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
    }

    private Vector3 GenerateSpawnPosition()
    {
      float arenaRadius = 8f;
    float arenaY = 2f;
    Vector3 center = Vector3.zero;

    if (arena != null)
    {
        Renderer r = arena.GetComponent<Renderer>();
        if (r != null)
        {
            arenaRadius = Mathf.Min(r.bounds.extents.x, r.bounds.extents.z) - edgePadding;
        }

        arenaY = arena.position.y;
        center = arena.position;  
    }

    Vector2 circle = Random.insideUnitCircle * arenaRadius;
    float spawnY = arenaY + 0.5f;

    return new Vector3(center.x + circle.x, spawnY, center.z + circle.y);
    }

    private void HandleGameOver()
    {
        gameOver = true;
    }
}