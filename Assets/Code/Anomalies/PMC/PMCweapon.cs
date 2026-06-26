using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PMCweapon : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] float accuracy;
    [SerializeField] string bulletName;
    [SerializeField] AudioClip fireSound;
    [SerializeField] float weaponFirerate;
    [SerializeField] float reloadSpeed;
    float nextTimeToFire;
    [SerializeField] int ammoInMag;
    int currentAmmo; 
    [HideInInspector] public bool isReloading = false;  
    AudioSource soundSource; 
    [SerializeField] Transform gunBarell; 
    [SerializeField] ParticleSystem muzzelFlash; 
    Animator handAnimations; 
    bool isSprinting;
     
    PhotonView PV;  

    ////----------------------START----------------------------
    void Start()
    { 
        soundSource = GetComponent<AudioSource>(); 
        PV = GetComponent<PhotonView>();
        currentAmmo = ammoInMag; 
        handAnimations = GetComponent<Animator>();
        handAnimations.SetBool("isReloading", false);
    }  
    public void FullAuto(Transform target)
    { 
        if (Time.time >= nextTimeToFire && currentAmmo > 0 && !isReloading)
        {
            nextTimeToFire = Time.time + 1f / weaponFirerate;
            currentAmmo--;
            Fire(target);
        }
        else if(!isReloading && currentAmmo < 1)
        {
            StartCoroutine(Reload( reloadSpeed));
        }
    }

    public void Semi(Transform target)
    {
        if (Time.time >= nextTimeToFire && currentAmmo > 0 && !isReloading)
        {
            nextTimeToFire = Time.time + 3f / weaponFirerate;
            Fire(target);
        }
        else if(!isReloading && currentAmmo < 1)
        {
            StartCoroutine(Reload( reloadSpeed));
        }
    }  

    void Fire(Transform target)
    {
        //recoil 
        currentAmmo--;
        handAnimations.SetTrigger("Shoot");
        PV.GetComponent<PhotonView>().RPC("BulletShoot", RpcTarget.All);

        float deviation = 1.0f - accuracy;
        Vector3 shootDirection = gunBarell.transform.position + Random.insideUnitSphere * deviation * 2;
        gunBarell.transform.LookAt(target);
        PhotonNetwork.Instantiate("PhotonPrefabs/Temporary/" + bulletName, shootDirection, gunBarell.transform.rotation);
    } 

    [PunRPC]
    void BulletShoot()
    {
        soundSource.PlayOneShot(fireSound);
        muzzelFlash.Play(); 
    }  
    IEnumerator Reload(float reloadSpeed)
    { 
        isReloading = true;
        handAnimations.SetBool("isReloading", true); 

        yield return new WaitForSeconds(reloadSpeed);

        handAnimations.SetBool("isReloading", false);

        currentAmmo = ammoInMag;

        yield return new WaitForSeconds(0.5f);
        isReloading = false;
    }  
}
