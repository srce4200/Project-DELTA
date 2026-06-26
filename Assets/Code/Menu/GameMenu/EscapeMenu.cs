using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class EscapeMenu : MonoBehaviour
{
    [SerializeField] GameObject escapeMenu;
    ChatControl chatController;
    bool active;
    bool dontChangeNext = false;
    private void Start()
    {
        chatController = GetComponentInParent<ChatControl>();
        escapeMenu.SetActive(active);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            active = !active;
            UpdateMenuState();
        }
    }
    void UpdateMenuState()
    {
        escapeMenu.SetActive(active);
        if (active)
        {
            dontChangeNext = chatController.AnyMovementLocked();
        }
        if (dontChangeNext)
        {
            if (!active)
                dontChangeNext = false;
            return;
        }

        if (active)
        {
            chatController.LockControls(true, true);
            chatController.EnableMouseInput(true);
            //Cursor.visible = true;
        }
        else
        {
            chatController.LockControls(false, false);
            chatController.EnableMouseInput(false);
            //Cursor.visible = false;
        } 

    }
    public void DisconnectAndLoadMainMenu()
    {
        RoomManager.Instance.LeaveRoomNow();
    }
}
