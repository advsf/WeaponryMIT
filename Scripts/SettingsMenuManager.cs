using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    public static SettingsMenuManager instance;

    [Header("Sections")]
    [SerializeField] private GameObject mainSettingsMenu;
    [SerializeField] private GameObject gameplayMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject videoMenu;
    [SerializeField] private GameObject audioMenu;

    [Header("Gameplay Menu")]
    [SerializeField] private TMP_InputField sensInputField;
    [SerializeField] private Slider sensSlider;

    [Header("Controls Menu")]

    [Header("Video Menu")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Audio Menu")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider audioSlider;
    
    private Resolution[] resolutions;

    private void Awake()
    {
        mainSettingsMenu.SetActive(false);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        InitializeSettings();
        HandleSettingUpQuality();
        HandleSettingUpResolutions();
    }

    private void InitializeSettings()
    {
        // initialize the text values with the data
        if (PlayerPrefs.GetFloat("sens") == 0)
            PlayerPrefs.SetFloat("sens", 100);

        sensSlider.value = PlayerPrefs.GetFloat("sens");
        sensInputField.text = PlayerPrefs.GetFloat("sens").ToString();
        audioSlider.value = PlayerPrefs.GetFloat("audio");
    }

    private void HandleSettingUpQuality()
    {
        qualityDropdown.value = PlayerPrefs.GetInt("quality");
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("quality"));
        qualityDropdown.RefreshShownValue();
    }

    private void HandleSettingUpResolutions()
    {
        // get all resolutions
        resolutions = Screen.resolutions;

        // if there already is data
        if (PlayerPrefs.HasKey("resolution"))
        {
            // set up the resolution with the data
            Resolution savedResolution = resolutions[PlayerPrefs.GetInt("resolution")];
            Screen.SetResolution(savedResolution.width, savedResolution.height, Screen.fullScreen);

            // create the options
            List<string> options = new();

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "hz";
                options.Add(option);
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = PlayerPrefs.GetInt("resolution");
            resolutionDropdown.RefreshShownValue();
        }

        // if there is no data
        else
        {
            // gathers the user's resolution and displays it to the dropdown
            resolutionDropdown.ClearOptions();
            
            // create the option
            List<string> options = new();

            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "hz";
                options.Add(option);

                // if the resolution matches with the current resolution
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height && resolutions[i].refreshRateRatio.Equals(Screen.currentResolution.refreshRateRatio))
                    currentResolutionIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    // handle changing sections
    public void EnableSettingMenu() => mainSettingsMenu.SetActive(true);

    public void LeaveSettingMenu()
    {
        mainSettingsMenu.SetActive(false);

        // if we're in a game
        if (GlobalSettingManager.instance != null)
            GlobalSettingManager.instance.EnableGlobalSettingsMenuFromSetting();

        // otherwise we're in the main menu
        else
            LobbyUIManager.instance.TurnOnMainMenu();
    }

    public void TurnOnGameplaySetting()
    {
        gameplayMenu.SetActive(true);
        controlsMenu.SetActive(false);
        videoMenu.SetActive(false);
        audioMenu.SetActive(false);
    }

    public void TurnOnControlSettings()
    {
        gameplayMenu.SetActive(false);
        controlsMenu.SetActive(true);
        videoMenu.SetActive(false);
        audioMenu.SetActive(false);

    }

    public void TurnOnVideoSetting()
    {
        gameplayMenu.SetActive(false);
        controlsMenu.SetActive(false);
        videoMenu.SetActive(true);
        audioMenu.SetActive(false);
    }

    public void TurnOnAudioSetting()
    {
        gameplayMenu.SetActive(false);
        controlsMenu.SetActive(false);
        videoMenu.SetActive(false);
        audioMenu.SetActive(true);
    }

    // gameplay setting related 
    
    // changed by the handler
    public void HandleSensitivityChange()
    {
        sensInputField.text = sensSlider.value.ToString();
        PlayerPrefs.SetFloat("sens", sensSlider.value);
    }

    // changed by manually changing the text
    public void HandleSettingSenstivityChangeThroughText(string change)
    {
        if (int.TryParse(change, out int result))
        {
            sensSlider.value = result;
            sensInputField.text = sensSlider.value.ToString();
            PlayerPrefs.SetFloat("sens", sensSlider.value);
        }
    }

    // if the condition is true, set the value to 0, else set it to 1
    public void EnableHub(bool condition) => PlayerPrefs.SetInt("isHubEnabled", condition ? 0 : 1);

    // control settings related

    // video settings related
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("quality", qualityIndex);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        PlayerPrefs.SetInt("resolution", resolutionIndex);
    }

    public void SetFullscreen(bool isEnabled) => Screen.fullScreen = isEnabled;

    // audio setting related
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("audio", volume);
    }
}
