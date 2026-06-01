using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletTrail : MonoBehaviour
{
    [Header("Trail")]
    [SerializeField] private int pointCount = 4;
    [SerializeField] private float trailLength = 4f;

    private LineRenderer line;

    private Vector3 spawnPosition;
    private Vector3 launchDirection;
    private Vector3 lastPosition;

    private Vector3[] points;
    private float[] distancesFromHead;

    private float traveledDistance;
    private bool initialized;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;

        pointCount = Mathf.Max(2, pointCount);
        points = new Vector3[pointCount];
        distancesFromHead = new float[pointCount];

        line.positionCount = pointCount;
    }

    private void OnEnable()
    {
        initialized = false;

        if (line != null)
            line.enabled = false;
    }

    public void BeginTrail(Vector3 bulletSpawnPosition, Vector3 bulletLaunchDirection)
    {
        spawnPosition = bulletSpawnPosition;
        launchDirection = bulletLaunchDirection.normalized;
        lastPosition = transform.position;
        traveledDistance = 0f;
        initialized = true;

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            distancesFromHead[i] = t * trailLength;
            points[i] = spawnPosition;
        }

        line.positionCount = pointCount;
        line.enabled = true;
        UpdateLine();
    }

    private void LateUpdate()
    {
        if (!initialized || line == null)
            return;

        Vector3 head = transform.position;

        float frameDistance = Vector3.Distance(lastPosition, head);
        traveledDistance += frameDistance;
        lastPosition = head;

        float visibleLength = Mathf.Min(trailLength, traveledDistance);

        for (int i = 0; i < pointCount; i++)
        {
            float desiredBackDistance = distancesFromHead[i];

            if (desiredBackDistance > visibleLength)
                desiredBackDistance = visibleLength;

            points[i] = head - launchDirection * desiredBackDistance;

            float behindSpawn = Vector3.Dot(points[i] - spawnPosition, launchDirection);
            if (behindSpawn < 0f)
                points[i] = spawnPosition;
        }

        UpdateLine();
    }

    private void UpdateLine()
    {
        for (int i = 0; i < pointCount; i++)
            line.SetPosition(i, points[i]);
    }

    private void OnDisable()
    {
        initialized = false;

        if (line != null)
            line.enabled = false;
    }
}