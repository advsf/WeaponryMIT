using UnityEngine;

public class PlayAnimationAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioClip soundEffect;
    [SerializeField] private AudioSource localSource;

    // all functions are called through the animation
    public void PlaySoundEffect() => localSource.PlayOneShot(soundEffect);
}
