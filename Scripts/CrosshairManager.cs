using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CrosshairManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image normalCrosshair;
    [SerializeField] private Image reloadingCrosshair;
    [SerializeField] private Image bodyHitMarker;
    [SerializeField] private Image headHitMarker;

    [Header("Settings")]
    [SerializeField] private float hitmarkerDisableTime;
    public bool isCrosshairOn = true; // manually turn all crosshairs on or off

    [Header("Script References")]
    [SerializeField] private HandleActiveGun changingGunScript;

    private Shoot shootScript;

    private bool isCrosshairFilling;

    private void Start()
    {
        if (!IsOwner) return;

        normalCrosshair.gameObject.SetActive(true);
        reloadingCrosshair.gameObject.SetActive(false);

        isCrosshairFilling = false;
    }

    private void Update()
    {
        shootScript = changingGunScript.currentWeapon.GetComponent<Shoot>();

        if (!IsOwner || !isCrosshairOn) return;

        // if there is no gun
        if (shootScript == null)
        {
            ResetCrosshairToNormal();
            StopAllCoroutines();
            return;
        }

        // if we're reloading
        if (shootScript.isReloading && !isCrosshairFilling)
        {
            isCrosshairFilling = true;

            normalCrosshair.gameObject.SetActive(false);
            reloadingCrosshair.gameObject.SetActive(true);

            StartCoroutine(ChangeCrosshairFillAmount());
        }
        else if (!shootScript.isReloading)
        {
            normalCrosshair.gameObject.SetActive(true);
            isCrosshairFilling = false;
        }
    }

    public void DisableCrosshair()
    {
        isCrosshairOn = false;
        TurnOffAllCrosshairs();
    }

   private void TurnOffAllCrosshairs()
    {
        normalCrosshair.gameObject.SetActive(false);
        reloadingCrosshair.gameObject.SetActive(false);
        bodyHitMarker.gameObject.SetActive(false);
        headHitMarker.gameObject.SetActive(false);
    }

    private void ResetCrosshairToNormal()
    {
        normalCrosshair.gameObject.SetActive(true);
        reloadingCrosshair.gameObject.SetActive(false);
        reloadingCrosshair.fillAmount = 0;
    }

    private IEnumerator ChangeCrosshairFillAmount()
    {
        float time = Time.time;

        while (shootScript.isReloading)
        {
            if (shootScript == null) break;

            reloadingCrosshair.fillAmount = (Time.time - time) / shootScript.reloadTime;
            yield return null;
        }

        // reset
        ResetCrosshairToNormal();
    }

    public void PlayBodyHitmarkerAnimation()
    {
        bodyHitMarker.gameObject.SetActive(false);
        bodyHitMarker.gameObject.SetActive(true);

        Invoke(nameof(StopBodyhitmarkerAnimation), hitmarkerDisableTime);
    }

    private void StopBodyhitmarkerAnimation() => bodyHitMarker.gameObject.SetActive(false);

    public void PlayHeadHitmarkerAnimation()
    {
        headHitMarker.gameObject.SetActive(false);
        headHitMarker.gameObject.SetActive(true);

        Invoke(nameof(StopHeadhitmarkerAnimation), hitmarkerDisableTime);
    }

    private void StopHeadhitmarkerAnimation() => headHitMarker.gameObject.SetActive(false);
}
