using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    public float minShakeImpulse = 1000f;
    public Transform trackedTransform;

    public IEnumerator Shake(float duration, float magnitude, float maxPointTimePercent)
    {
        float timeUntilMax = maxPointTimePercent * duration;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);

            float currentMagnitude;

            if (elapsed <= timeUntilMax)
            {
                currentMagnitude = Mathf.SmoothStep(0, magnitude, elapsed / timeUntilMax);
            }
            else
            {
                currentMagnitude = Mathf.SmoothStep(magnitude, 0, (elapsed - timeUntilMax) / (duration - timeUntilMax));
            }

            transform.position = trackedTransform.position + new Vector3(x, y, 0).normalized * currentMagnitude;
            elapsed += Time.deltaTime;

            yield return null;
        }
    }
}
