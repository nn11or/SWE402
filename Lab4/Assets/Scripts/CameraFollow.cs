using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;      // Reference to player
    public Vector3 offset;        // Offset from player

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}