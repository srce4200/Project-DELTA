using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfo : MonoBehaviour
{
    public static MapInfo Instance;
    public string mapName;
    public GameObject adminPanelDefault;
    public GameObject[] gameModes;

    [Header("Players")]
    public static string FolderFactionPath;

    public int TeamTickets;

    [Header("GameMode")]
    public int GameModeType;

    [Header("Active Tasks")]
    public List<Task> tasks;

    //[HideInInspector]public List<Transform> activePlayers = new List<Transform>();

    [HideInInspector] public List<Transform> activeBlufor = new List<Transform>();
    [HideInInspector] public List<Transform> activeRedfor = new List<Transform>();
    private void Awake()
    {
        adminPanelDefault.SetActive(true);
        GameModeType = Launcher.Instance.GameMode;
        Instance = this;
        
        print("Editor/CustomMission");
        if (GameModeType == -1)
        {
            
        }
        else
        {
            if(GameModeType == 3)
            {
                adminPanelDefault.SetActive(false);
            }
            gameModes[GameModeType].SetActive(true);
        }
    }
    private void Update()
    {
        if(TeamTickets <= 0)
        {
            StartCoroutine(EndGame());
        }
    }

    #region Task System

    public void AddTask(Task task)
    {
        tasks.Add(task);
        GetComponent<GameUIManager>().QueueNotificationTask(0, "New Task Assigned");
    }
    public void RemoveTask(Task task)
    {
        tasks.Remove(task);
        GetComponent<GameUIManager>().QueueNotificationTask(0, "Task Completed");
    }

    #endregion

    public void SetTickets(int am)
    {
        if (PhotonNetwork.IsMasterClient)
            GetComponent<PhotonView>().RPC(nameof(RPC_SetTickets), RpcTarget.AllBuffered, am);
    }
    [PunRPC]
    void RPC_SetTickets(int am)
    {
        TeamTickets = am;
    }
    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(3f);
        Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.Disconnect();
        yield return new WaitForSeconds(0.2f);

        PhotonNetwork.LoadLevel(0);
    }

    //public void AddAlivePlayer(Transform tr)
    //{
    //    activePlayers.Add(tr);
    //}
    //public void RemoveAlivePlayer(Transform tr)
    //{
    //    activePlayers.Remove(tr);
    //}
    public void AddAliveUnit(Transform tr, UnitSide unitSide)
    {
        if(unitSide == UnitSide.blufor)
            activeBlufor.Add(tr);
        else if (unitSide == UnitSide.redfor)
            activeRedfor.Add(tr);
    }
    public void RemoveAliveUnit(Transform tr, UnitSide unitSide)
    {
        if (unitSide == UnitSide.blufor)
            activeBlufor.Remove(tr);
        else if (unitSide == UnitSide.redfor)
            activeRedfor.Remove(tr);
    }
}
