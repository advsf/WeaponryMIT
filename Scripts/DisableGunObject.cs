using Unity.Netcode;
using UnityEngine;

public class DisableGunObject : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject gunObj;
    [SerializeField] private Shoot shoot;
    
    // keep in mind this script works side-by-side with the CheckIfWeaponShouldBeOn script
    private void Update() => gunObj.SetActive(shoot.enabled);    
}
