using UnityEngine;

public class MassGunShoot : MonoBehaviour
{

    public Camera cam;

    public float range = 100f;

    public float maxMassIncrease = 10f;
    public float maxMassDecrease = 0.1f;

    public float massIncreaseStepPercentage = 0.3f;

    public float tipDisplacementForward;

    public float laserModeSwitchDist;

    public LineRenderer addLine, subLine;

    public LayerMask layersToHit;

    private Collider otherCollider = null;

    private DecalController decalController;

    [HideInInspector]
    public Quaternion gunQuaternionUpClose;

    private void Start()
    {
        addLine.enabled = false;
        subLine.enabled = false;

        decalController = GetComponent<DecalController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (gunQuaternionUpClose != null)
        {
            Vector3 endPointPos = cam.transform.position + cam.transform.forward * laserModeSwitchDist;

            gunQuaternionUpClose = Quaternion.FromToRotation(cam.transform.forward, endPointPos - transform.position - transform.forward * tipDisplacementForward);
        }
        if (Input.GetButton("Fire1"))
        {
            Shoot(1 * Time.deltaTime);
        }else if (Input.GetButton("Fire2"))
        {
            Shoot(-1 * Time.deltaTime);
        }
        else
        {
            //hide the beam
            addLine.enabled = false;
            subLine.enabled = false;
        }
    }

    private void Shoot(float massTimesChange)
    {
        RaycastHit hitClose;
        RaycastHit hitFar;
        bool hasCloseHit, hasFarHit;

        LineRenderer line = massTimesChange > 0 ? addLine : subLine;

        if (otherCollider != null)
        {
            //hide both
            addLine.enabled = false;
            subLine.enabled = false;

            otherCollider.gameObject.GetComponent<InteractableObject>().ChangeMass(massTimesChange, maxMassDecrease, maxMassIncrease, massIncreaseStepPercentage);

            return;
        }

        hasFarHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out hitFar, range, layersToHit, QueryTriggerInteraction.Ignore);
        hasCloseHit = Physics.Raycast(transform.position + transform.forward * tipDisplacementForward, gunQuaternionUpClose * cam.transform.forward, out hitClose, range, layersToHit, QueryTriggerInteraction.Ignore);
        if ((hasCloseHit && hitFar.distance < laserModeSwitchDist && hitClose.distance < laserModeSwitchDist) || (hasFarHit && hitFar.distance >= laserModeSwitchDist))
        {
            RaycastHit hit = hasCloseHit && hitFar.distance < laserModeSwitchDist ? hitClose : hitFar;

            InteractableObject target = hit.transform.GetComponent<InteractableObject>();

            //hide both, reveal one
            addLine.enabled = false;
            subLine.enabled = false;
            // show the beam
            line.enabled = true;
            line.SetPosition(0, transform.position + transform.forward * tipDisplacementForward);
            line.SetPosition(1, hit.point);

            if (target == null || target.canMassBeChanged == false)
            {
                // show the burn mark
                decalController.spawnDecal(hit);
                return;
            };

            target.ChangeMass(massTimesChange, maxMassDecrease, maxMassIncrease, massIncreaseStepPercentage);
        }
        else
        {
            //hide both, reveal one
            addLine.enabled = false;
            subLine.enabled = false;
            // show the beam
            line.enabled = true;
            line.SetPosition(0, transform.position + transform.forward * tipDisplacementForward);
            line.SetPosition(1, transform.position + cam.transform.forward * (tipDisplacementForward + range));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<InteractableObject>() == null) return;
        otherCollider = other;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<InteractableObject>() == null) return;
        otherCollider = other;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<InteractableObject>() == null) return;
        otherCollider = null;
    }
}
