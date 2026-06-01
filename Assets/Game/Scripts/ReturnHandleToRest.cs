using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ReturnHandleToRest : MonoBehaviour
{
    [Header("Return")]
    [SerializeField] private float returnSpeed = 12f;
    [SerializeField] private float snapDistance = 0.001f;
    [SerializeField] private float snapAngle = 0.1f;

    private XRGrabInteractable grabInteractable;

    private Vector3 restLocalPosition;
    private Quaternion restLocalRotation;

    private bool isGrabbed;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        restLocalPosition = transform.localPosition;
        restLocalRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    private void LateUpdate()
    {
        if (isGrabbed)
            return;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            restLocalPosition,
            Time.deltaTime * returnSpeed
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            restLocalRotation,
            Time.deltaTime * returnSpeed
        );

        if (Vector3.Distance(transform.localPosition, restLocalPosition) <= snapDistance &&
            Quaternion.Angle(transform.localRotation, restLocalRotation) <= snapAngle)
        {
            transform.localPosition = restLocalPosition;
            transform.localRotation = restLocalRotation;
        }
    }
}