using UnityEngine;
using Unity.Netcode;

public class GlobalSettingManager : NetworkBehaviour
{
    [Header("Keybinds")]
    [SerializeField] private KeyCode enablerKey = KeyCode.Tab;

    [Header("UI References")]
    [SerializeField] private GameObject GlobalSettingsMenu;
    [SerializeField] private GameObject background;
    
    [HideInInspector] public bool isOn;

    public static GlobalSettingManager instance;

    private void Start()
    {
        instance = this;

        Time.timeScale = 1;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GlobalSettingsMenu.SetActive(false);
        background.SetActive(false);
        SettingsMenuManager.instance.transform.GetChild(0).gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        instance = null;
    }

    private void Update()
    {
        // activate or deactivate the settings menu
        if (Input.GetKeyDown(enablerKey) && !GlobalSettingsMenu.activeInHierarchy)
            TurnOnGlobalSettingsMenu();
        // if we disable the settings menu
        else if (Input.GetKeyDown(enablerKey) && SettingsMenuManager.instance.transform.GetChild(0).gameObject.activeInHierarchy)
            TurnOffSettingsMenu();
        // if we disable the entire menu
        else if (Input.GetKeyDown(enablerKey) && GlobalSettingsMenu.activeInHierarchy)
            TurnOffGlobalSettingsMenu();

        // check if the either of the settings menu is opened (remember that the singleton is the parent with the actual UI as its child)
        if (GlobalSettingsMenu.activeInHierarchy || SettingsMenuManager.instance.transform.GetChild(0).gameObject.activeInHierarchy)
            isOn = true;
        else
            isOn = false;
    }

    private void TurnOnGlobalSettingsMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GlobalSettingsMenu.SetActive(true);
        background.SetActive(true);
        SettingsMenuManager.instance.LeaveSettingMenu();
    }

    public void TurnOffGlobalSettingsMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GlobalSettingsMenu.SetActive(false);
        background.SetActive(false);
        SettingsMenuManager.instance.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void TurnOnSettingsMenu()
    {
        SettingsMenuManager.instance.EnableSettingMenu();
        background.SetActive(true);
        GlobalSettingsMenu.SetActive(false);
    }

    public void TurnOffSettingsMenu()
    {
        SettingsMenuManager.instance.LeaveSettingMenu();
        GlobalSettingsMenu.SetActive(true);
    }

    public void EnableGlobalSettingsMenuFromSetting() => GlobalSettingsMenu.SetActive(true);

    public void LeaveGame()
    {
        Debug.Log("LEAVING GAME!!!");

        if (IsServer)
            LeaveGameClientRpc();
        else
            LeaveGameServerRpc();
    }

    [ServerRpc]
    private void LeaveGameServerRpc() => LeaveGameClientRpc();

    [ClientRpc]
    private void LeaveGameClientRpc() => StartCoroutine(TransitionManager.instance.PlayTransitionBackToLobby());

    public void QuitGame() => Application.Quit();
}
