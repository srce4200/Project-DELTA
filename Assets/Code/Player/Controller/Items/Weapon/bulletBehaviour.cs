using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class bulletBehaviour : MonoBehaviour
{
    public float bulletSpeed;
    public float bulletDamage;
    public bool targetPlayer;
    [Header("is Explosive?")]
    public float radius;
    public bool explosiveDamage;
    public bool rigidbodyMovement = true;

    Vector3 prevPos;

    [SerializeField] GameObject hitEffectFlesh;
    [SerializeField] GameObject hitEffectEnviroment;

    PhotonView pv;
    private void Start()
    {
        pv = GetComponent<PhotonView>();
        prevPos = transform.position;
    }
    private void Update()
    {
        if(!rigidbodyMovement)
            transform.Translate(0f, 0f, bulletSpeed * Time.deltaTime);

        RaycastHit[] hits = Physics.RaycastAll(prevPos, (transform.position - prevPos).normalized, (transform.position - prevPos).magnitude);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.tag == "enemy" && !targetPlayer)
            {
                TakeDamage(hits[i].collider.gameObject.GetComponent<Bodypart>(), bulletDamage);
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, hits[i].normal);
                SpawnEffect(hitEffectFlesh, hits[i].point, rot);
            }
            if (hits[i].collider.gameObject.tag == "ermacore faction" && targetPlayer)
            {
                hits[i].collider.gameObject.GetComponent<playerHealth>().TakeDamage(bulletDamage / 2);

                Quaternion rot = Quaternion.FromToRotation(Vector3.up, hits[i].normal);
                SpawnEffect(hitEffectFlesh, hits[i].point, rot);
            }
            else
            {
                Quaternion rot = Quaternion.FromToRotation(Vector3.forward, hits[i].normal);
                SpawnEffect(hitEffectEnviroment, hits[i].point, rot);
            }
        }
    }
    void SpawnEffect(GameObject effect, Vector3 pos, Quaternion rotation)
    {
        if (explosiveDamage == true)
        { 
            if(Vector3.Distance(CameraShake.Instance.transform.position, transform.position) < 15)
                CameraShake.Instance.Shake(0.2f, 0.2f);

            GameObject clone = Instantiate(effect, pos, rotation);
            AreaDamageEnemies(clone.transform.position, radius, bulletDamage);
        }
        else if (explosiveDamage == false)
        {
            GameObject clone = Instantiate(effect, pos, rotation);
            Destroy(gameObject);
        }
    }

    void AreaDamageEnemies(Vector3 location, float radius, float damage)
    {
        Collider[] objectsInRange = Physics.OverlapSphere(location, radius);
        foreach (Collider col in objectsInRange)
        {
            Health enemy = col.transform.root.GetComponent<Health>();
            if (enemy != null)
            {
                // linear falloff of effect
                float proximity = (location - enemy.transform.position).magnitude;
                float effect = 1 - (proximity / radius);

                print(damage * effect);
                enemy.TakeDamage(damage * effect);
            }
        }
        Destroy(gameObject);
    }
    void TakeDamage(Bodypart bodypa, float damage)
    {
        if (pv.IsMine) 
        { 
            bodypa.TakeDamage(damage);
        }
    }
}