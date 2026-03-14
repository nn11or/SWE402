using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public variables so you can adjust them in the Inspector
    public float speed = 10.0f;
    public float rotationSpeed = 100.0f;

    private Rigidbody rb;
    private float moveInput;
    private float rotateInput;

    void Start()
    {
        // Get the Rigidbody component attached to this character
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Read input from WASD or Arrow keys
        moveInput = Input.GetAxis("Vertical");
        rotateInput = Input.GetAxis("Horizontal");
    }

    // FixedUpdate is used for physics calculations
    void FixedUpdate()
    {
        // Move the character forward or backward
        Vector3 movement = transform.forward * moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // Rotate the character left or right
        float turn = rotateInput * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}
