using UnityEngine;
using System.Collections;

public class HalfCylinderWallSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public float radius = 5f;
    public float height = 4f;
    public float startAngle = 0f;
    public float endAngle = 180f;

    [Header("Prefabs")]
    public GameObject[] prefabs;
    public int maxAlive = 10;

    [Header("Spawn Timing")]
    public float startSpawnDelay = 2f;
    public float minimumSpawnDelay = 0.4f;
    public float speedIncreaseEvery = 10f;
    public float spawnDelayMultiplier = 0.85f;

    [Header("Placement")]
    public bool faceCenter = true;
    public Transform parent;

    private int aliveCount;
    private float currentSpawnDelay;

    private void Start()
    {
        currentSpawnDelay = startSpawnDelay;
        StartCoroutine(SpawnLoop());
        StartCoroutine(SpeedIncreaseLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (aliveCount < maxAlive)
                SpawnOne();

            yield return new WaitForSeconds(currentSpawnDelay);
        }
    }

    private IEnumerator SpeedIncreaseLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(speedIncreaseEvery);

            currentSpawnDelay *= spawnDelayMultiplier;
            currentSpawnDelay = Mathf.Max(currentSpawnDelay, minimumSpawnDelay);
        }
    }

    public void SpawnOne()
    {
        if (prefabs == null || prefabs.Length == 0)
            return;

        float angle = Random.Range(startAngle, endAngle) * Mathf.Deg2Rad;
        float y = Random.Range(0f, height);

        Vector3 localPosition = new Vector3(
            Mathf.Cos(angle) * radius,
            y,
            Mathf.Sin(angle) * radius
        );

        Vector3 worldPosition = transform.TransformPoint(localPosition);

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        GameObject spawned = Instantiate(prefab, worldPosition, Quaternion.identity, parent);

        if (faceCenter)
        {
            Vector3 centerPoint = transform.TransformPoint(new Vector3(0f, y, 0f));
            Vector3 directionToCenter = centerPoint - spawned.transform.position;

            if (directionToCenter != Vector3.zero)
                spawned.transform.rotation = Quaternion.LookRotation(directionToCenter, Vector3.up);
        }

        aliveCount++;

        GrowAndDieOnHit growScript = spawned.GetComponent<GrowAndDieOnHit>();

        if (growScript != null)
            growScript.Initialize(this);
    }

    public void NotifyPrefabDied()
    {
        aliveCount = Mathf.Max(0, aliveCount - 1);
        SpawnOne();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        int segments = 40;

        Vector3 previousBottom = Vector3.zero;
        Vector3 previousTop = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;

            Vector3 localBottom = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            Vector3 localTop = localBottom + Vector3.up * height;

            Vector3 worldBottom = transform.TransformPoint(localBottom);
            Vector3 worldTop = transform.TransformPoint(localTop);

            // Vertical lines
            Gizmos.DrawLine(worldBottom, worldTop);

            // Connect arc lines
            if (i > 0)
            {
                Gizmos.DrawLine(previousBottom, worldBottom);
                Gizmos.DrawLine(previousTop, worldTop);
            }

            previousBottom = worldBottom;
            previousTop = worldTop;
        }

        // Draw center line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.up * height
        );
    }
}