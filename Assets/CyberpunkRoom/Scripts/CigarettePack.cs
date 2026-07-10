using System;
using System.Xml;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Pack of smokes as a dispenser. Held by hand A. After activation dispenses a single cigarette with animiation.
/// Forces Hand B to grab the cigarette instead of packOfSmokes by filtering XRGrabInteractable
/// </summary>
public class CigarettePack : MonoBehaviour, IXRSelectFilter
{
    enum PresentationState {Empty, Presenting, Ready, Retracting}
    
    [Header("References")]
    [SerializeField] private XRGrabInteractable packInteractable;
    [SerializeField] private Animator packAnimator;
    [SerializeField] private Transform presentationSlot;
    [SerializeField] private CigarettePool pool;
    
    [Header("Config")]
    [SerializeField] private int startingCount = 20;
    [SerializeField] private string openBoolParameter = "isOpen";
    
    // Runtime
    private PresentationState state = PresentationState.Empty;
    private bool isOpen;
    private int remainingCigarettes;
    private Cigarette currentCigarette;
    private int isOpenHash;
    
    // IXRSelectFilter
    public bool canProcess => isActiveAndEnabled;

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (!packInteractable.isSelected) return true;
        
        return packInteractable.interactorsSelecting.Contains(interactor);
    }
    // Lifecycle
    public void Reset()
    {
        packInteractable = GetComponent<XRGrabInteractable>();
        if (packAnimator == null) packAnimator = GetComponentInChildren<Animator>();
    }

    public void Awake()
    {
        if (packInteractable == null) packInteractable = GetComponent<XRGrabInteractable>();
        if (packAnimator == null) packAnimator = GetComponentInChildren<Animator>();
        isOpenHash = Animator.StringToHash("isOpen");
        remainingCigarettes = startingCount;
    }

    public void OnEnable()
    {
        packInteractable.selectEntered.AddListener(OnPackGrabbed);
        packInteractable.selectExited.AddListener(OnPackReleased);
        packInteractable.activated.AddListener(OnOpen);
        packInteractable.deactivated.AddListener(OnClose);
        
        packInteractable.selectFilters.Add(this);
    }

    public void OnDisable()
    {
        packInteractable.selectEntered.RemoveListener(OnPackGrabbed);
        packInteractable.selectExited.RemoveListener(OnPackReleased);
        packInteractable.activated.RemoveListener(OnOpen);
        packInteractable.deactivated.RemoveListener(OnClose);
        
        packInteractable.selectFilters.Remove(this);
        UnsubscribeCurrent();
    }
    
    // Pack Events

    private void OnPackGrabbed(SelectEnterEventArgs args)
    {
        state = PresentationState.Empty;
        isOpen = false;
    }

    private void OnPackReleased(SelectExitEventArgs args)
    {
        isOpen = false;
        packAnimator.SetBool(openBoolParameter, false);
        RecallCurrentToPool();
        state = PresentationState.Empty;
    }

    private void OnOpen(ActivateEventArgs args)
    {
        if (!packInteractable.isSelected || remainingCigarettes <= 0 || isOpen) return;
        isOpen = true;
        packAnimator.SetBool(openBoolParameter, true);

        if (state == PresentationState.Empty)
            Present();
    }

    private void OnClose(DeactivateEventArgs args)
    {
        if (!isOpen)  return;
        isOpen = false;
        packAnimator.SetBool(openBoolParameter, false);

        if (state == PresentationState.Presenting || state == PresentationState.Ready)
            state = PresentationState.Retracting;
        else
            state = PresentationState.Empty;
    }
    
    // Presentation
    private void Present()
    {
        if (remainingCigarettes <= 0 || state != PresentationState.Empty) return;
        if (pool == null || presentationSlot == null) return;

        currentCigarette = pool.Get();
        if (currentCigarette == null) return;

        var t = currentCigarette.transform;
        t.SetParent(presentationSlot, false);
        t.localPosition =  Vector3.zero;
        t.localRotation =  Quaternion.identity;

        currentCigarette.EnterPresentedMode();
        currentCigarette.Interactable.selectEntered.AddListener(OnCigaretteTaken);
        
        state = PresentationState.Presenting;
    }
    
    // Animation Event on opening animation
    public void OnPresentComplete()
    {
        if (state != PresentationState.Presenting) return;
        state = PresentationState.Ready;
        currentCigarette?.SetGrabbable(true);
    }

    private void OnCigaretteTaken(SelectEnterEventArgs args)
    {
        var taken = currentCigarette;
        UnsubscribeCurrent();

        taken.transform.SetParent(null, true);
        currentCigarette = null;
        remainingCigarettes--;
        state = PresentationState.Empty;
    }
    
    // Animation Event on close animation
    public void OnRetractComplete()
    {
        if (state != PresentationState.Retracting) return;
        RecallCurrentToPool();
        state = PresentationState.Empty;
    }
    
    // Helper methods
    private void RecallCurrentToPool()
    {
        if (currentCigarette == null) return;
        UnsubscribeCurrent();
        pool.Return(currentCigarette);
        currentCigarette =  null;
    }

    private void UnsubscribeCurrent()
    {
        if (currentCigarette != null)
            currentCigarette.Interactable.selectEntered.RemoveListener(OnCigaretteTaken);
    }
}