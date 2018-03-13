using UnityEngine;

public class NameFollow : MonoBehaviour
{
    public Transform target;
    public bool canMove = false;

    void Update()
    {
        if (canMove && target != null)
        {
            transform.position = target.position;
        }
    }
}
