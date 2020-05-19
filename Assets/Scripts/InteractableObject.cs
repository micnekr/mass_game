using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public bool canCauseCameraShake = true;

    public float baseShakeMagnitude = 0.01f;

    public CameraShake cameraShake;

    private void OnCollisionEnter(Collision collision)
    {
        if (canCauseCameraShake && collision.impulse.magnitude > cameraShake.minShakeImpulse)
        {
            float shakeMagnitudeMultiplier = Mathf.Log10(collision.impulse.magnitude - cameraShake.minShakeImpulse);
            StartCoroutine(cameraShake.Shake(1.5f, baseShakeMagnitude * shakeMagnitudeMultiplier, 0.2f));
        }
    }
}
