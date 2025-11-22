using UnityEngine;
using Unity.Netcode;

public class HandleCanvasCustomSettings : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cameraOverlayDisableHUDs; // huds that can be switched off
    [SerializeField] private GameObject spaceOverlayDisableHUDS;
    [SerializeField] private GameObject[] huds; // this is only used for huds that cannot be parented either of the disable HUDS due to anchor positions

    private void Update()
    {
        // if hud should be enabled
        if (PlayerPrefs.GetInt("isHubEnabled") == 0)
        {
            cameraOverlayDisableHUDs.SetActive(true);
            spaceOverlayDisableHUDS.SetActive(true);

            foreach (GameObject hud in huds)
                hud.SetActive(true);
        }

        // if hud should be disabled
        else
        {
            cameraOverlayDisableHUDs.SetActive(false);
            spaceOverlayDisableHUDS.SetActive(false);

            foreach (GameObject hud in huds)
                hud.SetActive(false);
        }
    }
}
