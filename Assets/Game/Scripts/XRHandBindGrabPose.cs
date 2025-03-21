using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRHandBindGrabPose : MonoBehaviour
{
    public AnimationCurve lerpCurve;
    public float lerpDuration = 0.25f;
    public XRGrabInteractable interactable;

    public XRHandData objectHandData;
    private XRHandData handData;
    public XRHandData OtherHand; // Reference to the other hand's XRHandData

    private Coroutine mainHandLerpCoroutine; // Reference to the coroutine for mainHandBone lerping

    private void Start()
    {
        if (objectHandData == null)
        {
            objectHandData = GetComponent<XRHandData>();
        }
        HideHandMesh();
    }

    private void OnEnable()
    {
        // Subscribe to the SelectEntered and SelectExited events
        interactable.selectEntered.AddListener(BindGrabPose);
        interactable.selectExited.AddListener(ReleaseBindPose);
    }

    private void OnDisable()
    {
        // Unsubscribe from the events to avoid memory leaks
        interactable.selectEntered.RemoveListener(BindGrabPose);
        interactable.selectExited.RemoveListener(ReleaseBindPose);
    }

    public void BindGrabPose(SelectEnterEventArgs args)
    {
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
        {
            // Ignore sockets (we don’t want to animate the hand pose)
            return;
        }
        // Ensure the interactor object is valid and get its transform
        Transform interactorTransform = args.interactorObject.transform;
        if (interactorTransform == null)
        {
            Debug.LogWarning("BindGrabPose: interactorObject's transform is null. Cannot bind pose.");
            return;
        }

        // Fetch the XRHandData from the interactor's GameObject
        handData = interactorTransform.parent.GetComponentInChildren<XRHandData>();
        if (handData == null)
        {
            Debug.LogWarning("BindGrabPose: XRHandData not found on interactor object.");
            return;
        }

        if (handData.handType != objectHandData.handType)
        {
            Debug.LogWarning($"BindGrabPose: Hand type mismatch. Expected {objectHandData.handType}, got {handData.handType}.");
            return;
        }

        // Disable the hand animator if it exists
        Animator handAnimator = handData.animator;
        if (handAnimator != null)
        {
            handAnimator.enabled = false;
        }

        // Start lerping the main hand bone and fingers
        if (mainHandLerpCoroutine != null)
        {
            StopCoroutine(mainHandLerpCoroutine);
        }

        // Start the coroutine to lerp the hand's position and rotation
        mainHandLerpCoroutine = StartCoroutine(LerpMainHandToObjectPosition(handData, objectHandData));

        // Start the finger rotation lerp
        StartCoroutine(LerpHandPose(handData, objectHandData));
    }

    public void ReleaseBindPose(SelectExitEventArgs args)
    {
        // Ensure the interactor object is valid and get its transform
        Transform interactorTransform = args.interactorObject.transform;
        if (interactorTransform == null)
        {
            Debug.LogWarning("ReleaseBindPose: interactorObject's transform is null. Cannot release pose.");
            return;
        }

        // Fetch the XRHandData from the interactor's GameObject
        XRHandData releasingHandData = interactorTransform.parent.GetComponentInChildren<XRHandData>();
        if (releasingHandData == null)
        {
            Debug.LogWarning("ReleaseBindPose: XRHandData not found on interactor object.");
            return;
        }

        // Start lerping back to the initial position and rotation
        StartCoroutine(LerpMainHandBackToInitialPosition(releasingHandData));

        // Re-enable the hand animator if it exists
        Animator handAnimator = releasingHandData.animator;
        if (handAnimator != null)
        {
            handAnimator.enabled = true;
        }
    }

    private IEnumerator LerpHandPose(XRHandData playerHand, XRHandData objectHand)
    {
        float timeElapsed = 0f;

        while (timeElapsed < lerpDuration)
        {
            float t = lerpCurve.Evaluate(timeElapsed / lerpDuration);

            // Lerp the rotations for each finger bone
            LerpFingerBones(playerHand, objectHand, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void LerpFingerBones(XRHandData playerHand, XRHandData objectHand, float t)
    {
        // Thumb
        for (int i = 0; i < playerHand.thumbBones.Length; i++)
        {
            playerHand.thumbBones[i].rotation = Quaternion.Lerp(
                playerHand.thumbBones[i].rotation,
                objectHand.thumbBones[i].rotation,
                t
            );
        }

        // Index
        for (int i = 0; i < playerHand.indexBones.Length; i++)
        {
            playerHand.indexBones[i].rotation = Quaternion.Lerp(
                playerHand.indexBones[i].rotation,
                objectHand.indexBones[i].rotation,
                t
            );
        }

        // Middle
        for (int i = 0; i < playerHand.middleBones.Length; i++)
        {
            playerHand.middleBones[i].rotation = Quaternion.Lerp(
                playerHand.middleBones[i].rotation,
                objectHand.middleBones[i].rotation,
                t
            );
        }

        // Ring
        for (int i = 0; i < playerHand.ringBones.Length; i++)
        {
            playerHand.ringBones[i].rotation = Quaternion.Lerp(
                playerHand.ringBones[i].rotation,
                objectHand.ringBones[i].rotation,
                t
            );
        }

        // Pinky
        for (int i = 0; i < playerHand.pinkyBones.Length; i++)
        {
            playerHand.pinkyBones[i].rotation = Quaternion.Lerp(
                playerHand.pinkyBones[i].rotation,
                objectHand.pinkyBones[i].rotation,
                t
            );
        }
    }
    

    private IEnumerator LerpMainHandToObjectPosition(XRHandData playerHand, XRHandData objectHand)
    {
        float timeElapsed = 0f;
        Vector3 initialPosition = playerHand.mainHandBone.position;
        Quaternion initialRotation = playerHand.mainHandBone.rotation;

        // Lerp to the object's main hand position and rotation
        while (timeElapsed < lerpDuration)
        {
            float t = lerpCurve.Evaluate(timeElapsed / lerpDuration);
            playerHand.mainHandBone.position = Vector3.Lerp(initialPosition, objectHand.mainHandBone.position, t);
            playerHand.mainHandBone.rotation = Quaternion.Lerp(initialRotation, objectHand.mainHandBone.rotation, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and rotation are exactly correct
        playerHand.mainHandBone.position = objectHand.mainHandBone.position;
        playerHand.mainHandBone.rotation = objectHand.mainHandBone.rotation;
    }
    

    private IEnumerator LerpMainHandBackToInitialPosition(XRHandData playerHand)
    {
        float timeElapsed = 0f;
        Vector3 initialPosition = playerHand.initialHandPosition.transform.localPosition;
        Quaternion initialRotation = playerHand.initialHandPosition.transform.localRotation;

        while (timeElapsed < lerpDuration)
        {
            float t = lerpCurve.Evaluate(timeElapsed / lerpDuration);
            playerHand.mainHandBone.localPosition = Vector3.Lerp(playerHand.mainHandBone.localPosition, initialPosition, t);
            playerHand.mainHandBone.localRotation = Quaternion.Lerp(playerHand.mainHandBone.localRotation, initialRotation, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and rotation are exactly correct
        playerHand.mainHandBone.localPosition = initialPosition;
        playerHand.mainHandBone.localRotation = initialRotation;
    }

    private void HideHandMesh()
    {
        var meshRenderer = objectHandData.GetComponentInChildren<SkinnedMeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    // Editor-only functionality
#if UNITY_EDITOR
    [CustomEditor(typeof(XRHandBindGrabPose))]
    public class XRHandBindGrabPoseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            XRHandBindGrabPose script = (XRHandBindGrabPose)target;

            // Add a button to copy transforms
            if (GUILayout.Button("Copy Transforms to Other Hand"))
            {
                if (script.objectHandData == null || script.OtherHand == null)
                {
                    Debug.LogWarning("Both ObjectHandData and OtherHand must be assigned.");
                    return;
                }

                CopyHandTransforms(script.objectHandData, script.OtherHand);
                Debug.Log("Transforms copied successfully!");
            }
        }

        private void CopyHandTransforms(XRHandData fromHand, XRHandData toHand)
        {
            // Copy main hand transform
            toHand.mainHandBone.localPosition = fromHand.mainHandBone.localPosition;
            toHand.mainHandBone.localRotation = fromHand.mainHandBone.localRotation;

            // Copy thumb bones
            CopyBoneArray(fromHand.thumbBones, toHand.thumbBones);

            // Copy index bones
            CopyBoneArray(fromHand.indexBones, toHand.indexBones);

            // Copy middle bones
            CopyBoneArray(fromHand.middleBones, toHand.middleBones);

            // Copy ring bones
            CopyBoneArray(fromHand.ringBones, toHand.ringBones);

            // Copy pinky bones
            CopyBoneArray(fromHand.pinkyBones, toHand.pinkyBones);
        }

        private void CopyBoneArray(Transform[] fromBones, Transform[] toBones)
        {
            if (fromBones.Length != toBones.Length)
            {
                Debug.LogWarning("Bone arrays do not match in length.");
                return;
            }

            for (int i = 0; i < fromBones.Length; i++)
            {
                toBones[i].localPosition = fromBones[i].localPosition;
                toBones[i].localRotation = fromBones[i].localRotation;
            }
        }
    }
#endif
}