using UnityEngine;
using Unity.Netcode;

public class AnimationController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private MovementScript movement;
    [SerializeField] private KeyCode danceKey;

    public bool isDancing = false;

    private int walkingHash;
    private int runningHash;
    private int slidingHash;
    private int dancing1Hash;
    private int fallingHash;

    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        if (!IsOwner) return;

        // performance bonus
        walkingHash = Animator.StringToHash("isWalking");
        runningHash = Animator.StringToHash("isRunning");
        slidingHash = Animator.StringToHash("isSliding");
        dancing1Hash = Animator.StringToHash("isDancing1");
        fallingHash = Animator.StringToHash("isFalling");
    }

    private void Update()
    {
        if (!IsOwner) return;

        // conditions
        bool isMoving = movement.walking || movement.running;
        bool isCrouching = movement.sliding;
        bool isRunning = movement.running;

        // falling animation
        if (!movement.isGrounded)
            animator.SetBool(fallingHash, true);
        else
            animator.SetBool(fallingHash, false);

        // walking animation
        if (isMoving && !isRunning && !isCrouching && movement.isGrounded)
            animator.SetBool(walkingHash, true);
        else
            animator.SetBool(walkingHash, false);
        
        // running animation
        if (isMoving && isRunning && !isCrouching && movement.isGrounded)
            animator.SetBool(runningHash, true);
        else
            animator.SetBool(runningHash, false);

        // sliding animation
        if (movement.sliding)
            animator.SetBool(slidingHash, true);
        else
            animator.SetBool(slidingHash, false);

        // dancing 1 animation
        if (Input.GetKey(danceKey))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                DisableAllAnimations();
                animator.SetBool(dancing1Hash, true);

                isDancing = true;
            }
        }

        // stop dancing
        if (isMoving || isCrouching || isRunning || Input.GetKey(KeyCode.Space)) 
        {
            animator.SetBool(dancing1Hash, false);

            isDancing = false;
        }
    }

    private void DisableAllAnimations()
    {
        // disable all but the dancing animations
        animator.SetBool(walkingHash, false);
        animator.SetBool(runningHash, false);
        animator.SetBool(slidingHash, false);
    }
}
