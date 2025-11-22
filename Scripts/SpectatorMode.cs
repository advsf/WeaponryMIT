using UnityEngine;
using Unity.Netcode;

public class SpectatorMode : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private GameObject playerObj;
    [SerializeField] private Health healthScript;

    public bool isSpectating;

    // determine how fast the specator moves
    private float speed;

    // for inputs
    private float horizontalInput;
    private float verticalInput;

    // input checks
    private bool isGoingUp;
    private bool isGoingDown;
    private bool isSprinting;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        rb.useGravity = true;
        isSpectating = false;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (isSpectating)
        {
            rb.useGravity = false;

            EnablePlayerMovement(false);

            // spectate when dead
            GetInput();
            SpeedControl();
        }
    }

    public void StopSpectating()
    {
        isSpectating = false;
        rb.useGravity = true;
    }

    private void GetInput()
    {
        // get input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // set bools
        isGoingUp = Input.GetKey(KeyCode.Space);
        isGoingDown = Input.GetKey(KeyCode.LeftControl);

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // increase speed if sprinting
        speed = isSprinting ? runSpeed : walkSpeed;
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !isSpectating) return;

        Vector3 moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(10f * speed * moveDirection.normalized, ForceMode.Force);

        // upwards force
        if (isGoingUp)
            rb.AddForce(10f * speed * Vector3.up, ForceMode.Force);

        // downwards force
        if (isGoingDown)
            rb.AddForce(10f * speed * Vector3.down, ForceMode.Force);

        // if the player isnt moving set the velocity to 0
        if (horizontalInput == 0 && verticalInput == 0)
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new(0f, 0f, 0f), Time.deltaTime * 10f);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.linearVelocity = new Vector3(limitedVel.x, limitedVel.y, limitedVel.z);
        }
    }

    // used for changing between spectating and moving
    public void EnablePlayerMovement(bool condition)
    {
        playerObj.GetComponent<CapsuleCollider>().enabled = condition;
        playerObj.GetComponent<MovementScript>().enabled = condition;
        playerObj.GetComponent<Sliding>().enabled = condition;
        playerObj.GetComponent<MovementAbilities>().enabled = condition;
    }
}
