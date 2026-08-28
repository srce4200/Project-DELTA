using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.Collections;

public enum WaypointType {Move, Flank, TakeCover,  PatrolArea}
public enum FormationType { Column, Line, Wedge }

[Serializable]
public class Waypoint
{
    public WaypointType wpType;
    public Vector3 wpTargetPos;
    public float waitDuration;
    public Waypoint(WaypointType wpType, Vector3 wpTargetPos, float waitDuration)
    {
        this.wpType = wpType;
        this.wpTargetPos = wpTargetPos;
        this.waitDuration = waitDuration;
    }
}

public class PMCsquad : MonoBehaviour
{
    public List<PMCrunner> squadMembers = new List<PMCrunner>();
    List<Transform> sharedTargets = new List<Transform>();
    List<Waypoint> wps = new List<Waypoint>();
    Vector3 lastTargetPos;
    enum LastPosState {safe, dead, searchable, active }
    LastPosState lastPosState = LastPosState.safe;
    bool lastPosDowngrading;

    [Header("Formation")]
    [SerializeField] FormationType formation = FormationType.Wedge;
    [SerializeField] float formationSpacing = 3.5f;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || squadMembers.Count == 0) return;

        //------------TargetRefresh----------------//
        sharedTargets = TargetRefresh();

        for (int i = 0; i < squadMembers.Count; i++)
        {
            if (squadMembers[i] == null) continue;

            MemberTaskAssign(squadMembers[i]);
        }
    }
    void MemberTaskAssign(PMCrunner p)
    {
        if (sharedTargets.Count > 0) //enemy found directly
        {
            Transform assignedTarget = sharedTargets[ sharedTargets.Count-1]; //multiple targets??? ->need better multi targeting
            lastTargetPos = assignedTarget.position;
            lastPosState = LastPosState.active;
            
            p.SetTarget(assignedTarget);
            
            if (p.curState != AiState.combat) //if not yet in combat, do combat
            {
                    p.SwitchState(AiState.combat);
                    //eyes on, take cover, engage,
                    //might not be this so maybe need run help
                if((int)UnityEngine.Random.Range(0,10) > 4)
                    p.AssignWaypoint(new Waypoint(WaypointType.TakeCover, assignedTarget.position, UnityEngine.Random.Range(5, 10)));
                else
                    p.AssignWaypoint(new Waypoint(WaypointType.Flank, assignedTarget.position, 10));
            }
            
            StopCoroutine(StartLastPosDegredation());
            lastPosDowngrading = false;
        }
        else
        {
            if(lastPosState != LastPosState.safe && !lastPosDowngrading)
            {
                lastPosDowngrading = true;
                StartCoroutine(StartLastPosDegredation());
            }
                
            switch(lastPosState) //need reduction time outs
            {
                case LastPosState.active:
                    p.AssignWaypoint(new Waypoint(WaypointType.Flank, lastTargetPos, 10));
                    p.SetTarget(null);
                    break;
                case LastPosState.searchable:
                    p.SwitchState(AiState.aware);
                    p.AssignWaypoint(new Waypoint(WaypointType.PatrolArea, lastTargetPos, 5));
                    break;
                case LastPosState.dead:
                    p.AssignWaypoint(new Waypoint(WaypointType.PatrolArea, lastTargetPos, 7));
                    break;
                case LastPosState.safe:
                    p.SwitchState(AiState.safe);
                    break;
            }
            
        }
    }
    IEnumerator StartLastPosDegredation()
    {
        yield return new WaitForSeconds(30f);

        switch(lastPosState) //need reduction time outs
        {
            case LastPosState.active:
                lastPosState = LastPosState.searchable;
                break;
            case LastPosState.searchable:
                lastPosState = LastPosState.dead;
                break;
            case LastPosState.dead:
                lastPosState = LastPosState.safe;

                if (squadMembers[0] == null) squadMembers.RemoveAt(0);
                else MoveSquadInFormation(squadMembers[0].transform.position);

                break;
            case LastPosState.safe:
                lastPosDowngrading = false;
                yield return null;
                break;
        }
    }
    
    List<Transform> TargetRefresh()
    {
        List<Transform> targetList = new List<Transform>();
        for (int i = 0; i < squadMembers.Count; i++)
        {
            if (squadMembers[i] == null) continue;

            Transform t = squadMembers[i].LookForTargetsInFOV();
            if (t != null)
            {
                targetList.Add(t);
            }
        }
        
        return targetList;
    }
    public void Alert(Vector3 alPos) //if we not in combat and hear sound, we investigate
    {
        if(lastPosState != LastPosState.active)
        {
            lastTargetPos = alPos;
            lastPosState = LastPosState.searchable;
        }
    }
    
    #region Formation
 
    // Moves the whole squad toward 'destination' while holding a formation shape, similar to
    // an Arma squad leader's "Move" order shuffling the team into formation. This rides on the
    // existing waypoint-following system (WaypointType.Move) per member - it just offsets each
    // member's target position before handing it off, so PMCrunner's movement code is untouched.
    public void MoveSquadInFormation(Vector3 destination, float waitDuration = 0f)
    {
        Vector3 facing = (destination - transform.position);
        facing.y = 0f;
        facing = facing.sqrMagnitude > 0.01f ? facing.normalized : transform.forward;

        for (int i = 1; i < squadMembers.Count; i++)
        {
            if (squadMembers[i] == null) continue;
            Vector3 offset = GetFormationOffset(i, facing);
            squadMembers[i].AssignWaypoint(new Waypoint(WaypointType.Move, destination + offset, waitDuration), true);
        }
    }

    Vector3 GetFormationOffset(int index, Vector3 facing)
    {
        Vector3 right = Vector3.Cross(Vector3.up, facing).normalized;

        switch (formation)
        {
            case FormationType.Line:
                {
                    float side = (index % 2 == 0) ? 1f : -1f;
                    int rank = (index / 2) + 1;
                    return right * side * rank * formationSpacing;
                }
            case FormationType.Column:
                return -facing * index * formationSpacing;
            case FormationType.Wedge:
            default:
                {
                    float side = (index % 2 == 0) ? 1f : -1f;
                    int rank = (index / 2) + 1;
                    return (right * side * rank * formationSpacing) - (facing * rank * formationSpacing * 0.5f);
                }
        }
    }
    #endregion
}