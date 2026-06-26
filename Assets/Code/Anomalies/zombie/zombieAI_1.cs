using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class zombieAI_1 : MonoBehaviourPunCallbacks
{
    /*[SerializeField] scriptableZombie zombieStats;

    public enum AiState { idle, chasing, attacking };
    public AiState aiState = AiState.idle;

    bool isStealth;

    float nextTimeToFire, attackDamage, rateOfFire, distanceToEnemy;

    Transform target;
    string enemyTag = "ermacore faction";
    Animator zombieAnim;
    NavMeshAgent nm;
    AudioSource audioSource;

    #region Abilities
    //red zombie
    SkinnedMeshRenderer rendererZombie;
    public LayerMask enemyMask;
    float abilityTime;
    float currentTime;
    #endregion

    void Awake()
    {
        SetVariables();

        if (!PhotonNetwork.IsMasterClient)
            return;
        
        TeleportToNavMesh(); 
    }

    #region Zombie Setup

    void SetVariables()
    {   
        GetComponent<Health>().health = zombieStats.health;
        rendererZombie = GetComponentInChildren<SkinnedMeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        nm = GetComponent<NavMeshAgent>();
        zombieAnim = GetComponent<Animator>();
        isStealth = zombieStats.allowStealth;
         
        attackDamage = zombieStats.damage;
        rateOfFire = zombieStats.attackSpeed;
        nm.speed = zombieStats.zombieSpeed;
        abilityTime = zombieStats.abilityTimeStrength;
        currentTime = abilityTime;
    }
    void TeleportToNavMesh()
    {
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(transform.position, out myNavHit, 100, -1))
        {
            transform.position = myNavHit.position;
        }
    }

    #endregion

    void Update()
    {
        if (!photonView.IsMine)
            return;

        UpdateTarget();

        if (target == null)
            return;

        ZombieRunner();
    }

    #region Zombie State
    void ZombieRunner()
    {   
        float distanceTo = Vector3.Distance(transform.position, target.position);

        switch (aiState)
        {
            case AiState.idle:
                Idle();

                if (distanceTo < zombieStats.detectDistance)
                {
                    aiState = AiState.chasing;
                }                   
                break;
            ////////////////////
            case AiState.chasing:
                Chasing();
                if (distanceTo < 30)
                        RedZombie(true); 

                if (distanceTo > zombieStats.detectDistance || target == null)
                {
                    aiState = AiState.idle;                                        
                }
                else if (distanceTo < 3)
                {
                    RedZombie(false);
                    aiState = AiState.attacking;
                }                    
                break;
            ////////////////////
            case AiState.attacking:
                Attacking();
                if (distanceTo > 5)
                {
                    aiState = AiState.chasing;  
                }
                        
                break; 
            default:
                break;
        }
    }
    #endregion

    #region Zombie Classes
    void RedZombie(bool abilityActive)
    {   
        if(zombieStats.zombieType == scriptableZombie.Type.redZombie)
        {
            if (0 < currentTime && abilityActive == true)
            {
                rendererZombie.enabled = false;
                currentTime = currentTime - Time.deltaTime;
            }
            else
            {
                if (currentTime < abilityTime)
                    currentTime = currentTime + Time.deltaTime;
                rendererZombie.enabled = true;
            }
        }
    }

    void Screamer()
    {
        isStealth = false;

        if (zombieStats.zombieType == scriptableZombie.Type.screamer)
        { 
            audioSource.PlayOneShot(zombieStats.abilitySound); 
            Collider[] zombies = Physics.OverlapSphere(transform.position, 300f, enemyMask); 
            for (int i = 0; i < zombies.Length && zombies[i] != null; i++)
            { 
                zombieAI_1 zombie = zombies[i].GetComponent<zombieAI_1>(); 
                if (zombie != null)
                {
                    zombie.SoundAlert();
                }
            }
        }
    }

    #endregion

    #region States

    void Idle()
    {
        zombieAnim.SetBool("alerted", false);
        zombieAnim.SetBool("isSprinting", false);
        nm.SetDestination(transform.position);
    }

    #region chasing---
    bool advancedChasing = false;
    void Chasing()
    {
        zombieAnim.SetBool("alerted", true);
        zombieAnim.SetBool("isSprinting", true);

        if(!audioSource.isPlaying && UnityEngine.Random.Range(0f, 2f) < 0.01f)
            audioSource.PlayOneShot(zombieStats.chaseSfx[UnityEngine.Random.Range(0, zombieStats.AttackSfx.Count - 1)]);

        if (advancedChasing)
        {
            // Calculate a point on a Catmull-Rom spline curve
            Vector3 p0 = transform.position;
            Vector3 p1 = target.position;
            Vector3 p2 = target.position + target.right * 5f;
            Vector3 p3 = target.position + target.forward * 10f;
            Vector3 curvePoint = CatmullRomSpline(p0, p1, p2, p3, 0.5f);

            // Set the zombie's destination to the curve point
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 0.1f, NavMesh.AllAreas))
            {
                nm.SetDestination(curvePoint);
            }
        }
        else
        {
            nm.SetDestination(target.position);
        }
    }
    Vector3 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 v0 = (p2 - p0) / 2f;
        Vector3 v1 = (p3 - p1) / 2f;

        Vector3 a = 2f * p1 - 2f * p2 + v0 + v1;
        Vector3 b = -3f * p1 + 3f * p2 - 2f * v0 - v1;
        Vector3 c = v0;
        Vector3 d = p1;

        return a * t3 + b * t2 + c * t + d;
    }
    #endregion

    void Attacking()
    {
        zombieAnim.SetBool("alerted", true);
        zombieAnim.SetBool("isSprinting", false);
        nm.SetDestination(transform.position);
        if (Time.time >= nextTimeToFire)
        {
            zombieAnim.SetTrigger("attack");
            nextTimeToFire = Time.time + 1f / rateOfFire + 0.2f;
            StartCoroutine(DelayDamage());
        }
    }
    IEnumerator DelayDamage()
    {
        yield return new WaitForSeconds(0f);
        audioSource.PlayOneShot(zombieStats.AttackSfx[UnityEngine.Random.Range(0, zombieStats.AttackSfx.Count - 1)]);
        if (target == null)
            yield break;

        if (target.tag == enemyTag)
            target.GetComponent<playerHealth>().pv.RPC("TakeDamage", RpcTarget.All, attackDamage);
        else
            target.GetComponent<DeleteObject>().Delete();
    }
    #endregion

    #region FindTarget
    void UpdateTarget()
    {
        if (isStealth) //handle FOV
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
            }
            if (nearestEnemy != null && shortestDistance <= zombieStats.detectDistance + 50)
            {
                Debug.DrawRay(Vector3.forward, transform.position);

                if (Vector3.Angle(Vector3.forward, transform.InverseTransformPoint(nearestEnemy.transform.position)) < zombieStats.fovAngle / 2f)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, transform.forward, out hit))
                    {
                        if (hit.collider.CompareTag(enemyTag))
                        { 
                            target = nearestEnemy.transform;
                        }
                    }
                }
            }
            else
            {
                target = null;
            }
        }
        else  //handle regular
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            float shortestDistance = Mathf.Infinity;
            GameObject nearestEnemy = null;

            for (int i = 0; i < enemies.Length; i++)
            {
                float distanceToEnemy = Vector3.SqrMagnitude(transform.position - enemies[i].transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemies[i];
                }
            }

            if (nearestEnemy != null && shortestDistance <= (zombieStats.detectDistance + 50) * (zombieStats.detectDistance + 50))
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

    #region Sound

    public void SoundAlert()
    {
        if(isStealth)
            Screamer();

        isStealth = false; 
    }

    #endregion*/


}
