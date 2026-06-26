using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RespawnsBox : Interactable
{
    [SerializeField] int respawnsAmaunt;

    PhotonView _pv; 
    private void Start()
    {
        _pv = GetComponent<PhotonView>();  
    }
    public override void Interact(GameObject player)
    {
        base.Interact(player); 
        DeleteCrate();
        GameObject.Find("--MapManager--").GetComponent<MapInfo>().TeamTickets += respawnsAmaunt;
    }
    public void DeleteCrate()
    { 
        _pv.RPC(nameof(DestroyObject), RpcTarget.All);
    }
    [PunRPC]
    void DestroyObject()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}
