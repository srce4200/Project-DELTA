using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : MonoBehaviour
{
    [Range(1f, 10f)]
    public float zoomIn = 1f;
    public Transform camZoomPos;

    [SerializeField] Camera sightCam;
    [SerializeField] PhotonView _pv;
    private void Awake()
    {
        if(!_pv.IsMine && sightCam != null)
            Destroy(sightCam.gameObject);
    }
    private void Start()
    {
        if (_pv != null)
        {
            if (sightCam != null)
            {
                sightCam.fieldOfView = 10f / zoomIn;
                if (!_pv.IsMine)
                {
                    sightCam.enabled = false;
                }
            }
        }
    }

}
