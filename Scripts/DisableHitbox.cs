using Unity.Netcode;
using UnityEngine;

public class DisableHitbox : NetworkBehaviour
{
    public static DisableHitbox instance;

    [SerializeField] private Health healthScript;

    [Header("Hitbox References")]
    [SerializeField] private BoxCollider headHitbox;
    [SerializeField] private BoxCollider bodyHitbox;
    [SerializeField] private BoxCollider legHitbox;
    [SerializeField] private CapsuleCollider playerCollider;

    private void Start()
    {
        if (IsOwner)
            instance = this;
    }

    public void DetermineHitboxActiveness(bool condition)
    {
        if (IsServer)
            SyncHitboxActivenessClientRpc(condition);
        else
            SyncHitboxActivenessServerRpc(condition);
    }

    [ServerRpc]
    private void SyncHitboxActivenessServerRpc(bool condition) => SyncHitboxActivenessClientRpc(condition);

    [ClientRpc]
    private void SyncHitboxActivenessClientRpc(bool condition)
    {
        // only for other clients
        if (!IsOwner)
            playerCollider.enabled = condition;

        headHitbox.enabled = condition;
        bodyHitbox.enabled = condition;
        legHitbox.enabled = condition;
    }
}
