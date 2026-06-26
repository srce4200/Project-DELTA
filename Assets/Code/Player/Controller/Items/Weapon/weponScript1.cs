using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System;

public class weponScript1 : Item
{
    #region Public/Private Fields
    TextMeshProUGUI fireModeText,weaponName,amauntText;

    [SerializeField] WeaponStats firearmStats;

    enum CurrentFireMode { semi, auto, underbarrel};
    CurrentFireMode currentFireMode = CurrentFireMode.semi;
    int currentFireModeInt;
    float nextTimeToFire;

    [Header("Aiming")]
    public Scope scope;
    ProceduralAim procAim;    
    float defaultSensitivty;
    bool isAiming;

    [Header("Ammo managment")]
    [Range(0f, 15f)] public int maxMagsStored;    
    List<int> magList = new List<int>();
    int currentMagID;
    int currentAmmo;
    
    [HideInInspector]public int currentUnderbarrelAmmoStored;
    int maxUnderbarrelAmmoStored = 6;
    int currentUnderbarrelAmmo; 

    Transform ammoDisplayList;
    GameObject ammoDisplayMagPrefab;

    [Header("Other")]
    [SerializeField] LayerMask enemyMask;
    [SerializeField] Transform gunBarell;
    [SerializeField] Transform casingSpawn;
    [SerializeField] GameObject casingToSpawn;
    [SerializeField] ParticleSystem muzzelFlash;

    ProceduralRecoil proceduralRecoil;

    [Header("Animations")]
    bool lowReady = false;
    #endregion

