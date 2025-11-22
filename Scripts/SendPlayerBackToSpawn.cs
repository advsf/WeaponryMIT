using UnityEngine;

public class SendPlayerBackToSpawn : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.position = SpawnPointManager.instance.DetermineSpawnPoint(0);
    }
}
