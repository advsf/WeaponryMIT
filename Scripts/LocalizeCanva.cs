using Unity.Netcode;
using UnityEngine;

public class LocalizeCanva : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // disable canva if not owner of this object
        if (!IsOwner) gameObject.SetActive(false);
    }
}
