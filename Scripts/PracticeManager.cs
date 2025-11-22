using UnityEngine;
using Unity.Netcode;

public class PracticeManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [Header("Bot-Related")]
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private Transform botParent;

    private bool isSpawned = false;

    private void Start()
    {
        NetworkManager.Singleton.StartHost();

        SpawnBots(50);
    }

    private void Update()
    {
        if (!isSpawned)
            SpawnPlayer();
    }

    private void SpawnBots(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject botObj = Instantiate(botPrefab, botParent);

            botObj.transform.position = new(Random.Range(6.65f, 19.2f), 0.5f, Random.Range(-16.5f, 15f));
            botObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
        }
    }

    private void SpawnPlayer()
    {
        if (NetworkManager.Singleton.LocalClient == null) return;

        NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = spawnPoint.transform.position;
        isSpawned = true;
    }

    public void EnableInfiniteBullet(bool condition) => HandleActiveGun.instance.isBulletInfinite = condition;

    public void RespawnBotWithSpecifiedAmount(string amount)
    {
        if (int.TryParse(amount, out int amountInt))
        {
            if (amountInt < 0 || amountInt > 50) return;

            foreach (Transform bot in botParent)
                Destroy(bot.gameObject);

            SpawnBots(amountInt);
        }
    }
}
