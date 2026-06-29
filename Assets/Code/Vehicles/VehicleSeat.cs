using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice;
using UnityEngine.UIElements.Experimental;

public class VehicleSeat : MonoBehaviour
{  
    VehicleSeatCtrl vsc;
    GameObject player;
    //Transform playerCam; 

    public AnimatorOverrideController seatAnim;
    RuntimeAnimatorController deafultAnimator; 

    public GameObject enableScript;


    private void Start()
    {
        vsc = GetComponent<VehicleSeatCtrl>(); 
    }
    private void OnEnable()
    {
        player = GetComponent<VehicleSeatCtrl>().playerObject;
        //playerCam = player.GetComponentInChildren<Camera>().gameObject.transform;

        GetComponent<PhotonView>().RPC("RPC_Anim", RpcTarget.AllBuffered, false, player.GetComponent<PhotonView>().ViewID);

        ActivateDeactivateSeatControl(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F)) 
        {
            vsc.enabled = true;
            vsc.ExitVehicle();
        } 
    }   


    private void OnDisable()
    {
        GetComponent<PhotonView>().RPC("RPC_Anim", RpcTarget.AllBuffered, true, player.GetComponent<PhotonView>().ViewID);

        ActivateDeactivateSeatControl(false);
    }

    void ActivateDeactivateSeatControl(bool boolean)
    {
        GunnerSeat gun = enableScript.GetComponent<GunnerSeat>();
        if (gun != null)
            gun.scriptEnabled = boolean;

        HeliInput heli = enableScript.GetComponent<HeliInput>();
        if (heli != null)
            heli.InputActive = boolean;
    }

    [PunRPC]
    void RPC_Anim(bool defaultAnim, int playerID)
    {
        PhotonView playerView = PhotonView.Find(playerID);
        GameObject pl = playerView.gameObject;

        if (defaultAnim)
            pl.GetComponent<Animator>().runtimeAnimatorController = deafultAnimator;
        else
        { 
            deafultAnimator = pl.GetComponent<Animator>().runtimeAnimatorController;
            pl.GetComponent<Animator>().runtimeAnimatorController = seatAnim; 
        }
    }
}
