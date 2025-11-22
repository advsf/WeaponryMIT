using Unity.Netcode;
using UnityEngine;

public class ChangeLayerForOtherClients : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // set the weapons and knife to default layer
        if (!IsOwner)
            SetLayerToChildObjects(gameObject);
    }

    // the reason why we set some of the weapons ot the default layer for non owner clients
    // is because without doing so, they can see the guns through walls
    private void SetLayerToChildObjects(GameObject parent)
    {
        // set this to the default layer
        parent.layer = 0;

        if (parent.transform.childCount == 0) return;

        foreach (Transform child in parent.transform)
            SetLayerToChildObjects(child.gameObject);
    }
}
