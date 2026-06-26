using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParenting : MonoBehaviourPunCallbacks
{
    [PunRPC]
    void SetParent(string parentName)
    {
        GameObject parentObject = GameObject.Find(parentName);
        transform.SetParent(parentObject.transform);
    }

    [PunRPC]
    void ClearParent()
    {
        transform.parent = null;
    }
}
