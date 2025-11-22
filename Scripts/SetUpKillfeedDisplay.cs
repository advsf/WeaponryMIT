using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SetUpKillfeedDisplay : MonoBehaviour
{
    [Header("Text References")]
    public string killerName;
    public string killedName;
    public ulong killerId;
    public ulong killedId;
    public string weaponUsed;
    public string killerTeam;
    public string killedTeam;
    public bool isHeadshot;

    [Header("Settings")]
    [SerializeField] private float timeBeforeDestruction;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI killerNameText;
    [SerializeField] private TextMeshProUGUI killerNameShadowText;
    [SerializeField] private TextMeshProUGUI killedNameText;
    [SerializeField] private TextMeshProUGUI killedNameShadowText;

    [Header("Silhouettes")]
    [SerializeField] private GameObject[] silhouettes;
    [SerializeField] private GameObject headshotSilhouetteObj;

    [Header("Animation Reference")]
    [SerializeField] private Animator animator;

    private Color ownPlayerColor = Color.yellow;
    private Color teamPlayerColor = new(0.1321645f, 0.7171375f, 0.8490566f);
    private Color enemyPlayerColor = Color.red;

    private int exitHash;

    private void Start()
    {
        exitHash = Animator.StringToHash("isExit");

        // set up texts
        SetUpTexts();

        // set up the colors
        HandleColors();

        // determine the gun silhouette to use
        foreach (GameObject silhoutte in silhouettes)
            silhoutte.SetActive(weaponUsed == silhoutte.name);

        // check if headshot or not
        headshotSilhouetteObj.SetActive(isHeadshot);

        Invoke(nameof(PlayExitAnimation), timeBeforeDestruction - 0.4f);
        Invoke(nameof(DestroyKillfeed), timeBeforeDestruction);
    }

    private void SetUpTexts()
    {
        // check if the names are us, then change it to "Me"
        killerName = killerId == NetworkManager.Singleton.LocalClientId
            ? "Me" : killerName;

        killedName = killedId == NetworkManager.Singleton.LocalClientId
            ? "Me" : killedName;

        // set up the killernametext
        killerNameText.text = killerName;
        killerNameShadowText.text = killerName;

        // set up the killednametext
        killedNameText.text = killedName;
        killedNameShadowText.text = killedName;
    }

    private void HandleColors()
    {
        // if the killer is the local player
        if (killerId == NetworkManager.Singleton.LocalClientId)
        {
            ownPlayerColor.a = 1f;
            killerNameText.color = ownPlayerColor;

            ownPlayerColor.a = 0.1f;
            killerNameShadowText.color = ownPlayerColor;
        }

        // if the killer is on our name
        else if (killerTeam == PlayerInfo.instance.teamColor.Value)
        {
            ownPlayerColor.a = 1f;
            killerNameText.color = teamPlayerColor;

            ownPlayerColor.a = 0.1f;
            killerNameShadowText.color = teamPlayerColor;
        }

        // if the killer is an enemy
        else
        {
            enemyPlayerColor.a = 1f;
            killerNameText.color = Color.red;

            enemyPlayerColor.a = 0.1f;
            killerNameShadowText.color = Color.red;
        }

        // if the killed is the local player
        if (killedId == NetworkManager.Singleton.LocalClientId)
        {
            ownPlayerColor.a = 1f;
            killedNameText.color = ownPlayerColor;

            ownPlayerColor.a = 0.1f;
            killedNameShadowText.color = ownPlayerColor;
        }

        // if the killed is on our name
        else if (killedTeam == PlayerInfo.instance.teamColor.Value)
        {
            ownPlayerColor.a = 1f;
            killedNameText.color = teamPlayerColor;

            ownPlayerColor.a = 0.1f;
            killedNameShadowText.color = teamPlayerColor;
        }

        // if the killed is the enemy
        else
        {
            enemyPlayerColor.a = 1f;
            killedNameText.color = Color.red;

            enemyPlayerColor.a = 0.1f;
            killedNameShadowText.color = Color.red;
        }
    }

    private void PlayExitAnimation() => animator.SetTrigger(exitHash);

    private void DestroyKillfeed() => Destroy(gameObject);
}
