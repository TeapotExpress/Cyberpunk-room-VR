using UnityEngine;

public class SlingshotProjectileSpawner : MonoBehaviour
{
    [Header("String Pull Reference")]
    [SerializeField] private SlingshotStringPull stringPull;

    [Header("String Bones")]
    [SerializeField] private Transform leftStringAnchor;
    [SerializeField] private Transform rightStringAnchor;
    [SerializeField] private Transform middleStringBone;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileHoldPoint;

    [Header("Launch")]
    [SerializeField] private float launchSpeedPerMeter = 35f;
    [SerializeField] private float minLaunchSpeed = 4f;
    [SerializeField] private float maxLaunchSpeed = 30f;
    [SerializeField] private bool flyStraight = true;

    [Header("Collision Enable Threshold")]
    [SerializeField] private float enableCollisionPastArmsDistance = 0.05f;

    [Header("Aim Raycast")]
    [SerializeField] private GameObject crosshairPrefab;
    [SerializeField] private float aimRayDistance = 50f;
    [SerializeField] private LayerMask aimRayMask = ~0;
    [SerializeField] private float crosshairSurfaceOffset = 0.01f;

    [Header("Debug")]
    [SerializeField] private float gizmoArrowLength = 0.5f;

    private GameObject currentProjectile;
    private Rigidbody currentProjectileRb;
    private Collider[] currentProjectileColliders;

    private GameObject crosshairInstance;

    private bool wasPulledLastFrame;
    private bool waitingToEnableCollision;

    private void OnEnable()
    {
        wasPulledLastFrame = false;
        waitingToEnableCollision = false;
    }

    private void OnDisable()
    {
        HideCrosshair();
    }

    private void Update()
    {
        if (stringPull == null || projectilePrefab == null)
            return;

        UpdateAimCrosshair();

        if (stringPull.IsPulled && !wasPulledLastFrame)
            SpawnProjectile();

        if (stringPull.IsPulled && currentProjectile != null)
            HoldProjectileAtString();

        if (!stringPull.IsPulled && wasPulledLastFrame)
            ReleaseProjectile();

        if (waitingToEnableCollision && currentProjectile != null)
            TryEnableCollisionAfterCrossingArms();

        wasPulledLastFrame = stringPull.IsPulled;
    }

    private void SpawnProjectile()
    {
        Vector3 spawnPosition = GetProjectileHoldPosition();

        currentProjectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        currentProjectileRb = currentProjectile.GetComponent<Rigidbody>();
        currentProjectileColliders = currentProjectile.GetComponentsInChildren<Collider>();

        SetProjectileColliders(false);

        if (currentProjectileRb != null)
        {
            currentProjectileRb.isKinematic = true;
            currentProjectileRb.useGravity = false;
        }
    }

    private void HoldProjectileAtString()
    {
        currentProjectile.transform.position = GetProjectileHoldPosition();

        Vector3 direction = GetLaunchDirection();

        if (direction.sqrMagnitude > 0.0001f)
            currentProjectile.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void ReleaseProjectile()
    {
        if (currentProjectile == null || currentProjectileRb == null)
        {
            ClearProjectile();
            return;
        }

        Vector3 direction = GetLaunchDirection();

        if (direction.sqrMagnitude <= 0.0001f)
        {
            ClearProjectile();
            return;
        }

        float pullDistance = GetPullDistanceFromTriangle();

        float launchSpeed = Mathf.Clamp(
            pullDistance * launchSpeedPerMeter,
            minLaunchSpeed,
            maxLaunchSpeed
        );

        currentProjectileRb.isKinematic = false;
        currentProjectileRb.useGravity = !flyStraight;
        currentProjectileRb.linearVelocity = direction * launchSpeed;
        currentProjectileRb.angularVelocity = Vector3.zero;

        waitingToEnableCollision = true;
        HideCrosshair();
    }

    private void TryEnableCollisionAfterCrossingArms()
    {
        Vector3 center = GetAnchorCenter();
        Vector3 direction = GetLaunchDirectionFromProjectile(currentProjectile.transform.position);

        float crossedDistance = Vector3.Dot(
            currentProjectile.transform.position - center,
            direction
        );

        if (crossedDistance >= enableCollisionPastArmsDistance)
        {
            SetProjectileColliders(true);
            waitingToEnableCollision = false;
            ClearProjectile();
        }
    }

    private void UpdateAimCrosshair()
    {
        if (!stringPull.IsPulled)
        {
            HideCrosshair();
            return;
        }

        Vector3 origin = GetProjectileHoldPosition();
        Vector3 direction = GetLaunchDirection();

        if (direction.sqrMagnitude <= 0.0001f)
        {
            HideCrosshair();
            return;
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, aimRayDistance, aimRayMask))
        {
            ShowCrosshair(hit);
        }
        else
        {
            HideCrosshair();
        }
    }

    private void ShowCrosshair(RaycastHit hit)
    {
        if (crosshairPrefab == null)
            return;

        if (crosshairInstance == null)
            crosshairInstance = Instantiate(crosshairPrefab);

        crosshairInstance.SetActive(true);

        crosshairInstance.transform.position =
            hit.point + hit.normal * crosshairSurfaceOffset;

        crosshairInstance.transform.rotation =
            Quaternion.LookRotation(hit.normal);
    }

    private void HideCrosshair()
    {
        if (crosshairInstance != null)
            crosshairInstance.SetActive(false);
    }

    private void SetProjectileColliders(bool enabled)
    {
        if (currentProjectileColliders == null)
            return;

        foreach (Collider col in currentProjectileColliders)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }

    private Vector3 GetProjectileHoldPosition()
    {
        if (projectileHoldPoint != null)
            return projectileHoldPoint.position;

        if (middleStringBone != null)
            return middleStringBone.position;

        return transform.position;
    }

    private Vector3 GetAnchorCenter()
    {
        return (leftStringAnchor.position + rightStringAnchor.position) * 0.5f;
    }

    private Vector3 GetLaunchDirection()
    {
        return (GetAnchorCenter() - GetProjectileHoldPosition()).normalized;
    }

    private Vector3 GetLaunchDirectionFromProjectile(Vector3 projectilePosition)
    {
        return (projectilePosition - GetProjectileHoldPosition()).normalized;
    }

    private float GetPullDistanceFromTriangle()
    {
        return Vector3.Distance(GetAnchorCenter(), GetProjectileHoldPosition());
    }

    private void ClearProjectile()
    {
        currentProjectile = null;
        currentProjectileRb = null;
        currentProjectileColliders = null;
    }

    private void OnDrawGizmos()
    {
        if (leftStringAnchor == null || rightStringAnchor == null)
            return;

        Vector3 left = leftStringAnchor.position;
        Vector3 right = rightStringAnchor.position;
        Vector3 center = (left + right) * 0.5f;
        Vector3 pull = GetProjectileHoldPosition();
        Vector3 direction = (center - pull).normalized;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(left, pull);
        Gizmos.DrawLine(right, pull);
        Gizmos.DrawLine(left, right);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, 0.04f);

        Gizmos.color = Color.green;
        Vector3 end = pull + direction * gizmoArrowLength;
        Gizmos.DrawLine(pull, end);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center + direction * enableCollisionPastArmsDistance, 0.05f);
    }
}