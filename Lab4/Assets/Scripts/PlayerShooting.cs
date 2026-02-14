using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public BulletPool pool;
    public Transform firePoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (pool == null) return;

        GameObject b = pool.GetBullet();
        if (b == null) return;

        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + transform.forward * 1f;
        b.transform.position = spawnPos;
        b.transform.rotation = transform.rotation;

        
        Bullet bulletScript = b.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.SetPool(pool);
    }
}