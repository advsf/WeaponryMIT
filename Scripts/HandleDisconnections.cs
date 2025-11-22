using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class HandleDisconnections : NetworkBehaviour
{
    public static HandleDisconnections instance;

    [SerializeField] private GameObject errorMessageObj;
    [SerializeField] private TextMeshProUGUI errorMessage;

    private bool isGameEndedNaturally = false;

    private void Start()
    {
        instance = this;

        NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnection;

        NetworkManager.Singleton.OnServerStopped += Singleton_OnServerStopped;
    }

    private void Singleton_OnServerStopped(bool obj)
    {
        GoBackToLobby();
    }

    private void CleanUp()
    {
        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);
    }

    public void GoBackToLobby()
    {
        isGameEndedNaturally = true;

        NetworkManager.Singleton.Shutdown();

        CleanUp();

        SceneManager.LoadScene("Lobby");
    }

    public void HandleDisconnection(ulong obj)
    {
        if (IsServer)
            HandleDisconnectionClientRpc(PlayerInfo.instance.teamColor.Value.ToString());
        else
            HandleDisconnectionServerRpc(PlayerInfo.instance.teamColor.Value.ToString());
    }

    [ServerRpc]
    private void HandleDisconnectionServerRpc(string teamColor) => HandleDisconnectionClientRpc(teamColor);

    [ClientRpc]
    private void HandleDisconnectionClientRpc(string teamColor)
    {
        // if the game ended naturally
        if (isGameEndedNaturally || GameManager.instance.blueTeamWonAmount.Value == 5 || GameManager.instance.redTeamWonAmount.Value == 5 
            || GameManager.instance.blueTeamOvertimeWonAmount.Value == 2 || GameManager.instance.redTeamOvertimeWonAmount.Value == 2) return;

        // when a player leaves without the game ending, we have to give the exp to the other team
        GameManager.instance.GiveExpToOtherPlayerAfterLeaving(teamColor);

        errorMessageObj.SetActive(true);
        errorMessage.text = "yo lil bro ur opponent left so u get 10 exp hooray";

        Invoke(nameof(GoBackToLobby), 3f);
    }
}
