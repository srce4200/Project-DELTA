using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;

public class BuildBox : Interactable
{
    [SerializeField] int buildCredits;
    [SerializeField] GameObject buildMenu;

    ChatControl chatContr;

    [Space]
    [SerializeField] TextMeshProUGUI creditsText;

    PhotonView _pv;
    bool active;
    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        buildMenu.SetActive(false);
        active = false;
    }
    public override void Interact(GameObject player)
    {
        base.Interact(player);
        chatContr = player.GetComponent<ChatControl>();
        OpenMenu();
    }
    void OpenMenu()
    {
        active = true;
        buildMenu.SetActive(true);
        creditsText.SetText(buildCredits + "");
        chatContr.LockControls(false, false);
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseMenu()
    {
        buildMenu.SetActive(false);
        active = false;
        chatContr.LockControls(true, true);
        Cursor.lockState = CursorLockMode.Locked;
    }
    void CloseForBuild()
    {
        buildMenu.SetActive(false);
        active = false;
        chatContr.LockControls(true, false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        if (active)
        {
            creditsText.SetText(buildCredits + "");
        }
    }

    #region SpawnObject
    public void SpawnBarrier(int barrierId)
    {
        if(barrierId == 0 && buildCredits >= 50)
        {
            _pv.RPC(nameof(UpdateCredits), RpcTarget.AllBuffered, 50);
            SpawnObject("PhotonPrefabs/SpawnableObstacles/h-barrier-big-spawnvariant");
        }
        if (barrierId == 1 && buildCredits >= 30)
        {
            _pv.RPC(nameof(UpdateCredits), RpcTarget.AllBuffered, 30);
            SpawnObject("PhotonPrefabs/SpawnableObstacles/h-barrier-small-placableVariant");
        }
        else if (barrierId == 2 && buildCredits >= 30)
        {
            _pv.RPC(nameof(UpdateCredits), RpcTarget.AllBuffered, 30);
            SpawnObject("PhotonPrefabs/SpawnableObstacles/C-barrier-low-placableVariant");
        }
    }
    void SpawnObject(string path)
    {
        CloseForBuild();
        chatContr.GetComponent<BuildController>().enabled = true;
        chatContr.GetComponent<BuildController>().SetObject(path, true);
    }
    #endregion

    public void DeleteBarrier()
    {
        chatContr.GetComponent<BuildController>().enabled = true;
        chatContr.GetComponent<BuildController>().SetObject(null, false);
        _pv.RPC(nameof(UpdateCredits), RpcTarget.AllBuffered, 10);
        CloseForBuild();
    }

    [PunRPC]
    void UpdateCredits(int amaunt)
    {
        buildCredits -= amaunt;
        creditsText.SetText(buildCredits + "");
        if(buildCredits <= 0)
            _pv.RPC(nameof(DestroyObject), RpcTarget.All);
    }

    public void DeleteCrate()
    {
        CloseMenu();
        _pv.RPC(nameof(DestroyObject), RpcTarget.All);
    }
    [PunRPC]
    void DestroyObject()
    {
        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}
