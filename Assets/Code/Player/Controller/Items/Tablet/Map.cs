using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.LowLevel;
using TMPro;

public class Map : Item
{
    [Header("Animations")]
    public RuntimeAnimatorController animator;
    
    bool zoomedIn;
    ChatControl controls;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        controls = transform.root.GetComponent<ChatControl>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        
        inventoryManager.weaponName.text = gameObject.name;
        inventoryManager.ammoDisplayList.gameObject.SetActive(false);
        handAnimations.runtimeAnimatorController = animator;

        procAim.Aim(false, 8, 1, null);
    }
    public override void OnDisable()
    {
        ShutDown();
    }
    // Update is called once per frame
    void Update()
    {
        if (PV ==null || !PV.IsMine)
            return;
        
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
            MapCamMovement();
        }
        else
        {
            procAim.Aim(false, 8, 1, null);
        }
    }

    #endregion

    #region Functions

    void StartUp()
    {
        controls.LockMovement(true);
    }

    void ShutDown()
    {
        controls.LockMovement(false);
    }

    #endregion

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
            Vector3 moveDelta = new Vector3(-mouseX, 0f, -mouseY);

            MapCamController.Instance.MoveCam(moveDelta);
        }
    }
}
