using Unity.Netcode;

public class SetObjectName : NetworkBehaviour
{
    // change the name of the player's object
    // which will come in handy later for leaderboards and other stuff

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            gameObject.name = LobbyManager.instance.username;
            ChangeObjectNameClientRpc(LobbyManager.instance.username);
        }
        else
            ChangeObjectNameServerRpc(LobbyManager.instance.username);
    }

    [ServerRpc]
    private void ChangeObjectNameServerRpc(string username) 
    {
        gameObject.name = username;

        // sync name with clients
        ChangeObjectNameClientRpc(username);
    }

    [ClientRpc]
    private void ChangeObjectNameClientRpc(string username) => gameObject.name = username;
}
