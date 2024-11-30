#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[ExecuteInEditMode]
[RequireComponent(typeof(XRHandData))]
public class BoneVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    public float sphereRadius = 0.01f;
    public Color sphereColor = Color.red;
    public Color lineColor = Color.yellow;
    public float lineThickness = 1f;

    [Header("Editor Options")]
    public bool showInPlayMode = false;

    private XRHandData handData;

    private void OnDrawGizmos()
    {
        if (!showInPlayMode && Application.isPlaying) return;

        // Ensure XRHandData is attached
        if (handData == null)
        {
            handData = GetComponent<XRHandData>();
        }

        if (handData == null) return;

        // Draw bones for each finger
        DrawFinger(handData.thumbBones, "Thumb");
        DrawFinger(handData.indexBones, "Index");
        DrawFinger(handData.middleBones, "Middle");
        DrawFinger(handData.ringBones, "Ring");
        DrawFinger(handData.pinkyBones, "Pinky");
    }

    private void DrawFinger(Transform[] fingerBones, string fingerName)
    {
        if (fingerBones == null || fingerBones.Length == 0) return;

        // Temporarily override depth test to render on top
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        // Draw spheres and lines
        for (int i = 0; i < fingerBones.Length; i++)
        {
            Transform bone = fingerBones[i];
            if (bone == null) continue;

            // Draw filled sphere
            Handles.color = sphereColor;
            // Using SphereHandleCap to draw a filled sphere
            Handles.SphereHandleCap(0, bone.position, Quaternion.identity, sphereRadius, EventType.Repaint);

            // Handle sphere selection when clicked
            HandleSphereSelection(bone);

            // Set line color to a distinct value (lineColor)
            Handles.color = lineColor;

            // Draw line to next bone
            if (i < fingerBones.Length - 1 && fingerBones[i + 1] != null)
            {
                Handles.DrawAAPolyLine(lineThickness, bone.position, fingerBones[i + 1].position);
            }
        }

        // Reset depth test to default
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
    }

    private void HandleSphereSelection(Transform bone)
    {
        // Only select if we are in edit mode
        if (!Application.isPlaying && Event.current.type == EventType.MouseDown)
        {
            // Check if the mouse is over the sphere
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the ray hits the current bone's position (considering sphere radius)
                if (Vector3.Distance(hit.point, bone.position) <= sphereRadius)
                {
                    // Select the corresponding bone (transform)
                    Selection.activeTransform = bone;
                    Event.current.Use();  // Prevent further processing of the event
                }
            }
        }
    }
}
#endif
