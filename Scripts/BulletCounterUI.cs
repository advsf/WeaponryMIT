using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class BulletCounterUI : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Image bulletBar;
    [SerializeField] private float smoothAmount; 
    [SerializeField] private HandleActiveGun changingGunScript;

    private Shoot ShootScript { get => changingGunScript.currentWeapon.GetComponent<Shoot>(); }

    private void Update()
    {
        if (!IsOwner) return;

        if (ShootScript != null)
            bulletBar.fillAmount = Mathf.MoveTowards(bulletBar.fillAmount, (float) ShootScript.currentBulletCount / ShootScript.maxBulletCount, Time.deltaTime * smoothAmount);
    }
}
