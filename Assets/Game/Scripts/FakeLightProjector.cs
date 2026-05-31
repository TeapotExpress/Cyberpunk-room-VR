using UnityEngine;

[ExecuteAlways]
public class FakeLightProjector : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float rayDistance = 1000f;
    [SerializeField] private LayerMask raycastMask = ~0;

    [Header("Light")]
    [SerializeField] [ColorUsage(true, true)] private Color lightColor = Color.white;
    [SerializeField] private float lightRadius = 5f;

    [Range(0f, 1f)]
    [SerializeField] private float lightSoftness = 0.5f;

    [Header("Shader Globals")]
    [SerializeField] private string lightPosName = "_LightPos";
    [SerializeField] private string lightDirName = "_LightDir";
    [SerializeField] private string lightColorName = "_LightColor";
    [SerializeField] private string lightRadiusName = "_LightRadius";
    [SerializeField] private string lightSoftnessName = "_LightSoftness";

    private void Update()
    {
        UpdateLight();
    }

    private void OnValidate()
    {
        UpdateLight();
    }

    private void UpdateLight()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        Vector3 hitPos = origin + direction * rayDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, raycastMask))
        {
            hitPos = hit.point;
        }

        Shader.SetGlobalVector(lightPosName, hitPos);
        Shader.SetGlobalVector(lightDirName, direction.normalized);
        Shader.SetGlobalColor(lightColorName, lightColor);
        Shader.SetGlobalFloat(lightRadiusName, lightRadius);
        Shader.SetGlobalFloat(lightSoftnessName, lightSoftness);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = lightColor;

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, raycastMask))
        {
            Gizmos.DrawLine(origin, hit.point);
            Gizmos.DrawWireSphere(hit.point, 0.15f);
        }
        else
        {
            Gizmos.DrawLine(origin, origin + direction * rayDistance);
        }
    }
}