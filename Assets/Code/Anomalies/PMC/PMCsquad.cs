using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PMCsquad : MonoBehaviour
{
    public List<PMCrunner> squadMembers = new List<PMCrunner>();
    List<Transform> sharedTargets = new List<Transform>();

    void Update()
    {
        //if (!PhotonNetwork.IsMasterClient || squadMembers.Count == 0) return;

        //------------TargetRefresh----------------//
        sharedTargets.Clear();
        for (int i = 0; i < squadMembers.Count; i++)
        {
            if (squadMembers[i] == null) continue;

            Transform t = squadMembers[i].LookForTargetsInFOV();
            if (t != null)
            {
                sharedTargets.Add(t);
            }
        }

        List<Transform> targetList = new List<Transform>(sharedTargets);

        for (int i = 0; i < squadMembers.Count; i++)
        {
            if (squadMembers[i] == null) continue;

            if (targetList.Count > 0) //enemy found
            {
                Transform assignedTarget = targetList[i % targetList.Count]; //multiple targets??? ->need better multi targeting
                squadMembers[i].SetTarget(assignedTarget);

                if (squadMembers[i].curState != AiState.combat) //if not yet in combat, do combat
                {
                    squadMembers[i].SwicthState(AiState.combat);
                }
            }
            else
            {
                squadMembers[i].SetTarget(null);//we dont see it, let single unit logic

                // Revert to aware states safely if they aren't completing an action route
                if (squadMembers[i].curState == AiState.combat) //target gone, go aware
                {
                    squadMembers[i].SwicthState(AiState.aware);
                }
            }
        }
    }
}