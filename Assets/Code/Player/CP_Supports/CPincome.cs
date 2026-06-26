using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPincome : MonoBehaviour
{
    public bool isCaptured;
    public int cpIncomeAmaunt;
    public float avaibleTime;

    int capturingPlayers;

    PhotonView _pv;
    private void Start()
    {
        if(avaibleTime >= 0)
        {
            _pv = GetComponent<PhotonView>();
            if (!_pv.IsMine)
                return;
            InvokeRepeating(nameof(UpdateCapStatus), 10, 1);
            GameObject.Find("--MapManager--").GetComponentInChildren<SupportsMenu>().incomePoints.Add(this);
            GetComponent<Animator>().enabled = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "ermacore faction")
        {
            capturingPlayers++;
            isCaptured = true;
            GetComponent<Animator>().enabled = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "ermacore faction")
        {
            capturingPlayers--;
            if (capturingPlayers <= 0)
            {
                GetComponent<Animator>().enabled = false;
                isCaptured = false;
            }
        }
    }
    void UpdateCapStatus()
    {
        if (isCaptured)
        {
            avaibleTime -= 1;
            if(avaibleTime <= 0)
            {
                GetComponent<DeleteObject>().Delete();
            }
        }
    }
}
