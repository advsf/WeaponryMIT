using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

    public bool isTesting;
    [HideInInspector] public Lobby hostLobby;
    [HideInInspector] public Lobby joinedLobby;
    [HideInInspector] public string username;
    [HideInInspector] public string id;

    private float heartbeatTimer;
    private float lobbyTimer;
    private bool didHostStart = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);
    }

    private async void Start()
    {
        // use only for debugging purposes
        if (isTesting)
        {
            InitializationOptions initializeOptions = new();

            initializeOptions.SetProfile(UnityEngine.Random.Range(0, 99999).ToString());

            await UnityServices.InitializeAsync(initializeOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        // for default builds
        else
        {
            await UnityServices.InitializeAsync();

            id = AuthenticationService.Instance.PlayerId;

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Update()
    {
        if (username == null) return;

        HandleLobbyHeartBeat();
        UpdateLobbyData();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer < 0.0f)
            {
                float heartbeatTimerMax = 15.0f;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void UpdateLobbyData()
    {
        if (joinedLobby != null)
        {
            lobbyTimer -= Time.deltaTime;

            if (lobbyTimer < 0.0f)
            {
                float lobbyMaxTimer = 1.1f;
                lobbyTimer = lobbyMaxTimer;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }

        // joining
        if (joinedLobby != null && joinedLobby.Data["RelayCode"].Value != "0")
        {
            if (id != joinedLobby.HostId && !didHostStart)
            {
                SceneManager.LoadScene("MainGame");

                RelayManager.instance.JoinRelay(joinedLobby.Data["RelayCode"].Value);
            }

            joinedLobby = null;
        }
    }

    public async Task<Lobby> CreateLobby(string lobbyName, string gameMode, int maxPlayers, bool isPrivate)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    { "HostRank", new DataObject(DataObject.VisibilityOptions.Public, RankManager.instance.ranks[PlayerPrefs.GetInt("currentRank")].name) },
                    { "HostName", new DataObject(DataObject.VisibilityOptions.Public, username) },
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            return joinedLobby;
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    // implement into ui later
    public async Task<List<Lobby>> RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // filter fora available lobbies
            options.Filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            return lobbyListQueryResponse.Results;
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);

            return null;
        }
    }

    public async Task<Lobby> JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            return joinedLobby;
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public async Task<Lobby> JoinLobbyById(string id)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByCodeOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            return joinedLobby;
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public async Task<Lobby> QuickJoinMatch()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            joinedLobby = lobby;

            return joinedLobby;
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, username)  },
                { "RankName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, RankManager.instance.ranks[PlayerPrefs.GetInt("currentRank")].name) },
                { "kdRatio", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Math.Round(PlayerPrefs.GetInt("killsCount") / (double) (PlayerPrefs.GetInt("deathsCount") == 0 ? 1.0f : PlayerPrefs.GetInt("deathsCount")), 2).ToString()) },
                { "winCount", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerPrefs.GetInt("winCount").ToString()) },
                { "loseCount", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerPrefs.GetInt("loseCount").ToString()) }
            }
        };
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, id);

            joinedLobby = null;
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void KickPlayer()
    {
        try
        {
            // fix to allow kicking not just the first player
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void StartMatch()
    {
        if (id == joinedLobby.HostId || isTesting)
        {
            try
            {
                didHostStart = true;

                SceneManager.LoadScene("MainGame");

                string relayCode = await RelayManager.instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
                });

                joinedLobby = lobby;
            }

            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public void StartPracticeMatch() => SceneManager.LoadScene("Practice");

    public void StartTutorial() => SceneManager.LoadScene("Tutorial");
}
