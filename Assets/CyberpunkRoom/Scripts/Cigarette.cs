using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;               // SelectExitEventArgs
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRGrabInteractable (XRI 3.0+)

/// <summary>
/// Pojedynczy papieros. Normalny XRGrabInteractable, którym łapie ręka B.
/// Zarządza tylko własnym stanem "prezentowany" vs "łapalny". Dalszy los po
/// złapaniu (palenie, upuszczenie, despawn) należy do osobnej logiki.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class Cigarette : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable interactable;
    [SerializeField] private Rigidbody body;
    [SerializeField] private Collider grabCollider;

    public XRGrabInteractable Interactable => interactable;

    private void Reset()  => CacheRefs();
    private void Awake()  => CacheRefs();

    private void OnEnable()
    {
    
        interactable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        interactable.selectExited.RemoveListener(OnReleased);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (body == null) return;
        body.transform.parent = null;
        body.isKinematic = false;
        body.useGravity = true;
    }

    private void CacheRefs()
    {
        if (interactable == null) interactable = GetComponent<XRGrabInteractable>();
        if (body == null)         body = GetComponent<Rigidbody>();
        if (grabCollider == null) grabCollider = GetComponent<Collider>();
    }


    public void EnterPresentedMode()
    {
        if (body != null)
        {
            body.isKinematic = true;
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = Vector3.zero;
#else
            body.velocity = Vector3.zero;
#endif
            body.angularVelocity = Vector3.zero;
        }
        SetGrabbable(false);
    }

    public void SetGrabbable(bool value)
    {
        if (grabCollider != null) grabCollider.enabled = value;
    }

    public void OnReturnedToPool()
    {
        SetGrabbable(false);
    }
}