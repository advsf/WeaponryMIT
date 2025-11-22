using UnityEngine;
using Unity.Netcode;

public class ChangeRotation : NetworkBehaviour
{
    [SerializeField] private Transform orientation;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (transform.rotation != orientation.rotation) transform.rotation = Quaternion.Lerp(transform.rotation, orientation.rotation, Time.deltaTime * 0.01f);
    }
}
