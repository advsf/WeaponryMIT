using Unity.Netcode;
using UnityEngine;
using TMPro;

public class SetUsernameTag : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameTag;
    private Color blueColor = new(0.3216002f, 0.6180096f, 0.9339623f);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        if (IsServer)
            SetUsernameClientRpc(LobbyManager.instance.username);
        else
            SetUsernameServerRpc(LobbyManager.instance.username);
    }

    [ServerRpc]
    private void SetUsernameServerRpc(string username) => SetUsernameClientRpc(username);

    [ClientRpc]
    private void SetUsernameClientRpc(string username)
    {
        usernameTag.text = username;

        Invoke(nameof(ChangeUsernameTextColor), 1f);
    }

    private void ChangeUsernameTextColor()
    {
        // if on the same team
        if (PlayerInfo.instance.teamColor.Value == GetComponentInParent<PlayerInfo>().teamColor.Value)
            usernameTag.color = blueColor;

        // if not on the same team
        else
            usernameTag.color = Color.red;
    }
}