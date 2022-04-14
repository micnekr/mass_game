using UnityEngine;

public class ColliderConnector : MonoBehaviour
{
    public ButtonPress other;

    private void OnCollisionEnter(Collision collision)
    {
        other.otherCollided(collision);
    }
}
