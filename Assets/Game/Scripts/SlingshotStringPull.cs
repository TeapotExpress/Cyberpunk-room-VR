using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class SlingshotStringPull : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform stringBone;

    [Header("Return")]
    [SerializeField] private float returnSpeed = 18f;
    [SerializeField] private float snapDistance = 0.001f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private Transform pullingHand;

    private Vector3 restBoneLocalPosition;
    private Quaternion restBoneLocalRotation;

    private Vector3 restGrabPointLocalPosition;
    private Quaternion restGrabPointLocalRotation;

    private Vector3 grabOffsetWorld;

    public bool IsPulled => pullingHand != null;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;

        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;

        restBoneLocalPosition = stringBone.localPosition;
        restBoneLocalRotation = stringBone.localRotation;

        restGrabPointLocalPosition = transform.localPosition;
        restGrabPointLocalRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabString);
        grabInteractable.selectExited.AddListener(OnReleaseString);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabString);
        grabInteractable.selectExited.RemoveListener(OnReleaseString);
    }

    private void OnGrabString(SelectEnterEventArgs args)
    {
        pullingHand = args.interactorObject.transform;
        grabOffsetWorld = transform.position - pullingHand.position;
    }

    private void OnReleaseString(SelectExitEventArgs args)
    {
        pullingHand = null;
    }

    private void LateUpdate()
    {
        if (pullingHand != null)
        {
            Vector3 targetPosition = pullingHand.position + grabOffsetWorld;

            transform.position = targetPosition;
            stringBone.position = targetPosition;

            return;
        }

        stringBone.localPosition = Vector3.Lerp(
            stringBone.localPosition,
            restBoneLocalPosition,
            Time.deltaTime * returnSpeed
        );

        stringBone.localRotation = Quaternion.Slerp(
            stringBone.localRotation,
            restBoneLocalRotation,
            Time.deltaTime * returnSpeed
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            restGrabPointLocalPosition,
            Time.deltaTime * returnSpeed
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            restGrabPointLocalRotation,
            Time.deltaTime * returnSpeed
        );

        if (Vector3.Distance(transform.localPosition, restGrabPointLocalPosition) <= snapDistance)
        {
            transform.localPosition = restGrabPointLocalPosition;
            transform.localRotation = restGrabPointLocalRotation;

            stringBone.localPosition = restBoneLocalPosition;
            stringBone.localRotation = restBoneLocalRotation;
        }
    }

    public Vector3 GetPullVectorWorld()
    {
        return transform.position - transform.parent.TransformPoint(restGrabPointLocalPosition);
    }

    public float GetPullDistance()
    {
        return GetPullVectorWorld().magnitude;
    }
}