using UnityEngine;

public class AimCrosshairProvider : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private float surfaceOffset = 0.01f;

    [Header("Crosshair")]
    [SerializeField] private GameObject crosshairPrefab;

    [Header("Debug")]
    [SerializeField] private float gizmoArrowLength = 1f;
    [SerializeField] private bool drawFullAimRay = true;

    private GameObject crosshairInstance;

    public Vector3 AimPoint { get; private set; }
    public Vector3 AimDirection { get; private set; }
    public bool HasHit { get; private set; }

    private void Awake()
    {
        if (rayOrigin == null)
            rayOrigin = transform;

        UpdateAim();
    }

    private void LateUpdate()
    {
        UpdateAim();
    }

    private void UpdateAim()
    {
        if (rayOrigin == null)
            return;

        Vector3 origin = rayOrigin.position;
        Vector3 direction = rayOrigin.forward.normalized;

        AimDirection = direction;

        if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                maxDistance,
                hitMask,
                QueryTriggerInteraction.Ignore))
        {
            HasHit = true;
            AimPoint = hit.point;

            ShowCrosshair(
                hit.point + hit.normal * surfaceOffset,
                Quaternion.LookRotation(hit.normal)
            );
        }
        else
        {
            HasHit = false;
            AimPoint = origin + direction * maxDistance;

            ShowCrosshair(
                AimPoint,
                Quaternion.LookRotation(-direction)
            );
        }
    }

    private void ShowCrosshair(Vector3 position, Quaternion rotation)
    {
        if (crosshairPrefab == null)
            return;

        if (crosshairInstance == null)
        {
            crosshairInstance = Instantiate(crosshairPrefab);

            // Safety: make sure it cannot raycast-block itself.
            SetLayerRecursively(crosshairInstance, LayerMask.NameToLayer("Ignore Raycast"));
        }

        crosshairInstance.SetActive(true);
        crosshairInstance.transform.position = position;
        crosshairInstance.transform.rotation = rotation;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer < 0)
            return;

        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void OnDisable()
    {
        if (crosshairInstance != null)
            crosshairInstance.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Transform originTransform = rayOrigin != null ? rayOrigin : transform;

        Vector3 start = originTransform.position;
        Vector3 direction = originTransform.forward.normalized;

        Gizmos.color = Color.green;

        if (drawFullAimRay)
            Gizmos.DrawLine(start, start + direction * maxDistance);
        else
            Gizmos.DrawLine(start, start + direction * gizmoArrowLength);

        Vector3 arrowEnd = start + direction * gizmoArrowLength;
        DrawArrowHead(arrowEnd, direction, gizmoArrowLength * 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(start + direction * maxDistance, 0.15f);
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