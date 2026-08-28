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
     
    PhotonView PV;
    bool isBursting;
    int burstCount = 5;
    ////----------------------START----------------------------
    void Start()
    {
        soundSource = GetComponent<AudioSource>(); 
        PV = GetComponent<PhotonView>();
        currentAmmo = ammoInMag; 
        handAnimations = GetComponent<Animator>();
        handAnimations.SetBool("isReloading", false);
    }  
    public void SetSprintAnim(bool sprint)
    {
        handAnimations.SetBool("isSprinting", sprint);
    }
    public void FullAuto(Transform target)
    { 
        if (Time.time >= nextTimeToFire && currentAmmo > 0 && !isReloading)
        {
            nextTimeToFire = Time.time + 0.5f / weaponFirerate;
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
    public void Burst(Transform target)
    {
        if (!isBursting && !isReloading && currentAmmo > 0 && Time.time >= nextTimeToFire)
        {
            StartCoroutine(BurstRoutine(target));
        }
        else if (!isReloading && currentAmmo < 1)
        {
            StartCoroutine(Reload(reloadSpeed));
        }
    }

    IEnumerator BurstRoutine(Transform target)
    {
        isBursting = true;
        for (int i = 0; i < burstCount; i++)
        {
            if (currentAmmo <= 0 || target == null) break;
            currentAmmo--;
            Fire(target);
            yield return new WaitForSeconds(0.5f / weaponFirerate);
        }
        // Small enforced pause after a burst before another fire command is accepted -
        // stops the AI from chaining bursts into a de-facto full-auto stream.
        nextTimeToFire = Time.time + 1f / weaponFirerate;
        isBursting = false;
    }

    void Fire(Transform target)
    {
        //recoil 
        currentAmmo--;
        handAnimations.SetTrigger("Shoot");
        PV.RPC("BulletShoot", RpcTarget.All);
        
        //gunBarell.transform.LookAt(target);
        float deviation = 1.0f - accuracy;

        float spreadX = Random.Range(-deviation, deviation) * 15f;
        float spreadY = Random.Range(-deviation, deviation) * 15f;

        Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);
        Quaternion finalBulletRotation = gunBarell.transform.rotation * spreadRotation;

        PhotonNetwork.Instantiate("PhotonPrefabs/Temporary/" + bulletName, gunBarell.transform.position, finalBulletRotation);
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
