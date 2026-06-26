using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class CheckArmband : MonoBehaviour
{
    [SerializeField] Renderer playerBody;
    [SerializeField] Material alphaTeamMat;
    [SerializeField] Material bravoTeamMat;
    [SerializeField] Material charlieTeamMat;
    [SerializeField] Material noteamTeamMat;
    // Start is called before the first frame update
    void Start()
    {
        PhotonTeam pTeam = PhotonNetwork.LocalPlayer.GetPhotonTeam();
        /*if(pTeam == null)
        {
            SwitchMaterial(noteamTeamMat);
        }
        else if(pTeam.Code == 1)
        {
            SwitchMaterial(alphaTeamMat);
        }
        else if(pTeam.Code == 2)
        {
            SwitchMaterial(bravoTeamMat);
        }
        else if(pTeam.Code ==3)
        {
            SwitchMaterial(charlieTeamMat);
        }*/
    }
    void SwitchMaterial(Material material)
    {
        Material[] materials = playerBody.materials;
        materials[3] = material;
        playerBody.materials = materials;
    }
}
