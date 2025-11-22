using UnityEngine;

public class SettingsAudioManager : MonoBehaviour
{
    public static SettingsAudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}
