using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public enum AiState{ safe, aware, combat} 


public class PMCrunner : MonoBehaviour
{
    string enemyTag = "ermacore faction";
    PhotonView _pv;
    PMCmovement _Movement;
    public PMCweapon _Weapon;

    public AiState curState = AiState.safe;
    List<Transform> enemyColliders = new List<Transform>();
    [SerializeField] Transform headPivot;
    Transform currentTarget;
    Vector3 currentWaypoint;

    // Position tracking
    
    bool hasFlanked = false;
    bool takenCover = false;

    bool waypointInProgress;

    void Start()
    {
        _pv = GetComponent<PhotonView>();
        _Movement = GetComponent<PMCmovement>();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;


        if (curState == AiState.combat && currentTarget != null) //we see target, look at it
        {
            _Movement.LookAt(currentTarget);
        }
    }

    public void SetTarget(Transform target)
    {
        //if (target != null && currentTarget == null)
        //{
        //    SetDestination(_Movement.FindCover(target.position));
        //}

        currentTarget = target;

        if (currentTarget != null)
        {
            takenCover = false;
            hasFlanked = false;
        }
    }

    
    public void SwitchState(AiState newState)
    {
        curState = newState;
        StopAllCoroutines();

        switch (curState)
        {
            case AiState.safe:
                StartCoroutine(ResetScan());
                break;
            case AiState.aware:
                StartCoroutine(ScanEnvironment());
                break;
            case AiState.combat:
                //StartCoroutine(ScanEnvironment());
                StartCoroutine(CombatBehaviorLoop());
                break;
        }
    }

    IEnumerator CombatBehaviorLoop()
    {
        while (curState == AiState.combat)
        {
            if (currentTarget != null)
            {
                // Direct line of sight check
                Vector3 dir = (currentTarget.position - headPivot.position).normalized;
                RaycastHit hit;

                if (Physics.Raycast(headPivot.position, dir, out hit, 200f))
                {
                    if (hit.transform == currentTarget)
                    {;
                        _Weapon.Semi(currentTarget);

                        //if(!takenCover)
                        //    TriggerFindCover();
                    }
                    else 
                    {
                        // Target hidden behind obstacle (e.g. building) -> Trigger Flank

                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void AssignWaypoint()
    {

    }
    public void TriggerFlankRoute(Vector3 targetPos)
    {
        if(!hasFlanked)
            FlankRoute(targetPos);
    }
    void FlankRoute(Vector3 targetPos)
    {
        hasFlanked = true;

        Vector3 dirToTarget = (targetPos - transform.position).normalized;
        // Generate a perpendicular offset vector relative to the target line to move around the building
        Vector3 leftOrRight = Random.value > 0.5f ? Vector3.Cross(dirToTarget, Vector3.up) : -Vector3.Cross(dirToTarget, Vector3.up);

        Vector3 flankPosition = targetPos + (leftOrRight * 12f) - (dirToTarget * 4f);

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(flankPosition, out navHit, 15f, NavMesh.AllAreas))
        {
            SetDestination(navHit.position);
        }
    }
    
    public void TriggerAreaPatrol(Vector3 targetPos, float patrolTime)
    {
        
    }
    void AreaPatrol(Vector3 targetPos)
    {

    }
    public void TriggerFindCover(Vector3 enemyPos)
    {
        if (!takenCover)
        {
            SetDestination(FindCover(enemyPos));
        }
    }
    Vector3 FindCover(Vector3 enemyPos)
    {
        for(int i = 0; i < 20; i++)
        {
            Vector3 randomPos = new Vector3(transform.position.x + Random.Range(-20, 20), transform.position.y , transform.position.z + Random.Range(-20, 20));
            
            RaycastHit hit;
            NavMeshHit hitNav;

            if (NavMesh.FindClosestEdge(randomPos, out hitNav, NavMesh.AllAreas))
            {
                randomPos = hitNav.position; 

                if (Physics.Raycast(randomPos + Vector3.up, -(randomPos + Vector3.up - enemyPos), out hit, Mathf.Infinity)){ //we can hide?
                    print(enemyPos);
                    if(!hit.collider.tag.Equals("ermacore faction"))
                    {
                        return randomPos;
                    }
                }
            }
        }
        print("ZERO");
        return Vector3.zero;
    }
    
    #region SetDestination
    
    public void SetDestination(Vector3 pos)
    {
        StopCoroutine(MoveToDestination());
        currentWaypoint = pos;
        StartCoroutine(MoveToDestination());
    }

    IEnumerator MoveToDestination()
    {
        bool shouldSprint = (curState == AiState.combat);
        _Movement.MoveTo(currentWaypoint, shouldSprint);

        while (Vector3.Distance(transform.position, currentWaypoint) > 2f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        _Movement.StopMoving();
    }
    #endregion

    #region Scanning

    public Transform LookForTargetsInFOV()
    {
        for (int i = enemyColliders.Count - 1; i >= 0; i--)
        {
            if (enemyColliders[i] == null)
            {
                enemyColliders.RemoveAt(i);
                continue;
            }

            Transform target = enemyColliders[i];
            Vector3 directionToTarget = (target.position - headPivot.position + Vector3.up).normalized;

            // This calculates the absolute 3D angle (handles both horizontal and vertical deviation)
            float angleToTarget = Vector3.Angle(headPivot.forward, directionToTarget);

            // If the angle is less than 40, they are inside the "cone" of vision
            if (angleToTarget < 60)
            {
                RaycastHit hit;
                if (Physics.Raycast(headPivot.position, directionToTarget, out hit, 250))
                {
                    if (hit.transform.CompareTag(enemyTag))
                    {
                        return target;
                    }
                }
            }
        }
        return null;
    }
    float maxAngle = 90;
    IEnumerator ResetScan()
    {
        while (true)
        {
            float angle = Mathf.Sin(Time.time) * maxAngle;
            headPivot.localRotation = Quaternion.Euler(0, angle, 0);
            if (angle < 1 && angle > -1) break;
            yield return null;
        }
    }
    IEnumerator ScanEnvironment()
    {
        while (true)
        {
            float angle = Mathf.Sin(Time.time * 2f) * maxAngle;
            headPivot.localRotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }
    }
    private void OnDrawGizmos()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            if (!enemyColliders.Contains(other.transform))
            {
                enemyColliders.Add(other.transform);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enemyTag)) 
            enemyColliders.Remove(other.transform);
    }
    #endregion
}