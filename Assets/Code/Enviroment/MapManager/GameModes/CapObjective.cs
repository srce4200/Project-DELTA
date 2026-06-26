using Photon.Pun;
using UnityEngine;

public class CapObjective : MonoBehaviour
{
    public float capValueGoal;
    [HideInInspector] public float curCapValue = 0;
    int redPlayers = 0;
    int bluePlayers = 0;
    [HideInInspector] public bool capped;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("ScanForCaping", 5f, 1);
    }


    void ScanForCaping()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Collider[] allOverlappingColliders = Physics.OverlapBox(transform.position, GetComponent<BoxCollider>().size / 2);
        if (allOverlappingColliders.Length > 0)
        {
            foreach (Collider collider in allOverlappingColliders)
            {
                if (collider.transform.tag == "enemy")
                {
                    redPlayers = 1;
                }
                else
                {
                    redPlayers = 0;
                }

                if (collider.transform.tag == "ermacore faction")
                {
                    bluePlayers = 1;
                }
                else
                {
                    bluePlayers = 0;
                }
            }
        }
        if (bluePlayers > redPlayers) 
        {
            curCapValue++;
            if(curCapValue > capValueGoal)
            {
                capped = true;
            }
        }
    }
}
