using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerSeat : MonoBehaviour
{
    public bool scriptEnabled;
    [SerializeField] float rotationLimitMax;
    [SerializeField] float rotationLimitMin;

    [Space]
    [SerializeField] Transform gunnerPivot;
    [SerializeField] LayerMask enemyMask;
    [SerializeField] Transform gunBarell;
    [SerializeField] ParticleSystem muzzelFlash;
    int currentAmmo = 100;
    float nextTimeToFire;
    public ScriptableSeat gunnerSeatStats;
    public AudioSource soundSource;
    public PhotonView PV;

    float pitch = 0;
    float yaw = 0;

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine || !scriptEnabled) 
            return;

        yaw += Input.GetAxis("Mouse X");
        pitch += Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch, rotationLimitMin, rotationLimitMax);
        gunnerPivot.transform.localEulerAngles = new Vector3(pitch, yaw, 0f);

        if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + 1f / gunnerSeatStats.weaponFirerate;
            currentAmmo--;
            Fire();
        }

    }
    void Fire()
    {  
        PV.RPC("BulletShoot", RpcTarget.All);
        PhotonNetwork.Instantiate("PhotonPrefabs/Temporary/" + gunnerSeatStats.bulletName, gunBarell.transform.position, gunBarell.transform.rotation);
    }  

    [PunRPC]
    void BulletShoot()
    {
        soundSource.PlayOneShot(gunnerSeatStats.fireSound);
        muzzelFlash.Play();

        //might impact the game lag
        Collider[] zombies = Physics.OverlapSphere(transform.position, 100f, enemyMask);
        for (int i = 0; i < zombies.Length; i++)
        {
            //zombies[i].GetComponent<zombieAI_1>().SoundAlert();
        }
    }
}
