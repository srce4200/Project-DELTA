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
    [SerializeField] PMCweapon pmcWepon;
    [Header("Animation & Locomotion")]
    Animator animator;
    [SerializeField] float walkSpeed = 2.5f;
    [SerializeField] float sprintSpeed = 6.0f;
    float currentSpeed;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        currentSpeed = agent.velocity.magnitude;
        bool sprint = currentSpeed > walkSpeed + 0.5f;
        bool walking = currentSpeed > 0.3f;
        animator.SetBool("isWalking", walking);
        animator.SetBool("isSprinting", sprint);
        pmcWepon.SetHandAnimations(walking, sprint);

        if (agent.isOnOffMeshLink) //found on reddit for offmeshlink speed control
        {
            OffMeshLinkData data = agent.currentOffMeshLinkData;

            //calculate the final point of the link
            Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

            //Move the agent to the end point
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, walkSpeed * Time.deltaTime);

            //when the agent reach the end point you should tell it, and the agent will "exit" the link and work normally after that
            if (agent.transform.position == endPos)
            {
                agent.CompleteOffMeshLink();
            }
        }
    }

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
    public void SetAutoRotation(bool enable)
    {
        if (agent != null) agent.updateRotation = enable;
    }

}