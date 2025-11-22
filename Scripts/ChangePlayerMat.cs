using Unity.Netcode;
using UnityEngine;

public class ChangePlayerMat : NetworkBehaviour
{
    [SerializeField] private GameObject playerObj;
    [SerializeField] private Material blueTeamMat;
    [SerializeField] private Material redTeamMat;

    private void Start()
    {
        Invoke(nameof(InvokeChangePlayerMatColor), 0.5f);
    }

    private void InvokeChangePlayerMatColor()
    {
        if (IsServer)
            ChangePlayerMatColorClientRpc(PlayerInfo.instance.teamColor.Value.ToString());
        else
            ChangePlayerMatColorServerRpc(PlayerInfo.instance.teamColor.Value.ToString());
    }

    [ServerRpc]
    private void ChangePlayerMatColorServerRpc(string teamColor) => ChangePlayerMatColorClientRpc(teamColor);

    [ClientRpc]
    private void ChangePlayerMatColorClientRpc(string teamColor)
    {
        HandleChangingPlayerMat(teamColor);
    }

    private void HandleChangingPlayerMat(string teamColor)
    {
        // we dont change our own player models
        if (IsOwner) return;

        // if on the same team
        if (PlayerInfo.instance.teamColor.Value == teamColor)
            playerObj.GetComponent<SkinnedMeshRenderer>().material = blueTeamMat;

        // if not on the same team
        else
            playerObj.GetComponent<SkinnedMeshRenderer>().material = redTeamMat;
    }
}
