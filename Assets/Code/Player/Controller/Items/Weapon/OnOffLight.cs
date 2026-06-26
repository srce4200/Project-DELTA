using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OnOffLight : MonoBehaviour
{
    [SerializeField] KeyCode keyBind;
    public GameObject itemObject;
    bool onOff = false;

    PhotonView pv;
    // Start is called before the first frame update
    void Awake()
    {
        itemObject.SetActive(false);
        pv = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine)
            return;
        if (Input.GetKeyDown(keyBind) && pv.IsMine && !Input.GetKey(KeyCode.LeftControl) && itemObject != null)
        {
            if(onOff == false)
            {
                onOff = true;
                pv.RPC(nameof(FlashLight), RpcTarget.All, true);
            }
            else if(onOff == true)
            {
                onOff = false;
                pv.RPC(nameof(FlashLight), RpcTarget.All, false);
            }
        }
    }
    [PunRPC]
    void FlashLight(bool OnOff)
    {
        itemObject.SetActive(OnOff);
    }
}
