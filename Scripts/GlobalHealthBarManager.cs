using UnityEngine;
using UnityEngine.UI;

public class GlobalHealthBarManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image foreground;
    [SerializeField] private Health healthScript;

    [Header("Settings")]
    [SerializeField] private float smoothAmount = 5.0f;

    private void Update() => foreground.fillAmount = Mathf.MoveTowards(foreground.fillAmount, healthScript.currentHealth / healthScript.maxHealth, smoothAmount * Time.deltaTime);
}
