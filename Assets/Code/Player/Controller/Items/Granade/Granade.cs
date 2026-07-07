using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Granade : Item
{
    [SerializeField] string nadeName;

    [Header("Ammo&Force managment")]
    public int currentGranadesStored;
    public float throwForce = 15f;

    int currentGranades;

    [Header("Animations")]
    [SerializeField] RuntimeAnimatorController nadeAnimations;

    public override void Start()
    {
        base.Start();
        
        currentGranades = 1;
        currentGranadesStored -= 1;
    }
    public override void OnEnable()
    {
        base.OnEnable();
        inventoryManager.weaponName.text = gameObject.name;
        inventoryManager.ammoDisplayList.gameObject.SetActive(false);
        
        handAnimations.runtimeAnimatorController = nadeAnimations;

        isReloading = false;
        handAnimations.SetBool("isReloading", false);
        handAnimations.SetBool("LowReady", false);

        inventoryManager.amauntText.SetText((currentGranades + currentGranadesStored).ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (PV == null || !PV.IsMine)
            return;
            
        procAim.Aim(false, 8, 1, null);

        Animations_movement();

        if (Input.GetKeyDown(KeyCode.Mouse0) && isReloading == false && currentGranades == 1)
        {
            StartCoroutine(DelayShoot());
        }
    }
    public override void Animations_movement()
    {
        base.Animations_movement();
    }

    [PunRPC]
    void ThrowGranade()
    {        
        //soundSource.PlayOneShot(firearmStats.fireSoundUnder);
        GameObject bult = Instantiate((GameObject)Resources.Load("PhotonPrefabs/Temporary/" + nadeName), transform.position, Quaternion.Euler(0, 0, 0));
        bult.GetComponent<Rigidbody>().AddForce(-transform.forward * throwForce, ForceMode.Impulse);
    }
    IEnumerator DelayShoot()
    {
        handAnimations.SetTrigger("shoot");
        currentGranades = 0;
        yield return new WaitForSeconds(0.5f);
        PV.RPC("ThrowGranade", RpcTarget.All);
        
        inventoryManager.amauntText.SetText((currentGranades + currentGranadesStored).ToString());

        if (currentGranadesStored > 0)
        {
            StartCoroutine(Reload());
        }
    }
    IEnumerator Reload()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
        isReloading = true;
        handAnimations.SetBool("isReloading", true);

        yield return new WaitForSeconds(1);

        handAnimations.SetBool("isReloading", false);

        currentGranades = 1;
        currentGranadesStored -= 1;

        gameObject.GetComponent<Renderer>().enabled = true;

        yield return new WaitForSeconds(0.5f);
        isReloading = false;
    }
    public override void Rearm()
    {
        currentGranades = currentGranadesStored;
        inventoryManager.amauntText.SetText((currentGranades + currentGranadesStored).ToString());
    }
}
