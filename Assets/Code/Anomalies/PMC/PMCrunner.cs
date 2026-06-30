using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

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
    Vector3 lastKnownTarget;
    bool hasFlanked = false;
    bool takenCover = false;

    void Start()
    {
        _pv = GetComponent<PhotonView>();
        _Movement = GetComponent<PMCmovement>();
    }

    void Update()
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        if (Input.GetKeyDown(KeyCode.F))
            takenCover = false;
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
            if(Vector3.Distance(lastKnownTarget, currentTarget.position) > 20)
            {
                takenCover = false;
            }
            lastKnownTarget = currentTarget.position;
            hasFlanked = false;
        }
    }

    public void SetDestination(Vector3 pos)
    {
        StopCoroutine(MoveToDestination());
        currentWaypoint = pos;
        StartCoroutine(MoveToDestination());
    }

    public void SwicthState(AiState newState)
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
                    {
                        lastKnownTarget = currentTarget.position;
                        _Weapon.Semi(currentTarget);

                        if(!takenCover)
                            TriggerFindCover();
                    }
                    else if (!hasFlanked)
                    {
                        // Target hidden behind obstacle (e.g. building) -> Trigger Flank
                        
                        TriggerFlankRoute();
                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    void TriggerFlankRoute()
    {
        hasFlanked = true;

        Vector3 directionToLastKnown = (lastKnownTarget - transform.position).normalized;
        // Generate a perpendicular offset vector relative to the target line to move around the building
        Vector3 leftOrRight = Random.value > 0.5f ? Vector3.Cross(directionToLastKnown, Vector3.up) : -Vector3.Cross(directionToLastKnown, Vector3.up);

        Vector3 flankPosition = lastKnownTarget + (leftOrRight * 12f) - (directionToLastKnown * 4f);

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(flankPosition, out navHit, 15f, NavMesh.AllAreas))
        {
            SetDestination(navHit.position);
        }
    }
    void TriggerFindCover()
    {
        takenCover = true;
        Vector3 coverPos = (_Movement.FindCover(currentTarget.position));
        NavMeshHit navHit;
        SetDestination(coverPos);
        if (NavMesh.SamplePosition(coverPos, out navHit, 15f, NavMesh.AllAreas))
        {
            
        }
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


    #region Scanning
    float targetTimeOut = 5f;
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
        if (currentTarget != null && targetTimeOut > 0)
        {
            targetTimeOut-= Time.deltaTime;
            return currentTarget;
        }
        targetTimeOut = 5f;
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
            enemyColliders.Add(other.transform);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enemyTag)) 
            enemyColliders.Remove(other.transform);
    }
    #endregion
}