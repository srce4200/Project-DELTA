using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteObject : MonoBehaviour
{ 
    public void Delete()
    { 
        GetComponent<PhotonView>().RPC(nameof(DeletGlobaly), RpcTarget.All);
    }
    [PunRPC]
    void DeletGlobaly()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}
