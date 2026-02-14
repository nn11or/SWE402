using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] asteroidPrefabs;
    public Transform arena;
    public bool gameOver { get; private set; }

    public float startSpawnInterval = 2.0f;
    public float minSpawnInterval = 0.6f;

    public float startSpeedMultiplier = 1.0f;
    public float maxSpeedMultiplier = 2.2f;

    float halfWidth = 25f;
    float halfDepth = 25f;

    float currentInterval;
    float currentSpeedMult;

    Coroutine spawnRoutine;

    void Start()
    {
        if (arena != null)
        {
            Vector3 s = arena.localScale;
            halfWidth = s.x * 10f * 0.5f;
            halfDepth = s.z * 10f * 0.5f;
        }

        currentInterval = startSpawnInterval;
        currentSpeedMult = startSpeedMultiplier;

        spawnRoutine = StartCoroutine(SpawnLoop());
        StartCoroutine(DifficultyLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(currentInterval);
        }
    }

    void SpawnOne()
    {
        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0) return;

        int index = Random.Range(0, asteroidPrefabs.Length);
        GameObject prefab = asteroidPrefabs[index];

        Vector3 pos = GetRandomEdgePosition();
        Quaternion rot = GetRotationTowardCenter(pos);

        GameObject a = Instantiate(prefab, pos, rot);

        AsteroidMovement m = a.GetComponent<AsteroidMovement>();
        if (m != null)
        {
            m.speed *= currentSpeedMult;
        }
    }

    Vector3 GetRandomEdgePosition()
    {
        float x, z;
        int edge = Random.Range(0, 4);

        if (edge == 0)
        {
            x = Random.Range(-halfWidth, halfWidth);
            z = halfDepth + 1f;
        }
        else if (edge == 1)
        {
            x = Random.Range(-halfWidth, halfWidth);
            z = -halfDepth - 1f;
        }
        else if (edge == 2)
        {
            x = halfWidth + 1f;
            z = Random.Range(-halfDepth, halfDepth);
        }
        else
        {
            x = -halfWidth - 1f;
            z = Random.Range(-halfDepth, halfDepth);
        }

        return new Vector3(x, 0.5f, z);
    }

    Quaternion GetRotationTowardCenter(Vector3 fromPos)
    {
        Vector3 center = Vector3.zero;
        Vector3 dir = (center - fromPos);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            dir = Vector3.forward;

        return Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    IEnumerator DifficultyLoop()
    {
        float t = 0f;

        while (true)
        {
            yield return new WaitForSeconds(5f);
            t += 5f;

            currentInterval = Mathf.Max(minSpawnInterval, currentInterval - 0.1f);
            currentSpeedMult = Mathf.Min(maxSpeedMultiplier, currentSpeedMult + 0.05f);
        }
    }

  public void StopSpawning()
{
    if (gameOver) return;
    gameOver = true;

    if (spawnRoutine != null)
        StopCoroutine(spawnRoutine);

    StopAllCoroutines();
} 
}