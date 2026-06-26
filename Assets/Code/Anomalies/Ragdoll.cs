using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ragdoll : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetRigidbodyState(true);
    }

    // Update is called once per frame
    public void DoRagdoll()
    {
        if(GetComponent<ZombieAI>() != null)
            Destroy(GetComponent<ZombieAI>());
        else
            Destroy(GetComponent<demonAI>());

        GetComponent<Animator>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;

        SetRigidbodyState(false);
    }
    public void SetRagdoll(bool state)
    {
        GetComponent<Animator>().enabled = state;
        //GetComponent<NavMeshAgent>().enabled = state;

        SetRigidbodyState(state);
    }
    void SetRigidbodyState(bool state)
    {
        Rigidbody[] rigidbodys = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rigidbody in rigidbodys)
        {
            rigidbody.isKinematic = state;
        }

        GetComponent<Rigidbody>().isKinematic = state;
        GetComponent<PhotonTransformView>().enabled = state;
    }
}
