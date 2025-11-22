using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class HealthBarManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBar;

    [Header("Settings")]
    [SerializeField] private float smoothAmount;
    [SerializeField] private Health healthScript;

    private void Update()
    {
        if (!IsOwner) return;

        healthBar.fillAmount = Mathf.MoveTowards(healthBar.fillAmount, healthScript.currentHealth / healthScript.maxHealth, smoothAmount * Time.deltaTime);
    }
}
