using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class PMCmovement : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] float rotationSpeed = 8f;

    [Header("Animation & Locomotion")]
    Animator animator;
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 6.0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float currentSpeed = agent.velocity.magnitude;
        animator.SetBool("isWalking", currentSpeed > 0.3f);
        animator.SetBool("isSprinting", currentSpeed > walkSpeed + 0.5f);
    }

    // Notice we added a "sprint" boolean to dictate locomotion type
    public void MoveTo(Vector3 destination, bool sprint = false)
    {
        if (!agent.isActiveAndEnabled) return;

        agent.isStopped = false;
        agent.speed = sprint ? sprintSpeed : walkSpeed;
        agent.SetDestination(destination);
    }

    public void StopMoving()
    {
        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // Force immediate stop
        }
    }

    public void LookAt(Transform target)
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Prevent the AI from tilting upwards/downwards

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    public Vector3 FindCover(Vector3 enemyPos)
    {
        for(int i = 0; i < 20; i++)
        {
            Vector3 randomPos = new Vector3(transform.position.x + Random.Range(-20, 20), transform.position.y , transform.position.z + Random.Range(-20, 20));

            RaycastHit hit;
            if(Physics.Raycast(randomPos + new Vector3(0, 50, 0), transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                randomPos = hit.point; 

                if (Physics.Raycast(randomPos + Vector3.up, -(randomPos + Vector3.up - enemyPos), out hit, Mathf.Infinity)){ //we can hide?
                    //Debug.DrawLine(randomPos, hit.point);
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
}