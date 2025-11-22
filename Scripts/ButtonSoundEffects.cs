using UnityEngine;

public class ButtonSoundEffects : MonoBehaviour
{
    [Header("Sound References")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip onHoverSoundEffect;

    private void Update()
    {
        if (source == null)
            source = GameObject.Find("SettingsAudioSource").GetComponent<AudioSource>();
    }

    public void PlayOnHover() => source.PlayOneShot(onHoverSoundEffect);
}
