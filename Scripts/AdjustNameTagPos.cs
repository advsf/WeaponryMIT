using Unity.Netcode;
using UnityEngine;

public class AdjustNameTagPos : NetworkBehaviour
{
    [SerializeField] private Vector3 offset;
    private Vector3 originalPos;

    private void Start()
    {
        originalPos = new(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKey(KeyCode.LeftControl))
            transform.localPosition = new(originalPos.x + offset.x, originalPos.y + offset.y, originalPos.z + offset.z);
        else
            transform.localPosition = originalPos;
    }
}
