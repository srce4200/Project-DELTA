using Photon.Pun;
using System.Collections;
using UnityEngine;

public class TriggerBase : MonoBehaviour
{
    public int syncToId = -1;
    [HideInInspector] public TriggerModule triggerModule;
    public void TriggerCont()
    {
        if (syncToId != -1)
        {
            ObjectModuleInfoHolder[] trgMo = transform.parent.GetComponentsInChildren<ObjectModuleInfoHolder>();
            foreach (ObjectModuleInfoHolder info in trgMo)
            {
                if (info.objectID == syncToId)
                {
                    triggerModule = info.GetComponent<TriggerModule>();
                    if (PhotonNetwork.OfflineMode || !PhotonNetwork.IsMasterClient)
                        SyncToGizmo();
                    break;
                }
            }
        }
    }
    public IEnumerator CheckTrigger()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            //Debug.Log("UNITSPAWN: waiting " + triggerModule.triggered);
            if (triggerModule.triggered)
            {
                ExecuteCommand();
                break; // Exit the loop once triggered
            }
        }
    }
    void SyncToGizmo()
    {
        GetComponent<CustomObjectSettings>().DisplaySyncTo(transform, triggerModule.transform);
    }

    public virtual void ExecuteCommand()
    {

    }
}
