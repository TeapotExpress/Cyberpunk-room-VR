using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;                 // event args (Select/Activate/Deactivate)
using UnityEngine.XR.Interaction.Toolkit.Interactables;   // XRBaseInteractable (XRI 3.0)
using UnityEngine.XR.Interaction.Toolkit.Interactors;     // InteractorHandedness

/// <summary>
/// Lighter visual with a flint wheel. Opens on XR Activate, closes on Deactivate,
/// and ignites the flame with the primary button of the HOLDING hand (A when held
/// in the right hand, X in the left) — but only while open. Deactivate or dropping
/// the lighter always extinguishes the flame and shuts the lid.
/// Source of truth are the private booleans _isOpen / _isFlame — the Animator
/// parameters are pushed from them.
/// </summary>
[RequireComponent(typeof(Animator))]
public class LighterVisual : MonoBehaviour
{
    private enum LighterState { Closed, Opening, Open, Flame, Closing }

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Spark VFX")]
    [SerializeField] private ParticleSystem sparks;
    [SerializeField] private int sparksPerStrike = 20;
    [SerializeField] private int sparksVariation = 8;

    [Header("Input (XRI 3.0)")]
    [Tooltip("Grab Interactable we read Select and Activate/Deactivate (trigger) from.")]
    [SerializeField] private XRBaseInteractable interactable;
    [Tooltip("Ignite action for the LEFT hand (X button / left primaryButton).")]
    [SerializeField] private InputActionReference igniteActionLeft;
    [Tooltip("Ignite action for the RIGHT hand (A button / right primaryButton).")]
    [SerializeField] private InputActionReference igniteActionRight;

    // --- SOURCE OF TRUTH ---
    private bool _isOpen;
    private bool _isFlame;
    private LighterState _state = LighterState.Closed;

    public bool IsLit => _isFlame;
    // Ignite action bound to whichever hand currently holds the lighter.
    private InputActionReference _activeIgnite;

    private static readonly int OpenHash  = Animator.StringToHash("isOpen");
    private static readonly int FlameHash = Animator.StringToHash("isFlame");

    private void Reset() => animator = GetComponent<Animator>();

    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
            interactable.activated.AddListener(OnActivated);
            interactable.deactivated.AddListener(OnDeactivated);
        }
        ApplyAnimatorParams(); // sync the visual with the current state
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
            interactable.activated.RemoveListener(OnActivated);
            interactable.deactivated.RemoveListener(OnDeactivated);
        }
        UnbindIgnite();
    }

    // ---------- SELECT: bind ignite to the holding hand ----------
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // handedness is exposed on IXRInteractor, so no cast is needed here.
        BindIgnite(ResolveIgnite(args.interactorObject.handedness));
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        UnbindIgnite();

        // Dropped mid-use? Force it shut so we never leave a floating flame.
        if (_state != LighterState.Closed && _state != LighterState.Closing)
            Close();
    }

    // Pick the ignite action matching the holding hand; fall back to whatever is assigned.
    private InputActionReference ResolveIgnite(InteractorHandedness handedness) => handedness switch
    {
        InteractorHandedness.Left  => igniteActionLeft,
        InteractorHandedness.Right => igniteActionRight,
        _ => igniteActionRight != null ? igniteActionRight : igniteActionLeft
    };

    private void BindIgnite(InputActionReference reference)
    {
        UnbindIgnite();
        if (reference == null) return;

        _activeIgnite = reference;
        _activeIgnite.action.performed += OnIgnitePressed;
        _activeIgnite.action.Enable();
    }

    private void UnbindIgnite()
    {
        if (_activeIgnite == null) return;

        // Only detach our handler; leave the action enabled so we don't clobber a
        // shared action that the Input Action Manager may also be driving.
        _activeIgnite.action.performed -= OnIgnitePressed;
        _activeIgnite = null;
    }

    // ---------- INPUT HANDLERS ----------
    private void OnActivated(ActivateEventArgs _)     => Open();
    private void OnDeactivated(DeactivateEventArgs _) => Close();
    private void OnIgnitePressed(InputAction.CallbackContext _) => TryIgnite();

    // ---------- PUBLIC ACTIONS ----------
    public void Open()
    {
        // Can only open from Closed, or while closing (reverse mid-motion).
        if (_state == LighterState.Open || _state == LighterState.Opening || _state == LighterState.Flame)
            return;
        SetState(LighterState.Opening);
    }

    public void Close()
    {
        if (_state == LighterState.Closed || _state == LighterState.Closing)
            return;
        SetState(LighterState.Closing);
    }

    public void TryIgnite()
    {
        if (_state == LighterState.Flame)
        {
            EmitSparks();
            return;
        }
        if (_state != LighterState.Open) return; // flame ONLY when fully open
        SetState(LighterState.Flame);
    }

    /// <summary>Spark burst — fired on ignition, but public so it can also be
    /// triggered from an Animation Event on the frame the wheel strikes the flint.</summary>
    public void EmitSparks()
    {
        if (sparks == null) return;
        int count = sparksPerStrike + Random.Range(-sparksVariation, sparksVariation);
        sparks.Emit(Mathf.Max(1, count));
    }

    // ---------- STATE MACHINE ----------
    private void SetState(LighterState next)
    {
        _state = next;
        switch (next)
        {
            case LighterState.Opening:
                _isOpen  = true;   // Animator starts playing the open clip
                _isFlame = false;
                break;

            case LighterState.Open:
                _isOpen  = true;
                break;

            case LighterState.Flame:
                _isFlame = true;
                break;

            case LighterState.Closing:
                _isOpen  = false;  // retract the lid...
                _isFlame = false;  // ...and put out the flame
                break;

            case LighterState.Closed:
                _isOpen  = false;
                _isFlame = false;
                break;
        }
        ApplyAnimatorParams();
    }

    private void ApplyAnimatorParams()
    {
        if (animator == null) return;
        animator.SetBool(OpenHash,  _isOpen);
        animator.SetBool(FlameHash, _isFlame);
    }

    // ---------- ANIMATION EVENTS ----------
    // Call from the LAST frame of the open clip:
    public void OnOpenComplete()
    {
        if (_state == LighterState.Opening) SetState(LighterState.Open);
    }

    // Call from the LAST frame of the close clip:
    public void OnCloseComplete()
    {
        if (_state == LighterState.Closing) SetState(LighterState.Closed);
    }
}