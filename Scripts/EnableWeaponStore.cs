using UnityEngine;
using Unity.Netcode;

public class EnableWeaponStore : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode enablerKey = KeyCode.B;

    [Header("UI References")]
    [SerializeField] private GameObject weaponStoreObj;

    private RotateCam lookScript;
    private HandleActiveGun activeGunScript;

    private void Update()
    {
        if (lookScript == null)
            lookScript = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<RotateCam>();

        if (activeGunScript == null)
            activeGunScript = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<HandleActiveGun>();

        if (Input.GetKeyDown(enablerKey))
        {
            // either disable or enable it
            weaponStoreObj.SetActive(!weaponStoreObj.activeInHierarchy);

            // when the store is open
            if (weaponStoreObj.activeInHierarchy)
            {
                // enable cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                lookScript.enabled = false;
            }

            // when its not open
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                lookScript.enabled = true;
            }
        }
    }
}
