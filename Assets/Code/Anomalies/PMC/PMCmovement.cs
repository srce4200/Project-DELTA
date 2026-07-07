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
    [SerializeField] float walkSpeed = 2.5f;
    [SerializeField] float sprintSpeed = 6.0f;

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
    public void LookAt(Vector3 target)
    {
        if (target == Vector3.zero) return;

        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Prevent the AI from tilting upwards/downwards

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    
}