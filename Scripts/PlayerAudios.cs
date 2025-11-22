using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerAudios : NetworkBehaviour
{
    [Header("Audios")]
    [SerializeField] private AudioSource localSource;
    [SerializeField] private AudioSource globalSource; // for everyone else to hear
    [SerializeField] private AudioClip[] footstepAudio;
    [SerializeField] private AudioClip slidingSound;
    [SerializeField] private AudioClip landingSound;

    [Header("Settings")]
    [SerializeField] private float walkingStepSoundCooldown = 0.5f;
    [SerializeField] private float runningStepSoundCooldown = 0.3f;
    [SerializeField] private float slidingStepSoundCooldown = 0.6f;
    [SerializeField] private bool syncAudio = true;

    [Header("References")]
    [SerializeField] private MovementScript movement;

    private float timer = 0f;
    private bool isSliding;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
            globalSource.mute = true;
    }
    private void Update()
    {
        if (!IsOwner) return;

        timer -= Time.deltaTime;

        // allow slide audio to play instantly
        if (movement.sliding && !isSliding)
            timer = 0f;

        if (timer <= 0)
        {
            // walking audio
            if (movement.walking)
            {
                PlayFootstepSound(localSource, footstepAudio, walkingStepSoundCooldown);

                // sync audio
                if (IsServer && syncAudio)
                    SyncFootstepSoundClientRpc();
                else
                    SyncFootstepSoundServerRpc();
            }

            // running audio
            if (movement.running)
            {
                PlayFootstepSound(localSource, footstepAudio, runningStepSoundCooldown);

                // sync audio
                if (IsServer && syncAudio)
                    SyncFootstepSoundClientRpc();
                else
                    SyncFootstepSoundServerRpc();
            }

            // sliding audio
            if (movement.sliding)
            {
                isSliding = true;

                PlaySlidingSound(localSource, slidingSound, slidingStepSoundCooldown);

                if (IsServer && syncAudio)
                    SyncSlidingAudioClientRpc();
                else
                    SyncSlidingAudioServerRpc();
            }
        }
    }

    public void PlayFootstepSound(AudioSource source, AudioClip[] sound, float cooldown)
    {
        // choose random audio and set the volume randomly from 0.75 to 1.25
        source.PlayOneShot(sound[Random.Range(0, 1)], Random.Range(0.75f, 1.25f));

        timer = cooldown;
    }

    public void PlaySlidingSound(AudioSource source, AudioClip sound, float cooldown)
    {
        source.PlayOneShot(sound, Random.Range(0.75f, 1f));

        timer = cooldown;

        // check if the user stops sliding mid animation
        StartCoroutine(CheckIfSliding());
    }

    public void PlayLandingSound()
    {
        // only play this locally unless if you want to change that in the future then why not
        localSource.PlayOneShot(landingSound);
    }

    private IEnumerator CheckIfSliding()
    {
        float timer = slidingStepSoundCooldown;

        while (timer > 0)
        {
            if (!movement.sliding)
            {
                localSource.Stop();

                isSliding = false;
                yield break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        // reset
        isSliding = false;
    }

    [ServerRpc]
    private void SyncFootstepSoundServerRpc()
    {
        if (movement.walking)
            PlayFootstepSound(globalSource, footstepAudio, walkingStepSoundCooldown);
        else
            PlayFootstepSound(globalSource, footstepAudio, runningStepSoundCooldown);
    }

    [ClientRpc]
    private void SyncFootstepSoundClientRpc()
    {
        if (movement.walking)
            PlayFootstepSound(globalSource, footstepAudio, walkingStepSoundCooldown);
        else
            PlayFootstepSound(globalSource, footstepAudio, runningStepSoundCooldown);
    }

    [ServerRpc]
    private void SyncSlidingAudioServerRpc() => PlaySlidingSound(globalSource, slidingSound, slidingStepSoundCooldown);

    [ClientRpc]
    private void SyncSlidingAudioClientRpc() => PlaySlidingSound(globalSource, slidingSound, slidingStepSoundCooldown);
}