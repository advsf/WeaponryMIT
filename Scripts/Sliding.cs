using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sliding : NetworkBehaviour
{
    [Header("Input References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference slideAction;

    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    public RotateCam cam;
    private Rigidbody rb;
    private MovementScript movement;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    public float gravityMultiplier;
    public float slideCooldown;
    private float slideTimer;

    private float startYScale;

    private bool canSlide = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<MovementScript>();

        startYScale = playerObj.localScale.y;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        slideAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        slideAction.action.Disable();
    }

    private void Update()
    {
        if (!IsOwner || StopEverythingWithUIOpened.IsUIOpened) return;

        if (slideAction.action.ReadValue<float>() > 0f && (movement.walking || movement.running) && canSlide && !movement.dashing)
            StartSlide();

        if (slideAction.action.ReadValue<float>() == 0f && movement.sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (movement.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        canSlide = false;

        movement.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, movement.crouchYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Impulse);

        // tilt the camera
        cam.DoTilt(5f);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * moveAction.action.ReadValue<Vector2>().y + orientation.right * moveAction.action.ReadValue<Vector2>().x;

        // sliding normal
        if (rb.linearVelocity.magnitude > 4f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    public void StopSlide()
    {
        movement.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);

        // reset the camera
        cam.DoTilt(0f);

        // handle the cooldown UI
        StartCoroutine(HandleAbilitiesCooldownUI.instance.HandleSlidingCooldownUI());

        // cooldown
        Invoke(nameof(ResetSlide), slideCooldown);
    }

    private void ResetSlide()
    {
        canSlide = true;
    }
}
