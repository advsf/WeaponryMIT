using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

public class Shoot : NetworkBehaviour
{
    [Header("Setting")]
    [SerializeField] private float pullOutDuration;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletLifetime;
    [SerializeField] private float shotCooldown;
    public int maxBulletCount;
    public int currentBulletCount;
    public float reloadTime;
    public bool isBulletInfinite = false;

    [Header("Damage Settings")]
    public float headshotDamage;
    public float bodyshotDamage;
    public float legshotDamage;

    [Header("Recoil Settings")]
    [SerializeField] private Recoil recoilScript;
    [SerializeField] private float timeBeforeRecoil;
    [SerializeField] private float recoilSmoothingAmount;
    [SerializeField] private bool isHeavyWeapon;
    [SerializeField] private bool isLightPistol;
    [SerializeField] private bool isHeavyPistol;
    private float startShootingTime; // only use this for automatic guns

    [Header("Scope Settings")]
    [SerializeField] private float firstScopeInAmount;
    [SerializeField] private float secondScopeInAmount;
    [SerializeField] private float scopeOutAmount;
    [SerializeField] private float scopeInSmoothAmount;
    [SerializeField] private float scopeOutSmoothAmount;
    [SerializeField] private float zoomAdjustSmoothAmount;
    [SerializeField] private Vector3 scopeInPos;
    // for the weaponholder if you disable the sway, we need consistently so we reset the pos of this
    [SerializeField] private Vector3 weaponHolderScopeInPos;
    private bool isFirstLevelZoomed = false;
    private bool isSecondLevelZoomed = false;

