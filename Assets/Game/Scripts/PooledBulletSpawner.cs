using UnityEngine;

public class PooledBulletSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AimCrosshairProvider aimProvider;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Pool")]
    [SerializeField] private int poolSize = 50;

    [Header("Shooting")]
    [SerializeField] private float defaultFireDelay = 0.08f;
    [SerializeField] private float firstShotOffset = 0f;
    [SerializeField] private float bulletSpeed = 80f;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private bool useGravity = false;

    [Header("Debug")]
    [SerializeField] private float gizmoArrowLength = 1f;

    private GameObject[] bullets;
    private Rigidbody[] rigidbodies;
    private BulletTrail[] trails;
    private float[] despawnTimes;

    private int nextBulletIndex;
    private bool isFiring;
    private float activeFireDelay;
    private float nextFireTime;

    private void Awake()
    {
        CreatePool();
    }

    private void Update()
    {
        DespawnExpiredBullets();

        if (!isFiring)
            return;

        if (Time.time >= nextFireTime)
        {
            FireImmediate();
            nextFireTime = Time.time + activeFireDelay;
        }
    }

    public void StartFiring()
    {
        StartFiring(defaultFireDelay);
    }

    public void StartFiring(float delayBetweenBullets)
    {
        activeFireDelay = Mathf.Max(0.001f, delayBetweenBullets);
        isFiring = true;

        nextFireTime = Time.time + Mathf.Max(0f, firstShotOffset);
    }

    public void StopFiring()
    {
        isFiring = false;
    }

    public void Fire()
    {
        FireImmediate();
    }

    private void CreatePool()
    {
        bullets = new GameObject[poolSize];
        rigidbodies = new Rigidbody[poolSize];
        trails = new BulletTrail[poolSize];
        despawnTimes = new float[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);

            bullets[i] = bullet;
            rigidbodies[i] = bullet.GetComponent<Rigidbody>();
            trails[i] = bullet.GetComponent<BulletTrail>();
        }
    }

    private void FireImmediate()
    {
        if (bulletPrefab == null || aimProvider == null)
            return;

        int bulletIndex = nextBulletIndex;

        GameObject bullet = bullets[bulletIndex];
        Rigidbody rb = rigidbodies[bulletIndex];
        BulletTrail trail = trails[bulletIndex];

        nextBulletIndex = (nextBulletIndex + 1) % poolSize;

        Vector3 origin = transform.position;
        Vector3 target = aimProvider.AimPoint;
        Vector3 direction = (target - origin).normalized;

        bullet.transform.position = origin;
        bullet.transform.rotation = Quaternion.LookRotation(direction);
        bullet.SetActive(true);

        if (trail != null)
            trail.BeginTrail(origin, direction);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = useGravity;
            rb.linearVelocity = direction * bulletSpeed;
            rb.angularVelocity = Vector3.zero;
        }

        despawnTimes[bulletIndex] = Time.time + bulletLifetime;
    }

    private void DespawnExpiredBullets()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!bullets[i].activeSelf)
                continue;

            if (Time.time < despawnTimes[i])
                continue;

            DespawnBullet(i);
        }
    }

    private void DespawnBullet(int index)
    {
        Rigidbody rb = rigidbodies[index];

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        bullets[index].SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Vector3 start = transform.position;

        Gizmos.color = Color.blue;
        Vector3 forwardDirection = transform.forward.normalized;
        Vector3 forwardEnd = start + forwardDirection * gizmoArrowLength;
        Gizmos.DrawLine(start, forwardEnd);
        DrawArrowHead(forwardEnd, forwardDirection, gizmoArrowLength * 0.2f);

        if (aimProvider != null)
        {
            Vector3 aimDirection = (aimProvider.AimPoint - start).normalized;

            Gizmos.color = Color.red;
            Vector3 aimEnd = start + aimDirection * gizmoArrowLength;
            Gizmos.DrawLine(start, aimEnd);
            DrawArrowHead(aimEnd, aimDirection, gizmoArrowLength * 0.2f);
        }
    }

    private void DrawArrowHead(Vector3 tip, Vector3 direction, float size)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion look = Quaternion.LookRotation(direction);

        Vector3 right = look * Quaternion.Euler(0, 160, 0) * Vector3.forward;
        Vector3 left = look * Quaternion.Euler(0, 200, 0) * Vector3.forward;

        Gizmos.DrawLine(tip, tip + right * size);
        Gizmos.DrawLine(tip, tip + left * size);
    }
}