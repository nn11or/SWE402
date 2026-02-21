using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float gravityModifier = 1.5f;

    [Header("Powerup")]
    public float powerupStrength = 15f;
    public float powerupDuration = 5f;
    public GameObject powerupIndicator;

    private Rigidbody rb;
    private GameObject focalPoint;
    private bool hasPowerup;
    public ParticleSystem powerupCollectFX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");

        Physics.gravity *= gravityModifier;

        if (powerupIndicator != null)
            powerupIndicator.SetActive(false);
    }

    void FixedUpdate()
    {
        float forwardInput = Input.GetAxis("Vertical");
        Vector3 forceDirection = focalPoint.transform.forward * forwardInput;
        rb.AddForce(forceDirection * speed);
    }

    void Update()
    {
        if (transform.position.y < -10f)
        {
            GameManager.RaiseGameOver();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            if (powerupIndicator != null) powerupIndicator.SetActive(true);
            if (powerupCollectFX != null) powerupCollectFX.Play();
            Destroy(other.gameObject);
            StartCoroutine(PowerupCountdown());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasPowerup && collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            if (enemyRb == null) return;

            Vector3 awayFromPlayer = (collision.transform.position - transform.position).normalized;
            enemyRb.AddForce(awayFromPlayer * powerupStrength, ForceMode.Impulse);
        }
    }

    IEnumerator PowerupCountdown()
    {
        yield return new WaitForSeconds(powerupDuration);
        hasPowerup = false;
        if (powerupIndicator != null) powerupIndicator.SetActive(false);
    }
}