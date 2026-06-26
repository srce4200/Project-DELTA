using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAim : MonoBehaviour
{
    public Transform camTransform;
    Camera cam;
    Vector3 startPos;
    float startFov;

    float fovMultiplier;
    bool zoomIn;

    // Start is called before the first frame update
    void Start()
    {
        this.enabled = GetComponent<PhotonView>().IsMine;

        startPos = camTransform.localPosition;
        cam = camTransform.GetComponent<Camera>();
        startFov = cam.fieldOfView;
    }
    public void Aim(bool isAiming, float aimSpeed, float zoomInRatio, Transform aimPos)
    {
        if (isAiming)
        {
            if (zoomIn)
            {
                if (aimPos != null)
                    camTransform.position = Vector3.Lerp(camTransform.position, aimPos.position, aimSpeed * Time.deltaTime);

                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, startFov / (zoomInRatio * fovMultiplier), aimSpeed * Time.deltaTime);
            }
            else
            {
                if (aimPos != null)
                    camTransform.position = Vector3.Lerp(camTransform.position, aimPos.position, aimSpeed * Time.deltaTime);

                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, startFov / zoomInRatio, aimSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (zoomIn)
            {
                camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, startPos, aimSpeed * Time.deltaTime);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, (startFov / fovMultiplier), aimSpeed * Time.deltaTime);
            }
            else
            {
                camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, startPos, aimSpeed * Time.deltaTime);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, startFov, aimSpeed * Time.deltaTime);
            }
        }
    }
    public void ZoomInNOut(bool isAiming, float zoomInRatio)
    {
        fovMultiplier = zoomInRatio;
        zoomIn = isAiming;
    }
}
