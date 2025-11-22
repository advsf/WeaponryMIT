using UnityEngine;
using Unity.Netcode;
public class WeaponSway : NetworkBehaviour
{
    [Header("External References")]
    [SerializeField] MovementScript movementScript;
    [SerializeField] Rigidbody rb;

    [Header("Sway")]
    public float step = 0.01f; // multiplied by the value from the mouse for 1 frame
    public float maxStepDistance = 0.06f; // max distacne from the local origin
    private Vector3 swayPos;

    [Header("Sway Rotation")]
    public float rotationStep = 4f; // multipled by the value from the mosue for 1 frame
    public float maxRotationStep = 5; // max rotation from the local identity rotation
    private Vector3 swayEulerRot;

    [Header("Bobbing")]
    public float speedCurve;
    private float CurveSin { get => Mathf.Sin(speedCurve); }
    private float CurveCos { get => Mathf.Sin(speedCurve); }

    public Vector3 travelLimit = Vector3.one * 0.025f; // max limits of travel from move input
    public Vector3 bobLimit = Vector3.one * 0.01f; // limits of travel from bobbing over time 

    private Vector3 bobPosition;

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    private Vector3 bobEulerRot;

    [Header("Settings")]
    public bool sway = true;
    public bool swayRotation = true;
    public bool bobOffset = true;
    public bool bobSway = true;

    // store inputs internally
    Vector2 walkInput;
    Vector2 lookInput;

    private float smooth = 10.0f;
    private float smoothRot = 12f;

    // get originalPos
    private Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // get input
        GetInput();

        // sway calculations
        Sway();
        SwayRotation();

        // bob calculations
        BobOffset();
        BobRotation();

        // apply all transformations
        CompositePositionRotation();
    }

    private void GetInput()
    {
        if (StopEverythingWithUIOpened.IsUIOpened) return;

        walkInput.x = Input.GetAxisRaw("Horizontal");
        walkInput.y = Input.GetAxisRaw("Vertical");
        walkInput = walkInput.normalized;

        lookInput.x = Input.GetAxisRaw("Mouse X");
        lookInput.y = Input.GetAxisRaw("Mouse Y");
    }

    private void Sway()
    {
        // disabled if sway is false
        if (!sway)
        {
            swayPos = Vector3.zero;
            return;
        }

        Vector3 invertedLook = lookInput * -step;
        invertedLook.x = Mathf.Clamp(invertedLook.x, -maxStepDistance, maxStepDistance);
        invertedLook.y = Mathf.Clamp(invertedLook.y, -maxStepDistance, maxStepDistance);

        swayPos = invertedLook;
    }

    private void SwayRotation()
    {
        if (!swayRotation)
        {
            swayEulerRot = Vector3.zero;
            return;
        }

        Vector2 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);

        swayEulerRot = new(invertLook.y, invertLook.x, invertLook.x);
    }

    private void BobOffset()
    {
        if (!bobOffset)
        {
            bobPosition = Vector3.zero;
            return;
        }

        speedCurve += Time.deltaTime * (movementScript.isGrounded ? rb.linearVelocity.magnitude : 1f) + 0.01f;

        bobPosition.x = (CurveCos * bobLimit.x * (movementScript.isGrounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
        bobPosition.y = (CurveSin * bobLimit.y) - (rb.linearVelocity.y * travelLimit.y);
        bobPosition.z = -(walkInput.y * travelLimit.z);
    }

    private void BobRotation()
    {
        if (!bobSway)
        {
            bobEulerRot = Vector3.zero;
            return;
        }

        bobEulerRot.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));
        bobEulerRot.y = (walkInput != Vector2.zero ? multiplier.y * CurveCos : 0);
        bobEulerRot.z = (walkInput != Vector2.zero ? multiplier.z * CurveCos * walkInput.x : 0);
    }

    private void CompositePositionRotation()
    {
        // position
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos + swayPos + bobPosition, smooth * Time.deltaTime);

        // rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRot), smoothRot * Time.deltaTime);
    }
}
