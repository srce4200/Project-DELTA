using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralRecoil : MonoBehaviour
{
    PhotonView PV;
    [HideInInspector] public Transform rotationPoint;
    
    [Header("Speed Settings")]
    public float rotationalRecoilSpeed = 8f;
    [Space(5)]

    public float rotationalReturnSpeed = 38f;
    [Space(10)]

    [Header("Amount Settings:")]
    public Vector3 RecoilRotation = new Vector3(10, 5, 7);
    public Vector3 RecoilKickBack = new Vector3(0.015f, 0f, -0.2f);
    [Space(10)]
    public Vector3 RecoilRotationAim = new Vector3(10, 4, 6);
    public Vector3 RecoilKickBackAim = new Vector3(0.015f, 0f, -0.2f);

    Vector3 rotationalRecoil;
    Vector3 Rot;
    private void Start()
    {
        PV = transform.root.GetComponent<PhotonView>();
    }
    private void LateUpdate()
    {
        if (!PV.IsMine)
            Destroy(this);

        if (rotationPoint == null)
            return;

        rotationalRecoil = Vector3.Lerp(rotationalRecoil, Vector3.zero, rotationalReturnSpeed * Time.deltaTime);

        Rot = Vector3.Slerp(Rot, rotationalRecoil, rotationalRecoilSpeed * Time.deltaTime);
        rotationPoint.localRotation = Quaternion.Euler(Rot);
    }

    public void Recoil(bool aiming)
    {
        if(aiming)
        {
            rotationalRecoil += new Vector3(-RecoilRotationAim.x, Random.Range(-RecoilRotationAim.y, RecoilRotationAim.y), Random.Range(-RecoilRotationAim.z, RecoilRotationAim.z));
        }
        else
        {
            rotationalRecoil += new Vector3(-RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y), Random.Range(-RecoilRotation.z, RecoilRotation.z));
            rotationalRecoil += new Vector3(Random.Range(-RecoilKickBack.x, RecoilKickBack.x), Random.Range(-RecoilKickBack.y, RecoilKickBack.y), RecoilKickBack.z);
        }
    }
}
