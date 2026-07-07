using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRGrabInteractableTwoAttachment : XRGrabInteractable
{
    public Transform LeftAttachmentTransform;
    public Transform RightAttachmentTransform;
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.CompareTag("Left Hand"))
        {
            attachTransform = LeftAttachmentTransform;
        }
        else if (args.interactorObject.transform.CompareTag("Right Hand"))
        {
            attachTransform = RightAttachmentTransform;
        }
        base.OnSelectEntered(args); 
    }
}

