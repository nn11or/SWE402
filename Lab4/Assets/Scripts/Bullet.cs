using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float arenaLimit = 50f;

    private BulletPool pool;

    public void SetPool(BulletPool p)
    {
        pool = p;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        
        if (Mathf.Abs(transform.position.x) > arenaLimit || 
            Mathf.Abs(transform.position.z) > arenaLimit)
        {
            if (pool != null)
                pool.ReturnBullet(gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       if (other.CompareTag("Player")) return;

    if (other.CompareTag("Asteroid"))
    {
        Destroy(other.gameObject);

        if (pool != null)
            pool.ReturnBullet(gameObject);
        else
            gameObject.SetActive(false);
    }
    }
}