using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AdminClient : MonoBehaviour
{
    [SerializeField] string adminDeltaEyeName;
    AdminManager adminManager;
    PhotonView _pv;
    bool _initialized = false;
    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        if (_pv.IsMine)
        {
            InvokeRepeating(nameof(CheckForAdminPermission), 2, 2);
        }            
    }
    void CheckForAdminPermission()
    {
        if (PhotonNetwork.IsMasterClient && !_initialized)
        {
            _pv.RPC(nameof(RPC_AdminChange), RpcTarget.AllBuffered);
            _initialized = true;
        }
    }
    [PunRPC]
    void RPC_AdminChange()
    {
        adminManager = GameObject.Find(adminDeltaEyeName).GetComponent<AdminManager>();
        adminManager.playerDelta = gameObject;
    }
}
