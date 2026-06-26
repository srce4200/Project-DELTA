using Photon.Pun;
using UnityEngine;

public class SetTask : TriggerBase
{
    public string taskID;
    void Start()
    {
        TriggerCont();

        if (PhotonNetwork.OfflineMode || !PhotonNetwork.IsMasterClient)
            return;

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
        foreach(Task tsk in MapInfo.Instance.tasks)
        {
            if(tsk.taskID == taskID)
            {
                MapInfo.Instance.RemoveTask(tsk);
                break;
            }
        }
    }
}
