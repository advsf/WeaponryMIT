using Unity.Netcode;
using UnityEngine;

public class LocalizeCamera : NetworkBehaviour
{
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject scopeCam;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // disable every other camera that isn't owned by the player
        if (!IsOwner)
        {
            cam.GetComponent<Camera>().enabled = false;
            cam.GetComponent<AudioListener>().enabled = false;
            scopeCam.SetActive(false);
        }
    }
}
