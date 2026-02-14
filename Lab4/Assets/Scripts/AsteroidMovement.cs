using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    public float speed = 5f;
    public float extraMargin = 2f;

    float halfWidth = 25f;
    float halfDepth = 25f;

    void Start()
    {
        GameObject arenaObj = GameObject.Find("Arena");
        if (arenaObj != null)
        {
            Vector3 s = arenaObj.transform.localScale;
            halfWidth = s.x * 10f * 0.5f;
            halfDepth = s.z * 10f * 0.5f;
        }
    }

    void Update()
    {
        Vector3 dir = transform.forward;
        dir.y = 0f;
        dir = dir.normalized;

        transform.position += dir * speed * Time.deltaTime;

        if (transform.position.x > halfWidth + extraMargin ||
            transform.position.x < -halfWidth - extraMargin ||
            transform.position.z > halfDepth + extraMargin ||
            transform.position.z < -halfDepth - extraMargin)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
{
       if (!other.CompareTag("Player")) return;

    SpawnManager sm = FindObjectOfType<SpawnManager>();
    if (sm != null && !sm.gameOver)
    {
        Debug.Log("Game Over");
        sm.StopSpawning();

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        PlayerShooting ps = other.GetComponent<PlayerShooting>();
        if (ps != null) ps.enabled = false;
    }

    Destroy(gameObject);
}
}