using UnityEngine;

public class PickUp : MonoBehaviour
{
    public MassGunShoot massGunShoot;

    public Rigidbody rb;

    public float range;

    public float forceMultiplier;

    public float offsetBeforeCamera;

    public float newDrag;

    public float maxDistForFreeze;

    public float maxMass;

    public float maxTimeNotSeenUntilDrop;

    public float maxSpeed;

    public float rotationMultiplier;

    private bool canManuallyRelease = true;

    private GameObject lockedGameObject = null;

    private Rigidbody otherRb;

    private InteractableObject interactableObject;

    private bool locked = false;

    private bool prevUseGravity;

    private float prevMass;

    private float prevDrag;

    private float timeNotSeen = 0f;

    private CollisionDetectionMode prevCollisionDetectionMode;

    private Vector3 lastFramePos;

    private GameObject copy;

    void LateUpdate()
    {
        // trying to hold onto it, but it is too far
        if (lockedGameObject != null)
        {
            Vector3 towardsObject = (transform.position + transform.forward * offsetBeforeCamera) - lockedGameObject.transform.position;

            // if too far away
            if (towardsObject.sqrMagnitude > range * range)
            {
                ResetObj();
            }


            // check if still raycasted
            RaycastHit hit;
            bool hasHit = Physics.Raycast(transform.position, transform.forward, out hit, 10 * range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (!hasHit || hit.transform.gameObject.GetInstanceID() != lockedGameObject.GetInstanceID())
            {
                timeNotSeen += Time.deltaTime;
                if(timeNotSeen > maxTimeNotSeenUntilDrop)
                {
                    //break
                    ResetObj();
                    return;
                }
            }
            else
            {
                timeNotSeen = 0f;
            }

            // if can freeze
            if (towardsObject.sqrMagnitude < maxDistForFreeze * maxDistForFreeze)
            {
                locked = true;
                copy.GetComponent<MeshRenderer>().enabled = true;
                lockedGameObject.GetComponent<MeshRenderer>().enabled = false;
            }

            //can use force?
            if (!locked)
            {
                otherRb.AddForce(towardsObject * otherRb.mass * Time.deltaTime * forceMultiplier);
            }
            else
            {
                //otherRb.velocity = Vector3.zero;
                copy.transform.position = transform.position + transform.forward * offsetBeforeCamera;
            }

            if(interactableObject.touchingObjects.Count == 0)
            {
                otherRb.angularVelocity = Vector3.zero;
                lockedGameObject.transform.rotation = Quaternion.Slerp(lockedGameObject.transform.rotation, transform.rotation, rotationMultiplier * Time.deltaTime);
            }
            copy.transform.rotation = lockedGameObject.transform.rotation;
        }

        // if the button pressed
        if (Input.GetButton("Submit"))
        {
            // only change state if it can
            if (canManuallyRelease)
            {
                canManuallyRelease = false;
                // if locking
                if (lockedGameObject == null)
                {
                    RaycastHit hit;
                    bool hasHit = Physics.Raycast(transform.position, transform.forward, out hit, 10 * range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

                    if (!hasHit) return;

                    //if too far away, return
                    Vector3 towardsObject = transform.position - hit.transform.position;

                    // if too far away
                    if (towardsObject.sqrMagnitude > range * range) return;

                    //if hit, check if interactable
                    if (hit.transform.gameObject.GetComponent<InteractableObject>() != null)
                    {
                        //check if max mass exceeded
                        otherRb = hit.transform.gameObject.GetComponent<Rigidbody>();
                        if(otherRb.mass > maxMass)
                        {
                            return;
                        }

                        lockedGameObject = hit.transform.gameObject;

                        prevUseGravity = otherRb.useGravity;

                        otherRb.useGravity = false;

                        prevMass = rb.mass;

                        rb.mass += otherRb.mass;

                        prevDrag = otherRb.drag;

                        otherRb.drag = newDrag;

                        locked = false;

                        interactableObject = lockedGameObject.GetComponent<InteractableObject>();
                        interactableObject.pickUp = this;

                        prevCollisionDetectionMode = otherRb.collisionDetectionMode;
                        otherRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                        lastFramePos = lockedGameObject.transform.position;

                        // create a copy
                        copy =  Instantiate(lockedGameObject);

                        foreach (var comp in copy.GetComponents<Component>())
                        {
                            //Don't remove the Transform component
                            if (!(comp is Transform) && !(comp is MeshFilter) && !(comp is MeshRenderer))
                            {
                                DestroyImmediate(comp);
                            }
                        }

                        copy.GetComponent<MeshRenderer>().enabled = false;
                    }
                }
                // if unlocking
                else
                {
                    ResetObj();
                }
            }
        }
        // if the button has been released
        else
        {
            canManuallyRelease = true;
        }
    }

    private void FixedUpdate()
    {
        if(lockedGameObject != null && locked){
            otherRb.angularVelocity = Vector3.zero;

            lastFramePos = lockedGameObject.transform.position;

            Vector3 towardsObject = (transform.position + transform.forward * offsetBeforeCamera) - lockedGameObject.transform.position;
            otherRb.velocity = towardsObject / Time.fixedDeltaTime;
        }
    }

    private void ResetObj()
    {
        otherRb.useGravity = prevUseGravity;
        rb.mass = prevMass;

        otherRb.drag = prevDrag;

        interactableObject.pickUp = null;

        interactableObject = null;

        otherRb.collisionDetectionMode = prevCollisionDetectionMode;

        Destroy(copy);
        lockedGameObject.GetComponent<MeshRenderer>().enabled = true;

        otherRb.velocity = Vector3.ClampMagnitude(otherRb.velocity, maxSpeed);

        lockedGameObject = null;
        otherRb = null;
    }

    public void CheckMass()
    {
        if(otherRb.mass > maxMass)
        {
            ResetObj();
        }
    }

    public void LockedObjectCollided()
    {
        if (locked)
        {
            lockedGameObject.transform.position = lastFramePos;

            copy.GetComponent<MeshRenderer>().enabled = false;
            lockedGameObject.GetComponent<MeshRenderer>().enabled = true;

            locked = false;
        }
    }
}