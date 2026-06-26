using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.AI;

public class HivesPopulate : MonoBehaviour
{
    [SerializeField] GameObject[] objectivesPossibleLocations;
    public enum Difficulity { Easy, Medium, Hard};
    public Difficulity selectedDifficulity;
    CapObjective[] objectivePoints = { null, null, null, null, null, null };

    
    PhotonView _pv;
    int curObjective = 0;
    int curObjPos = 0;
    bool spawnedDefenseCounterAttack;
    Transform spawnPos;
    void Start()
    {
        _pv = GetComponent<PhotonView>();
        Setup();
        if (!PhotonNetwork.IsMasterClient)
            return;
        SpawnGuardingHorde();
    }
    void Setup()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            int num = (int)(UnityEngine.Random.Range(0, objectivesPossibleLocations.Length));
            _pv.RPC("SetupObjectives", RpcTarget.AllBuffered, num);
        }
    }
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (objectivePoints[curObjective].capped)
        {
            SpawnDistancedHorde(50, 75);
            ObjectiveCaptured();
            spawnedDefenseCounterAttack = false;
            _pv.RPC("NextAO", RpcTarget.AllBuffered);
        }
        else if (objectivePoints[curObjective].curCapValue != 0 && !spawnedDefenseCounterAttack)
        {
            SpawnDistancedHorde(50, 75);
            spawnedDefenseCounterAttack = true;
        }
    }
    #region objectives
    [PunRPC]
    void NextAO()
    {
            GameUIManager.Instance.QueueNotification(0, "Proceed to next objective");
            GameUIManager.Instance.QueueNotification(0, "Objective captured");
            spawnPos.position = objectivePoints[curObjective].transform.position;
    }
    [PunRPC]
    void SetupObjectives(int selectedLocation)
    {
        objectivesPossibleLocations[selectedLocation].transform.parent.gameObject.SetActive(true);
        curObjPos = selectedLocation;
        for(int i = 0; i < 5; i++) 
        {
            objectivePoints[i] = objectivesPossibleLocations[selectedLocation].transform.GetChild(i).GetComponent<CapObjective>();
        }
        spawnPos = objectivesPossibleLocations[selectedLocation].transform.parent.GetChild(0).transform;
    }
    void ObjectiveCaptured()
    {
        curObjective++;
        SpawnGuardingHorde();
        if (curObjective > 3)
        {
            print("GG");
        }
    }
    #endregion

    #region spawnHorde
    void SpawnGuardingHorde()
    {
        for (int i = 0; i < ZombiesNumber(); i++) 
        {
            PhotonNetwork.InstantiateRoomObject("PhotonPrefabs/Anomalies/Zombie/Zombie_Default", objectivePoints[curObjective].transform.position, Quaternion.identity);
            if (i%4==0)
            {
                PhotonNetwork.InstantiateRoomObject("PhotonPrefabs/Anomalies/Demons/Demon", objectivePoints[curObjective].transform.position, Quaternion.identity);
            }
        }

        SpawnDistancedHorde(20, 30);
    }
    void SpawnDistancedHorde(int minDistance, int maxDistance)
    {
        for (int i = 0; i < ZombiesNumber(); i++)
        {
            Vector3 randomPosition = GetRandomPositionAround(objectivePoints[curObjective].transform.position, minDistance, maxDistance);
            Vector3 navMeshPosition = GetClosestPointOnNavMesh(randomPosition);
            if (navMeshPosition != Vector3.zero)
            {
                PhotonNetwork.InstantiateRoomObject("PhotonPrefabs/Anomalies/Zombie/Zombie_Default", navMeshPosition, Quaternion.identity);
                if (i % 4 == 0)
                {
                    PhotonNetwork.InstantiateRoomObject("PhotonPrefabs/Anomalies/Demons/Demon", navMeshPosition, Quaternion.identity);
                }
            }
        }
    }
    #region Extras
    int ZombiesNumber()
    {
        return 3 * ((int)selectedDifficulity + 1) * PhotonNetwork.PlayerList.Length;
    }
    private Vector3 GetRandomPositionAround(Vector3 center, float minDistance, float maxDistance)
    {
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);
        return center + new Vector3(randomDirection.x, 0, randomDirection.y) * randomDistance;
    }

    private Vector3 GetClosestPointOnNavMesh(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 150f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return Vector3.zero;
    }
    #endregion
    #endregion
}
