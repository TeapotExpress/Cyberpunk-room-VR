using UnityEngine;
using UnityEngine.XR;

[ExecuteAlways]
public class MirrorCameraController : MonoBehaviour
{
    [Header("Source")]
    public Camera playerCamera;

    [Header("Mirror Cameras")]
    public Camera leftMirrorCamera;
    public Camera rightMirrorCamera;

    [Header("Render Textures")]
    public RenderTexture leftMirrorRenderTexture;
    public RenderTexture rightMirrorRenderTexture;

    [Header("Mirror Material")]
    public Material mirrorMaterial;
    public string leftTextureProperty = "_LeftTex";
    public string rightTextureProperty = "_RightTex";

    [Header("Quality")]
    [Range(0.1f, 1f)]
    public float renderScale = 1f;

    [Header("Clipping")]
    [Range(0f, 0.1f)]
    public float clippingOffset = 0.02f;

    [Header("Debug")]
    public bool swapEyes = false;
    public bool renderInEditor = true;

    private bool isRendering;

    private void OnEnable()
    {
        Application.onBeforeRender += RenderMirror;
        Setup();
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= RenderMirror;
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying && renderInEditor)
            RenderMirror();
    }

    private void Setup()
    {
        if (!playerCamera)
            return;

        ResizeRenderTextures();

        SetupMirrorCamera(leftMirrorCamera, leftMirrorRenderTexture);
        SetupMirrorCamera(rightMirrorCamera, rightMirrorRenderTexture);

        AssignTextures();
    }

    private void SetupMirrorCamera(Camera cam, RenderTexture rt)
    {
        if (!cam || !playerCamera)
            return;

        cam.enabled = false;
        cam.targetTexture = rt;

        cam.clearFlags = playerCamera.clearFlags;
        cam.backgroundColor = playerCamera.backgroundColor;
        cam.nearClipPlane = playerCamera.nearClipPlane;
        cam.farClipPlane = playerCamera.farClipPlane;
        cam.useOcclusionCulling = false;
    }

    private void RenderMirror()
    {
        if (isRendering)
            return;

        if (!playerCamera || !leftMirrorCamera || !rightMirrorCamera)
            return;

        isRendering = true;

        try
        {
            Setup();

            bool xr = Application.isPlaying && XRSettings.isDeviceActive;

            if (xr)
            {
                if (!swapEyes)
                {
                    RenderEye(Camera.StereoscopicEye.Left, XRNode.LeftEye, leftMirrorCamera, leftMirrorRenderTexture);
                    RenderEye(Camera.StereoscopicEye.Right, XRNode.RightEye, rightMirrorCamera, rightMirrorRenderTexture);
                }
                else
                {
                    RenderEye(Camera.StereoscopicEye.Right, XRNode.RightEye, leftMirrorCamera, leftMirrorRenderTexture);
                    RenderEye(Camera.StereoscopicEye.Left, XRNode.LeftEye, rightMirrorCamera, rightMirrorRenderTexture);
                }
            }
            else
            {
                RenderMono(leftMirrorCamera, leftMirrorRenderTexture);

                if (leftMirrorRenderTexture && rightMirrorRenderTexture)
                    Graphics.Blit(leftMirrorRenderTexture, rightMirrorRenderTexture);
            }
        }
        finally
        {
            isRendering = false;
        }
    }

    private void RenderMono(Camera mirrorCam, RenderTexture target)
    {
        Pose sourcePose = new Pose(
            playerCamera.transform.position,
            playerCamera.transform.rotation
        );

        RenderReflected(mirrorCam, target, sourcePose, playerCamera.projectionMatrix);
    }

    private void RenderEye(
        Camera.StereoscopicEye eye,
        XRNode node,
        Camera mirrorCam,
        RenderTexture target
    )
    {
        Vector3 localEyePosition = InputTracking.GetLocalPosition(node);

        Transform xrRoot = playerCamera.transform.parent;

        Vector3 worldEyePosition = xrRoot
            ? xrRoot.TransformPoint(localEyePosition)
            : playerCamera.transform.TransformPoint(localEyePosition);

        Pose sourcePose = new Pose(
            worldEyePosition,
            playerCamera.transform.rotation
        );

        RenderReflected(
            mirrorCam,
            target,
            sourcePose,
            playerCamera.GetStereoProjectionMatrix(eye)
        );
    }

    private void RenderReflected(
        Camera mirrorCam,
        RenderTexture target,
        Pose sourcePose,
        Matrix4x4 projection
    )
    {
        if (!mirrorCam || !target)
            return;

        Vector3 mirrorPosition = transform.position;
        Vector3 mirrorNormal = transform.forward.normalized;

        Vector3 reflectedPosition = ReflectPoint(
            sourcePose.position,
            mirrorPosition,
            mirrorNormal
        );

        Vector3 reflectedForward = Vector3.Reflect(
            sourcePose.rotation * Vector3.forward,
            mirrorNormal
        );

        Vector3 reflectedUp = Vector3.Reflect(
            sourcePose.rotation * Vector3.up,
            mirrorNormal
        );

        Quaternion reflectedRotation = Quaternion.LookRotation(
            reflectedForward,
            reflectedUp
        );

        mirrorCam.transform.SetPositionAndRotation(
            reflectedPosition,
            reflectedRotation
        );

        mirrorCam.targetTexture = target;
        mirrorCam.projectionMatrix = projection;

        ApplyObliqueClipping(
            mirrorCam,
            mirrorNormal,
            mirrorPosition + mirrorNormal * clippingOffset
        );

        mirrorCam.Render();

        mirrorCam.ResetProjectionMatrix();
    }

    private static Vector3 ReflectPoint(
        Vector3 point,
        Vector3 planePoint,
        Vector3 planeNormal
    )
    {
        return point - 2f * Vector3.Dot(point - planePoint, planeNormal) * planeNormal;
    }

    private void ApplyObliqueClipping(
        Camera cam,
        Vector3 planeNormal,
        Vector3 planePoint
    )
    {
        Vector4 clipPlaneWorld = new Vector4(
            planeNormal.x,
            planeNormal.y,
            planeNormal.z,
            -Vector3.Dot(planeNormal, planePoint)
        );

        Matrix4x4 viewMatrix = cam.worldToCameraMatrix;

        Vector4 clipPlaneCamera =
            Matrix4x4.Transpose(Matrix4x4.Inverse(viewMatrix)) * clipPlaneWorld;

        cam.projectionMatrix = cam.CalculateObliqueMatrix(clipPlaneCamera);
    }

    private void ResizeRenderTextures()
    {
        ResizeRenderTexture(leftMirrorRenderTexture);
        ResizeRenderTexture(rightMirrorRenderTexture);
    }

    private void ResizeRenderTexture(RenderTexture rt)
    {
        if (!rt)
            return;

        int baseWidth = Application.isPlaying && XRSettings.isDeviceActive
            ? XRSettings.eyeTextureWidth
            : Screen.width;

        int baseHeight = Application.isPlaying && XRSettings.isDeviceActive
            ? XRSettings.eyeTextureHeight
            : Screen.height;

        int width = Mathf.Max(1, Mathf.RoundToInt(baseWidth * renderScale));
        int height = Mathf.Max(1, Mathf.RoundToInt(baseHeight * renderScale));

        if (rt.width == width && rt.height == height)
            return;

        rt.Release();
        rt.width = width;
        rt.height = height;
        rt.Create();
    }

    private void AssignTextures()
    {
        if (!mirrorMaterial)
            return;

        mirrorMaterial.SetTexture(leftTextureProperty, leftMirrorRenderTexture);
        mirrorMaterial.SetTexture(rightTextureProperty, rightMirrorRenderTexture);
    }
}