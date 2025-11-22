using UnityEngine;
using Unity.Netcode;

public class HandleKillFeedboard : NetworkBehaviour
{
    public static HandleKillFeedboard instance;
    public GameObject killboardFeedPrefab;
    public Transform KillFeedboardParent;

    private void Start()
    {
        if (IsOwner)
            instance = this;
    }

    public void InstantiateNewFeed(string killerName, string killedName, ulong killerId, ulong killedId, string killerTeamName, string killedTeamName, string usedWeapon, bool isHeadshot)
    {
        if (!IsOwner) return;

        // sync
        if (IsServer)
            SyncKillfeedClientRpc(killerName, killedName, killerId, killedId, killerTeamName, killedTeamName, usedWeapon, isHeadshot);
        else
            SyncKillFeedServerRpc(killerName, killedName, killerId, killedId, killerTeamName, killedTeamName, usedWeapon, isHeadshot);
    }

    [ServerRpc]
    private void SyncKillFeedServerRpc(string killerName, string killedName, ulong killerId, ulong killedId, string killerTeamName, string killedTeamName, string usedWeapon, bool isHeadshot) => SyncKillfeedClientRpc(killerName, killedName, killerId, killedId, killerTeamName, killedTeamName, usedWeapon, isHeadshot);

    [ClientRpc]
    private void SyncKillfeedClientRpc(string killerName, string killedName, ulong killerId, ulong killedId, string killerTeamName, string killedTeamName, string usedWeapon, bool isHeadshot) => AddANewKillfeed(killerName, killedName, killerId, killedId, killerTeamName, killedTeamName, usedWeapon, isHeadshot);

    private void AddANewKillfeed(string killerName, string killedName, ulong killerId, ulong killedId, string killerTeamName, string killedTeamName, string usedWeapon, bool isHeadshot)
    {
        GameObject killfeed = Instantiate(instance.killboardFeedPrefab, instance.KillFeedboardParent);

        SetUpKillfeedDisplay setUpKillfeed = killfeed.GetComponent<SetUpKillfeedDisplay>();

        setUpKillfeed.killerName = killerName;
        setUpKillfeed.killedName = killedName;
        setUpKillfeed.killerId = killerId;
        setUpKillfeed.killedId = killedId;
        setUpKillfeed.killerTeam = killerTeamName;
        setUpKillfeed.killedTeam = killedTeamName;
        setUpKillfeed.weaponUsed = usedWeapon;
        setUpKillfeed.isHeadshot = isHeadshot;
    }
}
