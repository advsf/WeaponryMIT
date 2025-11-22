using UnityEngine;

public class StopEverythingWithUIOpened : MonoBehaviour
{
    public static bool IsUIOpened;

    private void Update()
    {
        // if all it exists
        if (GlobalSettingManager.instance != null && ChatboxManager.instance != null)
            IsUIOpened = GlobalSettingManager.instance.isOn || ChatboxManager.instance.IsOn;

        // if its only global setting menu
        else if (GlobalSettingManager.instance != null && ChatboxManager.instance == null)
            IsUIOpened = GlobalSettingManager.instance.isOn;

        // if its only chatbox manager
        else if (GlobalSettingManager.instance == null && ChatboxManager.instance != null)
            IsUIOpened = ChatboxManager.instance.IsOn;

        // if its both global settings
        else if (GlobalSettingManager.instance != null)
            IsUIOpened = GlobalSettingManager.instance.isOn;

        else
            IsUIOpened = false;
    }
}
