using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Zombie_explode : MonoBehaviourPun
{
    public float damage;
    public float radius;

    void Awake()
    {
        if(PhotonNetwork.IsMasterClient)
            AreaDamageEnemies(transform.position, radius, damage);
    } 
    void AreaDamageEnemies(Vector3 location, float radius, float damage)
    {
        Collider[] objectsInRange = Physics.OverlapSphere(location, radius);
        foreach (Collider col in objectsInRange)
        {
            playerHealth enemy = col.GetComponent<playerHealth>();
            DeleteObject delObj = col.GetComponent<DeleteObject>();

            if (enemy != null)
            {
                // linear falloff of effect
                float proximity = (location - enemy.transform.position).magnitude;
                float effect = 1 - (proximity / radius);

                enemy.TakeDamageNoRPC(damage * effect);
            }
            else if (delObj != null)
            {
                delObj.Delete();
            }
        }
    }
}