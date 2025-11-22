using UnityEngine;
using UnityEngine.InputSystem;

public class RebindMenuManager : MonoBehaviour
{
    [SerializeField] private InputActionReference[] actions;

    private void OnEnable()
    {
        // we disable the keybinds to avoid runtime errors
        foreach (InputActionReference action in actions)
            action.action.Disable();
    }

    private void OnDisable()
    {
        foreach (InputActionReference action in actions)
            action.action.Enable();
    }
}
