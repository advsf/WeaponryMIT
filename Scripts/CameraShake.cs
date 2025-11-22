using UnityEngine;
using Unity.Netcode;

public class CameraShake : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private int isShakingHash;

    private void Awake() => isShakingHash = Animator.StringToHash("isShaking");

    public void ShakeOnce()
    {
        if (!IsOwner) return;

        animator.SetBool(isShakingHash, true);

        Invoke(nameof(StopCameraShake), 0.1f);
    }   
    
    private void StopCameraShake() =>  animator.SetBool(isShakingHash, false);
}
