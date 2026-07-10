using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRGrabInteractableRoutedAttach : XRGrabInteractable
{
    public Transform LeftAttachmentTransform;
    public Transform RightAttachmentTransform;
    public Transform SocketAttachmentTransform;   // if left empty, act as two pose attachment

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor && SocketAttachmentTransform != null)
            attachTransform = SocketAttachmentTransform;
        else if (args.interactorObject.transform.CompareTag("Left Hand"))
            attachTransform = LeftAttachmentTransform;
        else if (args.interactorObject.transform.CompareTag("Right Hand"))
            attachTransform = RightAttachmentTransform;

        base.OnSelectEntering(args);
    }
}