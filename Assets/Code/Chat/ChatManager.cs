using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using Photon.Voice.Unity;

[System.Serializable]
public class Channel
{
    public Color textColor;
    public string channelName;
}

public class ChatManager : MonoBehaviour
{
    public TMP_InputField chatInput;
    public TextMeshProUGUI chatContent;
    public TextMeshProUGUI currentTextChannel;
    PhotonView _photon;
    List<string> _messages = new List<string>();

    [SerializeField] List<Channel> channels = new List<Channel>();
    int currentChannel = 0;
    [SerializeField] KeyCode changeChannelKey;
    [Space]
    [SerializeField] KeyCode voiceChatKey;
    [SerializeField] GameObject voiceChatIcon;
    Recorder vcRecorder;

    float _buildDelay = 0f;
    int maximumMessages = 5;
    // Start is called before the first frame update
    void Start()
    {
        chatInput.gameObject.SetActive(false);
        _photon = GetComponent<PhotonView>();
        vcRecorder = GetComponentInChildren<Recorder>();
        ChangeChannelText(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            if (_messages.Count > maximumMessages)
            {
                _messages.RemoveAt(0);
            }
            if (_buildDelay < Time.time)
            {
                BuildChatContents();
                _buildDelay = Time.time + 0.25f;
            }
        }
        else if (_messages.Count > 0)
        {
            _messages.Clear();
            chatContent.text = "";
        }

        OpenChat();
        VoiceChat();
        ChangeChannel();
    }

    #region Input (General)

    void VoiceChat()
    {
        bool active = Input.GetKey(voiceChatKey);
        vcRecorder.TransmitEnabled = active;
        voiceChatIcon.SetActive(active);
    }

    void OpenChat()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (chatInput.isActiveAndEnabled == false)
            {
                chatInput.gameObject.SetActive(true);
                chatInput.ActivateInputField();
            }
            else
            {
                SubmitChat();
            }
        }
    }

    void ChangeChannel()
    {
        if (Input.GetKeyDown(changeChannelKey))
        {
            currentChannel = (currentChannel + 1) % channels.Count;
            ChangeChannelText();
        }
    }

    void ChangeChannelText()
    {
        currentTextChannel.SetText(channels[currentChannel].channelName);
        currentTextChannel.color = channels[currentChannel].textColor;
    }

    #endregion

    #region Text Message

    [PunRPC]
    void RPC_AddNewMessage(string msg)
    {
        _messages.Add(msg);
    }

    public void SendChat(string msg)
    {
        string colr = ColorUtility.ToHtmlStringRGB(channels[currentChannel].textColor);
        string newMessage = "<color=#" + colr + ">" + (string)PhotonNetwork.NickName + ": " + "</color>" + msg;
        _photon.RPC("RPC_AddNewMessage", RpcTarget.All, newMessage);
    }
     
    public void SubmitChat()
    {
        string inputText = chatInput.text;
        if (string.IsNullOrWhiteSpace(inputText))
        {
            chatInput.ActivateInputField();
            chatInput.text = "";
            return;
        }

        // Split input into separate lines
        string[] lines = inputText.Split('\n');
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (!string.IsNullOrEmpty(trimmedLine))
            {
                SendChat(trimmedLine);
            }
        }

        chatInput.text = "";
        chatInput.DeactivateInputField();
        chatInput.gameObject.SetActive(false);

        // Rebuild chat contents to display new messages immediately
        BuildChatContents();
    }

    void BuildChatContents()
    {
        string newContents = string.Join("\n", _messages.ToArray());
        chatContent.text = newContents;
        chatContent.ForceMeshUpdate(); // Update the text mesh to reflect the changes immediately
    }

    #endregion
}
