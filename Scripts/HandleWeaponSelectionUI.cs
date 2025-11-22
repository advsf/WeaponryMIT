using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class HandleWeaponSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private HandleActiveGun gunScript;

    [Header("UI References")]
    [SerializeField] private GameObject weaponSelectionObj;
    [SerializeField] private TextMeshProUGUI weaponSelectionText;
    [SerializeField] private GameObject[] gunSilhouettes;

    [Header("Setting")]
    [SerializeField] private float selectionDuration;
    [SerializeField] private float endingDuration;

    [Header("Audio References")]
    [SerializeField] private AudioClip selectedSoundEffect;
    [SerializeField] private AudioSource localAudioSource;

    // animator hashes
    private int choosingHash;
    private int stoppingHash;

    private int weaponIndex;

    private void Start()
    {
        weaponSelectionObj.SetActive(false);

        choosingHash = Animator.StringToHash("isChoosing");
        stoppingHash = Animator.StringToHash("isExiting");
    }

    public void StartAnimation(int weaponIndex)
    {
        // the rolling audio is played through the animator component

        // this function is called from gamemanager
        this.weaponIndex = weaponIndex;

        // disable every silhouette
        foreach (GameObject gun in gunSilhouettes)
            gun.SetActive(false);

        // enable the obj
        weaponSelectionObj.SetActive(true);

        weaponSelectionText.text = "NEXT WEAPON IS...";

        // play animation
        animator.SetBool(choosingHash, true);

        Invoke(nameof(DisplayChosenWeapon), endingDuration);
    }

    public void DisplayChosenWeapon()
    {
        // play selected sound effect
        localAudioSource.PlayOneShot(selectedSoundEffect);

        // stop animation
        animator.SetBool(choosingHash, false);

        // display selected weapon
        foreach (GameObject gun in gunSilhouettes)
        {
            gun.SetActive(false);

            // compare the name to the selected weapon's name
            if (gun.name == gunScript.gunArray[weaponIndex].name)
            {
                gun.SetActive(true);
                weaponSelectionText.text = gun.name.ToUpper();
            }
        }

        // enable weapon
        gunScript.ChoseWhichWeaponToSelect(weaponIndex);

        Invoke(nameof(StopAnimation), endingDuration);
    }

    public void StopAnimation()
    {
        animator.SetBool(stoppingHash, true);

        Invoke(nameof(DisableSelectionUI), endingDuration + 1f);
    }

    public void DisableSelectionUI() => weaponSelectionObj.SetActive(false);
}
