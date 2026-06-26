using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class FAKHeal : MonoBehaviour
{
    public int maxFakStored = 3;
    [HideInInspector] public int currentFakStored;
    public float healingSpeed;
    bool isHealing = false;
    public float healAmaunt = 100f;

    InventoryManager inventoryManager;
    bool isSprinting;

    playerHealth myplayerHp;
    Camera cam;

    [Header("Animations")]
    [SerializeField] RuntimeAnimatorController animator;
    Animator handAnimations;

    PhotonView PV;
    // Start is called before the first frame update
    void Start()
    {
        myplayerHp = GetComponentInParent<playerHealth>();
        cam = handAnimations.GetComponentInChildren<Camera>();
        currentFakStored = maxFakStored;
        PV = GetComponent<PhotonView>();
    }
    void OnEnable()
    {
        inventoryManager = transform.root.GetComponent<InventoryManager>();
        inventoryManager.weaponName.text = gameObject.name;
        inventoryManager.ammoDisplayList.gameObject.SetActive(false);
        handAnimations = inventoryManager.handAnimations;
        handAnimations.runtimeAnimatorController = animator;

        handAnimations.SetBool("isReloading", false);
        handAnimations.SetBool("LowReady", false);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine)
            return;

        transform.root.GetComponent<ProceduralAim>().Aim(false, 8, 0, null);

        HandleAnimations();

        if (Input.GetKeyDown(KeyCode.Mouse0) && myplayerHp.currentHealth < 100 && !isHealing && currentFakStored > 0)
        {
            StartCoroutine(Heal(myplayerHp));
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 5f))
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

        playerHp.pv.RPC("TakeDamage", RpcTarget.All, (double)(-healAmaunt)); // Call the TakeDamage method with a negative value to heal

        handAnimations.SetBool("isReloading", false);
        isHealing = false;
    }

    #region General

    void HandleAnimations()
    { 
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
        {
            handAnimations.SetBool("isWalking", true);
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            handAnimations.SetBool("isSprinting", isSprinting);
        }
        else
            handAnimations.SetBool("isWalking", false);

    } 
    #endregion
}
