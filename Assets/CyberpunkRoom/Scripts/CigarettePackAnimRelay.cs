using UnityEngine;
public class CigarettePackAnimRelay : MonoBehaviour
{
    [SerializeField] private CigarettePack pack;
 
    private void Reset()  => pack = GetComponentInParent<CigarettePack>();
    private void Awake()
    {
        if (pack == null) pack = GetComponentInParent<CigarettePack>();
    }
 
    // ANIM EVENT (klip wysuwania)
    public void OnPresentComplete() => pack.OnPresentComplete();
 
    // ANIM EVENT (klip chowania)
    public void OnRetractComplete() => pack.OnRetractComplete();
}


