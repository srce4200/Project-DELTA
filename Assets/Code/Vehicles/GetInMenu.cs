using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetInMenu : Interactable
{
    [SerializeField] GameObject SelectionMenu;
    GameObject playerA;

    [SerializeField] List<VehicleSeatCtrl> vehicleSeats;
    private void Start()
    {
        SelectionMenu.SetActive(false);
    }
    public override void Interact(GameObject player)
    {
        base.Interact(player);

        playerA = player;

        EnableMenu();
    }
    public void EnterSeat(int vehicleID)
    {
        DisableMenu();
        vehicleSeats[vehicleID].Interact(playerA);
    }
    void EnableMenu()
    {
        SelectionMenu.SetActive(true);
        playerA.GetComponent<ChatControl>().LockControls(true, true);
        playerA.GetComponent<ChatControl>().EnableMouseInput(true);
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void DisableMenu()
    {
        playerA.GetComponent<ChatControl>().LockControls(false, false);
        playerA.GetComponent<ChatControl>().EnableMouseInput(false);
        SelectionMenu.SetActive(false);
    }

}
