using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class LeaderboardManager : NetworkBehaviour
{
    [Header("Keybinds")]
    [SerializeField] private KeyCode enablerKey = KeyCode.Tab;

    [Header("References")]
    [SerializeField] private GameObject leaderboardObj;
    [SerializeField] private Transform blueTeamCardsParent;
    [SerializeField] private Transform redTeamCardsParent;
    [SerializeField] private GameObject playerCardPrefab;

    [Header("WonCrosses References")]
    [SerializeField] private Sprite crownSprite;
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private Sprite crossSprite;
    [SerializeField] private Transform arrowsObj;
    [SerializeField] private Image[] blueTeamWonCrosses;
    [SerializeField] private Image[] redTeamWonCrosses;
    [SerializeField] private Image[] blueTeamOvertimeCrosses;
    [SerializeField] private Image[] redTeamOvertimeCrosses;

    [Header("Script References")]
    [SerializeField] private CrosshairManager crosshairManager;

    private bool canEnableLeaderboard = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;
    }

    private void Start()
    {
        if (!IsOwner) return;

        leaderboardObj.SetActive(false);

        HandleLeaderboardCrosses(1);

        Invoke(nameof(HandleSettingUpPlayerCards), 1.25f);
    }

    private void OnEnable()
    {
        GameManager.instance.roundCount.OnValueChanged += HandleReinitializingDats;
    }

    private void OnDisable()
    {
        GameManager.instance.roundCount.OnValueChanged -= HandleReinitializingDats;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // to keep up with the delay in the start method, we will make the player unable to open the leaderbaord until it's fully loaded
        if (!canEnableLeaderboard) return;

        // enable leaderboard UI
        leaderboardObj.SetActive(Input.GetKey(enablerKey));

        // disables the crosshair when the leaderboard is on
        if (leaderboardObj.activeInHierarchy)
            crosshairManager.DisableCrosshair();

        // otherwise disable it
        else
            crosshairManager.isCrosshairOn = true;

        HandleLeaderboardCrosses(GameManager.instance.isOvertime.Value ? GameManager.instance.overtimeRoundCount.Value : GameManager.instance.roundCount.Value);
    }

    private void HandleLeaderboardCrosses(int currentRound)
    {
        // handle the crowns that showcase at what round the player needs to win to win the game
        // if not overtime
        if (!GameManager.instance.isOvertime.Value)
            HandleAmountToWinCross(currentRound - 1);

        // if overtime
        else
            HandleAmountToOvertimeWinCross(GameManager.instance.blueTeamOvertimeWonAmount.Value, GameManager.instance.redTeamOvertimeWonAmount.Value);
    }

    public void HandleCurrentRoundArrows(int currentRound)
    {
        if (GameManager.instance.isOvertime.Value) return;

        // move the arrows to the current crosses position
        arrowsObj.gameObject.SetActive(true);

        arrowsObj.position = new Vector3(blueTeamWonCrosses[currentRound].transform.position.x, arrowsObj.position.y, arrowsObj.position.z);
    }

    public void HandleOvertimeCurrentRoundArrows(int currentOvertimeRound)
    {
        arrowsObj.position = new Vector3(blueTeamOvertimeCrosses[currentOvertimeRound].transform.position.x, arrowsObj.position.y, arrowsObj.position.z);
    }

    public void HandleWinCrosses(string wonTeam, int currentRound)
    {
        // if we win
        if (wonTeam == PlayerInfo.instance.teamColor.Value)
        {
            blueTeamWonCrosses[currentRound].sprite = crossSprite;
            blueTeamWonCrosses[currentRound].color = new(0.2688679f, 0.6182432f, 1, 1); // blue color

            redTeamWonCrosses[currentRound].enabled = false;
        }

        // if we lose
        else
        {
            redTeamWonCrosses[currentRound].sprite = crossSprite;
            redTeamWonCrosses[currentRound].color = new(0.9622642f, 0.3881591f, 0.3131897f, 1); // red color

            blueTeamWonCrosses[currentRound].enabled = false;
        }
    }

    public void HandleOvertimeWinCrosses(string wonTeam, int currentRound)
    {
        // if we win
        if (wonTeam == PlayerInfo.instance.teamColor.Value)
        {
            blueTeamOvertimeCrosses[currentRound].sprite = crossSprite;
            blueTeamOvertimeCrosses[currentRound].color = new(0.2688679f, 0.6182432f, 1, 1); // blue color

            redTeamOvertimeCrosses[currentRound].enabled = false;
        }

        // if we lose
        else
        {
            redTeamOvertimeCrosses[currentRound].sprite = crossSprite;
            redTeamOvertimeCrosses[currentRound].color = new(0.9622642f, 0.3881591f, 0.3131897f, 1); // red color

            blueTeamOvertimeCrosses[currentRound].enabled = false;
        }
    }

    private void HandleAmountToWinCross(int currentRound)
    {
        // similar to the valorant in-game leaderboard
        // the crosses that represent the cross that the teams need to win will be highlighted

        // if our team is blue
        if (PlayerInfo.instance.teamColor.Value == "Blue")
        {
            // if we can still win the game without going to overtime
            if ((currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.blueTeamWonAmount.Value)) < blueTeamWonCrosses.Length)
            {
                blueTeamWonCrosses[currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.blueTeamWonAmount.Value)].sprite = crownSprite;
                blueTeamWonCrosses[currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.blueTeamWonAmount.Value)].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color
            }

            // if the opponent can still win the game without going to overtime
            if ((currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.redTeamWonAmount.Value)) < redTeamWonCrosses.Length)
            {
                redTeamWonCrosses[currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.redTeamWonAmount.Value)].sprite = crownSprite;
                redTeamWonCrosses[currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.redTeamWonAmount.Value)].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color
            }
        }

        // if our team is red
        else if (PlayerInfo.instance.teamColor.Value == "Red")
        {
            if ((currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.redTeamWonAmount.Value)) < blueTeamWonCrosses.Length)
            {
                blueTeamWonCrosses[currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.redTeamWonAmount.Value)].sprite = crownSprite;
                blueTeamWonCrosses[currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.redTeamWonAmount.Value)].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color
            }

            if ((currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.blueTeamWonAmount.Value)) < blueTeamWonCrosses.Length)
            {
                redTeamWonCrosses[currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.blueTeamWonAmount.Value)].sprite = crownSprite;
                redTeamWonCrosses[currentRound - 1 + (GameManager.instance.roundsToWin - GameManager.instance.blueTeamWonAmount.Value)].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color
            }
        }
    }

    public void HandleAmountToOvertimeWinCross(int blueOvertimeWonAmount, int redOvertimeWonAmount)
    {
        // check if its the beginning of overtime
        if (blueOvertimeWonAmount == 0 && redOvertimeWonAmount == 0)
        {
            blueTeamOvertimeCrosses[1].sprite = crownSprite;
            blueTeamOvertimeCrosses[1].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color

            redTeamOvertimeCrosses[1].sprite = crownSprite;
            redTeamOvertimeCrosses[1].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color

            // exit because we dont need to do anything more
            return;
        }

        // if our team is blue
        if (PlayerInfo.instance.teamColor.Value == "Blue")
        {
            // if we won (disable opponents win crown)
            if (blueOvertimeWonAmount == 1)
            {
                blueTeamOvertimeCrosses[1].sprite = crownSprite;
                blueTeamOvertimeCrosses[1].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color

                redTeamOvertimeCrosses[1].sprite = dotSprite;
                redTeamOvertimeCrosses[1].color = new(0, 0, 0, 0.1568628f); // translucent black color
            }

            // if the opponents won (disable our win crown cause we cant win, we need to tie)
            else if (redOvertimeWonAmount == 1)
            {
                blueTeamOvertimeCrosses[1].sprite = dotSprite;
                blueTeamOvertimeCrosses[1].color = new(0, 0, 0, 0.1568628f); // translucent black color

                redTeamOvertimeCrosses[1].sprite = crownSprite;
                redTeamOvertimeCrosses[1].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color
            }
        }

        // if our team is red
        else if (PlayerInfo.instance.teamColor.Value == "Red")
        {
            // if we won (disable opponents win crown)
            if (redOvertimeWonAmount == 1)
            {
                blueTeamOvertimeCrosses[1].sprite = crownSprite;
                blueTeamOvertimeCrosses[1].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color

                redTeamOvertimeCrosses[1].sprite = dotSprite;
                redTeamOvertimeCrosses[1].color = new(0, 0, 0, 0.1568628f); // translucent black color
            }

            // if the opponents won
            else if (blueOvertimeWonAmount == 1)
            {
                blueTeamOvertimeCrosses[1].sprite = dotSprite;
                blueTeamOvertimeCrosses[1].color = new(0, 0, 0, 0.1568628f); // translucent black color

                redTeamOvertimeCrosses[1].sprite = crownSprite;
                redTeamOvertimeCrosses[1].color = new(1, 0.8820793f, 0.3160377f, 1); // yellow color
            }
        }
    }

    private void HandleReinitializingDats(int previous, int current)
    {
        if (!GameManager.instance.isOvertime.Value)
            // because when a new round commences, we need to make sure to remove the outdated crown symbols
            for (int i = current - 1; i < blueTeamWonCrosses.Length; i++)
            {
                blueTeamWonCrosses[i].sprite = dotSprite;
                blueTeamWonCrosses[i].color = new(0, 0, 0, 0.1568628f); // translucent black color

                redTeamWonCrosses[i].sprite = dotSprite;
                redTeamWonCrosses[i].color = new(0, 0, 0, 0.1568628f); // translucent black color
            }
    }

    public void ResetOvertimeCrosses()
    {
        for (int i = 0; i < blueTeamOvertimeCrosses.Length; i++)
        {
            blueTeamOvertimeCrosses[i].enabled = true;
            blueTeamOvertimeCrosses[i].sprite = dotSprite;
            blueTeamOvertimeCrosses[i].color = new(0, 0, 0, 0.1568628f); // translucent black color

            redTeamOvertimeCrosses[i].enabled = true;
            redTeamOvertimeCrosses[i].sprite = dotSprite;
            redTeamOvertimeCrosses[i].color = new(0, 0, 0, 0.1568628f); // translucent black color
        }
    }

    private void HandleSettingUpPlayerCards()
    {
        foreach (RectTransform child in blueTeamCardsParent)
            Destroy(child.gameObject);

        foreach (RectTransform child in redTeamCardsParent)
            Destroy(child.gameObject);

        string localClientTeamColor = PlayerInfo.instance.teamColor.Value.ToString();

        foreach (Transform player in GameObject.Find("Players").transform)
        {
            // because on the client side their own team should be the blue color, so just invert the order (if client is red team -> make it still appear blue)
            if (player.GetComponent<PlayerInfo>().teamColor.Value == localClientTeamColor)
            {
                GameObject playerCard = Instantiate(playerCardPrefab, blueTeamCardsParent);
                playerCard.GetComponent<SetPlayerCardLeardboardTexts>().targetPlayer = player.gameObject;
            }

            // if not on the same team
            else
            {
                GameObject playerCard = Instantiate(playerCardPrefab, redTeamCardsParent);
                playerCard.GetComponent<SetPlayerCardLeardboardTexts>().targetPlayer = player.gameObject;
            }
        }

        canEnableLeaderboard = true;
    }

    [ClientRpc]
    private void UpdateLeaderboardClientRpc() => Invoke(nameof(HandleSettingUpPlayerCards), (PlayerInfo.instance.ping.Value * 0.01f) + 0.1f);
}