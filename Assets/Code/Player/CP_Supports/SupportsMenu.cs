using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SupportsMenu : MonoBehaviour
{
    [SerializeField] float incomeRate;
    [SerializeField] int startingCp;
    public int currentCp;

    public List<CPincome> incomePoints = new List<CPincome>();

    public List<SupportScriptable> avaibleSupports = new List<SupportScriptable>();

    MapInfo mapInfo;
    PhotonView pv;
    // Start is called before the first frame update
    void Start()
    {
        mapInfo = MapInfo.Instance;
        pv = GetComponent<PhotonView>();
        currentCp = startingCp;
        if(incomePoints.Count > 0)
            InvokeRepeating(nameof(RunEveryMinute), 5f, incomeRate);
    }
    void RunEveryMinute()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC(nameof(GetIncome), RpcTarget.All);
        }
    }

    public void CallSupport(GameObject supportPrefab, int cpPrice, Vector3 coordinates)
    {
        Vector3 xyCords = new Vector3(coordinates.x, coordinates.y + 100, coordinates.z);
        print(xyCords);
        pv.RPC(nameof(CallSupportSpawn), RpcTarget.All, supportPrefab.name, xyCords, supportPrefab.transform.rotation);

        pv.RPC(nameof(UpdateIncome), RpcTarget.All, currentCp - cpPrice);
        print("Support requested: " + supportPrefab.name);
    }
    [PunRPC]
    void CallSupportSpawn(string prefabPath, Vector3 Cords, Quaternion objectRotation)
    {
        PhotonNetwork.InstantiateRoomObject(MapInfo.FolderFactionPath + prefabPath, Cords, objectRotation);
    }

    [PunRPC]
    void UpdateIncome(int amaunt)
    {
        currentCp = amaunt;
    }

    [PunRPC]
    void GetIncome()
    {
        for(int i = 0; i < incomePoints.Count; i++)
        {
            if (incomePoints[i].isCaptured)
            {
                currentCp += incomePoints[i].cpIncomeAmaunt;
            }            
        }
    }
}
