using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class MovementScript : NetworkBehaviour
{
    public static MovementScript instance;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;

    [Header("Movement")]
    public Transform orientation;
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float crouchYScale = 2f;

    [Header("Counter Movement")]
    public float counterMovementMultiplier = 12.0f;

    [Header("Knife Speed")]
    public float knifeMoveSpeed;
    public float knifeSprintSpeed;
    public float knifeSlideSpeed;
    public float knifeWallRunSpeed;

    [Header("Dashing")]
    public float dashSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float maxTime = 1.25f;
    bool readyToJump;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool isGrounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Script References")]
    [SerializeField] private HandleActiveGun changeGunScript;

    // for movement
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Vector3 originalScale;
    private Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        sliding,
        dashing,
        grounded,
        air
    }

    [Header("Movement Bools")]
    public bool sliding;
    public bool walking;
    public bool running;
    public bool dashing;

    private Vector2 movementInput;

    private void Awake()
    {
        // singleton design
        instance = this;

        // rigidbody related
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // jump related
        readyToJump = true;
        originalScale = transform.localScale;
    }
    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
    }

    private void Update()
    {
        if (!IsOwner) return;

        SpeedControl();

        GetInput();
        StateHandler();

        // handle jumping
        if (jumpAction.action.ReadValue<float>() > 0 && readyToJump && isGrounded)
            HandleJump();

        // ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Move();
    }

    private void GetInput()
    {
        if (StopEverythingWithUIOpened.IsUIOpened) return;

        movementInput = moveAction.action.ReadValue<Vector2>();
        running = sprintAction.action.ReadValue<float>() > 0 && movementInput != Vector2.zero && isGrounded;
    }

    private void StateHandler()
    {
        // Mode - Dashing
        if (dashing)
        {
            state = MovementState.dashing;
            moveSpeed = dashSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;
            moveSpeed = changeGunScript.isGunActive ? slideSpeed : knifeSlideSpeed;
        }

        // Mode - Sprinting
        else if (isGrounded && running)
        {
            state = MovementState.sprinting;
            moveSpeed = changeGunScript.isGunActive ? sprintSpeed : knifeSprintSpeed;
        }

        // Mode - Walking
        else if (isGrounded && !running && movementInput != Vector2.zero)
        {
            state = MovementState.walking;
            moveSpeed = changeGunScript.isGunActive ? walkSpeed : knifeMoveSpeed;
        }

        // Mode - Static (just grounded)
        else if (isGrounded)
            state = MovementState.grounded;

        // Mode - air
        else
            state = MovementState.air;

        walking = state == MovementState.walking;
        sliding = state == MovementState.sliding;
    }

    private void Move()
    {
        if (StopEverythingWithUIOpened.IsUIOpened) return;

        // calculates movement direction
        Vector3 moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;

        // calculates the desired velocity based on moveDirection and moveSpeed
        Vector3 desiredVelocity = moveDirection.normalized * moveSpeed;

        // calculates the difference between current velocity and desired velocity
        Vector3 velocityDifference = desiredVelocity - rb.linearVelocity;
        velocityDifference.y = 0.0f;

        // applies countermovement force
        if (isGrounded && moveDirection == Vector3.zero && !dashing)
            rb.AddForce(velocityDifference * counterMovementMultiplier, ForceMode.Acceleration);

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(20f * moveSpeed * GetSlopeMoveDirection(moveDirection), ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (isGrounded)
            rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);

        // in air
        else if (!isGrounded)
            rb.AddForce(airMultiplier * 10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
    }

    private void SpeedControl()
    {
        // if we have an UI open stop moving
        if (StopEverythingWithUIOpened.IsUIOpened)
            rb.linearVelocity = new(0f, rb.linearVelocity.y, 0f);
        else if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, rb.linearVelocity.normalized * moveSpeed, Time.deltaTime * 35f);
        }

        // not on slope
        else
        {
            Vector3 flatVel = new(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void HandleJump()
    {
        if (StopEverythingWithUIOpened.IsUIOpened) return;

        readyToJump = false;
        exitingSlope = true;

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
