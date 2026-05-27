using UnityEngine;

public class GrowAndDieOnHit : MonoBehaviour
{
    [Header("Growth")]
    public float maxScaleMultiplier = 2f;
    public float growSpeed = 0.5f;

    [Header("Death")]
    public LayerMask projectileLayers;
    private HalfCylinderWallSpawner spawner;
    private Vector3 startScale;
    private Vector3 targetScale;
    private bool isDead;

    public void Initialize(HalfCylinderWallSpawner owningSpawner)
    {
        spawner = owningSpawner;

        startScale = transform.localScale;
        targetScale = startScale * maxScaleMultiplier;
    }

    private void Start()
    {
        startScale = transform.localScale;
        targetScale = startScale * maxScaleMultiplier;
    }

    private void Update()
    {
        if (isDead)
            return;

        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            targetScale,
            growSpeed * Time.deltaTime
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckDeath(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckDeath(other.gameObject);
    }

    private void CheckDeath(GameObject other)
    {
        if (isDead)
            return;

        bool layerMatches =
            (projectileLayers.value & (1 << other.layer)) != 0;

        if (layerMatches)
        {
            isDead = true;

            if (spawner != null)
                spawner.NotifyPrefabDied();

            Destroy(gameObject);
        }
    }
}