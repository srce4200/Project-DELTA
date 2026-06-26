using Photon.Pun;
using System.Collections;
using System.IO;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class UnitSpawn : TriggerBase
{
    public int unitAmaunt = 5;
    public string pathToUnit;

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
        for (int i = 0; i < unitAmaunt; i++)
        {
            PhotonNetwork.InstantiateRoomObject(pathToUnit, transform.position, Quaternion.Euler(0, 0, 0));
        }
    }
}
