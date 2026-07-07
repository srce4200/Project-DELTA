using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class FAKHeal : Item
{
    public int maxFakStored = 3;
    int currentFakStored;
    
    public float healingSpeed;
    bool isHealing = false;
    public float healAmaunt = 100f;

    playerHealth myplayerHp;

    [Header("Animations")]
    [SerializeField] RuntimeAnimatorController animator;
    
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        myplayerHp = GetComponentInParent<playerHealth>();
        currentFakStored = maxFakStored;
    }
    public override void OnEnable()
    {
        base.OnEnable();
        
        inventoryManager.weaponName.text = gameObject.name;
        inventoryManager.ammoDisplayList.gameObject.SetActive(false);
        handAnimations = inventoryManager.handAnimations;
        handAnimations.runtimeAnimatorController = animator;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (PV == null || !PV.IsMine)
            return;

        //check if you can delete this, already handled in Item.OnEabled
        procAim.Aim(false, 8, 0, null);

        Animations_movement();

        if (Input.GetKeyDown(KeyCode.Mouse0) && myplayerHp.currentHealth < 100 && !isHealing && currentFakStored > 0)
        {
            StartCoroutine(Heal(myplayerHp));
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, 5f))
            {
                if (hit.transform.tag == "ermacore faction")
                {
                    playerHealth otherPlayerHealth = hit.collider.GetComponent<playerHealth>();
                    if (otherPlayerHealth != null && otherPlayerHealth.currentHealth < otherPlayerHealth.health && !isHealing && currentFakStored > 0)
                    {
                        StartCoroutine(Heal(otherPlayerHealth));
                    }
                }
            }
        }
        else
        {
            inventoryManager.amauntText.SetText("<b>" + currentFakStored + "</b>" + "/--");
        }
    }

    IEnumerator Heal(playerHealth playerHp)
    {
        isHealing = true;
        handAnimations.SetBool("isReloading", true);
        yield return new WaitForSeconds(healingSpeed);
        currentFakStored -= 1;
        //try calling pv from here if you see this
        playerHp.pv.RPC("TakeDamage", RpcTarget.All, (double)(-healAmaunt)); // Call the TakeDamage method with a negative value to heal

        handAnimations.SetBool("isReloading", false);
        isHealing = false;
    }

    #region General

    public override void Animations_movement()
    {
        base.Animations_movement();
    }
    public override void Rearm()
    {
        currentFakStored = maxFakStored;
    }
    #endregion
}
