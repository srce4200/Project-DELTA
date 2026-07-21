using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.Collections;

public enum WaypointType {Move, Flank, TakeCover,  PatrolArea}

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
                p.AssignWaypoint(new Waypoint(WaypointType.TakeCover, assignedTarget.position, 5));
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
                    p.AssignWaypoint(new Waypoint(WaypointType.Flank, lastTargetPos, 3));
                    p.SetTarget(null);
                    break;
                case LastPosState.searchable:
                    p.SwitchState(AiState.aware);
                    p.AssignWaypoint(new Waypoint(WaypointType.PatrolArea, lastTargetPos, 3));
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
}