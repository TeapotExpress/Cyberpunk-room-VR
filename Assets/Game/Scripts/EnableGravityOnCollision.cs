using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnableGravityOnCollision : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.useGravity = true;
    }
}