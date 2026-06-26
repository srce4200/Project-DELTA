using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bodypart : MonoBehaviour
{
    public float damageMultiplier = 1f;
    [SerializeField] Health health;
    public void TakeDamage(float damage)
    {
        if(health == null)
            transform.root.GetComponent<Health>().TakeDamage(damage * damageMultiplier);
        else
            health.TakeDamage(damage * damageMultiplier);
    }
}
