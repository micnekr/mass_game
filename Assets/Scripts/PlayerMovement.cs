using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // movement
    public float maxSlopeAngle = 30f;
    public float maxMoveSpeed = 10f;
    public float accelerationSpeed = 240f;
    public float velocityNegationMultiplier = 0.2f;
    public float maxSpeed = 10f;

    // Jumping and falling
    public float jumpForce = 200f;
    public float addedGravity = 5f;
    public float stopSpeedMultiplier = 1.3f;
    public float airSpeedMultiplier = 0.05f;
    public float jumpCooldown = 0.5f;

    // effects

    // dust
    public float minImpulseForDust = 15f;
    public ParticleSystem dust;

    private Rigidbody rb;

    private float xInput, yInput;
    private Vector3 desiredDir;
    private bool jumpPressed;

    // landing camera animations
    public float minImpulseForHeadDip = 6f;
    public float maxCameraDipDistance = -0.5f;
    public float headDipDampening = 0.5f;
    public float headDipReturnThreshold = 0.001f;
    public float cameraReturnTime = 0.5f;
    public Follow headFollowScript;

    private bool isOnGround = false;

    private bool isCamDipping = false;
    private bool isCamReturning = false;

    private float timeSinceCameraStartedReturn;
    private float timeSinceLastJump = 0f;

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
            if (isCamReturning)
            {
                headFollowScript.runtimeOffset.y = Mathf.SmoothStep(maxCameraDipDistance, 0, timeSinceCameraStartedReturn/cameraReturnTime);
                timeSinceCameraStartedReturn += Time.deltaTime;
            }
            else
            {
                float headDipSpeed = (maxCameraDipDistance - headFollowScript.runtimeOffset.y) * headDipDampening;
                headFollowScript.runtimeOffset.y += headDipSpeed;
            }

            if(Mathf.Abs(headFollowScript.runtimeOffset.y - maxCameraDipDistance) < headDipReturnThreshold && !isCamReturning)
            {
                isCamReturning = true;
                timeSinceCameraStartedReturn = 0f;
            }
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
        desiredDir.Set(xInput, 0f, yInput);

        // gets local velocity 
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

        //Jump
        timeSinceLastJump += Time.fixedDeltaTime;

        if (jumpPressed && timeSinceLastJump > jumpCooldown)
        {
            Jump();
            timeSinceLastJump = 0f;
        }
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
            Debug.Log("Playing move camera down");
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
