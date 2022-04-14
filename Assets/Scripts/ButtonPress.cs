using UnityEngine;
using System.Collections;

public class ButtonPress : MonoBehaviour
{
    public float maxPerpendicularVelocityForDetecting = 0.01f;
    public float minActivationMass = 1f;

    public GameObject buttonTop;

    public Activatable activatable;

    public Collider buttonTopCollider;

    public float minImpulseForPush;

    private bool isPushedByImpulse = false;

    private Animator animator;

    private ArrayList gameobjectsInRange = new ArrayList();

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        bool isPressed;

        // if not pushed by impulse, check mass
        if (!isPushedByImpulse)
        {
            float totalMass = 0f;
            for (int i = 0; i < gameobjectsInRange.Count; i++)
            {
                Rigidbody otherRb = ((GameObject)gameobjectsInRange[i]).GetComponent<Rigidbody>();
                InteractableObject interactableObject = ((GameObject)gameobjectsInRange[i]).GetComponent<InteractableObject>();

                if (otherRb == null) continue;
                if (interactableObject == null) continue;

                //if held, check if touches the button
                if (interactableObject.pickUp != null)
                {
                    bool hasFound = false;
                    for (int j = 0; j < interactableObject.touchingObjects.Count; j++)
                    {
                        if (((GameObject)interactableObject.touchingObjects[j]).GetInstanceID() == buttonTop.GetInstanceID())
                        {
                            hasFound = true;
                        }
                    }
                    if (!hasFound) continue;
                }

                // gets local velocity 
                Vector3 localVel = transform.InverseTransformDirection(otherRb.velocity);

                if (Mathf.Abs(localVel.y) < maxPerpendicularVelocityForDetecting)
                {
                    totalMass += otherRb.mass;
                }
            }
            isPressed = minActivationMass <= totalMass;
        }
        else {
            isPressed = true;
            isPushedByImpulse = false;
        }

        activatable.isActivated = isPressed || animator.GetCurrentAnimatorStateInfo(0).IsName("buttonPress");
        animator.SetBool("isPressed", isPressed);

        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("stateChange"))
        {
            for (int i = 0; i < gameobjectsInRange.Count; i++)
            {
                Rigidbody otherRb = ((GameObject)gameobjectsInRange[i]).GetComponent<Rigidbody>();

                if (otherRb == null) continue;
                otherRb.WakeUp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Rigidbody>() != null)
        {
            gameobjectsInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        for(int i = 0; i < gameobjectsInRange.Count; i++)
        {
            // if same object
            if(((GameObject)gameobjectsInRange[i]).GetInstanceID() == other.gameObject.GetInstanceID())
            {
                gameobjectsInRange.RemoveAt(i);
            }
        }
    }

    public void otherCollided(Collision collision)
    {
        if(collision.impulse.magnitude > minImpulseForPush)
        {
            isPushedByImpulse = true;
        }
    }
}
