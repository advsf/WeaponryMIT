using Unity.Netcode;
using UnityEngine;

public class DisableLocalHitbox : NetworkBehaviour
{
    private void Start()
    {
        if (!IsOwner) return;

        // changes every collider's layer to whatIsPlayer (LOCALLY), so that it will ignore the bullet particles hitting the local player's own hitbox while shooting down...
        foreach (BoxCollider collider in GetComponentsInChildren<BoxCollider>())
            collider.gameObject.layer = 11;
    }
}
