using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class isPvMine : MonoBehaviour
{
    public bool destroy = false;
    [SerializeField] PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        if(!destroy)
            gameObject.SetActive(photonView.IsMine);
        else
            Destroy(gameObject);
    }
}
