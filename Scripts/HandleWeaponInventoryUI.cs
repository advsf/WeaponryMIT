using UnityEngine;
using Unity.Netcode;

public class HandleWeaponInventoryUI : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject[] silhouettes;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private int equippedHash;

    [Header("Reference")]
    [SerializeField] private HandleActiveGun changingGunScript;

    [Header("Setting")]
    [SerializeField] private bool isMelee;

    private GameObject previousGun;
    private string weaponName;

    private void Start()
    {
        if (!IsOwner) return;

        equippedHash = Animator.StringToHash("isEquipped");

        foreach (GameObject silhouette in silhouettes)
            silhouette.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (changingGunScript.currentGun != null)
            weaponName = changingGunScript.currentGun.name;

        // if weapon changes or is empty
        if (previousGun != changingGunScript.currentGun || changingGunScript.currentGun == null)
            foreach (GameObject silhouette in silhouettes)
                silhouette.SetActive(false);

        // melee weapon
        if (isMelee)
        {
            // right now there is only one melee so we can just set the silhouette to always true
            silhouettes[0].SetActive(true);

            if (changingGunScript.isKnifeActive)
                animator.SetBool(equippedHash, true);
            else
                animator.SetBool(equippedHash, false);
        }

        else
        {

            // play animation
            if (changingGunScript.isGunActive)
                animator.SetBool(equippedHash, true);
            else
                animator.SetBool(equippedHash, false);

            // determine which weapon silhouette should be active
            foreach (GameObject silhoutte in silhouettes)
                silhoutte.SetActive(weaponName == silhoutte.name);
        }

        previousGun = changingGunScript.currentGun;
    }
}