    [Header("Sway Settings")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private WeaponSway swayScript;

    [Header("Sounds")]
    // change this into individual 
    [SerializeField] private AudioClip[] gunSoundEffects;
    // index 0: shooting sound
    // index 1: reload sound
    // index 2: scope in sound effect
    // index 3: scope out sound effect
    // index 4: zoom adjust sound effect

    [SerializeField] private AudioClip emptyGunSoundEffect;
    [SerializeField] private AudioClip headHitmarkerSoundEffect;
    [SerializeField] private AudioClip bodyHitMarkerSoundEffect;
    [SerializeField] private AudioClip killSoundEffect;
    [SerializeField] private AudioClip pullOutSoundEffect;

    [Header("Prefabs")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("Audio References")]
    [SerializeField] private AudioSource localSource;
    [SerializeField] private AudioSource globalSource;

    [Header("UI References")]
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject killMessageObj;
    [SerializeField] private TextMeshProUGUI killMessageText;

    [Header("Script Referneces")]
    [SerializeField] private PlayerInfo playerInfo;
    [SerializeField] private CrosshairManager crosshairManager;
    [SerializeField] private Health healthScript;
    [SerializeField] private HandleActiveGun gunChangeScript;
    [SerializeField] private RotateCam lookScript;

    [Header("Other References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform guntip;
    [SerializeField] private Transform orientation;
    [SerializeField] private Camera zoomCam;

    [Header("Currency")]
    public int moneyPerKill;

    // particle effects
    private ParticleSystem impactEffect;

    // animator hashes
    private int shootHash;
    private int reloadHash;
    private int pullOutHash;

    [Header("Setting")]
    public bool isAutomatic;
    public bool isShotgun; // implement later if u think adding a shotgun is good
    public bool IsAimable;
    public bool isWallBangable;
    public bool canPullOut = false;
    public bool isPractice = false; // used when

    [Header("Ragdoll References")]
    [SerializeField] private GameObject blueTeamRagdollPrefab;
    [SerializeField] private GameObject redTeamRagdollPrefab;
    [SerializeField] private float ragdollDuration;
    [SerializeField] private float ragdollYSpawnPosOffset;

    // layer masks
    [Header("Layer Masks")]
    [SerializeField] private LayerMask everythingButPlayerLayerMask;
    [SerializeField] private LayerMask playerHitboxMask;

    [Header("Other")]
    public bool isReloading;
    public bool isShooting;
    public bool isAiming;
    public bool isPullingOut;
    public bool isReadyToShoot = true;

    // get the ray direction
    private Ray RayDirection { get => Camera.main.ViewportPointToRay(new(0.5f, 0.5f, 0f)); }

    private bool isHeadshot = false;

    // get original pos of a gun (for those that is aimable)
    private Vector3 originalPos;

    // global variable
    private NetworkObject hitPlayer;

    // to prevent single multi kills due to high rate of fire
    List<ulong> killedPlayerIds = new();

    private void Start()
    {
        if (!IsOwner) return;

        originalPos = transform.localPosition;

        // get the animator hashes (performance boost)
        shootHash = Animator.StringToHash("isShot");
        reloadHash = Animator.StringToHash("isReloaded");
        pullOutHash = Animator.StringToHash("isPullingOut");

        zoomCam.fieldOfView = scopeOutAmount;

        // initialize effects
        impactEffect = GameObject.Find("BulletImpactEffect").GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (!IsOwner) return;

        // play the pulling out animation
        if (canPullOut)
        {
            animator.SetTrigger(pullOutHash);

            // play pull out sound effect
            localSource.PlayOneShot(pullOutSoundEffect);

            // while the pull out animation is happening, dont allow the player to shoot
            isPullingOut = true;
            Invoke(nameof(AllowShootingAfterPulledOut), pullOutDuration);
        }
    }

    private void OnDisable()
    {
        // remove the killed player dicts
        killedPlayerIds.Clear();

        // reset zoom level
        if (IsAimable)
        {
            isFirstLevelZoomed = false;
            isSecondLevelZoomed = false;
        }

        // prevent the gun from reloading while its not active
        isReloading = false;

        animator.SetBool(reloadHash, false);
    }

    private void Update()
    {
        // if not owner or is the weapon being pulled out or if the settings menu is on
        if (!IsOwner || isPullingOut || StopEverythingWithUIOpened.IsUIOpened) return;

        // handle shooting
        if (isAutomatic)
            HandleAutomaticGuns();
        else
            HandleNonAutomaticGuns();

        // handle aiming
        if (IsAimable)
            HandleAiming();

        // handle recording shooting time for automatic weapons to deal with recoil
        if (!Input.GetKey(KeyCode.Mouse0))
            startShootingTime = Mathf.Lerp(startShootingTime, Time.time, recoilSmoothingAmount);

        // becuase unity is dumb and when the player dies while reloading or something it causes the animator reference to be set to null so yeah
        animator = transform.GetComponent<Animator>();

        // handle reloading
        HandleReloading();
    }

    private void HandleAutomaticGuns()
    {
        if (Input.GetKey(KeyCode.Mouse0) && isReadyToShoot && !isReloading && currentBulletCount > 0)
        {
            // play animation
            animator.SetTrigger(shootHash);

            // handle recoil
            if (Time.time - startShootingTime >= timeBeforeRecoil)
                HandleRecoil();

            // player detection
            if (!isPractice)
                PlayerDetection();
            // non player detection
            else
                NonplayerDetection();

            // visual effects related
            SpawnBullet();
            PlayShotSoundEffect(true);
            PlayEffects();

            // prevent spam of bullets
            ShotCooldown();

            // subtract the current bullet count;
            if (!isBulletInfinite)
                currentBulletCount--;
        }

        // if there is no bullets
        else if (Input.GetKey(KeyCode.Mouse0) && isReadyToShoot && !isReloading && currentBulletCount <= 0)
        {
            // play empty mag sound effect
            localSource.PlayOneShot(emptyGunSoundEffect);

            ShotCooldown();
        }
    }

    private void HandleNonAutomaticGuns()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && isReadyToShoot && !isReloading && currentBulletCount > 0)
        {
            // play animation
            animator.SetTrigger(shootHash);

            // handle recoil
            HandleRecoil();

            // player detection
            if (!isPractice)
                PlayerDetection();
            // non player detection
            else
                NonplayerDetection();

            // visual effects related
            SpawnBullet();
            PlayShotSoundEffect(true);
            PlayEffects();

            // prevent spam of bullets
            ShotCooldown();

            // subtract the current bullet count;
            if (!isBulletInfinite)
                currentBulletCount--;
        }

        // if the bullets are empty
        else if (Input.GetKeyDown(KeyCode.Mouse0) && isReadyToShoot && !isReloading && currentBulletCount <= 0)
        {
            // play empty mag sound effect
            localSource.PlayOneShot(emptyGunSoundEffect);

            ShotCooldown();
        }
    }
    
    private void HandleReloading()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isShooting && !isReloading && currentBulletCount < maxBulletCount)
        {
            // play animation
            animator.SetBool(reloadHash, true);

            // play sound effect
            PlayShotSoundEffect(false);

            // reset the bullet count
            ReloadGun();

            // stop animation
            Invoke(nameof(StopReloadAnimation), reloadTime);
        }
    }

    // shit code here watch out
    private void HandleAiming()
    {
        // play sound effect
        if (Input.GetKeyDown(KeyCode.Mouse1) && !isReloading && !isShooting && !isFirstLevelZoomed && !isSecondLevelZoomed)
            localSource.PlayOneShot(gunSoundEffects[2]);

        // zoom in sound effect
        if (Input.GetKeyDown(KeyCode.Mouse1) && !isReloading && !isShooting && isFirstLevelZoomed)
            localSource.PlayOneShot(gunSoundEffects[4]);

        // zoom out sound effect
        if (Input.GetKeyDown(KeyCode.Mouse1) && !isReloading && !isShooting && isSecondLevelZoomed)
            localSource.PlayOneShot(gunSoundEffects[3]);

        // handle zoom level (first level)
        if (Input.GetKeyDown(KeyCode.Mouse1) && !isFirstLevelZoomed && !isSecondLevelZoomed && !isReloading && !isShooting)
        {
            isAiming = true;
            isFirstLevelZoomed = true;
        }

        // second zoom level
        else if (Input.GetKeyDown(KeyCode.Mouse1) && isFirstLevelZoomed && !isReloading && !isShooting)
        {
            isFirstLevelZoomed = false;
            isSecondLevelZoomed = true;
        }

        // allow player to scope out (after reloading and shooting too)
        else if (Input.GetKeyDown(KeyCode.Mouse1) && isSecondLevelZoomed || isReloading || isShooting)
        {
            isFirstLevelZoomed = false;
            isSecondLevelZoomed = false;

            isAiming = false;
        }

        // handle zooming in
        if (isAiming && !isReloading && !isShooting)
        {
            // disable sway because it's weird that the gun sways while being zoomed in
            swayScript.enabled = false;

            // smoothly increase the zoom amount
            zoomCam.fieldOfView = Mathf.Lerp(zoomCam.fieldOfView, isFirstLevelZoomed ? firstScopeInAmount : secondScopeInAmount, isFirstLevelZoomed ? Time.deltaTime * scopeInSmoothAmount : zoomAdjustSmoothAmount);

            // reset the pos and rotation of weaponHolder
            weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition, weaponHolderScopeInPos, Time.deltaTime * scopeInSmoothAmount);

            Vector3 rotation = Vector3.Slerp(new(weaponHolder.localRotation.x, weaponHolder.localRotation.y, weaponHolder.localRotation.z), new(0f, 0f, 0f), Time.deltaTime * scopeOutSmoothAmount);
            weaponHolder.localRotation = Quaternion.Euler(rotation);

            // move the gun
            transform.localPosition = Vector3.Lerp(transform.localPosition, scopeInPos, Time.deltaTime * scopeInSmoothAmount);
        }

        // zoom out
        else 
        {
            isFirstLevelZoomed = false;
            isSecondLevelZoomed = false;

            // move gun back to original pos
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, Time.deltaTime * scopeOutSmoothAmount);
            zoomCam.fieldOfView = Mathf.Lerp(zoomCam.fieldOfView, scopeOutAmount, Time.deltaTime * scopeOutSmoothAmount);

            // enable sway
            swayScript.enabled = true;

            isAiming = false;
        }
    }

    private void PlayerDetection()
    {
        // guess what! detection is client-sided! so fuck you you "server-side is supreme" fucks
        if (isWallBangable)
        {
            RaycastHit[] hits = Physics.RaycastAll(RayDirection, Mathf.Infinity);

            // the value doesnt matter, only the key will be used to check if we hit the same player or not
            Dictionary<string, string> hitPlayers = new();

            foreach (RaycastHit hit in hits)
            {
                // if we hit the same player
                if (hit.transform.GetComponentInParent<MovementScript>()
                    && hitPlayers.ContainsKey(hit.transform.GetComponentInParent<MovementScript>().name)) 
                    continue;

                // deal damages and check if we kill him
                CheckIfHitPlayer(hit);

                // add the hit player to the dictionary
                if (hit.transform.GetComponentInParent<MovementScript>()
                    && hit.transform.GetComponentInParent<MovementScript>() != transform.GetComponentInParent<MovementScript>())
                    hitPlayers.Add(hit.transform.GetComponentInParent<MovementScript>().name, hit.transform.GetComponentInParent<MovementScript>().name);
            } 
        }

        else if (Physics.Raycast(RayDirection, out RaycastHit hit, Mathf.Infinity))
            CheckIfHitPlayer(hit);
    }

    private void NonplayerDetection()
    {
        if (isWallBangable)
        {
            RaycastHit[] hits = Physics.RaycastAll(RayDirection, Mathf.Infinity, playerHitboxMask);

            foreach (RaycastHit hit in hits)
                // deal damages and check if we kill him
                CheckIfHitNonPlayer(hit);
        }

        else if (Physics.Raycast(RayDirection, out RaycastHit hit, Mathf.Infinity, playerHitboxMask))
            CheckIfHitNonPlayer(hit);
    }

    private void CheckIfHitPlayer(RaycastHit hit)
    {
        // if not player 
        if (!hit.transform.GetComponentInParent<MovementScript>()) return;

        // if hit own player
        if (hit.transform.GetComponentInParent<MovementScript>() == transform.GetComponentInParent<MovementScript>()) return;

        // if hit teammate
        if (hit.transform.GetComponentInParent<PlayerInfo>().teamColor.Value == PlayerInfo.instance.teamColor.Value) return;

        // get the hit player
        hitPlayer = hit.transform.GetComponentInParent<MovementScript>().GetComponent<NetworkObject>();

        // if kill somebody we already killed
        if (killedPlayerIds.Contains(hitPlayer.OwnerClientId)) return;

        // determine damage
        float damage = DetermineDamage(hit);

        // sync damage across the network
        if (IsServer)
            DamagePlayerClientRpc(hitPlayer, damage);
        else
            DamagePlayerServerRpc(hitPlayer, damage);

        // check if you killed a player
        if (IsServer && hitPlayer.gameObject.GetComponent<Health>().currentHealth <= 0.0f)
            KillDetection();
        else if (!IsServer)
            ValidateKillServerRpc(hitPlayer);

        // reset bools
        isHeadshot = false;
    }

    private void CheckIfHitNonPlayer(RaycastHit hit)
    {
        if (!hit.transform.GetComponentInParent<NonPlayerHealth>()) return;

        // determine damage
        float damage = DetermineDamage(hit);

        hit.transform.GetComponentInParent<NonPlayerHealth>().DecreaseHealth(damage);
    }

    private void KillDetection()
    {
        // again check again to make sure that the player is dead
        if (killedPlayerIds.Contains(hitPlayer.OwnerClientId)) return;

        // add killfeed
        HandleKillFeedboard.instance.InstantiateNewFeed(LobbyManager.instance.username, hitPlayer.gameObject.name, NetworkManager.Singleton.LocalClientId, hitPlayer.GetComponentInParent<NetworkObject>().OwnerClientId,
            PlayerInfo.instance.teamColor.Value.ToString(), hitPlayer.GetComponentInParent<PlayerInfo>().teamColor.Value.ToString(), gameObject.name, isHeadshot);

        // spawn ragdoll
        SyncRagdoll();

        // increase kill counter
        playerInfo.kills.Value++;

        // increase kill data
        PlayerPrefs.SetInt("killsCount", GameManager.instance.isRanked ? PlayerPrefs.GetInt("killsCount") + 1 : PlayerPrefs.GetInt("killsCount"));

        // play a ding sound
        PlayHitSoundEffect();

        // get the id of the dead player to send personalized rpcs
        ulong deadPlayerId = hitPlayer.OwnerClientId;

        // add it to the killed player dict to prevent single multi kills (explained in the variable scope)
        killedPlayerIds.Add(deadPlayerId);

        DisplayKillTextUI(hitPlayer.gameObject.name);

        // send message to server to check if the round is over
        if (IsServer)
            GameManager.instance.EndRound(PlayerInfo.instance.teamColor.Value.ToString());
        else
            EndRoundServerRpc(PlayerInfo.instance.teamColor.Value.ToString());

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

    }
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
    private void EndRoundServerRpc(string wonTeam) => GameManager.instance.EndRound(wonTeam);

    [ServerRpc]
    private void SpawnRagdollServerRpc(Vector3 deathPos, Vector3 deathRot, string teamColor) => SpawnRagdollClientRpc(deathPos, deathRot, teamColor);

    [ClientRpc]
    private void SpawnRagdollClientRpc(Vector3 deathPos, Vector3 deathRot, string teamColor)
    {
        // cause we already spawned it client-sided
        if (IsOwner) return;

        SpawnRagdoll(deathPos, deathRot, teamColor);
    }

    private void HandleRecoil()
    {
        if (isHeavyWeapon)
            recoilScript.HeavyRecoilFire();
        else if (isAutomatic)
            recoilScript.AutomaticRecoilFire();
        else if (isLightPistol)
            recoilScript.LightPistolRecoilFire();
        else if (isHeavyPistol)
            recoilScript.HeavyPistolRecoilFire();
    }

    private void DisplayKillTextUI(string hitUsername)
    {
        killMessageObj.SetActive(false);
        killMessageObj.SetActive(true);
        killMessageText.text = hitUsername;

        Invoke(nameof(UndisplayKillTextUI), 2.5f);
    }

    private void UndisplayKillTextUI() => killMessageObj.SetActive(false);

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
        Debug.Log("you died");
    }

    private void PlayEffects()
    {
        if (Physics.Raycast(RayDirection, out RaycastHit hit, Mathf.Infinity, everythingButPlayerLayerMask))
        {
            if (IsServer)
                PlayGunEffectClientRpc(hit.point, hit.normal);
            else
                PlayGunEffectServerRpc(hit.point, hit.normal);
            return;
        }
    }

    private void PlayGunEffect(Vector3 point, Vector3 normal)
    {
        // for other clients to initialize cause im dumb as fuck
        if (impactEffect == null)
            impactEffect = GameObject.Find("BulletImpactEffect").GetComponent<ParticleSystem>();

        // impact effect
        impactEffect.transform.position = point;
        impactEffect.transform.forward = normal;
        impactEffect.Play();
    }

    [ServerRpc] 
    private void PlayGunEffectServerRpc(Vector3 point, Vector3 normal) => PlayGunEffectClientRpc(point, normal);

    [ClientRpc]
    private void PlayGunEffectClientRpc(Vector3 point, Vector3 normal) => PlayGunEffect(point, normal);

    private float DetermineDamage(RaycastHit hit)
    {
        switch(hit.collider.name)
        {
            case "HeadHitbox":
                localSource.PlayOneShot(headHitmarkerSoundEffect);
                crosshairManager.PlayHeadHitmarkerAnimation();
                isHeadshot = true;
                return headshotDamage;
            case "BodyHitbox":
                localSource.PlayOneShot(bodyHitMarkerSoundEffect);
                crosshairManager.PlayBodyHitmarkerAnimation();
                isHeadshot = false;
                return bodyshotDamage;
            case "LegHitbox":
                localSource.PlayOneShot(bodyHitMarkerSoundEffect);
                crosshairManager.PlayBodyHitmarkerAnimation();
                isHeadshot = false;
                return legshotDamage;
            default:
                return 0;
        }
    }

    private void PlayHitSoundEffect() => localSource.PlayOneShot(killSoundEffect);

    [ServerRpc]
    private void DamagePlayerServerRpc(NetworkObjectReference player, float damage) => DamagePlayerClientRpc(player, damage);

    [ClientRpc]
    private void DamagePlayerClientRpc(NetworkObjectReference player, float damage)
    {
        if (player.TryGet(out NetworkObject playerObj))
            playerObj.GetComponent<Health>().DecreaseHealth(damage);
    }

    private void SpawnBullet()
    {
        // spawn locally for that juicy zero lag
        GameObject bullet = Instantiate(bulletPrefab, guntip.position, guntip.rotation);

        // add force to local bullet
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpeed * CalculateBulletDirection(), ForceMode.Impulse);

        // sync bullet
        if (IsServer)
            SyncBulletClientRpc(CalculateBulletDirection(), NetworkManager.LocalClientId);
        else
            SpawnBulletServerRpc(CalculateBulletDirection());
    }


    [ServerRpc]
    private void SpawnBulletServerRpc(Vector3 dir, ServerRpcParams serverRpcParams = default) => SyncBulletClientRpc(dir, serverRpcParams.Receive.SenderClientId);

    [ClientRpc]
    private void SyncBulletClientRpc(Vector3 dir, ulong shooterId)
    {
        // cause we already spawned a bullet locally, do not spawn another one
        if (NetworkManager.LocalClientId.Equals(shooterId)) return;

        GameObject bullet = Instantiate(bulletPrefab, guntip.position, guntip.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpeed * dir, ForceMode.Impulse);
    }

    private Vector3 CalculateBulletDirection()
    {
        if (Physics.Raycast(RayDirection, out RaycastHit hit, Mathf.Infinity, everythingButPlayerLayerMask) && !isWallBangable)
            return (hit.point - guntip.position).normalized;
        else
            return RayDirection.direction;
    }

    private void ShotCooldown()
    {
        // doesnt allow the user to spam fire (obviously), so we use the invoke method to add a delay between every shot
        isShooting = true;
        isReadyToShoot = false;
 
        Invoke(nameof(ResetCooldown), shotCooldown);
    }

    private void ResetCooldown()
    {
        isShooting = false;
        isReadyToShoot = true;
    }

    private void PlayShotSoundEffect(bool isShooting)
    {
        // either play shooting or reloading sound effect
        if (isShooting)
        {
            localSource.PlayOneShot(gunSoundEffects[0]);

            if (IsServer)
                PlaySoundEffectClientRpc(0);
            else
                PlaySoundEffectServerRpc(0);
        }
        else
            localSource.PlayOneShot(gunSoundEffects[1]);
    }

    private void ReloadGun()
    {
        isReloading = true;

        // substract by 0.2 because invoke takes a bit to load
        Invoke(nameof(ReloadBullets), reloadTime - 0.2f);
    }

    private void ReloadBullets()
    {
        // if the player at any moment switches to a different weapon then dont reload 
        if (!isReloading) return;

        currentBulletCount = maxBulletCount;
        isReloading = false;
    }

    private void StopReloadAnimation() => animator.SetBool(reloadHash, false);

    [ServerRpc]
    private void PlaySoundEffectServerRpc(int index) => PlaySoundEffectClientRpc(index);

    [ClientRpc]
    private void PlaySoundEffectClientRpc(int index) => globalSource.PlayOneShot(gunSoundEffects[index]);

    [ServerRpc]
    private void ValidateKillServerRpc(NetworkObjectReference hitPlayer, ServerRpcParams serverRpcParams = default)
    {
        // checks if the player is already dead server side
        // essentially this function only checks if the player's health is below 0 and nothing more as of 5/12/2024
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

    private void AllowShootingAfterPulledOut() => isPullingOut = false;
}
