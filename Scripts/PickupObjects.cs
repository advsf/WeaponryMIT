using UnityEngine;
using Unity.Netcode;

public class PickupObjects : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pickupRange;
    [SerializeField] private KeyCode pickupKey = KeyCode.Mouse0;

    [Header("References")]
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform pickupTarget;

    private Rigidbody currentObject;

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKey(pickupKey))
        {
            Ray ray = cam.ViewportPointToRay(new(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask))
            {
                // picking up the object
                currentObject = hit.rigidbody;
                currentObject.useGravity = false;
            }
        }

        else if (currentObject != null)
        {
            currentObject.useGravity = true;
            currentObject = null;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        
        if (currentObject)
        {
            Vector3 directionToPoint = pickupTarget.position - currentObject.position;
            float distanceToPoint = directionToPoint.magnitude;

            currentObject.linearVelocity = 8f * distanceToPoint * directionToPoint;
        }
    }
}
