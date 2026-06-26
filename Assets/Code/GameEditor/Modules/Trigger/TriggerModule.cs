using Photon.Pun;
using UnityEngine;

public class TriggerModule : MonoBehaviour
{
    public float triggerSize = 10;
    public string triggerer = "ermacore faction";
    public string specificTargetName;

    [HideInInspector] public bool triggered = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateTriggerSize();        
    }

    private void Update()
    {
        /*if (!triggered)
        {
            if (string.co)
            {
                triggered = true;
                //GetComponent<PhotonView>().RPC("SyncTrigger", RpcTarget.AllBuffered, true);
            }
        }*/
    }

    #region setup
    void Setup()
    {
        if (!PhotonNetwork.OfflineMode && PhotonNetwork.IsMasterClient)
        {
            transform.localScale = new Vector3(triggerSize, triggerSize, triggerSize);
        }
    }
    public void UpdateTriggerSize()
    {
        transform.localScale = new Vector3(triggerSize, triggerSize, triggerSize);
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (string.Compare(specificTargetName,"") != 0)
            return;

        if (other.CompareTag(triggerer)) 
        {
            triggered = true;
            //GetComponent<PhotonView>().RPC("SyncTrigger", RpcTarget.AllBuffered, true);
        }
    }
    /*

    [PunRPC]
    private void SyncTrigger(bool state)
    {
        triggered = state;
    }*/
}
