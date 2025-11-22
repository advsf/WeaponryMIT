using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class ChatboxManager : NetworkBehaviour 
{
    public static ChatboxManager instance;

    // no need to switch to the new input system because you cant seem to rebind the right enter key...
    [Header("Input")]
    [SerializeField] private KeyCode enablerKey;

    [Header("Prefab References")]
    [SerializeField] private GameObject openChatText; // to be put in the scrollview with the chat being opened
    [SerializeField] private GameObject closedChatText; // similar to valorant, when the chat isnt opened show the message without the inputfield

    [Header("References")]
    [SerializeField] private GameObject closedChat; // when the chat isnt opened
    [SerializeField] private GameObject openedChat; // when the chat is opened
    [SerializeField] private Transform openChatParent; 
    [SerializeField] private Transform closedChatParent;

    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI currentChatOption; // implemenet later, but it indicates whether its a team or all chat

    [Header("Spam Detection")]
    [SerializeField] private int maxAmountOfText = 5; // max amount of time 
    [SerializeField] private float spamDuration; // time before user can type again
    private int amountOfTextSent = 0;
    private bool canText = true;

    [Header("Script References")]
    [SerializeField] private HandleActiveGun handleActiveGunScript;

    public bool IsOn { get => openedChat.activeInHierarchy; }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        instance = this;

        closedChat.SetActive(true);
        openedChat.SetActive(false);

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsOwner) return;

        // open up the chatbox
        if (Input.GetKeyDown(enablerKey) && !openedChat.activeInHierarchy)
        {
            openedChat.SetActive(true);
            closedChat.SetActive(false);

            // allow cursor movement
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            // autofocuses the inputfield
            inputField.Select();
            inputField.ActivateInputField();
        }
        // or close it
        else if (Input.GetKeyDown(enablerKey))
        {
            // allow cursor movement
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            closedChat.SetActive(true);
            openedChat.SetActive(false);
        }

        HandleSpamming();
    }

    private void HandleSpamming()
    {
        if (amountOfTextSent >= maxAmountOfText)
            StartCoroutine(PreventSendingMessage());
    }

    private IEnumerator PreventSendingMessage()
    {
        canText = false;
        inputField.interactable = false;

        float time = Time.time;
        while (Time.time - time <= spamDuration)
        {
            inputField.text = $"You must wait {spamDuration - Mathf.FloorToInt(Time.time - time)} seconds before texting again!";
            yield return null;
        }

        canText = true;
        inputField.interactable = true;
        inputField.text = "";
    }

    private void DecreaseAmountOfTextSentInt()
    {
        // this function is called every time a user sends a message
        // essentially, if the user spams a message and reaches the max amount of texts allocated
        // they will be muted
        // but if they dont spam, this allows the user to freely text without any mute
        if (amountOfTextSent <= 0) return;

        amountOfTextSent--;
    }

    public void HandleFormattingTexts(string teamColor, string username, string text)
    {
        // if there isnt a message
        if (text.Length == 0)
            return;

        GameObject openTextObj = Instantiate(openChatText, openChatParent);
        GameObject closedTextObj = Instantiate(closedChatText, closedChatParent);

        // if we sent out the message
        if (username == LobbyManager.instance.username)
        {
            openTextObj.GetComponent<TextMeshProUGUI>().text = $"<color=yellow>{username}:<color=white> {text}";
            closedTextObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"<color=yellow>{username}:<color=white> {text}";
        }

        // if on the same team, make the username text blue
        else if (teamColor == PlayerInfo.instance.teamColor.Value)
        {
            // #26B5E3 == light blue
            openTextObj.GetComponent<TextMeshProUGUI>().text = $"<color=#26B5E3>{username}:<color=white> {text}";
            closedTextObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"<color=#26B5E3>{username}:<color=white> {text}";
        }

        // if not on the same team
        else
        {
            openTextObj.GetComponent<TextMeshProUGUI>().text = $"<color=red>{username}:<color=white> {text}";
            closedTextObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"<color=red>{username}:<color=white> {text}";
        }
    }

    public void SendMessage() 
    {
        if (!canText) return;

        // spam detection related
        amountOfTextSent++;
        Invoke(nameof(DecreaseAmountOfTextSentInt), 2f);

        if (IsHost)
            SendTextClientRpc(PlayerInfo.instance.teamColor.Value.ToString(), LobbyManager.instance.username, inputField.text);
        else
            SendTextServerRpc(PlayerInfo.instance.teamColor.Value.ToString(), LobbyManager.instance.username, inputField.text);

        // reset the inputfield text
        inputField.text = "";
    }

    [ServerRpc]
    private void SendTextServerRpc(string teamColor, string username, string text) => SendTextClientRpc(teamColor, username, text);

    [ClientRpc]
    private void SendTextClientRpc(string teamColor, string username, string text) => ChatboxManager.instance.HandleFormattingTexts(teamColor, username, text);
}
