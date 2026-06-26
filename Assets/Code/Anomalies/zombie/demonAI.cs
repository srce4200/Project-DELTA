using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class demonAI : MonoBehaviour
{
    
    public ScriptableDemon demonStats;

    string enemyTag = "ermacore faction";
    public enum AiState { idle, patroling, chasing, attacking, fleeing, stalking };
    public AiState aiState = AiState.idle;

    float distanceToEnemy, nextTimeToFire;
    Transform target;
    bool fleeing; // Start is called before the first frame update
    Health demonHealth;
    SoundAlert soundAlert;
    Animator zombieAnim;
    NavMeshAgent nm;
    AudioSource audioSource;
    void Start()
    {
        nm = GetComponent<NavMeshAgent>();
        zombieAnim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        soundAlert = GetComponent<SoundAlert>();
        demonHealth = GetComponent<Health>();
        SetValues();

        if (!PhotonNetwork.IsMasterClient)
            return;

        InvokeRepeating(nameof(UpdateDemon), 2, 0.3f);        
    }

    void SetValues()
    {
        nm.speed = demonStats.speed;
        demonHealth.health = demonStats.health;
    }
    void UpdateDemon()
    {
        if (soundAlert.SoundAlerted)
            Alerted();
        else
            UpdateTarget();
    }
    // Update is called once per frame
    void Update()
    {
        switch (aiState)
        {
            case AiState.idle:
                if(target == null)
                {
                    if(UnityEngine.Random.Range(0, 100) < 40)
                    {
                        aiState = AiState.patroling;
                    }
                    else
                    {
                        Idle();
                    }
                }
                else
                {
                    aiState = AiState.stalking;
                }
                break;
            case AiState.patroling:
                bool waypoitnDone = Patrol();
                if (waypoitnDone)
                {
                    aiState = AiState.idle; //switch back after patrol point done
                }
                break;
            case AiState.stalking:
                if (target == null) //hard reset
                    aiState = AiState.idle;
                if(demonHealth.health < demonStats.health / 2)
                {
                    aiState = AiState.fleeing;
                }

                if (distanceToEnemy < (demonStats.detectDistance / 2))
                {
                    aiState = AiState.chasing;
                }
                else
                {
                    Stalk();
                }
                break;
            case AiState.chasing:
                if(target == null) //hard reset
                    aiState = AiState.idle;

                if (distanceToEnemy <= demonStats.attackDistance)
                {
                    aiState = AiState.attacking;
                }
                else if (distanceToEnemy > (demonStats.detectDistance / 2))
                {
                    aiState = AiState.stalking;
                }
                else
                {
                    Chase();
                }
                break;
            case AiState.attacking:
                if (target == null)
                {//hard reset
                    aiState = AiState.idle;
                }

                if (distanceToEnemy > demonStats.attackDistance)
                {
                    aiState = AiState.chasing;
                }
                else
                {
                    Attack();
                }
                break;
            case AiState.fleeing:
                if (Flee())
                {
                    aiState = AiState.idle;
                }
                break;
            default:
                break;
        }
    }

    #region Idle
    void Idle()
    {
        zombieAnim.SetBool("walking", false);
        zombieAnim.SetBool("alerted", false);
        zombieAnim.SetBool("chasing", false);
        nm.SetDestination(transform.position);
    }
    #endregion

    #region Patrol
    bool Patrol()
    {
        nm.speed = demonStats.speed * 0.5f;
        zombieAnim.SetBool("walking", true);
        zombieAnim.SetBool("alerted", false);
        zombieAnim.SetBool("chasing", false);

        if (nm.remainingDistance < 0.5f)
        {
            nm.SetDestination(GetPatrolPoint());
            return true;
        }

        return false;
    }
    Vector3 GetPatrolPoint()
    {
        float patrolRadius = 50f;
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position; 

        // Ensure the point is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            return hit.position; // Return the valid point on the NavMesh
        }

        // Fallback in case a point cannot be found
        return transform.position;
    }
    #endregion

    #region Stalk
    void Stalk()
    {
        nm.speed = demonStats.speed * 0.75f;
        zombieAnim.SetBool("walking", true);
        zombieAnim.SetBool("alerted", true);
        zombieAnim.SetBool("chasing", false);

        if (nm.remainingDistance < 0.5f)
        {
            nm.SetDestination(GetStalkPoint());
        }
    }
    Vector3 GetStalkPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * demonStats.stalkDistance;
        randomDirection += target.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, demonStats.stalkDistance, NavMesh.AllAreas))
        {
            return hit.position; 
        }

        return transform.position;
    }
    #endregion

    #region Chase
    void Chase()
    {
        nm.speed = demonStats.speed;
        zombieAnim.SetBool("walking", true);
        zombieAnim.SetBool("alerted", true);
        zombieAnim.SetBool("chasing", true);
        Destination(target.position);
    }
    #endregion

    #region Attack
    void Attack() 
    {
        zombieAnim.SetBool("walking", false);
        zombieAnim.SetBool("alerted", true);
        zombieAnim.SetBool("chasing", true);
        nm.SetDestination(transform.position);
        if (Time.time >= nextTimeToFire)
        {
            zombieAnim.SetTrigger("Attack");
            nextTimeToFire = Time.time + 1f / demonStats.attackSpeed + 0.2f;
            StartCoroutine(DelayDamage());
        }
    }
    IEnumerator DelayDamage()
    {
        yield return new WaitForSeconds(0f);
        audioSource.PlayOneShot(demonStats.attackSfx[UnityEngine.Random.Range(0, demonStats.attackSfx.Count - 1)]);
        if (target == null)
            yield break;

        if (target.tag == enemyTag)
            target.GetComponent<playerHealth>().pv.RPC("TakeDamage", RpcTarget.All, (double)demonStats.damage);
        else
            target.GetComponent<DeleteObject>().Delete();
    }
    #endregion

    #region Fleeing
    bool Flee()
    {
        zombieAnim.SetBool("walking", true);
        zombieAnim.SetBool("alerted", true);
        zombieAnim.SetBool("chasing", false);

        if (!fleeing) 
        { 
            nm.SetDestination(GetFleePoint());
            fleeing = true;
        }
        else if (nm.remainingDistance < 0.5f)
        {
            fleeing = false;
            return true; //fleeing done
        }

        return false;
    }
    Vector3 GetFleePoint()
    {
        float fleeDistance = 40f;

        Vector3 fleeDirection = transform.position - target.position;
        fleeDirection.Normalize();
        float randomAngle = UnityEngine.Random.Range(-30f, 30f);
        fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;
        Vector3 newFleePosition = transform.position + fleeDirection * fleeDistance;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newFleePosition, out hit, 5, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }
    #endregion

    #region DetectTarget
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = 1000;
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
        if (nearestEnemy != null && shortestDistance <= demonStats.detectDistance + 50)
        {
            Vector3 directionToEnemy = (nearestEnemy.transform.position - transform.position).normalized;
            //Debug.DrawRay(transform.position, directionToEnemy * demonStats.detectDistance, Color.red);
            if (Vector3.Angle(transform.forward, directionToEnemy) < demonStats.fovAngle / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + new Vector3(0, 2, 0), directionToEnemy, out hit, demonStats.detectDistance))
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
    #endregion

    #region Alert
    void Alerted()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = 1000;
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
        if (nearestEnemy != null && shortestDistance <= demonStats.detectDistance + 50)
        {
            Debug.DrawRay(Vector3.forward, transform.position);

            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
            soundAlert.SoundAlerted = false;
        }
    }
    #endregion

    void Destination(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        nm.CalculatePath(destination, path);
        nm.path = path;
    }

    #region Debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(nm.destination, 1);

        if (nm != null && nm.hasPath)
        {
            Gizmos.color = Color.red;
            var path = nm.path;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }

        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -demonStats.fovAngle / 2f, 0) * transform.forward * (demonStats.detectDistance + 50);
        Vector3 rightBoundary = Quaternion.Euler(0, demonStats.fovAngle / 2f, 0) * transform.forward * (demonStats.detectDistance + 50);

        // Draw lines representing the FOV
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        DrawFOVArc();
    }

    private void DrawFOVArc()
    {
        int segments = 20;  // Number of segments to create the arc
        float angleStep = demonStats.fovAngle / segments;
        Vector3 previousPoint = transform.position + Quaternion.Euler(0, -demonStats.fovAngle / 2f, 0) * transform.forward * (demonStats.detectDistance + 50);

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -demonStats.fovAngle / 2f + angleStep * i;
            Vector3 nextPoint = transform.position + Quaternion.Euler(0, currentAngle, 0) * transform.forward * (demonStats.detectDistance + 50);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
    #endregion
}
