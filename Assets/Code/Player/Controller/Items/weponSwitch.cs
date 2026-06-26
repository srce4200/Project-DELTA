using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class weponSwitch : MonoBehaviour
{
    [Header("Holders")]
    public GameObject primaryHolder;
    public GameObject secondaryHolder;
    public GameObject itemHolder;
    public GameObject healHolder;
    public GameObject throwableHolder;

    [Header("Other")]
    [SerializeField] int selectedWepon = 0;
    [SerializeField] Animator handAnimator;
    PhotonView PVswitch;

    private void Start()
    {
        PVswitch = GetComponent<PhotonView>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!PVswitch.IsMine)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) && selectedWepon != 0)
        {
            PVswitch.RPC(nameof(IndexChange), RpcTarget.All, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && selectedWepon != 1)
        {
            PVswitch.RPC(nameof(IndexChange), RpcTarget.All, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && selectedWepon != 2)
        {
            PVswitch.RPC(nameof(IndexChange), RpcTarget.All, 2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && selectedWepon != 3)
        {
            PVswitch.RPC(nameof(IndexChange), RpcTarget.All, 3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && selectedWepon != 4)
        {
            PVswitch.RPC(nameof(IndexChange), RpcTarget.All, 4);
        }
    }
    [PunRPC]
    void IndexChange(int index)
    {
        if (index == 0)
        {
            selectedWepon = 0;
            StartCoroutine(switchPrimary());
        }
        else if (index == 1)
        {
            selectedWepon = 1;
            StartCoroutine(switchSecondary());
        }
        else if (index == 2)
        {
            selectedWepon = 2;
            StartCoroutine(switchItem());
        }
        else if (index == 3)
        {
            selectedWepon = 3;
            StartCoroutine(switchHeal());
        }
        else if (index == 4)
        {
            selectedWepon = 4;
            StartCoroutine(switchThrowable());
        }
    }
    IEnumerator switchPrimary()
    {
        handAnimator.SetTrigger("weponSwitch");
        yield return new WaitForSeconds(0.5f);
        itemHolder.SetActive(false);
        primaryHolder.SetActive(true);
        secondaryHolder.SetActive(false);
        healHolder.SetActive(false);
        throwableHolder.SetActive(false);
    }
    IEnumerator switchSecondary()
    {
        handAnimator.SetTrigger("weponSwitch");
        yield return new WaitForSeconds(0.5f);
        primaryHolder.SetActive(false);
        itemHolder.SetActive(false);
        secondaryHolder.SetActive(true);
        healHolder.SetActive(false);
        throwableHolder.SetActive(false);
    }
    IEnumerator switchItem()
    {
        handAnimator.SetTrigger("weponSwitch");
        yield return new WaitForSeconds(0.5f);
        itemHolder.SetActive(true);
        primaryHolder.SetActive(false);
        secondaryHolder.SetActive(false);
        healHolder.SetActive(false);
        throwableHolder.SetActive(false);
    }
    IEnumerator switchHeal()
    {
        handAnimator.SetTrigger("weponSwitch");
        yield return new WaitForSeconds(0.5f);
        healHolder.SetActive(true);
        primaryHolder.SetActive(false);
        secondaryHolder.SetActive(false);
        itemHolder.SetActive(false);
        throwableHolder.SetActive(false);
    }
    IEnumerator switchThrowable()
    {
        handAnimator.SetTrigger("weponSwitch");
        yield return new WaitForSeconds(0.5f);
        healHolder.SetActive(false);
        primaryHolder.SetActive(false);
        secondaryHolder.SetActive(false);
        itemHolder.SetActive(false);
        throwableHolder.SetActive(true);
    }
}
