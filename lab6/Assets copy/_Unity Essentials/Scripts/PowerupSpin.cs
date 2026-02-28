using UnityEngine;

public class PowerupSpin : MonoBehaviour
{
    public float rotateSpeed = 120f;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("IsSpinning", true);
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}