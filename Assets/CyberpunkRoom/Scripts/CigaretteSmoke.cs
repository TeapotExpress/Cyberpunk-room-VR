using UnityEngine;

/// <summary>
/// Drives the smoke visuals for a cigarette. Owns two particle systems:
///   - smolder: continuous thin wisp from the burning tip, gated on the lit state.
///   - exhale:  optional one-shot puff, triggered manually (e.g. after a drag).
///
/// The smolder is a looping rate-over-time system. Instead of Play/Stop, the lit
/// state toggles emission.enabled, so already-spawned smoke keeps rising and fades
/// out on its own lifetime when the cigarette is extinguished, rather than popping.
/// Simulation space is expected to be World so the trail stays in the air as the
/// cigarette moves.
/// </summary>
[DisallowMultipleComponent]
public class CigaretteSmoke : MonoBehaviour
{
    [Header("Particle Systems")]
    [Tooltip("Continuous wisp rising from the burning tip. Looping, rate-over-time, World space.")]
    [SerializeField] private ParticleSystem smolder;

    [Tooltip("Optional one-shot exhale puff. Burst-only, Play On Awake OFF.")]
    [SerializeField] private ParticleSystem exhale;

    private bool _lit;

    /// <summary>True while the tip is smoldering and emitting the wisp.</summary>
    public bool Lit => _lit;

    private void Awake()
    {
        // Force a clean, non-emitting start regardless of how the prefab was authored.
        // The system is left Playing but with emission disabled, so toggling the lit
        // state later is a single cheap flag flip with no restart/prewarm cost.
        if (smolder != null)
        {
            var emission = smolder.emission;
            emission.enabled = false;
            smolder.Play();
        }
    }

    /// <summary>
    /// Enables or disables the smoldering wisp. Existing particles are left to finish
    /// their lifetime, so the smoke tapers off instead of vanishing when extinguished.
    /// </summary>
    public void SetLit(bool lit)
    {
        if (smolder == null || _lit == lit) return;
        _lit = lit;

        var emission = smolder.emission;
        emission.enabled = lit;
    }

    /// <summary>
    /// Fires a single exhale puff. Safe to call while unlit; the puff is a discrete
    /// burst and is independent of the smolder state.
    /// </summary>
    public void Exhale(int count = 12)
    {
        if (exhale == null) return;
        exhale.Emit(count);
    }
}