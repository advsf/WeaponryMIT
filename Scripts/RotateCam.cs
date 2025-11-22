using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class RotateCam : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health healthScript;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camHolder;

    private float MouseX { get => Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens; }
    private float MouseY { get => Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens; }

    private float xRotation;
    private float yRotation;

    private float sens = 100.0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // if the ui is opened, or if the player is dead or isnt spectating
        if (!IsOwner || StopEverythingWithUIOpened.IsUIOpened || (healthScript.currentHealth <= 0.0f && !Health.instance.isSpectating)) return;

        // gets the saved sens
        sens = PlayerPrefs.GetFloat("sens");

        yRotation += MouseX;
        xRotation -= MouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoFov(float endValue, float duration = 0.25f)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, duration);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new(0, 0, zTilt), 0.25f);
    }
}
