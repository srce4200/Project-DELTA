using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSeatCtrl : MonoBehaviour
{
    [SerializeField]VehicleSeat vs;
    [HideInInspector]public GameObject playerObject;
    [HideInInspector]public Vector3 enterPos;
    [HideInInspector]public Vector3 playerScale;
    PhotonView _photonView;
    private void Start()
    { 
        _photonView = GetComponent<PhotonView>();
    }
    public  void Interact(GameObject player)
    {
        if (!vs.enabled)
        {
            playerObject = player;
            playerScale = player.transform.localScale; 
            EnterVehicle();
        } 
    }
    private void EnterVehicle()
    {
        vs.enabled = true;
        enterPos = gameObject.transform.root.InverseTransformPoint(playerObject.transform.position);

        playerObject.GetComponent<CharacterController>().enabled = false; 
        playerObject.GetComponent<ChatControl>().EnableMouseInput(false);
        playerObject.GetComponent<ChatControl>().LockControls(true, true);

        // Synchronize position and rotation across the network
        _photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        _photonView.RPC("SyncPlayerPositionRotation", RpcTarget.AllBuffered, playerObject.GetComponent<PhotonView>().ViewID, true, null, null); 
    } 
    public void ExitVehicle()
    {
        playerObject.GetComponent<ChatControl>().EnableMouseInput(true);
        playerObject.GetComponent<ChatControl>().LockControls(false, false);
        playerObject.GetComponent<CharacterController>().enabled = true;

        _photonView.RPC("SyncPlayerPositionRotation", RpcTarget.AllBuffered, playerObject.GetComponent<PhotonView>().ViewID, false, enterPos, playerScale);  
    }

    [PunRPC]
    private void SyncPlayerPositionRotation(int playerViewID, bool getIn, Vector3 enterPos2, Vector3 scale)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView != null)
        {
            if (getIn)
            {
                GameObject pl = playerView.gameObject;
                pl.transform.SetParent(transform, true);
                pl.transform.localPosition = Vector3.zero;
                pl.transform.localRotation = Quaternion.identity;
                this.enabled = false;
            }
            else
            {
                GameObject pl = playerView.gameObject;
                pl.transform.SetParent(null, false);
                pl.transform.position = gameObject.transform.root.TransformPoint(enterPos2);
                pl.transform.rotation = Quaternion.identity;
                pl.transform.localScale = scale;
                vs.enabled = false;
            }
        }
    }
}
