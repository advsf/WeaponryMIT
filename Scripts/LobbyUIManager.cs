using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System;
using Unity.Services.Lobbies;
using System.Collections;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager instance;

    [Header("Settings")]
    [SerializeField] private float maxCreateLobbyTimer = 10.0f;
    [SerializeField] Vector3 playerCardScale = new(1f, 1f, 1f);
    [SerializeField] float playerCardYOffset = 90f;
    [SerializeField] int maxLobbyNameLength = 28;
    [SerializeField] int maxUsernameLength = 10;

    [Header("MainLobby References")]
    [SerializeField] private GameObject mainLobbyMenu;

    [Header("Play Now References")]
    [SerializeField] private GameObject playNowMenu;

    [Header("CreateLobbySettings References")]
    [SerializeField] private GameObject lobbyCreationMenu;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TextMeshProUGUI lobbyCreationErrorMessage;
    [SerializeField] private TMP_Dropdown gameModeDropdowns;
    [SerializeField] private Toggle lobbyPrivacyToggle;

    [Header("LobbyInstance References")]
    [SerializeField] private GameObject lobbyInstanceMenu;
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI roomCode;
    [SerializeField] private TextMeshProUGUI currentLobbyPlayerCount;
    [SerializeField] private TextMeshProUGUI lobbyPrivacyText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private GameObject playerCardPrefab;
    [SerializeField] private Transform playerCardParent;
    [SerializeField] private Button startButton;

    [Header("JoinLobby References")]
    [SerializeField] private GameObject joinLobbyMenu;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private TextMeshProUGUI errorMessage;

    [Header("Username References")]
    public GameObject usernameMenu;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TextMeshProUGUI usernameErrorMessage;

    [Header("Player Stats References")]
    [SerializeField] private HandlePlayerStatsUI playerStatsUIScript;

    [Header("Restart Game References")]
    [SerializeField] private GameObject restartGameObj;

    [Header("Quick Join Reference")]
    [SerializeField] private GameObject quickJoinGameObj;

    [Header("Lobby List References")]
    [SerializeField] private GameObject lobbyListObj;
    [SerializeField] private GameObject lobbyCardPrefab;
    [SerializeField] private Transform container;

    // global variable
    private float createLobbyTimer;
    public bool canCreateLobby = true;

    private bool isSettingUpCards = false; // lobby instance player card creation cooldon related

    private Lobby lobby = null;

    private Resolution[] resolutions;

    private bool isWaitingForStartButtonToEnable = false;

    public bool IsOnMainMenu { get => mainLobbyMenu.activeInHierarchy; }

    private void Start()
    {
        instance = this;

        // just to make sure
        usernameMenu.SetActive(true);

        // if there is already a username
        if (PlayerPrefs.HasKey("username"))
        {
            LobbyManager.instance.username = PlayerPrefs.GetString("username");

            mainLobbyMenu.SetActive(true);
            usernameMenu.SetActive(false);
            lobbyInstanceMenu.SetActive(false);
            lobbyCreationMenu.SetActive(false);
            joinLobbyMenu.SetActive(false);
            playNowMenu.SetActive(false);
            return;
        }

        // disable all ui except for username authenticater
        else
        {
            mainLobbyMenu.SetActive(false);
            usernameMenu.SetActive(true);
            lobbyInstanceMenu.SetActive(false);
            lobbyCreationMenu.SetActive(false);
            joinLobbyMenu.SetActive(false);
            playNowMenu.SetActive(false);
        }

        lobbyListObj.SetActive(false);
        quickJoinGameObj.SetActive(false);

        // reset error messages
        usernameErrorMessage.text = "";
        lobbyCreationErrorMessage.text = "";

        // initialize data
        if (!PlayerPrefs.HasKey("sens"))
            PlayerPrefs.SetFloat("sens", 200f);

        if (!PlayerPrefs.HasKey("audio"))
            PlayerPrefs.SetFloat("audio", 1f);

        if (PlayerPrefs.HasKey("isFirsTimePlaying") || PlayerPrefs.GetString("username") == null)
            PlayerPrefs.SetInt("isFirstTimePlaying", 0);
    }

    private void Update()
    {
        // does nothing if the username isn't inputted
        if (usernameInput == null) return;

        // prevent lobby creation spam
        SetLobbyTimer();

        if (lobby != null)
            lobby = LobbyManager.instance.joinedLobby;

        if (lobbyInstanceMenu.activeInHierarchy && lobby != null)
        {
            Invoke(nameof(SetLobbyPlayerCount), 2f);
            Invoke(nameof(SetUpPlayerCards), 2f);

            // handle start button
            if (LobbyManager.instance.id.Equals(lobby.HostId))
            {
                if ((lobby.Players.Count >= 2 || LobbyManager.instance.isTesting) && !isWaitingForStartButtonToEnable)
                    StartCoroutine(EnableStartButton());
                else if (!isWaitingForStartButtonToEnable)
                    startButton.interactable = false;
            }

            else
                startButton.gameObject.SetActive(false);
        }
    }

    private IEnumerator EnableStartButton()
    {
        isWaitingForStartButtonToEnable = true;

        float time = Time.time;

        // wait five seconds before enabling the button
        // to ensure that all players are properly loaded into the lobby
        while (Time.time - time <= 5)
        {
            if (lobby.Players.Count < 2)
            {
                isWaitingForStartButtonToEnable = false;
                yield break;
            }

            yield return null;
        }

        isWaitingForStartButtonToEnable = false;
        startButton.interactable = true;
    }

    private async void InstantiateLobby(string lobbyName, string gameMode, int maxPlayers, bool isPrivate)
    {
        lobbyCreationMenu.SetActive(false);
        lobbyInstanceMenu.SetActive(true);

        lobby = await LobbyManager.instance.CreateLobby(lobbyName, gameMode, maxPlayers, isPrivate);

        this.lobbyName.text = lobby.Name;
        roomCode.text = lobby.LobbyCode;
        lobbyPrivacyText.text = isPrivate ? "Private" : "Public";
        gameModeText.text = $"GameMode: {gameMode}";
        currentLobbyPlayerCount.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

        // set up the player card for the host (as they just created it)
        SetUpPlayerCards();
    }

    private void SetLobbyTimer()
    {
        createLobbyTimer -= Time.deltaTime;
    }

    public void CheckLobbyTime()
    {
        if (createLobbyTimer < 0.0f)
        {
            createLobbyTimer = maxCreateLobbyTimer;
            canCreateLobby = true;
        }

        else
            canCreateLobby = false;
    }

    private bool CheckIfLobbyCanBeCreated()
    {
        if (canCreateLobby) return true;

        if (Time.time - createLobbyTimer >= maxCreateLobbyTimer)
        {
            canCreateLobby = true;
            return true;
        }

        return false;
    }

    private void SetUpPlayerCards()
    {
        if (isSettingUpCards) return;

        isSettingUpCards = true;

        // delete existing cards
        foreach (Transform child in playerCardParent)
            Destroy(child.gameObject);

        // gets the kd ratio (also avoids undefined behavior)
        double kdRatio = Math.Round(PlayerPrefs.GetInt("killsCount") / (double)(PlayerPrefs.GetInt("deathsCount") == 0 ? 1.0f : PlayerPrefs.GetInt("deathsCount")), 2);

        // instantiate the local player's card first
        InstantiatePlayerCard(RankManager.instance.ranks[PlayerPrefs.GetInt("currentRank")].name, LobbyManager.instance.username, LobbyManager.instance.id.Equals(lobby.HostId),
            kdRatio.ToString(), PlayerPrefs.GetInt("winCount").ToString(), PlayerPrefs.GetInt("loseCount").ToString());

        // instantiate other players' cards
        foreach (var player in lobby.Players)
        {
            // if the player isnt our own player (because we already made a card for the local player)
            if (!player.Id.Equals(LobbyManager.instance.id))
                InstantiatePlayerCard(player.Data["RankName"].Value, player.Data["PlayerName"].Value, player.Id.Equals(lobby.HostId), player.Data["kdRatio"].Value, player.Data["winCount"].Value, player.Data["loseCount"].Value);
        }

        Invoke(nameof(ResetSettingUpCardsCooldown), 2f);
    }

    private void ResetSettingUpCardsCooldown() => isSettingUpCards = false;

    private void InstantiatePlayerCard(string currentRank, string playerName, bool isHost, string kdRatio, string winCount, string loseCount)
    {
        GameObject card = Instantiate(playerCardPrefab, playerCardParent);
        card.transform.localScale = playerCardScale;

        // handle data
        HandleLobbyPlayerCards playerCardDatas = card.GetComponent<HandleLobbyPlayerCards>();

        playerCardDatas.rankImage.sprite = RankManager.instance.ranks[RankManager.instance.GetCurrentRankSprite(currentRank)];
        playerCardDatas.usernameShadowText.text = playerName;
        playerCardDatas.usernameText.text = playerName;
        playerCardDatas.kdRatio.text = $"K/D Ratio: {kdRatio}";
        playerCardDatas.winCount.text = $"Win count: {winCount}";
        playerCardDatas.loseCount.text = $"Lose count: {loseCount}";
        playerCardDatas.hostSymbol.SetActive(isHost);
    }

    public void SetUsername()
    {
        int length = usernameInput.text.ToString().Length;

        // if username is valid
        if (length > 0 && length < maxUsernameLength)
        {
            LobbyManager.instance.username = usernameInput.text.ToString();
            PlayerPrefs.SetString("username", usernameInput.text.ToString());

            // check if its the first time creating a username
            // meaning that its the first play session
            // this means that unity netcode hasnt fully registered the user until they restart their game
            // this is crucial because it breaks the lobby system i dunno how
            if (PlayerPrefs.GetInt("isFirstTimePlaying") == 0)
            {
                restartGameObj.SetActive(true);
                usernameMenu.SetActive(false);

                PlayerPrefs.SetInt("isFirstTimePlaying", 1);
            }

            // if its not the first time, proceed normally
            else
            {
                usernameMenu.SetActive(false);
                mainLobbyMenu.SetActive(true);
            }

            // reset error message
            usernameErrorMessage.text = "";
        }

        // error message
        else if (length >= maxUsernameLength)
        {
            usernameErrorMessage.enabled = true;
            usernameErrorMessage.text = "Username cannot be long!";
        }

        else
        {
            usernameErrorMessage.enabled = true;
            usernameErrorMessage.text = "Username cannot be empty!";
        }
    }

    public void CreateLobbySettingsMenu()
    {
        // sets the ui elements off
        mainLobbyMenu.SetActive(false);
        lobbyCreationMenu.SetActive(true);
    }

    public void CreateLobby()
    {
        int length = lobbyNameInputField.text.ToString().Length;

        if (!CheckIfLobbyCanBeCreated())
        {
            lobbyCreationErrorMessage.enabled = true;
            lobbyCreationErrorMessage.text = $"Cannot create a new lobby until {maxCreateLobbyTimer - Mathf.RoundToInt(Time.time - createLobbyTimer)} seconds";
            return;
        }

        if (length <= maxLobbyNameLength && length != 0)
        {
            string gameMode = gameModeDropdowns.options[gameModeDropdowns.value].text;
            int maxPlayers = 2;

            if (gameMode == "1v1")
                maxPlayers = 2;
            else if (gameMode == "2v2")
                maxPlayers = 4;
            else if (gameMode == "3v3")
                maxPlayers = 6;

            // creates the lobby in the background with all the user inputs
            InstantiateLobby(lobbyNameInputField.text, gameModeDropdowns.options[gameModeDropdowns.value].text, maxPlayers, lobbyPrivacyToggle.isOn);

            // resets the timer
            createLobbyTimer = Time.time;
            canCreateLobby = false;
        }

        // error message
        else if (length >= maxLobbyNameLength)
        {
            lobbyCreationErrorMessage.enabled = true;
            lobbyCreationErrorMessage.text = "Lobby name is too Long!";
        }

        else
        {
            lobbyCreationErrorMessage.enabled = true;
            lobbyCreationErrorMessage.text = "Lobby name cannot be empty!";
        }
    }

    public void TurnOnPlayNowMenu()
    {
        mainLobbyMenu.SetActive(false);
        playNowMenu.SetActive(true);
    }

    public void TurnOnMainMenu()
    {
        mainLobbyMenu.SetActive(true);
        playNowMenu.SetActive(false);
    }

    public void CreateJoinLobbyMenu()
    {
        joinLobbyMenu.SetActive(true);
        mainLobbyMenu.SetActive(false);
    }

    public void ExitJoinLobbyMenu()
    {
        joinLobbyMenu.SetActive(false);

        if (IsOnMainMenu)
            mainLobbyMenu.SetActive(true);
        else
            playNowMenu.SetActive(true);
    }

    public async void JoinLobbyThroughUI()
    {
        lobby = await LobbyManager.instance.JoinLobbyByCode(codeInputField.text);

        if (lobby != null)
        {
            lobbyName.text = lobby.Name;
            roomCode.text = lobby.LobbyCode;

            joinLobbyMenu.SetActive(false);
            lobbyInstanceMenu.SetActive(true);

            SetUpLobbyInformationText();
            SetUpPlayerCards();
        }
        else
        {
            errorMessage.enabled = true;
            errorMessage.text = "Room invalid or full!";
        }
    }

    public void JoinLobbyThroughLobbyList(Lobby joinedLobby)
    {
        lobby = joinedLobby;

        lobbyName.text = lobby.Name;
        roomCode.text = lobby.LobbyCode;

        // disable the lobby list
        lobbyListObj.SetActive(false);
        lobbyInstanceMenu.SetActive(true);

        SetUpLobbyInformationText();
        SetUpPlayerCards();
    }

    private void SetUpLobbyInformationText()
    {
        lobbyPrivacyText.text = lobby.IsPrivate ? "Private" : "Public";
        gameModeText.text = $"GameMode: {lobby.Data["GameMode"].Value}";
    }

    public void LeaveLobby()
    {
        LobbyManager.instance.LeaveLobby();

        foreach (Transform child in playerCardParent)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        // reset the text when leaving
        roomCode.text = "Waiting...";

        lobbyInstanceMenu.SetActive(false);

        if (IsOnMainMenu)
            mainLobbyMenu.SetActive(true);
        else
            playNowMenu.SetActive(true);
    }

    public void LeaveLobbySettingsMenu()
    {
        lobbyCreationMenu.SetActive(false);

        // reset errorMessage
        errorMessage.enabled = false;

        if (IsOnMainMenu)
            mainLobbyMenu.SetActive(true);
        else
            playNowMenu.SetActive(true);
    }

    public void ChangeUsername()
    {
        // if we're in a lobby or the settings menu
        if (lobbyInstanceMenu.activeInHierarchy || SettingsMenuManager.instance.transform.GetChild(0).gameObject.activeInHierarchy) return;

        // turn on window
        playerStatsUIScript.TurnOnWindowed();

        mainLobbyMenu.SetActive(false);
        usernameMenu.SetActive(true);
        lobbyCreationMenu.SetActive(false);
        joinLobbyMenu.SetActive(false);
        playNowMenu.SetActive(false);
    }

    public async void QuickJoinMatch()
    {
        quickJoinGameObj.SetActive(true);

        lobby = await LobbyManager.instance.QuickJoinMatch();

        // if we found a lobby
        if (lobby != null)
            Invoke(nameof(ShowLobbyAfterJoiningQuickLobby), 0.5f);

        // if we couldnt
        else
            Invoke(nameof(StopQuickJoinMatchUI), 3f);
    }

    private void ShowLobbyAfterJoiningQuickLobby()
    {
        StopQuickJoinMatchUI();

        lobbyName.text = lobby.Name;
        roomCode.text = lobby.LobbyCode;

        joinLobbyMenu.SetActive(false);
        lobbyInstanceMenu.SetActive(true);

        SetUpLobbyInformationText();
        SetUpPlayerCards();
    }

    private void StopQuickJoinMatchUI() => quickJoinGameObj.SetActive(false);

    public void OpenLobbyListUI()
    {
        lobbyListObj.SetActive(true);
        UpdateLobbyList();
    }

    public async void UpdateLobbyList()
    {
        List<Lobby> lobbyList = await LobbyManager.instance.RefreshLobbyList();

        // if there is no lobbies
        if (lobbyList == null) return;

        // destroy previous lobbies
        foreach (Transform child in container)
            Destroy(child.gameObject);

        // add lobbies
        foreach (Lobby lobby in lobbyList)
        {
            GameObject createdLobbyCard = Instantiate(lobbyCardPrefab, container);
            HandleLobbyCardData dataScript = createdLobbyCard.GetComponent<HandleLobbyCardData>();

            dataScript.representedLobby = lobby;
            dataScript.InitializeData();
        }
    }

    public void ExitLobbyList() => lobbyListObj.SetActive(false);

    private void SetLobbyPlayerCount()
    {
        if (lobby == null) return;

        currentLobbyPlayerCount.text = lobby.Players.Count.ToString() + "/" + lobby.MaxPlayers.ToString();
    }

    public void TurnOnSettingMenu()
    {
        SettingsMenuManager.instance.EnableSettingMenu();
        mainLobbyMenu.SetActive(false);
        playNowMenu.SetActive(false);
    }

    public void StartMatch() => LobbyManager.instance.StartMatch();    

    public void StartPractice() => LobbyManager.instance.StartPracticeMatch();

    public void StartTutorial() => LobbyManager.instance.StartTutorial();

    public void ExitGame() => Application.Quit();
}
