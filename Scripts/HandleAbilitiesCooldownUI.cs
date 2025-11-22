using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HandleAbilitiesCooldownUI : MonoBehaviour
{
    public static HandleAbilitiesCooldownUI instance;

    [Header("References")]
    [SerializeField] private Image dashCooldownUI;
    [SerializeField] private Image slidingCooldownUI;
    [SerializeField] private MovementAbilities movementAbilityScript;
    [SerializeField] private Sliding slidingScript;

    private void Start()
    {
        instance = this;

        dashCooldownUI.fillAmount = 1;
        slidingCooldownUI.fillAmount = 1;
    }

    public IEnumerator HandleDashCooldownUI()
    {
        dashCooldownUI.fillAmount = 0;

        float time = Time.time;

        while (Time.time - time <= movementAbilityScript.dashAbilityCooldown)
        {
            dashCooldownUI.fillAmount = (Time.time - time) / movementAbilityScript.dashAbilityCooldown;
            yield return null;
        }
    }

    public IEnumerator HandleSlidingCooldownUI()
    {
        slidingCooldownUI.fillAmount = 0;

        float time = Time.time;

        while (Time.time - time <= slidingScript.slideCooldown)
        {
            slidingCooldownUI.fillAmount = (Time.time - time) / slidingScript.slideCooldown;
            yield return null;
        }
    }
}
