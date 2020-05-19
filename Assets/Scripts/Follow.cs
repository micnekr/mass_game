using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform toFollow;
    public Transform rotationFollow;

    public Transform[] othersToMove;

    public Vector3 offset;
    public Vector3 runtimeOffset;

    public Vector3 rotationOffset;

    // Update is called once per frame
    void LateUpdate()
    {
        if (rotationFollow != null)
        {
            transform.rotation = rotationFollow.rotation * Quaternion.Euler(rotationOffset);
            transform.position = toFollow.position + transform.rotation * (offset + runtimeOffset);
        }
        else
        {
            transform.position = toFollow.position + offset + runtimeOffset;

            foreach(Transform other in othersToMove)
            {
                other.position = transform.position;
            }
        }
    }
}
