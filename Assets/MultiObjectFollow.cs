using UnityEngine;

public class MultiObjectFollow : MonoBehaviour
{
    public Transform transform1;
    public Transform transform2;

    void Update()
    {
        transform.position = (transform1.position + transform2.position) * 0.5f;
    }
}