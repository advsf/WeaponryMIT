using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class PlayerInfo : NetworkBehaviour
{
    public static PlayerInfo instance;

    [Header("Information")]
    public NetworkVariable<int> ping = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Stats")]
    public NetworkVariable<int> kills = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> deaths = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> roundsWon = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString32Bytes> teamColor = new("Blue", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> currentRankIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // use the int as an index to access the rank sprites
    public NetworkVariable<int> spawnIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // used to determine spawn point

    [Header("Settings")]
    [SerializeField] private float pingSmoothingFactor;

    private int currentPing;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner) 
            instance = this;
    }

    private void Start()
    {
        // initialize static datas
        if (IsOwner)
            currentRankIndex.Value = PlayerPrefs.GetInt("currentRank");
    }

    private void Update()
    {
        if (!IsOwner) return;

        int rawPing = (int)NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId);

        currentPing = Mathf.RoundToInt(Mathf.Lerp(currentPing, rawPing, pingSmoothingFactor));

        ping.Value = currentPing;
    }
}
