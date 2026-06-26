using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentPlayerToObject : MonoBehaviour
{
    public GameObject player;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "ermacore faction")
        {
            player = other.gameObject;
            print(other.name);
            PhotonView playerPhotonView = player.GetComponent<PhotonView>();
            playerPhotonView.RPC("SetParent", RpcTarget.All, gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "ermacore faction")
        {
            player = other.gameObject;
            print(other.name);
            PhotonView playerPhotonView = player.GetComponent<PhotonView>();
            playerPhotonView.RPC("ClearParent", RpcTarget.All);
        }
    }
}
