using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;

// handles every UI related to the round counter and intermission
public class RoundCounterManager : NetworkBehaviour
{
    public static RoundCounterManager instance;

    [Header("Notification UI References")]
    [SerializeField] private GameObject notificationObj;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("Main Round Counter UI References")]
    [SerializeField] private GameObject roundCounterUI;
    [SerializeField] private TextMeshProUGUI blueTeamWonRoundText;
    [SerializeField] private TextMeshProUGUI redTeamWondRoundText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI currentRoundText;

    [Header("Intermission References")]
    public float intermissionTransitionDuration;
    [SerializeField] private GameObject transitionImage;

    private void Start()
    {
        UpdateScoreCounter();
        UpdateCurrentRoundText();

        transitionImage.SetActive(false);

        if (IsOwner)
            instance = this;
    }

    private void OnEnable()
    {
        // subscribe to events
        GameManager.instance.blueTeamWonAmount.OnValueChanged += OnSomeValueChanged;
        GameManager.instance.redTeamWonAmount.OnValueChanged += OnSomeValueChanged;
    }

    private void OnDisable()
    {
        GameManager.instance.blueTeamWonAmount.OnValueChanged -= OnSomeValueChanged;
        GameManager.instance.redTeamWonAmount.OnValueChanged -= OnSomeValueChanged;
    }

    private void OnSomeValueChanged(int previous, int current) => UpdateScoreCounter();

    public void EnableUI() => roundCounterUI.SetActive(true);

    public void DisableUI() => roundCounterUI.SetActive(false);

    public void HandleStartingRound()
    {
        StartCoroutine(StartRoundTimer());
    }

    public void HandleStoppingRound()
    {
        // stop all text updates, allowing the next transition to occur smoothly
        StopAllCoroutines();
    }

    public void HandleIntermissionUI()
    {
        PlayIntermissionTransition();
        StartCoroutine(StartIntermissionTimer());
    }

    private void PlayIntermissionTransition()
    {
        transitionImage.SetActive(true);
        Invoke(nameof(StopIntermissionTransition), intermissionTransitionDuration);
    } 

    private void StopIntermissionTransition() => transitionImage.SetActive(false);

    private void UpdateScoreCounter()
    {
        // essentially, while the player may be on the red team server-side
        // we make the owner client's team on the blue team regardless
        if (PlayerInfo.instance.teamColor.Value == "Blue")
        {
            blueTeamWonRoundText.text = GameManager.instance.blueTeamWonAmount.Value.ToString();
            redTeamWondRoundText.text = GameManager.instance.redTeamWonAmount.Value.ToString();
        }
        else
        {
            blueTeamWonRoundText.text = GameManager.instance.redTeamWonAmount.Value.ToString();
            redTeamWondRoundText.text = GameManager.instance.blueTeamWonAmount.Value.ToString();
        }
    }

    public void ShowNotificationUI(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        notificationObj.SetActive(true);
        notificationText.text = message;

        // cause the whole animation is around 6 seconds
        Invoke(nameof(DisableNotificationUI), 5f);
    }

    private void DisableNotificationUI() => notificationObj.SetActive(false);

    public void UpdateCurrentRoundText() => currentRoundText.text = $"ROUND {GameManager.instance.roundCount.Value}";

    private IEnumerator StartIntermissionTimer()
    {
        float time = Time.time;

        // reset text color
        timerText.color = Color.white;

        while (Time.time - time <= GameManager.instance.intermissionTime)
        {
            int timeRemaining = Mathf.Max(0, Mathf.RoundToInt(GameManager.instance.intermissionTime - (Time.time - time)));
            int minutes = timeRemaining / 60;
            int seconds = timeRemaining % 60;

            if (minutes > 0)
                timerText.text = $"{minutes}:{seconds:D2}";
            else
                timerText.text = $"0:{seconds:D2}";

            yield return null;
        }
    }

    private IEnumerator StartRoundTimer()
    {
        float time = Time.time;

        while (Time.time - time <= GameManager.instance.roundTime)
        {
            int timeRemaining = Mathf.Max(0, Mathf.RoundToInt(GameManager.instance.roundTime - (Time.time - time)));
            int minutes = timeRemaining / 60;
            int seconds = timeRemaining % 60;

            if (minutes > 0)
                timerText.text = $"{minutes}:{seconds:D2}";
            else
                timerText.text = $"0:{seconds:D2}";

            // change text to red
            if (minutes < 1 && seconds <= 6)
                timerText.color = Color.red;

            yield return null;
        }
    }
}
