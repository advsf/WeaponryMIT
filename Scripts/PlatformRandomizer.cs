using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Randomizes which platform is breakable
/// First, we let the server decide by choosing a number between 0 and 1
/// If it's 0, make the left platform breakable. Else, make the right breakable.
/// Afterwards, send a clientrpc with the same index to sync across the network
/// </summary>
/// 
public class PlatformRandomizer : NetworkBehaviour
{
    [SerializeField] private Transform leftPlatform;
    [SerializeField] private Transform rightPlatform;

    private int index;
    private int currentPlayerCount = 1;

    public override void OnNetworkSpawn()
    {
        // initial set up the platforms
        base.OnNetworkSpawn();
        DecidePlatform();
    }

    private void Update()
    {
        // bad method fix if you can find a subscription-based solution (maybe use network variables?)
        if (!IsServer) return;

        // syncs the platform for late-joiners (clients who joined a bit later)
        if (currentPlayerCount != NetworkManager.Singleton.ConnectedClients.Count)
        {
            SyncPlatformEffectClientRpc(index);
            currentPlayerCount = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }

    public void DecidePlatform()
    {
        // this function decides which platform should be broken on contact
        if (!IsServer) return;

        index = Random.Range(0, 2);

        PlatformDecider(index);
    }

    [ClientRpc]
    public void SyncPlatformEffectClientRpc(int index) => PlatformDecider(index);

    public void PlatformDecider(int index)
    {
        if (index == 0)
            leftPlatform.GetComponent<PlatformEffect>().MakePlatformBreakable();
        else
            rightPlatform.GetComponent<PlatformEffect>().MakePlatformBreakable();
    }

    public void CommenceReset()
    {
        ResetPlatform();
        ResetPlatformClientRpc();

        // update which platforms should be broken
        DecidePlatform();
        SyncPlatformEffectClientRpc(index);
    }

    private void ResetPlatform()
    {
        EnablePlatform(leftPlatform.gameObject);
        EnablePlatform(rightPlatform.gameObject);
    }

    private void EnablePlatform(GameObject platform)
    {
        platform.GetComponent<MeshCollider>().enabled = true;
        platform.GetComponent<MeshCollider>().isTrigger = false;
        platform.GetComponent<MeshRenderer>().enabled = true;
        platform.layer = 7;
    }

    [ClientRpc]
    private void ResetPlatformClientRpc() => ResetPlatform();
}
