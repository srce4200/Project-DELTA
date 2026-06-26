using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.LowLevel;
using TMPro;

public class Map : MonoBehaviour
{
    InventoryManager inventoryManager;

    [Header("Zoom In")]
    [SerializeField] float zoomRatio;
    float startFov;
    float maxFov;
    [SerializeField] float lerpTime;
    Camera mainCam;

    PlayerMovement playerLook;

    [Header("Animations")]
    public RuntimeAnimatorController animator;
    Animator handAnimations;
    bool zoomedIn;
    bool isSprinting;

    PhotonView PV;
    ChatControl controls;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        playerLook = GetComponentInParent<PlayerMovement>();
        mainCam = playerLook.transform.GetComponentInChildren<Camera>();
        controls = transform.root.GetComponent<ChatControl>();

        startFov = 80;
        maxFov = startFov / zoomRatio;
    }

    void OnEnable()
    {
        inventoryManager = transform.root.GetComponent<InventoryManager>();
        inventoryManager.weaponName.text = gameObject.name;
        inventoryManager.ammoDisplayList.gameObject.SetActive(false);
        handAnimations = inventoryManager.handAnimations;
        handAnimations.runtimeAnimatorController = animator;

        handAnimations.SetBool("isReloading", false);
        handAnimations.SetBool("LowReady", false);
        transform.root.GetComponent<ProceduralAim>().Aim(false, 8, 0, null);
    }
    private void OnDisable()
    {
        ShutDown();
    }
    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine)
            return;
        
        transform.root.GetComponent<ProceduralAim>().Aim(false, 8, 0, null);
        HandleAnimations();
        ZoomIn();
    }

    #region General

    void HandleAnimations()
    {
        if(zoomedIn == true)
            handAnimations.SetBool("isWalking", false);
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
        {
            handAnimations.SetBool("isWalking", true);
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            handAnimations.SetBool("isSprinting", isSprinting);
        }
    }

    void ZoomIn()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            zoomedIn = !zoomedIn;
            if (zoomedIn)
            {
                StartUp();
            }
            else
            {
                ShutDown();
            }
        }
        if (zoomedIn)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, maxFov, lerpTime);
            controls.LockMovement(true);
        }
        else
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, startFov, lerpTime);
            controls.LockMovement(true);
        }
    }

    #endregion

    #region Functions

    void StartUp()
    {
        controls.LockMovement(true);
        mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, maxFov, lerpTime);
    }

    void ShutDown()
    {
        controls.LockMovement(false);
    }

    #endregion
}
