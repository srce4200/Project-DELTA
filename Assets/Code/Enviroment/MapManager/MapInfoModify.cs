using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfoModify : MonoBehaviour
{
    [SerializeField] int playerRespawnTickets;

    private void Awake()
    {
        //transform.root.GetComponent<MapInfo>().TeamTickets = playerRespawnTickets;
    }
}
