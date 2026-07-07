using UnityEngine;
using Photon.Pun;
public class ResupplyBox : Interactable
{
    [SerializeField] bool primaryAmmo;
    [SerializeField] bool secondaryAmmo;
    [SerializeField] int supplieAmaunt;
    
    PhotonView _pv;
    int usage;
    private void Start()
    {
        _pv = GetComponent<PhotonView>();
    }
    public override void Interact(GameObject player)
    {
        base.Interact(player);
        
        Rearm(player);
    }
    void Rearm(GameObject player)
    {
        weponSwitch gunsScript = player.GetComponentInChildren<weponSwitch>();
        if (primaryAmmo)
        {
            Item gunsScriptPrimary = gunsScript.primaryHolder.GetComponentInChildren<Item>();
            if (gunsScriptPrimary != null) 
            {
                gunsScriptPrimary.Rearm();
                usage += 1;
            }
        }
        if (secondaryAmmo)
        {
            Item gunsScriptSecondary = gunsScript.secondaryHolder.GetComponentInChildren<Item>();
            if (gunsScriptSecondary != null) 
            {
                gunsScriptSecondary.Rearm();
                usage += 1; 
            }
        }

        gunsScript.healHolder.GetComponentInChildren<Item>().Rearm();

        _pv.RPC(nameof(UseSupplies), RpcTarget.AllBuffered, usage);
    }

    [PunRPC]
    void UseSupplies(int amaunt)
    {        
        supplieAmaunt -= amaunt;
        usage = 0;
        print(amaunt);
        if(supplieAmaunt <= 0 && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
