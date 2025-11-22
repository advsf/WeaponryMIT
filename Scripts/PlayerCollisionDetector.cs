using UnityEngine;

public class PlayerCollisionDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<MovementScript>()) collision.collider.transform.position = SpawnPointManager.instance.DetermineSpawnPoint(0);
    }    
}
