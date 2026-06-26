using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

public class MenuChat : MonoBehaviour
{
    public TMP_InputField chatInput;
    public TextMeshProUGUI chatContent;
    PhotonView _photon;
    List<string> _messages = new List<string>();
    float _buildDelay = 0f;
    int maximumMessages = 5;
    // Start is called before the first frame update
    void Start()
    {
        _photon = GetComponent<PhotonView>();
    }

    [PunRPC]
    void RPC_AddNewMessage(string msg)
    {
        _messages.Add(msg);
    }

    public void SendChat(string msg)
    {
        string newMessage = "<color=#00bfff>" + (string)PhotonNetwork.NickName + ": " + "</color>" + msg;
        _photon.RPC("RPC_AddNewMessage", RpcTarget.All, newMessage);
    }

    public void SubmitChat()
    {
        string blankCheck = chatInput.text;
        blankCheck = Regex.Replace(blankCheck, @"\s", "");
        if (blankCheck == "")
        {
            chatInput.ActivateInputField();
            chatInput.text = "";
            return;
        }

        SendChat(chatInput.text);
        chatInput.text = "";
        chatInput.DeactivateInputField();
        chatInput.gameObject.SetActive(false);
    }

    void BuildChatContents()
    {
        string NewContents = "";
        foreach (string s in _messages)
        {
            NewContents += s + "\n";
        }
        chatContent.text = NewContents;
    }
    IEnumerator DeleteMessages()
    {
        yield return new WaitForSeconds(7f);
        _messages.RemoveAt(0);
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
}
