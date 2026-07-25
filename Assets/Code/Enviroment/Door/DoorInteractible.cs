using UnityEngine;

public class DoorInteractible : Interactable
{
    Animator anim;
    bool currentlyOpen = false;
    public override void Interact(GameObject player)
    {
        base.Interact(player);
    }
    public void OpenClose(bool isOpen)
    {
        if(currentlyOpen != isOpen)
        {
            anim.SetTrigger("Action");
            currentlyOpen = !currentlyOpen;
        }
    }
}
