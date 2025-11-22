using UnityEngine;
using Unity.Netcode;

public class SelectWeaponInPractice : MonoBehaviour
{
    [SerializeField] private HandleActiveGun handleGunScript;

    public void SelectWeapon() => NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<HandleActiveGun>().EquipSpecifiedWeapon(gameObject.name);
}
