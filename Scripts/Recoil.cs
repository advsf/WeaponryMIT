using Unity.Netcode;
using UnityEngine;

public class Recoil : NetworkBehaviour
{
    [Header("Heavy Gun Recoil")]
    [SerializeField] private float heavyRecoilX;
    [SerializeField] private float heavyRecoilY;
    [SerializeField] private float heavyRecoilZ;

    [Header("Automatic Recoil")]
    [SerializeField] private float autoRecoilX;
    [SerializeField] private float autoRecoilY;
    [SerializeField] private float autoRecoilZ;

    [Header("Light Pistol Recoil")]
    [SerializeField] private float lightPistolRecoilX;
    [SerializeField] private float lightPistolRecoilY;
    [SerializeField] private float lightPistolRecoilZ;

    [Header("Light Pistol Recoil")]
    [SerializeField] private float heavyPistolRecoilX;
    [SerializeField] private float heavyPistolRecoilY;
    [SerializeField] private float heavyPistolRecoilZ;

    [Header("Settings")]
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    // rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void HeavyRecoilFire() => targetRotation += new Vector3(heavyRecoilX, Random.Range(-heavyRecoilY, heavyRecoilY), Random.Range(-heavyRecoilZ, heavyRecoilZ));

    public void AutomaticRecoilFire() => targetRotation += new Vector3(autoRecoilX, Random.Range(-autoRecoilY, autoRecoilY), Random.Range(-autoRecoilZ, autoRecoilZ));

    public void LightPistolRecoilFire() => targetRotation += new Vector3(lightPistolRecoilX, Random.Range(lightPistolRecoilY - .75f, lightPistolRecoilY), Random.Range(-lightPistolRecoilZ, lightPistolRecoilZ));

    public void HeavyPistolRecoilFire() => targetRotation += new Vector3(heavyPistolRecoilX, Random.Range(heavyPistolRecoilY - .75f, heavyPistolRecoilY), Random.Range(-heavyPistolRecoilZ, heavyPistolRecoilZ));
}
