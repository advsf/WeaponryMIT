using UnityEngine;
using Unity.Netcode;

public class PlatformEffect : NetworkBehaviour
{
    [SerializeField] private ParticleSystem glassShatterEffect;
    [SerializeField] private AudioClip glassShatterSound;

    public void MakePlatformBreakable()
    {
        // locally set the meshcollider to trigger
        GetComponent<MeshCollider>().isTrigger = true;

        // set the layer to default (so the player cannot jump on it)
        gameObject.layer = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        // if not server or if the collision's layer doesnt have the movement script, meaning it's not a player
        if (!IsServer ||!other.GetComponent<MovementScript>()) return;

        // play on the host
        PlayGlassShatterEffect();

        // just to sync
        PlayEffectClientRpc();
    }

    [ClientRpc]
    private void PlayEffectClientRpc() => PlayGlassShatterEffect();

    private void PlayGlassShatterEffect()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<MeshCollider>().enabled = false;

        glassShatterEffect.transform.position = transform.position;
        glassShatterEffect.Play();

        // play sound
        GetComponent<AudioSource>().PlayOneShot(glassShatterSound);
    }
}
