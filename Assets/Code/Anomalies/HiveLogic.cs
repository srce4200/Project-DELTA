using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static UnityEngine.GraphicsBuffer;

public class HiveLogic : MonoBehaviour
{
    [SerializeField] string pathToObject;
    [SerializeField] Transform spawnPosition;
    [SerializeField] float delay = 20f;
    [SerializeField] float activeDistance;
    Transform target;
    string enemyTag = "ermacore faction";
    float distanceToEnemy;

    float resetTimer;
    private void Start()
    {
        resetTimer = delay;
        if (PhotonNetwork.IsMasterClient)
            InvokeRepeating(nameof(UpdateTarget), 5, 2);
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if(distanceToEnemy < activeDistance)
                Spawn();
        }
    }
    void Spawn()
    {
        if(0 >= delay)
        {            
            PhotonNetwork.InstantiateRoomObject(pathToObject, spawnPosition.position, spawnPosition.rotation);
            delay = resetTimer;
        }
        else
        {
            delay -= Time.deltaTime;
        }
    }

    #region FindTarget
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
            if (nearestEnemy != null && shortestDistance <= activeDistance + 50)
            {
                target = nearestEnemy.transform;
            }
            else
            {
                target = null;
            }
        }
    }
    #endregion

}
