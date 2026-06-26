using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChatControl : MonoBehaviour
{
    //[SerializeField] weponSwitch weponSwitch;

    [SerializeField] weponScript1[] wepons = new weponScript1[2];
    [SerializeField] OnOffLight[] lights;

    ChatManager chatManager;
    nightVision nvg;
    PlayerMovement movement;
    PhotonView _PV;
    [SerializeField] WeaponSway wpSway;
    weponSwitch wpSwitch;

    bool disableMovement;
    bool disableLook;
    bool currentlyChating;
    // Start is called before the first frame update
    void Start()
    {
        _PV = GetComponent<PhotonView>();
        if (!_PV.IsMine)
            return;
        Setup();
        currentlyChating = chatManager.chatInput.isFocused;
        LockControls(false, false);
    }

    void Setup()
    {
        movement = GetComponent<PlayerMovement>();
        chatManager = MapInfo.Instance.gameObject.GetComponentInChildren<ChatManager>();
        wpSwitch = GetComponentInChildren<weponSwitch>();
        wepons = GetComponentsInChildren<weponScript1>();
        nvg = GetComponentInChildren<nightVision>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_PV.IsMine)
            return;
        if(currentlyChating != chatManager.chatInput.isFocused)
        {
            currentlyChating = chatManager.chatInput.isFocused; 
            ControlsCheck();
        }
       
    }
    public void LockControls(bool disMovement, bool disMouseControl)
    {
        disableLook = disMouseControl;
        disableMovement = disMovement;
        ControlsCheck();
    }
    public void LockMovement(bool movement)
    {
        disableMovement = movement;
        ControlsCheck();
    }
    public void LockMouseMovement(bool mouseControl)
    {
        disableLook = mouseControl;
        ControlsCheck();
    }
    public void EnableMouseInput(bool mouseInput)
    {
        if (mouseInput)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    void ControlsCheck()
    {
        if (currentlyChating || disableLook)
        {
            DisableAllMouseControls();
        }
        else
        {
            EnableAllMouseControls();
        }

        if(currentlyChating || disableMovement)
        {
            movement.enabled = false;
            wpSway.enabled = false;
        }
        else
        {
            movement.enabled = true;
            wpSway.enabled = true;
        }

    }
    void DisableAllMouseControls()
    {
        movement.enabled = false;
        DisableNVG(false);
        wpSwitch.enabled = false;
        foreach (weponScript1 script in wepons)
        {
            script.enabled = false;
        }
    }

    void EnableAllMouseControls()
    {
        movement.enabled = true;
        DisableNVG(true);
        wpSwitch.enabled = true;
        foreach (weponScript1 script in wepons)
        {
            script.enabled = true;
        }
    }
    void DisableNVG(bool boolean)
    {
        if(nvg != null)
            nvg.enabled = boolean;
    }
    public bool AnyMovementLocked()
    {
        return disableLook || disableMovement;
    }
}
