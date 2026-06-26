using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    public int taskType;
    public string taskName;
    public string taskDescription;
    public string taskID;
    [HideInInspector] public Sprite taskIcon;
    public Transform position;
}
public class AddTask : TriggerBase
{
    public Task task = new Task();
    [SerializeField] Sprite[] taskSprites;
    MapInfo mi;

    void Start()
    {

        TriggerCont();

        if (PhotonNetwork.OfflineMode || !PhotonNetwork.IsMasterClient)
            return;

        task.position = transform;
        mi = MapInfo.Instance;
        task.taskIcon = taskSprites[task.taskType];

        if (triggerModule != null)
        {
            StartCoroutine(CheckTrigger());
        }
        else
        {
            ExecuteCommand();
        }
    }

    public override void ExecuteCommand()
    {
        mi.AddTask(task);
    }
    private void OnDestroy()
    {
        if (mi != null && mi.tasks.Contains(task))
            mi.RemoveTask(task);
    }
}
