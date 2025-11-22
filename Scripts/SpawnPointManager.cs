using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager instance;
    [SerializeField] private Transform[] blueSpawnPoint;
    [SerializeField] private Transform[] redSpawnPoint;

    private void Start() => instance = this;

    public Vector3 DetermineSpawnpointWithName(string teamColor)
    {
        if (teamColor == "Blue")
            return blueSpawnPoint[0].position;
        else
            return redSpawnPoint[0].position;
    }

    public Vector3 DetermineSpawnPoint(int spawnIndex)
    {
        if (PlayerInfo.instance.teamColor.Value.Equals("Blue"))
            return blueSpawnPoint[spawnIndex].position;
        else
            return redSpawnPoint[spawnIndex].position;
    }
}
