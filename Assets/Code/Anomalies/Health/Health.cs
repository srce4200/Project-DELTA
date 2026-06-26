using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Health : MonoBehaviour
{
    [HideInInspector]
    public float health = 100;
    [SerializeField] ParticleSystem dieEffect;
    float currentHealth;
    PhotonView pv;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = health;
        pv = GetComponent <PhotonView>();
    }

    public void TakeDamage(float damage)
    {
        pv.RPC(nameof(RPC_damage), RpcTarget.All, (double)damage);
    }
    [PunRPC]
    void RPC_damage(double damage)
    {
        currentHealth -= (float)damage;
        if (currentHealth <= 0)
        {
            Die();
        }


        GetComponent<SoundAlert>().SoundAlerted = true;
    }
    void Die()
    {
        //Instantiate(dieEffect, gameObject.transform.position, dieEffect.transform.rotation);
        GetComponent<Ragdoll>().DoRagdoll();

        Invoke("DieDelay", 30);
    }
    void DieDelay()
    {

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(this.gameObject);
    }
}
