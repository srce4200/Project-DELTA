using Photon.Pun;
using System.Collections;
using UnityEngine;

public class CallNotify : TriggerBase
{
    public int type = 0;
    public string msg = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        GameUIManager.Instance.QueueNotification(type, msg);
    }
}
