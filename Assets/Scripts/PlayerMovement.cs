using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // movement
    public float maxSlopeAngle = 30f;

    public float groundSpeedResistanceCooficient = 0.2f;

    public float groundMovementAcceleration = 2f;
    public float airMovementAcceleration = 1f;

    public float maxMovableSpeed = 20f;

    // Jumping and falling
    public float jumpForce = 200f;
    public float addedGravity = 5f;
    public float jumpCooldown = 0.5f;

    // effects

    // dust
    public float minImpulseForDust = 15f;
    public ParticleSystem dust;


    // speed lines
    public float minSpeedForSpeedlines = 15f;
    public ParticleSystem speedLines;
    public Transform speedLinesContainer;

    // landing camera animations
    public float minImpulseForHeadDip = 6f;
    public float maxCameraDipDistance = -0.5f;
    public float headDipDampening = 0.5f;
    public float headDipReturnThreshold = 0.001f;
    public float cameraReturnTime = 0.5f;
    public float gunDipMultiplier = 1.2f;
    public float resetDipThreshold;
    public Follow headFollowScript;
    public Follow gunFollowScript;

    public Animator gunWalkAnimator;

    private bool isOnGround = false;

    private bool isCamDipping = false;
    private bool isCamReturning = false;

    private float timeSinceCameraStartedReturn;
    private float timeSinceLastJump = 0f;


    private float xInput, yInput;
    private Vector3 desiredDir;
    private bool jumpPressed;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        desiredDir = new Vector3(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        jumpPressed = Input.GetButton("Jump");

        // if the camera is dipping, move the camera
        if (isCamDipping)
        {
            Debug.Log("dipping");
            if (isCamReturning)
            {
                headFollowScript.runtimeOffset.y = Mathf.SmoothStep(maxCameraDipDistance, 0, timeSinceCameraStartedReturn/cameraReturnTime);
                gunFollowScript.runtimeOffset.y = headFollowScript.runtimeOffset.y * gunDipMultiplier;
                timeSinceCameraStartedReturn += Time.deltaTime;
            }
            else
            {
                float headDipSpeed = (maxCameraDipDistance - headFollowScript.runtimeOffset.y) * headDipDampening;
                headFollowScript.runtimeOffset.y += headDipSpeed;
                gunFollowScript.runtimeOffset.y = headFollowScript.runtimeOffset.y * gunDipMultiplier;
            }

            if(Mathf.Abs(headFollowScript.runtimeOffset.y - maxCameraDipDistance) < headDipReturnThreshold && !isCamReturning)
            {
                isCamReturning = true;
                timeSinceCameraStartedReturn = 0f;
            }

            // when reset
            if(Mathf.Abs(headFollowScript.runtimeOffset.y) < resetDipThreshold)
            {
                isCamDipping = false;
            }
        // if walking, play the gun anim
        }else if ((xInput != 0 || yInput != 0) && isOnGround)
        {
            Debug.Log("E");
            gunWalkAnimator.SetBool("isWalking", true);
        }else gunWalkAnimator.SetBool("isWalking", false);

        if (speedLines.isPlaying) speedLinesContainer.rotation = Quaternion.FromToRotation(Vector3.back, rb.velocity);

        // speed lines
        if(speedLines.isPlaying && rb.velocity.magnitude < minSpeedForSpeedlines)
        {
            speedLines.Stop();
        }

        if (!speedLines.isPlaying && rb.velocity.magnitude >= minSpeedForSpeedlines)
        {
            speedLines.Play();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        // Better fall
        if (rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.down * addedGravity);
        }

        // Move
        // gets local velocity 
        Vector3 localVel = transform.InverseTransformDirection(rb.velocity);

        // limit the speed
        if (maxMovableSpeed < localVel.x && localVel.x > 0 || -maxMovableSpeed > localVel.x && localVel.x < 0) xInput = 0;
        if (maxMovableSpeed < localVel.z && localVel.z > 0 || -maxMovableSpeed > localVel.z && localVel.z < 0) yInput = 0;

        desiredDir.Set(xInput, 0f, yInput);

        float accelerationInConditions = isOnGround ? groundMovementAcceleration : airMovementAcceleration;

        float desiredDirMagnitude = desiredDir.magnitude;
        desiredDir.Normalize();
        desiredDir *= Mathf.Clamp(desiredDirMagnitude * accelerationInConditions, 0, accelerationInConditions);

        rb.AddForce(transform.forward * desiredDir.z * Time.fixedDeltaTime);
        rb.AddForce(transform.right * desiredDir.x * Time.fixedDeltaTime);

        cancelMovement(localVel);

        //air resistance
        //Vector3 airResistanceForce = -Vector3.Slerp(rb.velocity, maxSpeedInConditions * rb.velocity.normalized, slowingDownRate) * Time.fixedDeltaTime;
        // proportional to speed 2.  +1 to make sure it stops completely
        //Vector3 airResistanceForce = -rb.velocity * (rb.velocity.magnitude + 1) * speedResistanceCooficientInConditions;

        //rb.AddForce(airResistanceForce);


        /*// gets local velocity 
        Vector3 localVel = transform.InverseTransformDirection(rb.velocity);

        // get the vector to negate the speed and have own velocity
        desiredDir -= localVel * velocityNegationMultiplier;

        // limit it
        float desiredDirMagnitude = desiredDir.magnitude;
        desiredDir.Normalize();
        desiredDir *= Mathf.Clamp(desiredDirMagnitude * maxMoveSpeed, 0, maxMoveSpeed);

        if (xInput==0 && yInput == 0)
        {
            desiredDir *= stopSpeedMultiplier;

            // while flying, don't try to stop
            if (!isOnGround)
            {
                desiredDir *= 0;
            }
        }

        // slower movement in air
        if (!isOnGround)
        {
            desiredDir *= airSpeedMultiplier;
        }

        rb.AddForce(transform.forward * desiredDir.z * accelerationSpeed * Time.fixedDeltaTime);
        rb.AddForce(transform.right * desiredDir.x * accelerationSpeed * Time.fixedDeltaTime);
*/
        //Jump
        timeSinceLastJump += Time.fixedDeltaTime;

        if (jumpPressed && timeSinceLastJump > jumpCooldown)
        {
            Jump();
            timeSinceLastJump = 0f;
        }
    }

    private void cancelMovement(Vector3 relativeVelocity)
    {
        if (!isOnGround) return;

        Vector3 resistanceForce = new Vector3(0, 0, 0);

        if (xInput == 0 || Mathf.Sign(xInput) != Mathf.Sign(relativeVelocity.x))
        {
            resistanceForce += transform.right * - relativeVelocity.x * groundMovementAcceleration * Time.fixedDeltaTime * groundSpeedResistanceCooficient;
        }
        if (yInput == 0 || Mathf.Sign(yInput) != Mathf.Sign(relativeVelocity.z))
        {
            resistanceForce += transform.forward * -relativeVelocity.z * groundMovementAcceleration * Time.fixedDeltaTime * groundSpeedResistanceCooficient;
        }

        rb.AddForce(resistanceForce);
    }

    void Jump()
    {
        if (isOnGround)
        {
            rb.AddForce(Vector3.up * jumpForce);
        }
    }

    private bool CheckIsOnGround()
    {        
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.4f, Vector3.down, out hit, 1.1f))
        {
            float angle = Vector3.Angle(Vector3.up, hit.normal);

            // check if the floor
            if (maxSlopeAngle > angle)
            {
                return true;
            }
        }

        return false;
    }

    private void OnLanding(Collision collision)
    {
        
        // if the impulse is enough
        if (collision.impulse.magnitude >= minImpulseForDust)
        {
            dust.Play();
        }
        if (collision.impulse.magnitude >= minImpulseForHeadDip)
        {
            isCamDipping = true;
            isCamReturning = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool foundSuitableAngle = CheckIsOnGround();

        // if just landed
        if (!isOnGround && foundSuitableAngle){
            OnLanding(collision);
        }

        isOnGround = foundSuitableAngle;
    }

    void OnCollisionStay(Collision collision)
    {
        bool foundSuitableAngle = CheckIsOnGround();

        // if just landed
        if (!isOnGround && foundSuitableAngle)
        {
            OnLanding(collision);
        }

        isOnGround = foundSuitableAngle;
    }

    void OnCollisionExit(Collision collision)
    {
        isOnGround = false;
    }
}
