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
    Waypoint assignedWaypoint;
    Coroutine moveCoroutine;

    [SerializeField] private LayerMask visionMask;

    [Header("Aiming / Awareness")]
    [Tooltip("How far (deg) the head/torso can turn away from the body's forward before the whole body has to pivot - lets the AI keep a target in sight while still running toward cover.")]
    [SerializeField] float headMaxYaw = 100f;
    [SerializeField] float headTurnSpeed = 10f;
    Vector3 pointOfInterest; // where we're currently looking while investigating (aware state)
    Vector3 lastKnownShotDir;

    [Header("Fire Discipline")]
    [Tooltip("Inside this range the AI opens up on full auto.")]
    [SerializeField] float closeRange = 15f;
    [Tooltip("Between closeRange and this the AI fires controlled bursts. Beyond it, single well-aimed shots.")]
    [SerializeField] float midRange = 40f;
    bool suppressed;
    float suppressionTimer;
    bool combatReady;

    void Start()
    {
        _pv = GetComponent<PhotonView>();
        _Movement = GetComponent<PMCmovement>();
        enemyColliders = MapInfo.Instance.activePlayers; //it should bind to active playerrs list
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (suppressionTimer > 0f)
        {
            suppressionTimer -= Time.deltaTime;
            if (suppressionTimer <= 0f) suppressed = false;
        }

        if (curState == AiState.combat && currentTarget != null)
        {
            // Disable NavMesh path rotation so the body can face the target while moving
            _Movement.SetAutoRotation(false);
            _Movement.LookAt(currentTarget);

            AimAt(currentTarget.position);
            //_Weapon.AimWeaponAt(currentTarget); // Keep barrel pointed at player between shots
        }
        else
        {
            _Movement.SetAutoRotation(true);
        }
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    
    public void SwitchState(AiState newState)
    {
        curState = newState;
        StopAllCoroutines();
        assignedWaypoint = null;
        moveCoroutine = null;

        switch (curState)
        {
            case AiState.safe:
                pointOfInterest = Vector3.zero;
                StartCoroutine(ResetScan());
                break;
            case AiState.aware:
                StartCoroutine(ScanEnvironment());
                break;
            case AiState.combat:
                pointOfInterest = Vector3.zero;
                suppressed = false;
                suppressionTimer = 0f;
                headPivot.localRotation = Quaternion.identity;
                StartCoroutine(CombatBehaviorLoop());
                break;
        }
    }

    public void AssignWaypoint(Waypoint wp, bool force = false)
    {
        if (assignedWaypoint != null && !force) return;
        assignedWaypoint = wp;
        switch (assignedWaypoint.wpType)
        {
            case WaypointType.Move:
                SetDestination(assignedWaypoint.wpTargetPos);
                break;
            case WaypointType.Flank:
                FlankRoute(assignedWaypoint.wpTargetPos);
                break;
            case WaypointType.TakeCover:
                SetDestination(FindCover(assignedWaypoint.wpTargetPos));
                break;
            case WaypointType.PatrolArea:
                SetDestination(AreaPatrol(assignedWaypoint.wpTargetPos));
                break;
        }
    }

    IEnumerator CombatBehaviorLoop()
    {
        combatReady = false;
        yield return new WaitForSeconds(Random.Range(0.15f, 0.6f));//reaction time
        combatReady = true;

        while (curState == AiState.combat)
        {
            if (currentTarget != null)
            {
                // Direct line of sight check
                Vector3 dir = (currentTarget.position - headPivot.position).normalized;
                float dist = Vector3.Distance(headPivot.position, currentTarget.position);
                RaycastHit hit;

                if (Physics.Raycast(headPivot.position, dir, out hit, 200f))
                {
                    if (hit.transform == currentTarget)
                    {
                        lastKnownShotDir = dir;
                        if (combatReady) 
                            ChooseFireMode(dist);
                        AimAt(hit.point);
                    }
                    else if (combatReady && !suppressed)
                    {
                        suppressed = true;
                        suppressionTimer = 1.5f;
                        _Weapon.Burst(currentTarget);
                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    // Distance-based fire mode selection
    void ChooseFireMode(float distance)
    {
        if (distance <= closeRange) _Weapon.FullAuto(currentTarget);
        else if (distance <= midRange) _Weapon.Burst(currentTarget);
        else _Weapon.Semi(currentTarget);
    }

    void FlankRoute(Vector3 targetPos)
    {
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

    Vector3 AreaPatrol(Vector3 targetPos)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = targetPos + Random.insideUnitSphere * 15;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 7f, NavMesh.AllAreas))
            {
                return(hit.position);
            }
        }
        return Vector3.zero;
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
        return Vector3.zero;
    }
    
    #region SetDestination
    
    public void SetDestination(Vector3 pos)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        currentWaypoint = pos;
        moveCoroutine = StartCoroutine(MoveToDestination());
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

        if (assignedWaypoint != null)
            yield return new WaitForSeconds(assignedWaypoint.waitDuration);
        assignedWaypoint = null;
    }
    #endregion

    #region Scanning

    public Transform LookForTargetsInFOV()
    {
        if ((Time.frameCount + gameObject.GetInstanceID()) % 5 != 0) return null;//runs every fifth frame, not on every object

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

            if (angleToTarget < 60)
            {
                RaycastHit hit;
                if (Physics.Raycast(headPivot.position, directionToTarget, out hit, 250, visionMask))
                {
                    if (hit.transform.CompareTag(enemyTag))
                    {
                        return target;
                    }
                    else if (hit.transform.CompareTag("interactable"))
                    {
                        hit.transform.GetComponent<DoorInteractible>().OpenClose(true);
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
            if (pointOfInterest != Vector3.zero)
            {
                // Investigating a noise/hit rather than idly scanning - keep the head trained on it.
                AimAt(pointOfInterest);
            }
            else
            {
                float angle = Mathf.Sin(Time.time * 2f) * maxAngle;
                headPivot.localRotation = Quaternion.Euler(0, angle, 0);
            }
            yield return null;
        }
    }

    void AimAt(Vector3 worldPos)
    {
        if (headPivot == null) return;

        Vector3 dir = worldPos - headPivot.position;
        dir.y = 0f; // keep traversal on the horizontal plane; vertical aim is left to animation/IK
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion desiredWorldRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        Quaternion desiredLocalRot = Quaternion.Inverse(transform.rotation) * desiredWorldRot;

        float localYaw = desiredLocalRot.eulerAngles.y;
        if (localYaw > 180f) localYaw -= 360f;
        localYaw = Mathf.Clamp(localYaw, -headMaxYaw, headMaxYaw);

        Quaternion clampedLocalRot = Quaternion.Euler(0f, localYaw, 0f);
        headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, clampedLocalRot, Time.deltaTime * headTurnSpeed);
    }

    #endregion
}