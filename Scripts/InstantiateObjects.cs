using Unity.Netcode;
using UnityEngine;

public class InstantiateObjects : NetworkBehaviour
{
    [SerializeField] private Transform playerCam;

    private void Update()
    {
        if (IsOwner) return;

        playerCam.position = transform.position;
    }

}
