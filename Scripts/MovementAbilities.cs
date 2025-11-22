using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementAbilities : NetworkBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference dashAction;

    [Header("Dashing Settings")]
    [SerializeField] private float dashForce;
    [SerializeField] private float camFov;
    [SerializeField] private float camFovDuration;
    [SerializeField] private float dashDelayDuration;
    [SerializeField] private float dashAbilityDuration;

    [Header("Cooldowns")]
    public float dashAbilityCooldown;
    public float healAbilityCooldown;

    // enable or disable the abilites outright
    [Header("Settings Switch")]
    [SerializeField] private bool canDashAbility;
    [SerializeField] private bool canHealAbility;

    [Header("Audio References")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip dashSoundEffect;

    [Header("References")]
    [SerializeField] private Health healthScript;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private RotateCam playerCam;
    [SerializeField] private MovementScript movement;

    private bool canDash;
    private bool canHeal;

    private void Start()
    {
        if (!IsOwner) return;

        canDash = true;
        canHeal = true;
    }

    private void Update()
    {
        if (!IsOwner || StopEverythingWithUIOpened.IsUIOpened) return;

        // dashing
        if (dashAction.action.triggered && canDash && !movement.sliding)
            HandleDashing();

        // healing
        //if (Input.GetKeyDown(healKey) && canHeal)
          //  HandleHealing();
    }

    private void OnEnable()
    {
        dashAction.action.Enable();
    }

    private void OnDisable()
    {
        dashAction.action.Disable();
    }

    private void HandleHealing()
    {
        canHeal = false;
        healthScript.SetHealth(healthScript.maxHealth);

        Invoke(nameof(ResetHealCooldown), healAbilityCooldown);
    }

    private void HandleDashing()
    {
        // bools
        canDash = false;
        movement.dashing = true;

        // play dashing sound effect
        source.PlayOneShot(dashSoundEffect);

        // handle the cooldown UI
        StartCoroutine(HandleAbilitiesCooldownUI.instance.HandleDashCooldownUI());

        // camera effect
        playerCam.DoFov(camFov, camFovDuration);

        // add force
        Invoke(nameof(DelayDashForce), dashDelayDuration);

        Invoke(nameof(ResetFOV), camFovDuration);
        Invoke(nameof(StopDashing), dashAbilityDuration);
        Invoke(nameof(ResetDashCooldown), dashAbilityCooldown);
    }

    private Vector3 GetDashDirection()
    {
        // get inputs
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal == 0 && vertical == 0)
            return orientation.forward.normalized;
        else
            return (orientation.forward * vertical + orientation.right * horizontal).normalized;
    }

    private void DelayDashForce() => rb.AddForce(GetDashDirection() * dashForce, ForceMode.Impulse);

    private void StopDashing() => movement.dashing = false;

    private void ResetDashCooldown() => canDash = true;

    private void ResetHealCooldown() => canHeal = true;

    private void ResetFOV() => playerCam.DoFov(60f);
}
