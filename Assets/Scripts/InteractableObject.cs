using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour
{
    public bool canMassBeChanged = true;

    public bool canCauseCameraShake = true;

    public float baseShakeMagnitude = 0.01f;

    public CameraShake cameraShake;

    private float initMass;
    private Rigidbody rb;

    [HideInInspector]
    public PickUp pickUp;

    [HideInInspector]
    public ArrayList touchingObjects = new ArrayList();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            initMass = rb.mass;
        }
    }

    public void ChangeMass(float timesMass, float maxMassDecrease, float maxMassIncrease, float massIncreaseStepPercentage)
    {
        if (!canMassBeChanged) return;

        if (rb != null)
        {
            float changeInMass = rb.mass < initMass ? (1 - maxMassDecrease) * massIncreaseStepPercentage * initMass : (maxMassIncrease - 1) * massIncreaseStepPercentage * initMass;

            rb.mass += timesMass * changeInMass;

            rb.mass = Mathf.Clamp(rb.mass, maxMassDecrease * initMass, maxMassIncrease * initMass);
        }

        //alert the carrier if the mass changed
        if (pickUp != null)
        {
            pickUp.CheckMass();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (canCauseCameraShake && collision.impulse.magnitude > cameraShake.minShakeImpulse)
        {
            float shakeMagnitudeMultiplier = Mathf.Log10(collision.impulse.magnitude - cameraShake.minShakeImpulse);
            StartCoroutine(cameraShake.Shake(1.5f, baseShakeMagnitude * shakeMagnitudeMultiplier, 0.2f));
        }

        touchingObjects.Add(collision.gameObject);

        if(pickUp != null)
        {
            pickUp.LockedObjectCollided();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        for (int i = 0; i < touchingObjects.Count; i++)
        {
            // if same object
            if (((GameObject)touchingObjects[i]).GetInstanceID() == other.gameObject.GetInstanceID())
            {
                touchingObjects.RemoveAt(i);
            }
        }
    }
}
