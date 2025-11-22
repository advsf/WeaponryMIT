using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Health : NetworkBehaviour
{
    public static Health instance;

    [Header("Settings")]
    public float maxHealth;
    public float currentHealth;

    [Header("Bools")]
    public bool canSpectate;
    [SerializeField] private bool canCamDeathscreenAnimate;

    [Header("Audio References")]
    [SerializeField] private AudioSource localSource;
    [SerializeField] private AudioClip hurtSoundEffect;
    [SerializeField] private AudioClip killedSoundEffect;

    [Header("Spectator References")]
    [SerializeField] private Transform camHolderTransform;
    [SerializeField] private Animator camAnimator;
    [SerializeField] private float camAnimationSmoothness;

    [Header("Deathscreen Animation References")]
    [SerializeField] private SpectatorMode spectatorScript;
    [SerializeField] private float desiredAngle;

    [Header("Other References")]
    [SerializeField] private GameObject playerTag;
    [SerializeField] private HandleActiveGun changeGunScript;
    private Shoot shootScript;

    public bool isSpectating { get => spectatorScript.isSpectating; }

    private float previousCurrentHealth;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        camAnimator.enabled = false;
    }

    private void Start()
    {
        if (IsOwner)
            instance = this;

        currentHealth = maxHealth;
    }

    private void Update()
    {
        // handle the activeness of the player tag across all clients except the player
        if (!IsOwner)
            playerTag.SetActive(currentHealth > 0.0f);

        if (!IsOwner) return;

        // because the player can change their gun
        shootScript = changeGunScript.currentWeapon.GetComponent<Shoot>();

        // play damaged sound effect
        if (previousCurrentHealth != currentHealth && currentHealth > 0.0f && currentHealth < maxHealth)
        {
            localSource.PlayOneShot(hurtSoundEffect);
            previousCurrentHealth = currentHealth;
        }
    }

    public void DecreaseHealth(float amount)
    {
        // the reason why we dont sync with rpcs
        // is because this is called by another rpc in another behavior to every client
        currentHealth -= amount;

        if (currentHealth <= 0.0f)
            HandleDeath();
    }

    public void ResetHealth()
    {
        // this method is called by the gamemanager
        if (!IsOwner) return;

        SetHealth(maxHealth);
    }

    private void Specate()
    {
        spectatorScript.isSpectating = true;
    }

    private void PlayCamDeathscreenAnimation()
    {
        camAnimator.enabled = true;

        StartCoroutine(LerpCameraRotation());
        Invoke(nameof(StopCamDeathscreenAnimation), GameManager.instance.respawnCooldown);
    }

    private IEnumerator LerpCameraRotation()
    {
        float time = Time.time;

        while (Time.time - time <= GameManager.instance.respawnCooldown)
        {
            // because apparently with the movement script disabled the player moves on its own
            GetComponent<Rigidbody>().linearVelocity = new(0, 0, 0);

            // move the camrera to the specified angle
            camHolderTransform.rotation = Quaternion.Lerp(camHolderTransform.rotation, Quaternion.Euler(desiredAngle, camHolderTransform.rotation.y, camHolderTransform.rotation.z), Time.deltaTime * camAnimationSmoothness);
            yield return null;
        }
    }

    private void StopCamDeathscreenAnimation() => camAnimator.enabled = false;

    public void SetHealth(float amount)
    {
        currentHealth = amount;

        if (IsServer)
            SetHealthClientRpc(amount);
        else
            SetHealthServerRpc(amount);
    }

    [ServerRpc]
    private void SetHealthServerRpc(float amount) => SetHealthClientRpc(amount);

    [ClientRpc]
    private void SetHealthClientRpc(float amount)
    {
        if (IsOwner) return;

        currentHealth = amount;
    }

    private void HandleDeath()
    {
        if (!IsOwner) return;

        // disable movement
        GameManager.instance.HandlemovementScripts(false, gameObject);

        // disable hitbox
        DisableHitbox.instance.DetermineHitboxActiveness(false);

        // play sound effect
        localSource.PlayOneShot(killedSoundEffect);

        // increase death amount
        PlayerInfo.instance.deaths.Value++;

        PlayerPrefs.SetInt("deathsCount", GameManager.instance.isRanked ? PlayerPrefs.GetInt("deathsCount") + 1 : PlayerPrefs.GetInt("deathsCount"));

        // play death animation and (if 2v2 3v3) transitiont to spectating mode
        PlayCamDeathscreenAnimation();

        if (canSpectate)
            Invoke(nameof(Specate), GameManager.instance.respawnCooldown);
    }
}
