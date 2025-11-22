using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Melee : NetworkBehaviour
{
    [Header("Input")]
    // later make this changable in the setting
    [SerializeField] private KeyCode meleeKey;
    [SerializeField] private KeyCode inspectKey;

    [Header("Settings")]
    [SerializeField] private float meleeCooldown;
    [SerializeField] private int meleeDamage;
    [SerializeField] private float meleeRange;
    [SerializeField] private bool isPractice = false;

    [Header("Audio References")]
    [SerializeField] private AudioSource localSource;
    [SerializeField] private AudioClip pullOutSoundEffect;
    [SerializeField] private AudioClip swingSoundEffect;
    [SerializeField] private AudioClip killSoundEffect;
    [SerializeField] private AudioClip hitMarkerSoundEffect;

    [Header("UI References")]
    [SerializeField] private GameObject killMessageObj;
    [SerializeField] private TextMeshProUGUI killMessageText;

    [Header("Animator References")]
    [SerializeField] private Animator animator;
    // animator hashes (performance booster)
    private int stabHash;
    private int pullOutHash;
    private int inspectHash;

    [Header("Layermasks")]
    [SerializeField] private LayerMask playerHitboxMask;

    [Header("Currency")]
    [SerializeField] private int moneyPerKill;

    [Header("Ragdoll References")]
    [SerializeField] private GameObject blueTeamRagdollPrefab;
    [SerializeField] private GameObject redTeamRagdollPrefab;
    [SerializeField] private float ragdollDuration;
    [SerializeField] private float ragdollYSpawnPosOffset;

    [Header("Other References")]
    [SerializeField] private PlayerInfo kdaManager;
    [SerializeField] private CrosshairManager crosshairManager;
    [SerializeField] private Health healthScript;
    [SerializeField] private HandleActiveGun gunChangeScript;

    [Header("Bools")]
    // used to check if the melee cooldown is in effect or not
    [SerializeField] private bool canMelee = false;

    // to stop melee during when UIs are active
    public bool isAbleToMelee = true;

    private NetworkObject hitPlayer;

    private Ray RayDirection { get => Camera.main.ViewportPointToRay(new(0.5f, 0.5f, 0f)); }

    private void Start()
    {
        if (!IsOwner) return;

        stabHash = Animator.StringToHash("isStabbed");
        pullOutHash = Animator.StringToHash("isPulledOut");
    }

    private void OnEnable()
    {
        if (!IsOwner) return;

        canMelee = false;

        PlayPullOutAnimation();

        Invoke(nameof(AllowMelee), 0.65f);
    }

    private void Update()
    {
        // if not owner, if ui is opened, if we're able to melee, and if the pull out animation is completed
        if (!IsOwner || StopEverythingWithUIOpened.IsUIOpened && !isAbleToMelee && canMelee) return;

        if (Input.GetKeyDown(meleeKey) && canMelee)
            Stab();

        // add later
        if (Input.GetKeyDown(inspectKey)) return;
    }

    private void Stab()
    {
        canMelee = false;

        // play swing audio
        localSource.PlayOneShot(swingSoundEffect);

        animator.SetTrigger(stabHash);

        if (!isPractice)
            PlayerDetection();
        else
            NonPlayerDetection();

        Invoke(nameof(ResetMeleeCooldown), meleeCooldown);
    }

    private void PlayerDetection()
    {
        if (Physics.Raycast(RayDirection, out RaycastHit hit, meleeRange, playerHitboxMask))
        {
            // if it's not a player
            if (hit.transform.GetComponentInParent<MovementScript>() == null) return;

            // if the player self hits
            if (hit.transform.GetComponentInParent<MovementScript>().transform == transform.GetComponentInParent<MovementScript>().transform) return;

            // if hit teammate
            if (hit.transform.GetComponentInParent<PlayerInfo>().teamColor.Value == PlayerInfo.instance.teamColor.Value) return;

            // hitmarker animation
            crosshairManager.PlayHeadHitmarkerAnimation();

            // play hit marker audio
            localSource.PlayOneShot(hitMarkerSoundEffect);

            // get the hit player
            hitPlayer = hit.transform.GetComponentInParent<MovementScript>().GetComponent<NetworkObject>();

            // sync damage across the network
            if (IsServer)
                DamagePlayerClientRpc(hitPlayer, meleeDamage);
            else
                DamagePlayerServerRpc(hitPlayer, meleeDamage);

            // if kill
            if (IsServer && hitPlayer.gameObject.GetComponent<Health>().currentHealth <= 0.0f)
                KillDetection();
            else if (!IsServer)
                ValidateKillServerRpc(hitPlayer);
        }
    }

    private void NonPlayerDetection()
    {
        if (Physics.Raycast(RayDirection, out RaycastHit hit, meleeRange, playerHitboxMask))
        {
            // if it's not a bot
            if (!hit.transform.GetComponentInParent<NonPlayerHealth>()) return;

            // hitmarker animation
            crosshairManager.PlayHeadHitmarkerAnimation();

            // play hit marker audio
            localSource.PlayOneShot(hitMarkerSoundEffect);

            hit.transform.GetComponentInParent<NonPlayerHealth>().DecreaseHealth(meleeDamage);
        }
    }

    private void KillDetection()
    {
        // spawn ragdoll
        SyncRagdoll();

        // increase kill counter
        kdaManager.kills.Value++;

        // play a ding sound
        localSource.PlayOneShot(killSoundEffect);

        // get the client id of the dead player
        ulong deadPlayerId = hitPlayer.OwnerClientId;

        if (gunChangeScript.currentWeapon.GetComponent<Shoot>() != null)
            gunChangeScript.currentWeapon.GetComponent<Shoot>().currentBulletCount = gunChangeScript.currentWeapon.GetComponent<Shoot>().maxBulletCount;

        // check if the round is over
        if (IsServer)
            GameManager.instance.EndRound(PlayerInfo.instance.teamColor.Value.ToString());
        else
            EndRoundServerRpc(PlayerInfo.instance.teamColor.Value.ToString());

        DisplayKillTextUI(hitPlayer.gameObject.name);

        if (IsServer)
            SendMessageToDeadPlayerClientRpc(transform.GetComponentInParent<NetworkObject>(), new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { deadPlayerId }
                }
            });
        else
            SendMessageToDeadPlayerServerRpc(transform.GetComponentInParent<NetworkObject>(), deadPlayerId);

        // kill feed
        HandleKillFeedboard.instance.InstantiateNewFeed(LobbyManager.instance.username, hitPlayer.gameObject.name, NetworkManager.Singleton.LocalClientId, hitPlayer.GetComponentInParent<NetworkObject>().OwnerClientId,
            PlayerInfo.instance.teamColor.Value.ToString(), hitPlayer.GetComponentInParent<PlayerInfo>().teamColor.Value.ToString(), gameObject.name, false);
    }

    // this function is only for preventing the player to melee while their pullout animation
    private void AllowMelee() => canMelee = true;

    [ServerRpc]
    private void EndRoundServerRpc(string wonTeam) => GameManager.instance.EndRound(wonTeam);

    [ServerRpc]
    private void ValidateKillServerRpc(NetworkObjectReference hitPlayer, ServerRpcParams serverRpcParams = default)
    {
        // checks if the player is already dead server side
        if (hitPlayer.TryGet(out NetworkObject hitPlayerObj))
        {
            if (hitPlayerObj.GetComponent<Health>().currentHealth <= 0.0f)
                ValidateKillClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                    }
                });
        }
    }

    [ClientRpc]
    private void ValidateKillClientRpc(ClientRpcParams clientRpcParams = default) => KillDetection();

    private void SyncRagdoll()
    {
        Vector3 deathPos = hitPlayer.transform.position;
        // this gets the orientation of the hitplayer
        Vector3 deathRot = hitPlayer.transform.GetChild(1).eulerAngles;
        string teamColor = hitPlayer.GetComponentInParent<PlayerInfo>().teamColor.Value.ToString();

        // spawn locally
        SpawnRagdoll(deathPos, deathRot, teamColor);

        if (IsServer)
            SpawnRagdollClientRpc(deathPos, deathRot, teamColor);
        else
            SpawnRagdollServerRpc(deathPos, deathRot, teamColor);
    }

    private void SpawnRagdoll(Vector3 deathPos, Vector3 deathRot, string teamColor)
    {
        // creates ragdoll
        GameObject ragdoll = Instantiate(PlayerInfo.instance.teamColor.Value == teamColor ? blueTeamRagdollPrefab : redTeamRagdollPrefab, new(deathPos.x, deathPos.y + ragdollYSpawnPosOffset, deathPos.z), Quaternion.Euler(deathRot));

        // delete ragdoll after  seconds
        Destroy(ragdoll, ragdollDuration);
    }

    [ServerRpc]
    private void SpawnRagdollServerRpc(Vector3 deathPos, Vector3 deathRot, string teamColor) => SpawnRagdollClientRpc(deathPos, deathRot, teamColor);

    [ClientRpc]
    private void SpawnRagdollClientRpc(Vector3 deathPos, Vector3 deathRot, string teamColor)
    {
        // cause we already spawned it client-sided
        if (IsOwner) return;

        SpawnRagdoll(deathPos, deathRot, teamColor);
    }

    [ServerRpc]
    private void DamagePlayerServerRpc(NetworkObjectReference player, int damage) => DamagePlayerClientRpc(player, damage);

    [ClientRpc]
    private void DamagePlayerClientRpc(NetworkObjectReference player, int damage)
    {
        if (player.TryGet(out NetworkObject playerObj))
            playerObj.GetComponent<Health>().DecreaseHealth(damage);
    }

    [ServerRpc]
    private void SendMessageToDeadPlayerServerRpc(NetworkObjectReference killer, ulong deadPlayerId)
    {
        SendMessageToDeadPlayerClientRpc(killer, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { deadPlayerId }
            }
        });
    }

    [ClientRpc]
    private void SendMessageToDeadPlayerClientRpc(NetworkObjectReference killer, ClientRpcParams clientRpcParams = default)
    {
        if (killer.TryGet(out NetworkObject killerObj))
            FollowKilledPlayer.instance.FollowPlayer(killerObj);
    }

    private void DisplayKillTextUI(string hitUsername)
    {
        killMessageObj.SetActive(false);
        killMessageObj.SetActive(true);
        killMessageText.text = hitUsername;

        Invoke(nameof(UndisplayKillTextUI), 2.5f);
    }

    private void UndisplayKillTextUI() => killMessageObj.SetActive(false);

    private void PlayPullOutAnimation()
    {
        if (pullOutHash == 0)
            pullOutHash = Animator.StringToHash("isPulledOut");

        animator.SetTrigger(pullOutHash);

        localSource.PlayOneShot(pullOutSoundEffect);
    }

    private void ResetMeleeCooldown() => canMelee = true;
}
