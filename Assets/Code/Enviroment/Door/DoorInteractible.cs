using Unity.AI.Navigation;
using UnityEngine;

public class DoorInteractible : Interactable
{
    bool currentlyOpen = false;
    public override void Interact(GameObject player)
    {
        DoorManager.Instance.ChangeDoorState(this, !currentlyOpen);
    }
    private void Start()
    {
        GetComponent<NavMeshLink>().activated = currentlyOpen;
    }
    public void OpenClose(bool isOpen)
    {
        if(currentlyOpen != isOpen)
        {
            GetComponent<Animator>().SetTrigger("Action");
            currentlyOpen = !currentlyOpen;
            GetComponent<NavMeshLink>().activated = currentlyOpen;
        }
    }
}
