using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffExtra : MonoBehaviour
{
    [SerializeField] KeyCode keyBind;
    [SerializeField] GameObject itemObject;
    bool onOff = false;

    PhotonView pv;
    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine && Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(keyBind))
            {
                if (onOff == false)
                {
                    onOff = true;
                    pv.RPC(nameof(FlashLight), RpcTarget.All, true);
                }
                else if (onOff == true)
                {
                    onOff = false;
                    pv.RPC(nameof(FlashLight), RpcTarget.All, false);
                }
            } 
        }
    }
    [PunRPC]
    void FlashLight(bool OnOff)
    {
        itemObject.SetActive(OnOff);
    }
}
