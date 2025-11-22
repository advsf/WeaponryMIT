using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System.Collections;
using UnityEngine.InputSystem;

public class HandleActiveGun : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> currentWeaponName = new("NA", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public GameObject currentWeapon; // used in the leaderboard
    public static HandleActiveGun instance; // should only use singleton in practice mode

    // delete later when u add adjustable keybind settings
    [Header("Input")]
    [SerializeField] private InputActionReference weaponEquipAction;
    [SerializeField] private InputActionReference meleeEquipAction;

    [Header("Gun Array")]
    public GameObject[] gunArray;

    [Header("WeaponHolder References")]
    [SerializeField] private GameObject gunWeaponHolder;
    [SerializeField] private GameObject meleeHolder;

    [Header("Weapon References")]
    [SerializeField] private GameObject meleeObj;
    public GameObject currentGun = null;

    [Header("Settings")]
    public bool isBulletInfinite = false;

    [Header("Script References")]
    [SerializeField] private Health healthScript;
    [SerializeField] private AnimationController animationScript;

    // public variables for other useful stuff
    [Header("Bool Checks")]
    public bool isGunActive;
    public bool isKnifeActive;

    private void Start()
    {
        if (!IsOwner) return;

        instance = this;

        // initialize keybinds later

        gunWeaponHolder.SetActive(false);
        meleeHolder.SetActive(true);

        // get the knife and enable it (right now there's only 1 melee)
        meleeObj.SetActive(false);
        currentWeapon = meleeObj;

        isKnifeActive = true;
        isGunActive = false;
    }

    private void OnEnable()
    {
        weaponEquipAction.action.Enable();
        meleeEquipAction.action.Enable();
    }

    private void OnDisable()
    {
        weaponEquipAction.action.Disable();
        meleeEquipAction.action.Disable();
    }

    private void Update()
    {
        if (!IsOwner || StopEverythingWithUIOpened.IsUIOpened) return;

        if (currentGun != null)
            currentGun.GetComponent<Shoot>().enabled = true;

        // determine if the weapon should be active or not
        if (healthScript.currentHealth <= 0.0f || animationScript.isDancing)
            currentWeapon.SetActive(false);

        // if the weapon exists, set the currentweapon to true and set the networkvariable to the name of the weapon
        else if (currentWeapon != null)
        {
            currentWeapon.SetActive(true);
            currentWeaponName.Value = currentWeapon.name;
        }

        // handle custom weapon settings
        HandleActiveGunCustomSettings();

        // handling switching between guns and melee
        EquipWeapons();

        // sync which weapons should be synced to other clients
        SyncWeaponActiveness();

        // if the player dies, remove the weapon
        if (healthScript.currentHealth <= 0.0f)
            HandleDeath();
    }

    private void HandleActiveGunCustomSettings()
    {
        if (currentGun == null) return;

        if (isBulletInfinite)
            currentGun.GetComponent<Shoot>().isBulletInfinite = true;
        else
            currentGun.GetComponent<Shoot>().isBulletInfinite = false;
    }

    private void HandleDeath()
    {
        // remove the gun
        currentGun = null;

        // set active weapon to the melee
        gunWeaponHolder.SetActive(false);
        meleeHolder.SetActive(true);

        // set the currentweapon to melee
        currentWeapon = meleeHolder.transform.GetChild(0).gameObject;

        isKnifeActive = true;
        isGunActive = false;
    }

    private void EquipWeapons()
    {
        // equipping a primary weapon
        if (weaponEquipAction.action.triggered && currentGun != null)
            EquipGun();
        
        // equipping a melee
        if (meleeEquipAction.action.triggered || currentGun == null)
            EquipMelee();
    }

    private void EquipGun()
    {
        gunWeaponHolder.SetActive(true);
        meleeHolder.SetActive(false);

        currentWeapon = currentGun;

        isKnifeActive = false;
        isGunActive = true;
    }

    public void EquipMelee()
    {
        gunWeaponHolder.SetActive(false);
        meleeHolder.SetActive(true);

        currentWeapon = meleeObj;

        isKnifeActive = true;
        isGunActive = false;
    }

    public void DisableEveryGun()
    {
        foreach (GameObject gun in gunArray)
            gun.SetActive(false);
    }

    public void ChoseWhichWeaponToSelect(int index)
    {
        currentGun = null;

        // equip melee LES GO
        EquipMelee();

        foreach (GameObject gun in gunArray)
        {
            // disable gun
            gun.SetActive(false); 

            // if the gun is the one that we want to select
            if (gun == gunArray[index])
            {
                currentGun = gun;
                gun.SetActive(true);

                // reset bullet
                gun.GetComponent<Shoot>().currentBulletCount = gun.GetComponent<Shoot>().maxBulletCount;
            }
        }
    }

    // for practice mode only
    public void EquipSpecifiedWeapon(string desiredWeaponName)
    {
        currentGun = null;

        // equip melee LES GO
        EquipMelee();

        foreach (GameObject gun in gunArray)
        {
            gun.SetActive(false);

            // if the gun is the one that we want to select
            if (gun.name == desiredWeaponName)
            {
                currentGun = gun;

                // reset bullet
                gun.GetComponent<Shoot>().currentBulletCount = gun.GetComponent<Shoot>().maxBulletCount;
            }
        }
    }

    // during intermission
    public IEnumerator PreventShootingAndMeeleing()
    {
        float time = Time.time;

        while (Time.time - time <= GameManager.instance.intermissionTime)
        {
            if (currentGun != null)
                StopShooting();

            yield return null;
        }

        AllowShooting();
    }
    
    // for other UIs when activated
    public void StopShooting()
    {
        if (currentGun != null)
            currentGun.GetComponent<Shoot>().isReadyToShoot = false;
        
        meleeObj.GetComponent<Melee>().isAbleToMelee = false;
    }

    public void AllowShooting()
    {
        if (currentGun != null)
            currentGun.GetComponent<Shoot>().isReadyToShoot = true;
        
        meleeObj.GetComponent<Melee>().isAbleToMelee = true;
    }

    private void SyncWeaponActiveness()
    {
        // if primary weapon is active
        if (gunWeaponHolder.activeInHierarchy)
            if (IsHost)
                SyncWeaponActivenessClientRpc(true);
            else
                SyncWeaponActivenessServerRpc(true);

        // if melee is active
        else 
            if (IsHost)
                SyncWeaponActivenessClientRpc(false);
            else
                SyncWeaponActivenessServerRpc(false);
    }

    [ServerRpc]
    private void SyncWeaponActivenessServerRpc(bool isGunActive) => SyncWeaponActivenessClientRpc(isGunActive);

    [ClientRpc]
    private void SyncWeaponActivenessClientRpc(bool isGunActive)
    {
        if (IsOwner) return;

        // if dead or dancing sync the weapon not being active
        if (healthScript.currentHealth <= 0.0f || animationScript.isDancing)
        {
            gunWeaponHolder.SetActive(false);
            meleeHolder.SetActive(false);
            return;
        }

        // primary weapon
        if (isGunActive)
        {
            gunWeaponHolder.SetActive(true);
            meleeHolder.SetActive(false);

            foreach (GameObject weapon in gunArray)
            {
                if (weapon.name == currentWeaponName.Value)
                    weapon.SetActive(true);
                else
                    weapon.SetActive(false);
            }
        }

        // melee
        else
        {
            gunWeaponHolder.SetActive(false);
            meleeHolder.SetActive(true);
        }
    }
}
