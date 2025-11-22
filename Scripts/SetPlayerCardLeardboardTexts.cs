using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class SetPlayerCardLeardboardTexts : NetworkBehaviour
{
    public GameObject targetPlayer;

    [Header("Information")]
    public bool isBlueTeam = false;
    public bool isRedTeam = false;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI kdaText;
    [SerializeField] private TextMeshProUGUI currentWeaponText;
    [SerializeField] private TextMeshProUGUI pingText;

    [Header("UI References")]
    [SerializeField] private Image rankImage;
    [SerializeField] private GameObject blueBackground;
    [SerializeField] private GameObject redBackground;

    private PlayerInfo kdaManagerScript;
    private HandleActiveGun handleGunScript;
    private PlayerInfo playerInfo;

    private ulong playerId;

    // this script is used for handling each prefab in the leaderboard
    // and corresponds every text to a specific player
    // for static texts (only username)
    // NOTE: i have made it so that every player object's name will be set to their actual username

    private void OnEnable()
    {
        // set the scripts
        kdaManagerScript = targetPlayer.GetComponent<PlayerInfo>();
        handleGunScript = targetPlayer.GetComponent<HandleActiveGun>();
        playerInfo = targetPlayer.GetComponent<PlayerInfo>();
        playerId = targetPlayer.GetComponent<NetworkObject>().OwnerClientId;

        // disable rank image
        rankImage.sprite = null;

        // set up rank image
        Invoke(nameof(SetUpRankImage), PlayerInfo.instance.ping.Value * 0.001f);

        // set the team colors (local team because i might add a 2v2 mode)
        string localTeam = PlayerInfo.instance.teamColor.Value.ToString();

        // set the background colors
        if (playerInfo.teamColor.Value == localTeam)
            blueBackground.SetActive(true);
        else 
            redBackground.SetActive(true);
        
        // if this card is based on our local player
        if (playerId == NetworkManager.LocalClientId)
        {
            // sets the username text
            usernameText.text = $"{targetPlayer.name} (Me)";

            // for visual appeal, to make it more noticable and attractive
            SetEveryTextToGold();
        }

        // else set it normally
        else
            usernameText.text = targetPlayer.name;

        // if for some reason we cant get the ping
        pingText.text = "NA";
    }

    private void Update()
    {
        // set the texts
        kdaText.text = $"{kdaManagerScript.kills.Value} / {kdaManagerScript.deaths.Value}";
        currentWeaponText.text = handleGunScript.currentWeaponName.Value.ToString();
        pingText.text = playerInfo.ping.Value.ToString();
    }

    private void SetEveryTextToGold()
    {
        usernameText.color = Color.yellow;
        kdaText.color = Color.yellow;
        currentWeaponText.color = Color.yellow;
        pingText.color = Color.yellow;
    }

    private void SetUpRankImage() => rankImage.sprite = RankManager.instance.ranks[playerInfo.currentRankIndex.Value];
}
