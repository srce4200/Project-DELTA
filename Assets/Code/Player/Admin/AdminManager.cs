using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AdminManager : MonoBehaviour
{
    public GameObject playerDelta;

    PhotonView PV;
    [SerializeField] GameObject adminPanel;
    bool active;
    private void Start()
    {
        PV = GetComponent<PhotonView>();
        if(adminPanel != null)
            adminPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || adminPanel == null)
            return;
        
        if (Input.GetKeyDown(KeyCode.Y) && Application.systemLanguage == SystemLanguage.English || Input.GetKeyDown(KeyCode.Z) && !(Application.systemLanguage == SystemLanguage.English))
        {
            active = !active;
            print(active);
        }
        adminPanel.SetActive(active);
        if (playerDelta != null)
            PV.RPC(nameof(SetActivePlayerModel), RpcTarget.All, !active);
    }
    [PunRPC]
    void SetActivePlayerModel(bool activeOrNo)
    {
        playerDelta.SetActive(activeOrNo);
    }
}
