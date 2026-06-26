using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Granade : MonoBehaviour
{
    [SerializeField] string nadeName;

    [Header("Ammo&Force managment")]
    public int currentGranadesStored;
    public float throwForce = 15f;

    int currentGranades;

    bool isReloading = false;
    InventoryManager inventoryManager;

    AudioSource soundSource;


    PlayerMovement playerMove;
    Camera mainCam;

    [Header("Animations")]
    [SerializeField] RuntimeAnimatorController nadeAnimations;
    Animator handAnimations;
    bool isSprinting;

    PhotonView PV;
    void Start()
    {
        currentGranades = 1;
        currentGranadesStored -= 1;
        PV = GetComponent<PhotonView>();
        playerMove = GetComponentInParent<PlayerMovement>();
        mainCam = playerMove.transform.GetComponentInChildren<Camera>();
        soundSource = GetComponent<AudioSource>();
    }
    void OnEnable()
    {
        inventoryManager = transform.root.GetComponent<InventoryManager>();
        inventoryManager.weaponName.text = gameObject.name;
        inventoryManager.ammoDisplayList.gameObject.SetActive(false);
        handAnimations = inventoryManager.handAnimations;
        handAnimations.runtimeAnimatorController = nadeAnimations;

        isReloading = false;
        handAnimations.SetBool("isReloading", false);
        handAnimations.SetBool("LowReady", false);

        inventoryManager.amauntText.SetText((currentGranades + currentGranadesStored).ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine)
            return;
        transform.root.GetComponent<ProceduralAim>().Aim(false, 8, 0, null);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) && isReloading == false)
        {
            handAnimations.SetBool("isWalking", true);
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            if (playerMove.currentStamina > 0.2f)
            {
                handAnimations.SetBool("isSprinting", isSprinting);
            }
            else
            {
                handAnimations.SetBool("isSprinting", false);
            }
        }
        else
            handAnimations.SetBool("isWalking", false);

        if (Input.GetKeyDown(KeyCode.Mouse0) && isReloading == false && currentGranades == 1)
        {
            StartCoroutine(DelayShoot());
        }
    }

    [PunRPC]
    void ThrowGranade()
    {        
        //soundSource.PlayOneShot(firearmStats.fireSoundUnder);
        GameObject bult = Instantiate((GameObject)Resources.Load("PhotonPrefabs/Temporary/" + nadeName), transform.position, Quaternion.Euler(0, 0, 0));
        bult.GetComponent<Rigidbody>().AddForce(-transform.forward * throwForce, ForceMode.Impulse);
    }
    IEnumerator DelayShoot()
    {
        handAnimations.SetTrigger("shoot");
        currentGranades = 0;
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponent<PhotonView>().RPC("ThrowGranade", RpcTarget.All);
        
        inventoryManager.amauntText.SetText((currentGranades + currentGranadesStored).ToString());

        if (currentGranadesStored > 0)
        {
            StartCoroutine(Reload());
        }
    }
    IEnumerator Reload()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
        isReloading = true;
        handAnimations.SetBool("isReloading", true);

        yield return new WaitForSeconds(1);

        handAnimations.SetBool("isReloading", false);

        currentGranades = 1;
        currentGranadesStored -= 1;

        gameObject.GetComponent<Renderer>().enabled = true;

        yield return new WaitForSeconds(0.5f);
        isReloading = false;
    }
}
