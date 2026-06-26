using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Pun.UtilityScripts;

public class Tablet : MonoBehaviour
{
    [Header("Zoom In")]
    [SerializeField] float zoomRatio;
    float startFov;
    float maxFov;
    [SerializeField] float lerpTime;

    [Header("Animations")]
    public RuntimeAnimatorController animator;
    
    
    Camera mainCam;
    ChatControl chCont;
    PlayerMovement playerLook;
    Animator handAnimations;
    InventoryManager inventoryManager;
    PhotonView PV;

    bool zoomedIn;
    bool isSprinting;


    [Header("F1")]
    [SerializeField] TextMeshProUGUI mapNameText;

    [Header("F2")]
    [SerializeField] Transform taskList;
    [SerializeField] GameObject taskPrefab;

    [Header("F3")]
    [SerializeField] TextMeshProUGUI incomeText;
    [SerializeField] GameObject supportUiPrefab;
    [SerializeField] Transform supportsList;

    List<SupportScriptable> airdropSupports = new List<SupportScriptable>();

    [SerializeField] GameObject CoolDownUi;

    MapInfo mapInfo;
    SupportsMenu supportMain;

    // Start is called before the first frame update
    void Start()
    {
        playerLook = GetComponentInParent<PlayerMovement>();
        mainCam = playerLook.transform.GetComponentInChildren<Camera>();
        chCont = playerLook.GetComponent<ChatControl>();
        mapInfo = MapInfo.Instance;

        supportMain = mapInfo.GetComponent<SupportsMenu>();
        airdropSupports = supportMain.avaibleSupports;
        Setup();

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

        if(PV == null) //need get pv before start, otherwise error mp?
            PV = GetComponent<PhotonView>();
        if(!PV.IsMine)
            return;

        transform.root.GetComponent<ProceduralAim>().Aim(false, 8, 0, null);
        zoomedIn = false;
    }
    void OnDisable()
    {
        if (!PV.IsMine)
            return;
        ShutDown();
    }
    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine)
            return;

        HandleAnimations();
        ZoomIn();
    }

    #region General

    void HandleAnimations()
    {
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
        {
            handAnimations.SetBool("isWalking", true);
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            handAnimations.SetBool("isSprinting", isSprinting);
        }
        else
            handAnimations.SetBool("isWalking", false);

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
            transform.root.GetComponent<ProceduralAim>().Aim(true, 8, 2, null);
        }
        else
        {
            transform.root.GetComponent<ProceduralAim>().Aim(false, 8, 0, null);
        }
        
    }

    #endregion

    #region Functions

    void StartUp()
    {
        chCont.LockControls(true, true);
        chCont.EnableMouseInput(true);
        RequestMenu_Income();
    }
    void Setup()
    {
        foreach (SupportScriptable t in airdropSupports) 
        { 
            Instantiate(supportUiPrefab, supportsList).GetComponent<supportUi>().Setup(t.supportName, t.supportPrice, t.supportIcon, this);
        }
    }

    #region TaskMenu-F2

    public void RefreshTasks()
    {
        foreach (Transform task in taskList.transform)
        {
            Destroy(task.gameObject);  
        }

        foreach (Task task in mapInfo.tasks)
        {
            GameObject prefab = Instantiate(taskPrefab, taskList.transform);
            prefab.GetComponent<listTaskItem>().FixDescription(task.taskName, task.taskDescription, task.taskIcon, task.position);
        }
    }

    #endregion

    #region SquadMenu-F3

    void RequestMenu_Income()
    {
        incomeText.SetText(supportMain.currentCp + "CP");
    }
    public void RequestMenu_Support(int supportType)
    {
        StartCoroutine(CoolDown());
        if(supportMain.currentCp >= airdropSupports[supportType].supportPrice)
        {
            supportMain.CallSupport(airdropSupports[supportType].supportsPrefab, airdropSupports[supportType].supportPrice, playerLook.transform.position);
        }        
    }
    IEnumerator CoolDown()
    {
        CoolDownUi.SetActive(true);
        yield return new WaitForSeconds(10f);
        CoolDownUi.SetActive(false);
    }

    #endregion

    void ShutDown()
    {
        CoolDownUi.SetActive(false);
        chCont.LockControls(false, false);
        chCont.EnableMouseInput(false);
    }

    #endregion

}
