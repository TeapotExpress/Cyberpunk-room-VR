using UnityEngine;

public class XRHandData : MonoBehaviour
{
    public enum HandType { Left, Right }

    [Header("Hand Configuration")]
    public HandType handType;
    public Transform mainHandBone;  // The main hand bone (used for positioning the hand)
    public Animator animator;

    [Header("Finger Bones")]
    public Transform[] thumbBones;
    public Transform[] indexBones;
    public Transform[] middleBones;
    public Transform[] ringBones;
    public Transform[] pinkyBones;

    [Header("Finger Tip Bones")]
    public Transform thumbTip;
    public Transform indexTip;
    public Transform middleTip;
    public Transform ringTip;
    public Transform pinkyTip;

    [HideInInspector] public GameObject initialHandPosition; // Initial hand position GameObject

    void Start()
    {
        // Create InitialHandPosition as a sibling of mainHandBone
        initialHandPosition = new GameObject("InitialHandPosition");
        initialHandPosition.transform.SetParent(mainHandBone.parent); // Sibling to mainHandBone
        initialHandPosition.transform.localPosition = mainHandBone.localPosition;
        initialHandPosition.transform.localRotation = mainHandBone.localRotation;
        //initialHandPosition.transform.localScale = mainHandBone.localScale;
    }
}
