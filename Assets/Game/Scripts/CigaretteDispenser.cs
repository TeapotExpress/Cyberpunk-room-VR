using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CigaretteDispenser : MonoBehaviour
{
    [Header("Box")]
    [SerializeField] XRGrabInteractable boxInteractable;
    [SerializeField] Animator boxAnimator;
    [SerializeField] string openTrigger = "Open";

    [Header("Slot")]
    [SerializeField] XRGrabInteractable slotInteractable;

    [Header("Cigarette")]
    [SerializeField] GameObject cigarettePrefab;
    [SerializeField] Transform spawnPoint;

    private bool _dispensedThisHold;

    void OnEnable()  => slotInteractable.selectEntered.AddListener(OnSlotGrabbed);
    void OnDisable() => slotInteractable.selectEntered.RemoveListener(OnSlotGrabbed);

    void Update()
    {
        bool boxHeld = boxInteractable.isSelected;

        if (!boxHeld)
            _dispensedThisHold = false;

        // Slot is only active while box is held and no cigarette has been dispensed yet
        bool shouldBeActive = boxHeld && !_dispensedThisHold;
        if (slotInteractable.gameObject.activeSelf != shouldBeActive)
            slotInteractable.gameObject.SetActive(shouldBeActive);
    }

    void OnSlotGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor) return;

        _dispensedThisHold = true;
        // Update() deactivates the slot next frame, which causes XRIT to call SelectExit
        // on the slot and free the interactor. The cigarette spawns at the slot position
        // (right where the hand is), so XRIT's natural hover detection grabs it automatically.
        Instantiate(cigarettePrefab, spawnPoint.position, spawnPoint.rotation);

        if (boxAnimator != null)
            boxAnimator.SetTrigger(openTrigger);
    }
}
