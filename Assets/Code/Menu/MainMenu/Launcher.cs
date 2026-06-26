using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI; 

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    public int GameMode = 0; //0-Hive   //1-Survival
    string mapSelected;
    string factionSelected;
    [HideInInspector] public string missionData = "";


    [SerializeField] TMP_Dropdown mapDropdown;
    [SerializeField] TMP_Dropdown gamemodeDropdown;
    [SerializeField] TMP_Dropdown factionDropdown;

    [SerializeField] TextMeshProUGUI errorText;  
    [Space]
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TextMeshProUGUI roomNameText;
    [SerializeField] Slider maxPlayerSlider;

    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform roomListContent;
    [Space]
    [SerializeField] TMP_InputField playerName;
    [Space]    
    [SerializeField] Transform playerListContent;   
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;
 
    void Awake()
    {
        DisconnectFromMultiplayer();
        Instance = this;
    }

    #region StartMultiplayer
    public void ConnectToMultiplayer()
    {
        PhotonNetwork.OfflineMode = false;
        MenuManager.Instance.OpenMenu("loading");
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();

        if (PlayerPrefs.HasKey("Nickname"))
        {
            string savedNickname = PlayerPrefs.GetString("Nickname");
            playerName.text = savedNickname;
            PhotonNetwork.NickName = savedNickname;
        }

        PhotonNetwork.AddCallbackTarget(this);
    }
    #endregion

    #region Disconnect
    public void DisconnectFromMultiplayer()
    {
        StartCoroutine(DisconnectCoroutine());
    }

    IEnumerator DisconnectCoroutine()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null; // Wait until the network is fully disconnected
        }
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected: " + cause);
        MenuManager.Instance.OpenMenu("title");
        PhotonNetwork.OfflineMode = false;
    }
    #endregion

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        if(!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("find room");
        Debug.Log("Joined lobby");

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }
    public void SetMissionData(string missionDataString)
    {
        missionData = missionDataString;
    }
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        RoomOptions roomOptions = new RoomOptions();

        //------------------TESTING----------------------
        if(gamemodeDropdown.value==1)
        {
            GameMode = -1;
        }
        else
        {
            missionData = "";
            GameMode = gamemodeDropdown.value;
        }

        if (gamemodeDropdown.value == -1 && missionData == "")
            return;

        //------------------TESTING----------------------

        roomOptions.CustomRoomPropertiesForLobby = new string[]  { "map", "gamemode", "faction", "missionData" };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("gamemode", GameMode);

        mapSelected = mapDropdown.GetComponentInChildren<TextMeshProUGUI>().text;
        roomOptions.CustomRoomProperties.Add("map", mapSelected);
        print(mapSelected);

        factionSelected =  ("PhotonPrefabs/Player/") + factionDropdown.options[factionDropdown.value].text + "/"; 
        roomOptions.CustomRoomProperties.Add("faction", factionSelected);

        roomOptions.CustomRoomProperties.Add("missionData", missionData);

        roomOptions.MaxPlayers = (byte)maxPlayerSlider.value;

        PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions, null);       

        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        //remove SAVELOADEDITOR as its only for editor
        if(SaveLoadEditor.Instance != null)
            Destroy(SaveLoadEditor.Instance.gameObject);
        //---------------

        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        // Set the nickname based on the player input
        string newNickname = playerName.textComponent.text;
        PhotonNetwork.NickName = newNickname;

        // Save the nickname to PlayerPrefs
        PlayerPrefs.SetString("Nickname", newNickname);
        PlayerPrefs.Save();

        //set custom data
        if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("missionData", out object missionDataString))
        {
            missionData = (string)missionDataString;
        }

        //set faction
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("faction", out object factionValue))
        {
            factionSelected = (string)factionValue;
        }
        MapInfo.FolderFactionPath = factionSelected;

        //create joined players
        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
        print(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        Debug.LogError("Room Creation Failed: " + message);
        MenuManager.Instance.OpenMenu("error");
    }

    public void StartGame()
    {
        MapInfo.FolderFactionPath = factionSelected;

        //sync mission data than load
        PhotonNetwork.LoadLevel(mapSelected);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info, int gamemodeInt)
    {
        if (info == null)
        {
            Debug.LogError("RoomInfo is null.");
            return;
        }

        if (info.CustomProperties.TryGetValue("faction", out object factionValue))
        {
            factionSelected = (string)factionValue;
        }
        MapInfo.FolderFactionPath = factionSelected;

        PhotonNetwork.JoinRoom(info.Name);
        GameMode = gamemodeInt;
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    #region roomUi Update
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(
                roomList[i], (int)roomList[i].CustomProperties["gamemode"], (string)roomList[i].CustomProperties["map"], (string)roomList[i].CustomProperties["faction"]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
    #endregion
}
