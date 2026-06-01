using UnityEngine;
using UnityEngine.Events;

public class PooledBulletImpactDespawner : MonoBehaviour
{
    [Header("Death Effect")]
    [SerializeField] private GameObject deathObjectPrefab;
    [SerializeField] private float deathObjectLifetime = 2f;
    [SerializeField] private bool alignToHitNormal = true;

    [Header("Events")]
    public UnityEvent<Vector3, Quaternion> OnDespawned;

    private bool hasDespawned;

    private void OnEnable()
    {
        hasDespawned = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasDespawned)
            return;

        ContactPoint contact = collision.GetContact(0);

        Quaternion rotation = alignToHitNormal
            ? Quaternion.LookRotation(contact.normal)
            : transform.rotation;

        DespawnAt(contact.point, rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDespawned)
            return;

        DespawnAt(transform.position, transform.rotation);
    }

    public void DespawnAt(Vector3 position, Quaternion rotation)
    {
        hasDespawned = true;

        if (deathObjectPrefab != null)
        {
            GameObject deathObject = Instantiate(deathObjectPrefab, position, rotation);
            Destroy(deathObject, deathObjectLifetime);
        }

        OnDespawned?.Invoke(position, rotation);

        gameObject.SetActive(false);
    }
}