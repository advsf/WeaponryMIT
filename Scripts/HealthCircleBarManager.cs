using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class HealthCircleBarManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Settings")]
    [SerializeField] private float smoothAmount;

    [SerializeField] private Health healthScript;

    private void Update()
    {
        if (!IsOwner) return;

        AdjustCircleBar();
        SetColors();
    }

    private void AdjustCircleBar()
    {
        healthBar.fillAmount = Mathf.MoveTowards(healthBar.fillAmount, healthScript.currentHealth / healthScript.maxHealth, smoothAmount * Time.deltaTime);
        healthText.text = Mathf.Clamp(healthScript.currentHealth, 0f, healthScript.maxHealth).ToString();
    }

    private void SetColors()
    {
        if (healthScript.currentHealth >= 70)
            healthBar.color = Color.green;
        else if (healthScript.currentHealth >= 35)
            healthBar.color = Color.yellow;
        else
            healthBar.color = Color.red;
    }
}
