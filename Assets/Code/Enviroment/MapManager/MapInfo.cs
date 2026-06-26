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
    private void Awake()
    {
        adminPanelDefault.SetActive(true);
        GameModeType = Launcher.Instance.GameMode;
        Instance = this;
        if (GameModeType == -1)
        {
            print("Editor/CustomMission");
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


    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(3f);
        Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.Disconnect();
        yield return new WaitForSeconds(0.2f);

        PhotonNetwork.LoadLevel(0);
    }
}
