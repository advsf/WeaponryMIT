using UnityEngine;
using Unity.Netcode;

// this script is assigned to the firstCameraPos because why not LOL
public class CheckLanding : NetworkBehaviour
{
    [SerializeField] private MovementScript movement;
    [SerializeField] private PlayerAudios playerAudio;
    [SerializeField] private Animator animator;

    private int isLandedHash;
    private bool wasOnAir = false;

    private void Start()
    {
        isLandedHash = Animator.StringToHash("isLanded");
    }

    private void Update()
    {
        /// <summary>
        /// check if the player was on air and set wasOnAir to true
        /// and once the player is grounded again set wasOnAir to false and play the cam animation
        /// </summary>
        /// 

        if (!IsOwner) return;

        if (!movement.isGrounded)
            wasOnAir = true;

        if (movement.isGrounded && wasOnAir)
        {
            wasOnAir = false;

            animator.SetBool(isLandedHash, true);

            // play audio
            playerAudio.PlayLandingSound();

            Invoke(nameof(StopAnimation), 0.3f);
        }
    }

    private void StopAnimation() =>
        animator.SetBool(isLandedHash, false);
}
