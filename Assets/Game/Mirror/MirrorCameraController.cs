using UnityEngine;
using UnityEngine.XR;

[ExecuteAlways]
public class MirrorCameraController : MonoBehaviour
{
    public Camera playerCamera;
    public Camera mirrorCamera;

    [Tooltip("Assigned RenderTexture used by the mirror camera")]
    public RenderTexture mirrorRenderTexture;

    [Range(0.1f, 1.0f)]
    public float renderScale = 1.0f;

    [Tooltip("Stereo reflection offset compensation (0 = no shift, 0.5 = fully correct for left eye)")]
    [Range(0f, 0.5f)]
    public float stereoCompensation = 0.25f;

    [Tooltip("Extra clipping offset (in meters) beyond the mirror plane")]
    [Range(0f, 0.1f)]  // Adjust max as needed (e.g., 10cm = 0.1m)
    public float clippingOffset = 0.02f;

    private void Awake()
    {
        // Reserved for future setup, if needed
    }

    private void Start()
    {
        // One-time setup for mirror camera properties
        ResizeRenderTexture();

        if (mirrorCamera != null && playerCamera != null)
        {
            mirrorCamera.fieldOfView = playerCamera.fieldOfView;
            mirrorCamera.nearClipPlane = playerCamera.nearClipPlane;
            mirrorCamera.farClipPlane = playerCamera.farClipPlane;

            mirrorCamera.stereoTargetEye = StereoTargetEyeMask.Both;
        }
    }

    private void OnEnable()
    {
        Application.onBeforeRender += UpdateMirrorCamera;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= UpdateMirrorCamera;
    }

    private void UpdateMirrorCamera()
    {
        if (playerCamera == null || mirrorCamera == null)
            return;

        // Mirror plane
        Transform mirrorTransform = transform;
        Vector3 mirrorPos = mirrorTransform.position;
        Vector3 mirrorNormal = mirrorTransform.forward;

        // XR center eye or fallback
        Vector3 worldEyeCenter;

        if (XRSettings.isDeviceActive && Application.isPlaying)
        {
            Vector3 localCenterEye = InputTracking.GetLocalPosition(XRNode.CenterEye);
            worldEyeCenter = (playerCamera.transform.parent != null)
                ? playerCamera.transform.parent.TransformPoint(localCenterEye)
                : playerCamera.transform.position;
        }
        else
        {
            worldEyeCenter = playerCamera.transform.position;
        }

        // Reflect position
        Vector3 camToMirror = worldEyeCenter - mirrorPos;
        Vector3 reflectedPos = worldEyeCenter - 2f * Vector3.Dot(camToMirror, mirrorNormal) * mirrorNormal;

        // Stereo compensation offset
        if (XRSettings.isDeviceActive && Application.isPlaying)
        {
            float ipd = Vector3.Distance(
                InputTracking.GetLocalPosition(XRNode.LeftEye),
                InputTracking.GetLocalPosition(XRNode.RightEye)
            );

            reflectedPos -= mirrorCamera.transform.right * (ipd * stereoCompensation);
        }

        // Reflect forward and up
        Vector3 reflectedForward = Vector3.Reflect(playerCamera.transform.forward, mirrorNormal);
        Vector3 reflectedUp = Vector3.Reflect(playerCamera.transform.up, mirrorNormal);
        Quaternion reflectedRotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

        float originalRoll = playerCamera.transform.eulerAngles.z;
        float invertedRoll = -NormalizeAngle(originalRoll);
        Vector3 reflectedEuler = reflectedRotation.eulerAngles;
        reflectedEuler.z = invertedRoll;

        mirrorCamera.transform.SetPositionAndRotation(reflectedPos, Quaternion.Euler(reflectedEuler));

        // Projection matrix (must be updated every frame in VR)
        if (XRSettings.isDeviceActive && Application.isPlaying)
        {
            mirrorCamera.projectionMatrix = playerCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
        }
        else
        {
            mirrorCamera.ResetProjectionMatrix();
        }

        // Oblique near-plane clipping with offset
        if (Application.isPlaying)
        {
            ApplyObliqueClipping(mirrorCamera, mirrorNormal, mirrorPos + mirrorNormal * clippingOffset);
        }
    }

    private void ResizeRenderTexture()
    {
        if (mirrorRenderTexture == null)
            return;

        int baseWidth = XRSettings.isDeviceActive && Application.isPlaying
            ? XRSettings.eyeTextureWidth
            : Screen.width;

        int baseHeight = XRSettings.isDeviceActive && Application.isPlaying
            ? XRSettings.eyeTextureHeight
            : Screen.height;

        int width = Mathf.RoundToInt(baseWidth * renderScale);
        int height = Mathf.RoundToInt(baseHeight * renderScale);

        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);

        if (mirrorRenderTexture.width != width || mirrorRenderTexture.height != height)
        {
            mirrorRenderTexture.Release();
            mirrorRenderTexture.width = width;
            mirrorRenderTexture.height = height;
            mirrorRenderTexture.Create();
        }
    }

    private void ApplyObliqueClipping(Camera cam, Vector3 planeNormal, Vector3 planePoint)
    {
        Plane clipPlane = new Plane(planeNormal, planePoint);
        Vector4 clipPlaneWorld = new Vector4(
            clipPlane.normal.x, clipPlane.normal.y, clipPlane.normal.z,
            -Vector3.Dot(clipPlane.normal, planePoint)
        );

        Matrix4x4 viewMatrix = cam.worldToCameraMatrix;
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(viewMatrix)) * clipPlaneWorld;

        cam.projectionMatrix = cam.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    public void DebugSlider(float _stereoCompensation)
    {
        stereoCompensation = _stereoCompensation;
    }

    public void DebugClipSlider(float _clippingOffset)
    {
        clippingOffset = _clippingOffset;
    }
}
