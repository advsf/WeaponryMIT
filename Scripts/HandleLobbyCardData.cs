using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

public class HandleLobbyCardData : MonoBehaviour
{
    [Header("Informations")]
    public Lobby representedLobby; // the lobby that the card represents
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI hostName;
    [SerializeField] private TextMeshProUGUI gameModeAndPlayerCount;
    [SerializeField] private TextMeshProUGUI rankName;
    [SerializeField] private Image rankSymbol;
    [SerializeField] private Transform rankNameObj;

    private void Start()
    {
        rankNameObj.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (rankNameObj.gameObject.activeInHierarchy)
            rankNameObj.position = Input.mousePosition;
    }

    // when the player presses the button
    public async void JoinSelectedLobby()
    {
        Lobby joinedLobby = await LobbyManager.instance.JoinLobbyById(representedLobby.Id);

        if (joinedLobby != null)
            LobbyUIManager.instance.JoinLobbyThroughLobbyList(joinedLobby);
    }

    public void InitializeData()
    {
        lobbyName.text = representedLobby.Name;
        hostName.text = representedLobby.Data["HostName"].Value;
        gameModeAndPlayerCount.text = $"{representedLobby.Data["GameMode"].Value} | {representedLobby.Players.Count}/{representedLobby.MaxPlayers}";
        rankSymbol.sprite = RankManager.instance.ranks[RankManager.instance.GetCurrentRankSprite(representedLobby.Data["HostRank"].Value)];
        rankName.text = representedLobby.Data["HostRank"].Value;
    }

    public void ShowRankNameWhenHoverOver() => rankNameObj.gameObject.SetActive(true);

    public void DisableRankNameObject() => rankNameObj.gameObject.SetActive(false);
}
