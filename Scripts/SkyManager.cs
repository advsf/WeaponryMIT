using UnityEngine;

public class SkyManager : MonoBehaviour
{
    public static SkyManager instance;

    [SerializeField] private float skySpeed;
    [SerializeField] private Material dayMat;
    [SerializeField] private Material nightMat;

    private bool isDay = true;

    private void Start()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Update()
    {
        // rotates the skybox
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skySpeed);
    }

    public void ChangeSky()
    {
        // make it night
        if (isDay)
            RenderSettings.skybox = nightMat;

        // make it day
        else
            RenderSettings.skybox = dayMat;

        isDay = !isDay; 
    }
}
