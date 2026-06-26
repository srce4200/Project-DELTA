using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Granade_main : MonoBehaviour
{
    [SerializeField] bool OnCollisionExplode = false;

    public float cookOffTime;
    public float damage;
    public float radius;

    [SerializeField] GameObject particle;

    void Start()
    {
        if(!OnCollisionExplode)
            StartCoroutine(CookOff());
        else
            StartCoroutine(DirectCollision());
    }
    IEnumerator DirectCollision()
    {
        yield return new WaitForSeconds(0.1f);

        Collider[] objects = Physics.OverlapSphere(transform.position, 1);
        foreach (Collider h in objects)
        {
            SpawnEffect();
        }
    }
    IEnumerator CookOff()
    {
        yield return new WaitForSeconds(cookOffTime);
        SpawnEffect();        
    }
    
    void SpawnEffect()
    {
        GameObject clone = Instantiate(particle, gameObject.transform.position, gameObject.transform.rotation);

        if (Vector3.Distance(CameraShake.Instance.transform.position, transform.position) < 15)
            CameraShake.Instance.Shake(0.2f, 0.2f); CameraShake.Instance.Shake(0.2f, 0.2f);

        if (PhotonNetwork.IsMasterClient)
        {
            AreaDamageEnemies(clone.transform.position, radius, damage);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void AreaDamageEnemies(Vector3 location, float radius, float damage)
    {
        Collider[] objectsInRange = Physics.OverlapSphere(location, radius);
        foreach (Collider col in objectsInRange)
        {
            Health enemy = col.GetComponent<Health>();
            if (enemy != null)
            {
                // linear falloff of effect
                float proximity = (location - enemy.transform.position).magnitude;
                float effect = 1 - (proximity / radius);

                enemy.TakeDamage(damage * effect);
            }
        }
        Destroy(gameObject);
    } 
}
