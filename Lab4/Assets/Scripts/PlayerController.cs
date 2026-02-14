using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 100f;

    private Rigidbody rb;
    private float moveInput;
    private float rotationInput;
    public Transform arena;   
    public float margin = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        rotationInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
    // Rotate
    float rotation = rotationInput * rotationSpeed * Time.fixedDeltaTime;
    Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
    rb.MoveRotation(rb.rotation * turn);

    // Next position
    Vector3 nextPos = rb.position + transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;

    if (arena != null)
    {
        // Plane size in Unity is 10x10 by default, scaled by localScale
        float halfWidth  = arena.localScale.x * 10f * 0.5f;
        float halfDepth  = arena.localScale.z * 10f * 0.5f;

        nextPos.x = Mathf.Clamp(nextPos.x, -halfWidth + margin, halfWidth - margin);
        nextPos.z = Mathf.Clamp(nextPos.z, -halfDepth + margin, halfDepth - margin);
    }

    rb.MovePosition(nextPos);
    }
}