using System.Collections.Generic;
using UnityEngine;

public class CigarettePool : MonoBehaviour
{
    [SerializeField] private Cigarette prefab;
    [SerializeField] private int prewarm = 4;

    private readonly Queue<Cigarette> available = new();

    private void Awake()
    {
        for (int i = 0; i < prewarm; i++)
            available.Enqueue(CreateNew());
    }

    private Cigarette CreateNew()
    {
        var cig = Instantiate(prefab, transform);
        cig.gameObject.SetActive(false);
        return cig;
    }

    public Cigarette Get()
    {
        var cig = available.Count > 0 ? available.Dequeue() : CreateNew();
        cig.gameObject.SetActive(true);
        return cig;
    }

    public void Return(Cigarette cig)
    {
        if (cig == null) return;
        cig.OnReturnedToPool();
        cig.transform.SetParent(transform, false);
        cig.gameObject.SetActive(false);
        available.Enqueue(cig);
    }
}