using UnityEngine;

/// <summary>
/// Sits on the lighter's flame model — the object that carries the flame trigger
/// collider (isTrigger = true). While the lighter is lit (read from LighterVisual),
/// any cigarette whose collider stays inside the flame long enough gets ignited via
/// its CigaretteSmoke driver.
///
/// Design:
///   - Reads the flame state from LighterVisual instead of owning it, so there is a
///     single source of truth for "is the lighter burning".
///   - Uses OnTriggerStay so both orderings work: dipping an already-lit flame onto a
///     resting cigarette, AND holding a cigarette in the flame while flicking it on.
///   - Tracks a single dwell candidate (one cigarette at a time in a small flame), so
///     a brief brush-past does not light anything by accident.
/// </summary>
[DisallowMultipleComponent]
public class LighterFlameIgniter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The lighter this flame belongs to. Read-only source of the lit state.")]
    [SerializeField] private LighterVisual lighter;

    [Header("Ignition")]
    [Tooltip("Seconds a cigarette must stay in the flame before it lights. 0 = instant.")]
    [SerializeField] private float igniteDwell = 0.35f;

    // The cigarette's smoke driver currently sitting in the flame, and how long it has
    // been there. Single-candidate is enough for one small flame; two cigarettes wedged
    // into the same flame at once is not a case worth the extra bookkeeping.
    private CigaretteSmoke _candidate;
    private float _dwell;

    private void Reset()
    {
        // Convenience when the script is authored on a child flame object: auto-wire the
        // LighterVisual from a parent. Overridable in the Inspector.
        if (lighter == null) lighter = GetComponentInParent<LighterVisual>();
    }

    private void OnTriggerStay(Collider other)
    {
        // Cheapest gate first: no flame → nothing to do, and drop any stale dwell.
        if (lighter == null || !lighter.IsLit)
        {
            ResetCandidate();
            return;
        }

        // Is this a cigarette at all? The Cigarette component marks the grabbable root,
        // so this also filters out hands, controllers and the lighter's own colliders.
        var cig = other.GetComponentInParent<Cigarette>();
        if (cig == null) return;

        // Find its lit-state driver. Without one there is nothing to light.
        var smoke = cig.GetComponentInChildren<CigaretteSmoke>();
        if (smoke == null || smoke.Lit) return; // no driver, or already burning → ignore

        // New candidate → restart the dwell timer.
        if (_candidate != smoke)
        {
            _candidate = smoke;
            _dwell = 0f;
        }

        _dwell += Time.deltaTime;
        if (_dwell >= igniteDwell)
        {
            smoke.SetLit(true);
            ResetCandidate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the tracked cigarette leaves the flame before it lit, forget its dwell.
        if (_candidate == null) return;

        var cig = other.GetComponentInParent<Cigarette>();
        if (cig != null && cig.GetComponentInChildren<CigaretteSmoke>() == _candidate)
            ResetCandidate();
    }

    private void ResetCandidate()
    {
        _candidate = null;
        _dwell = 0f;
    }
}