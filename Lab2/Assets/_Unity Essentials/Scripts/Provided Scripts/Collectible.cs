using UnityEngine;

public class Collectible : MonoBehaviour
{
    // Rotation speed (editable from Inspector)
    public float rotationSpeed = 30f;

    // Reference to the VFX prefab (drag your particle prefab here in the Inspector)
    public GameObject collectVFX;

    void Update()
    {
        // Rotate the object continuously
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
    }

    // Detect when the player enters the trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Spawn the visual effect if one is assigned
            if (collectVFX != null)
            {
                Instantiate(collectVFX, transform.position, transform.rotation);
            }

            // Destroy the collectible object
            Destroy(gameObject);
        }
    }
}