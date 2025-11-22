using QFSW.QC;
using UnityEngine;
using Unity.Netcode;

// delete later and implement with the settings GUI
public class Commands : NetworkBehaviour
{
    public static Commands instance;

    [SerializeField] private Transform[] platforms;
    private void Awake()
    {
        instance = this;
    }

    public void ResetPlatform()
    {
        if (!IsServer) return;

        foreach (Transform platform in platforms)
            platform.GetComponent<PlatformRandomizer>().CommenceReset();
    }

    [Command]
    public void MuteAudio() =>
        AudioListener.pause = true;

    [Command]
    public void UnmuteAudio() =>
        AudioListener.pause = false;
}
