using UnityEngine;
using Unity.Netcode;

public class MapManager : NetworkBehaviour
{
    public static MapManager instance;

    [Header("References")]
    // map 0: crate map
    // map 1: drop down map
    // map 2: 
    [SerializeField] private GameObject[] maps;
    private int previousMapIndex = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        if (instance == null)
            instance = this;

        ChangeMapClientRpc(0);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        instance = null;
    }

    public void ChangeMap()
    {
        ChangeMapClientRpc(PickNewMap());
    }

    private int PickNewMap()
    {
        int newIndex = Random.Range(0, maps.Length);

        if (newIndex == previousMapIndex)
            return PickNewMap();
        else
            return newIndex;
    }

    [ClientRpc]
    private void ChangeMapClientRpc(int mapIndex)
    {
        foreach (GameObject map in maps)
            map.SetActive(false);

        maps[mapIndex].SetActive(true);

        previousMapIndex = mapIndex;
    }
}
