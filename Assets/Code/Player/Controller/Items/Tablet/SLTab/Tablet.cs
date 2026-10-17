using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Pun.UtilityScripts;

public class Tablet : Item
{
    [Header("Animations")]
    public RuntimeAnimatorController animator;
    
    ChatControl chCont;

    bool zoomedIn;

    [Header("F1")]
    [SerializeField] GameObject f1HolderObject;
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
    public override void Start()
    {
        base.Start();
        
        chCont = playerMove.GetComponent<ChatControl>();
        mapInfo = MapInfo.Instance;

        supportMain = mapInfo.GetComponent<SupportsMenu>();
        airdropSupports = supportMain.avaibleSupports;

        foreach (SupportScriptable t in airdropSupports)
        {
            Instantiate(supportUiPrefab, supportsList).GetComponent<supportUi>().Setup(t.supportName, t.supportPrice, t.supportIcon, this);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        
        inventoryManager.weaponName.text = gameObject.name;
        inventoryManager.ammoDisplayList.gameObject.SetActive(false);
    
        handAnimations.runtimeAnimatorController = animator;
        handAnimations.SetBool("isReloading", false);
        handAnimations.SetBool("LowReady", false);
        
        procAim.Aim(false, 8, 0, null);
        zoomedIn = false;
    }
    public override void OnDisable()
    {
        if (PV == null || !PV.IsMine)
            return;
        ShutDown();
    }
    // Update is called once per frame
    void Update()
    {
        if (PV == null || !PV.IsMine)
            return;

        if (f1HolderObject.activeSelf)
        {
            MapCamMovement();
        }

        Animations_movement();
        ZoomIn();
    }

    #region General
    public override void Animations_movement()
    {
        if(zoomedIn)
        {
            handAnimations.SetBool("isWalking", false);
            return;
        }
        
        base.Animations_movement();
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
            procAim.Aim(true, 8, 2, null);
        }
        else
        {
            procAim.Aim(false, 8, 0, null);
        }
        
    }

    #endregion

    #region Functions
    void MapCamMovement()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            MapCamController.Instance.ZoomIn(scrollInput);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            // Read raw screen delta (movement per frame)
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            Vector3 moveDelta = new Vector3(mouseX, 0f, mouseY);

            MapCamController.Instance.MoveCam(moveDelta);
        }
    }

    void StartUp()
    {
        chCont.LockControls(true, true);
        chCont.EnableMouseInput(true);
        RequestMenu_Income();
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
            supportMain.CallSupport(airdropSupports[supportType].supportsPrefab, airdropSupports[supportType].supportPrice, playerMove.transform.position);
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
