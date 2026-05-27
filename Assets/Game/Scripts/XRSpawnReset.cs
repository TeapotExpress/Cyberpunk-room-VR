using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class XRSpawnReset : MonoBehaviour
{
    public XROrigin xrOrigin;
    public Transform spawnPoint;

    IEnumerator Start()
    {
        yield return null; // let XR/Simulator apply tracking pose first
        ResetToSpawn();
    }

    public void ResetToSpawn()
    {
        Transform cam = xrOrigin.Camera.transform;

        // Move XR Origin so camera's X/Z lands on spawn point
        Vector3 delta = spawnPoint.position - cam.position;
        delta.y = 0f;
        xrOrigin.transform.position += delta;

        // Rotate XR Origin so camera faces spawn direction
        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 targetForward = spawnPoint.forward;
        targetForward.y = 0f;
        targetForward.Normalize();

        float angle = Vector3.SignedAngle(camForward, targetForward, Vector3.up);
        xrOrigin.RotateAroundCameraPosition(Vector3.up, angle);
    }
}