    ////----------------------START----------------------------
    private void Awake()
    {
        Setup();
    }
    public override void Start()
    {        
        base.Start();

        //recoil
        proceduralRecoil = GetComponent<ProceduralRecoil>();
        proceduralRecoil.rotationPoint = playerMove.recoilPivot;
        //AIM
        defaultSensitivty = playerMove.mouseSensitivity;        
        
        //AMMO
        Rearm();
        currentUnderbarrelAmmo = firearmStats.ammoInUnderbarrel;
        currentUnderbarrelAmmoStored = maxUnderbarrelAmmoStored;
    }
    public override void Setup()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
        inventoryManager.ammoDisplayList.gameObject.SetActive(true);
        fireModeText = inventoryManager.fireModeText;
        weaponName = inventoryManager.weaponName;
        amauntText = inventoryManager.amauntText;
        ammoDisplayList = inventoryManager.ammoDisplayList;
        ammoDisplayMagPrefab = inventoryManager.ammoDisplayMagPrefab;
        handAnimations = inventoryManager.handAnimations;
    }
    public override void OnEnable()
    {
        base.OnEnable();

        procAim = transform.root.GetComponent<ProceduralAim>();
        amauntText.gameObject.SetActive(false);
        ammoDisplayList.gameObject.SetActive(true);

        isAiming = false;
        isReloading = false;
        handAnimations.runtimeAnimatorController = firearmStats.animator;
        weaponName.SetText(firearmStats.weaponName);
        AmmoDisplayRefresh();
    } 

    private void Update()
    {
        if (!PV.IsMine)
            return;

        Animations_movement();

        if (!isSprinting && !lowReady && !isReloading)
        {
            FireModeCheck();
        }

        procAim.Aim(isAiming, 8, scope.zoomIn, scope.camZoomPos);
        LowReady();
        Aim();

        //reload
        if (Input.GetKeyDown(KeyCode.R) && isReloading == false && !lowReady)
        {
            if(currentFireMode != CurrentFireMode.underbarrel && magList.Count > 0)
                StartCoroutine(Reload(firearmStats.reloadSpeed));
            else if(currentFireMode == CurrentFireMode.underbarrel && currentUnderbarrelAmmoStored > 0)
                StartCoroutine(Reload(firearmStats.reloadSpeedUnder));
        }

        //ammo display
        if (currentFireMode != CurrentFireMode.underbarrel && magList.Count > 0 && isReloading == false)
        {
            RefreshCurrentMag();
        }
        else if(currentFireMode == CurrentFireMode.underbarrel)
        {
            RefreshCurrentUnderbarrel();
        }
    }

    #region Animaitons-movement
    public override void Animations_movement()
    {
        base.Animations_movement();
    }
    #endregion

    #region Aim + Lowready
    void Aim()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) 
        { 
            isAiming = !isAiming; 
            playerMove.isAiming = isAiming;  
        }

        if (isAiming && !isSprinting && !isReloading && !Input.GetKey(KeyCode.Mouse3))
            playerMove.mouseSensitivity = defaultSensitivty / scope.zoomIn;
        else
            playerMove.mouseSensitivity = defaultSensitivty;

    }
    bool inLoop;
    void LowReady()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        { 
            inLoop = !inLoop;
        }

        //RaycastHit hit;
        //float raycastDistance = 2f; // Adjust this distance to fit your needs
        //Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, raycastDistance, groundLayer) && 
        if (inLoop)
        {
            lowReady = true;
        }
        else if (!inLoop)
        {
            lowReady = false;
        }
        handAnimations.SetBool("LowReady", lowReady);
    }

    #endregion

    #region -- Fireing --

    #region Fire mode
    void FireModeCheck()
    {
        if (firearmStats.fireMode == WeaponStats.FireMode.semi_auto_underbarrel)
        {
            ChangeFireType(2);    
            if(currentFireMode == CurrentFireMode.semi)
            {
                Semi();
            }
            else if(currentFireMode == CurrentFireMode.auto)
            {
                FullAuto();
            }
            else
            {
                Underbarrel();
            }
        }
        else if (firearmStats.fireMode == WeaponStats.FireMode.semi_auto)
        {
            ChangeFireType(1);
            if (currentFireMode == CurrentFireMode.semi)
            {
                Semi();
            }
            if (currentFireMode == CurrentFireMode.auto)
            {
                FullAuto();
            }
        }
        else if(firearmStats.fireMode == WeaponStats.FireMode.auto)
        {
            FullAuto();
        }
        else
        {
            Semi();
        }
    }

    void ChangeFireType(int maxInt)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {  
            currentFireModeInt++;
            if (currentFireModeInt > maxInt)
                currentFireModeInt = 0;
            currentFireMode = (CurrentFireMode)currentFireModeInt;
             
            if (currentFireMode == CurrentFireMode.underbarrel)
            {
                handAnimations.runtimeAnimatorController = firearmStats.animatorUnder;
                amauntText.gameObject.SetActive(true);
                ammoDisplayList.gameObject.SetActive(false);
            }
            else
            {
                handAnimations.runtimeAnimatorController = firearmStats.animator;
                amauntText.gameObject.SetActive(false);
                ammoDisplayList.gameObject.SetActive(true);
            }
        }
    }

    void FullAuto()
    {
        fireModeText.SetText("FULL AUTO");
        if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + 1f / firearmStats.weaponFirerate;
            currentAmmo--;
            Fire();
        }
    }

    void Semi()
    {
        fireModeText.SetText("SEMI");
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentAmmo > 0)
        {
            currentAmmo--;
            Fire();
        }
    }

    void Underbarrel()
    {
        fireModeText.SetText("UNDERBARREL");
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentUnderbarrelAmmo > 0)
        {
            currentUnderbarrelAmmo--;
            Fire();
        }
    }

    #endregion

    void Fire()
    {
        proceduralRecoil.Recoil(isAiming);

        handAnimations.SetTrigger("shoot");

        if (casingSpawn != null)
        {
            GameObject temp = Instantiate(casingToSpawn, casingSpawn.position, casingSpawn.rotation, casingSpawn.transform);
            temp.GetComponent<Rigidbody>().AddForce(temp.transform.right * 100f); 
        }

        if (currentFireMode != CurrentFireMode.underbarrel)
        {
            PV.GetComponent<PhotonView>().RPC("BulletShoot", RpcTarget.All);
            PhotonNetwork.Instantiate("PhotonPrefabs/Temporary/" + firearmStats.bulletName, gunBarell.transform.position, gunBarell.transform.rotation);
        }
        else
        {
            gameObject.GetComponent<PhotonView>().RPC("UnderbarrelShoot", RpcTarget.All);
        }
    }

    [PunRPC]
    void UnderbarrelShoot()
    {
        soundSource.PlayOneShot(firearmStats.fireSoundUnder);
        GameObject bult = Instantiate((GameObject)Resources.Load("PhotonPrefabs/Temporary/glRound"), gunBarell.transform.position, Quaternion.Euler(90,0,0));
        bult.GetComponent<Rigidbody>().AddForce(transform.forward * -50, ForceMode.Impulse);
    }

    [PunRPC]
    void BulletShoot()
    {
        soundSource.PlayOneShot(firearmStats.fireSound);
        muzzelFlash.Play();

        //might impact the game lag
        Collider[] zombies = Physics.OverlapSphere(transform.position, 100f, enemyMask);
        for(int i = 0; i < zombies.Length; i++)
        {
            try { 
                zombies[i].transform.root.GetComponent<SoundAlert>().SoundAlerted = true;
            }catch(Exception e) { }
        }
    }


    #endregion

    #region -- ammo --
    IEnumerator Reload(float reloadSpeed)
    {
        if (currentAmmo == 0 && currentFireMode != CurrentFireMode.underbarrel)
        {
            magList.RemoveAt(currentMagID);
            if (magList.Count < 1)
            {
                AmmoDisplayRefresh();
                yield break;
            }
        }
        else if(currentFireMode != CurrentFireMode.underbarrel)
        {
            magList[currentMagID] = currentAmmo;
        }

        isReloading = true;
        handAnimations.SetBool("isReloading", true);

        soundSource.PlayOneShot(firearmStats.reloadSound);
        yield return new WaitForSeconds(reloadSpeed);

        handAnimations.SetBool("isReloading", false);

        if (currentFireMode != CurrentFireMode.underbarrel)
        {
            magList.Sort();

            currentMagID = GetNewMagID();
            currentAmmo = magList[currentMagID];

            AmmoDisplayRefresh();
        }
        else
        {
            currentUnderbarrelAmmo = firearmStats.ammoInUnderbarrel;
            currentUnderbarrelAmmoStored -= 1;
        }

        yield return new WaitForSeconds(0.5f);
        isReloading = false;
    }
    void RefreshCurrentUnderbarrel()
    {
        amauntText.text = (currentUnderbarrelAmmo + currentUnderbarrelAmmoStored).ToString();
    }
    int GetNewMagID()
    {
        int magID = 0;
        int maxValue = 0;

        for (int i = 0; i < magList.Count; i++)
        {
            if (magList[i] > maxValue)
            {
                maxValue = magList[i];
                magID = i;
            }
        }

        return magID;
    }
    void RefreshCurrentMag()
    {
        ammoDisplayList.GetChild(magList.Count - 1).GetComponentInChildren<Slider>().value = currentAmmo;
    }
    public void Rearm()
    {
        magList.Clear();
        for (int i = 0; i < maxMagsStored; i++)
        {
            magList.Add(firearmStats.ammoInMag);
        }
        currentMagID = 0;
        currentAmmo = magList[0];

        currentUnderbarrelAmmoStored = maxUnderbarrelAmmoStored;

        RefreshCurrentUnderbarrel();
        AmmoDisplayRefresh();
    }

    void AmmoDisplayRefresh()
    { 
        foreach (Transform trans in ammoDisplayList)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < magList.Count; i++)
        {
            GameObject temp = Instantiate(ammoDisplayMagPrefab, ammoDisplayList);
            temp.GetComponentInChildren<Slider>().maxValue = firearmStats.ammoInMag;
            temp.GetComponentInChildren<Slider>().value = magList[i];
        }
    } 

    #endregion


    private void OnDrawGizmos()
    {
        /*
        Gizmos.color = Color.red;
        Vector3 direction = gunBarell.transform.TransformDirection(Vector3.forward) * 200;
        Gizmos.DrawRay(gunBarell.transform.position, direction);

        Gizmos.color = Color.blue;
        Vector3 direction1 = playerMove.transform.GetComponentInChildren<Camera>().gameObject.transform.TransformDirection(Vector3.forward) * 200;
        Gizmos.DrawRay(playerMove.transform.GetComponentInChildren<Camera>().gameObject.transform.position, direction1);
        */
    }
}

