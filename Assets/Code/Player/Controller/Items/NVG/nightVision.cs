using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class nightVision : MonoBehaviour
{
    bool state = false;
    public bool playerNvg = false;
    [SerializeField] MeshRenderer nvgModel;
    [SerializeField] GameObject NVGfilter;
    [SerializeField] PhotonView pv;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && pv.IsMine)
        {
            state = !state;
            nvgModel.enabled = !state;
            NVGfilter.SetActive(state);
            if(playerNvg)
                GetComponent<Animator>().SetBool("turn", state);
        }
    }
}
