using UnityEngine;
using Unity.Netcode;
using System.Collections;
using TMPro;

public class FollowKilledPlayer : NetworkBehaviour
{
    public static FollowKilledPlayer instance;

    [Header("UI References")]
    [SerializeField] private GameObject killDeathUI;
    [SerializeField] private TextMeshProUGUI shadowText;
    [SerializeField] private TextMeshProUGUI foregroundText;

    [Header("Settings")]
    [SerializeField] private float smoothingFactor;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
            instance = this;
    }

    public void FollowPlayer(NetworkObject killer)
    {
        StartCoroutine(SetTransformToPlayer(killer));

        StartCoroutine(SetDeathscreenUI(killer.name.ToString()));
    }

    private IEnumerator SetTransformToPlayer(NetworkObject killer)
    {
        float time = Time.time;

        while (Time.time - time <= GameManager.instance.respawnCooldown - ((PlayerInfo.instance.ping.Value * 0.05f) - 0.1f) || Health.instance.currentHealth == 0.0f)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, new(killer.transform.position.x + 2.0f, killer.transform.position.y, killer.transform.position.z), Time.deltaTime * smoothingFactor);
            yield return null;
        }
    }

    public IEnumerator SetDeathscreenUI(string killerName)
    {
        killDeathUI.SetActive(true);
        foregroundText.text = $"YOU'VE BEEN OWNED BY {killerName}";
        shadowText.text = foregroundText.text.ToString();

        float time = Time.time;

        while (Time.time - time <= GameManager.instance.respawnCooldown - ((PlayerInfo.instance.ping.Value * 0.05f) - 0.1f) || Health.instance.currentHealth == 0.0f)
            yield return null;

        killDeathUI.SetActive(false);
    }
}
