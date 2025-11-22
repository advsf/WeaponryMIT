using TMPro;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;

public class HandleWeaponKeybinds : NetworkBehaviour
{
    [Header("Keybind Texts")]
    [SerializeField] private TextMeshProUGUI weaponKeyBindText;
    [SerializeField] private TextMeshProUGUI meleeKeyBindText;
    [SerializeField] private InputActionReference weaponSelectAction;
    [SerializeField] private InputActionReference meleeSelectAction;

    private void Update()
    {
        if (!IsOwner) return;

        weaponKeyBindText.text = weaponSelectAction.action.GetBindingDisplayString();
        meleeKeyBindText.text = meleeSelectAction.action.GetBindingDisplayString();
    }
}
