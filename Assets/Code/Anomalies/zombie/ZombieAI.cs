using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public scriptableZombie zombieStats;

    string enemyTag = "ermacore faction";
    public enum AiState { idle, patroling, chasing, climbing, attacking };
    public AiState aiState = AiState.idle;

    float distanceToEnemy, nextTimeToFire;
    Transform target;
    bool fleeing; // Start is called before the first frame update
    Health demonHealth;
    SoundAlert soundAlert;
    Animator zombieAnim;
    NavMeshAgent nm;
    AudioSource audioSource;

    static Stack<Transform> climbingStack = new Stack<Transform>();
    Vector3 climbStartPos;
    Vector3 climbEndPos;
    Transform climbTarget;
    bool isClimbing;

    void Start()
    {
        nm = GetComponent<NavMeshAgent>();
        zombieAnim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        soundAlert = GetComponent<SoundAlert>();
        demonHealth = GetComponent<Health>();
        SetValues();
        if (!PhotonNetwork.IsMasterClient)
            return;

        InvokeRepeating(nameof(UpdateZombie), 2, 0.4f);
    }

    void SetValues()
    {
        nm.speed = zombieStats.speed;
        demonHealth.health = zombieStats.health;
    }
    void UpdateZombie()
    {
        if (soundAlert.SoundAlerted)
            Alerted();
        else
            UpdateTarget();
    }
    // Update is called once per frame
    void Update()
    {
        switch (aiState)
        {
            case AiState.idle:
                if (target != null)
                {
                    aiState = AiState.chasing;
                }
                else if(Random.Range(0, 100) < 20)
                {
                    aiState = AiState.patroling;
                }
                else
                {
                    Idle();
                }
                break;
            case AiState.patroling:
                bool waypoitnDone = Patrol();
                if(target != null)
                {
                    aiState = AiState.chasing;
                }
                else if (waypoitnDone)
                {
                    aiState = AiState.idle; //switch back after patrol point done
                }
                break;
            case AiState.chasing:
                if (target == null) //hard reset
                    aiState = AiState.idle;

                if (distanceToEnemy <= zombieStats.attackDistance)
                {
                    aiState = AiState.attacking;
                }
                else
                {
                    Chase();
                }
                break;
            case AiState.climbing:
                Climb();
                break;
            case AiState.attacking:
                if (target == null)
                {//hard reset
                    aiState = AiState.idle;
                }

                if (distanceToEnemy > zombieStats.attackDistance)
                {
                    aiState = AiState.chasing;
                }
                else
                {
                    Attack();
                }
                break;
            default:
                break;
        }
    }

    #region Idle
    void Idle()
    {
        zombieAnim.SetBool("walking", false);
        zombieAnim.SetBool("alerted", false);
        zombieAnim.SetBool("chasing", false);
        nm.SetDestination(transform.position);
    }
    #endregion

    #region Patrol
    bool Patrol()
    {
        nm.speed = zombieStats.speed * 0.5f;
        zombieAnim.SetBool("walking", true);
        zombieAnim.SetBool("alerted", false);
        zombieAnim.SetBool("chasing", false);

        if (nm.remainingDistance < 0.5f)
        {
            nm.SetDestination(GetPatrolPoint());
            return true;
        }

        return false;
    }
    Vector3 GetPatrolPoint()
    {
        float patrolRadius = zombieStats.patrolDistance;
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        // Ensure the point is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            return hit.position; // Return the valid point on the NavMesh
        }

        // Fallback in case a point cannot be found
        return transform.position;
    }
    #endregion

    #region Chase
    void Chase()
    {
        nm.autoTraverseOffMeshLink = false;
        if (nm.isOnOffMeshLink)
        {
            print("yas");
            //StartCoroutine(FallFunc(nm));
        }

        if (distanceToEnemy < 7 && (target.position.y - transform.position.y) > 2)
        {
            Debug.Log("I waanna climb :D");
            //aiState = AiState.climbing;
            //return;
        }


        nm.speed = zombieStats.speed;
        zombieAnim.SetBool("walking", true);
        zombieAnim.SetBool("alerted", true);
        zombieAnim.SetBool("chasing", true);
        if(target != null) 
            Destination(target.position);
    }
    #endregion

    #region Climbing
    public void Climb()
    {
        if (!isClimbing)
        {
            // Disable NavMeshAgent for manual movement
            nm.enabled = false;

            // Check if the zombie is already near the player on the same Y level
            Vector3 playerPosition = target.position;
            float yDifference = Mathf.Abs(transform.position.y - playerPosition.y);
            float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);

            if (yDifference <= 1.0f && distanceToPlayer <= 2.0f) // Adjust these thresholds based on your needs
            {
                // If the zombie is on the same level or very close, place it on the NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(new Vector3(playerPosition.x, playerPosition.y, playerPosition.z), out hit, 10f, NavMesh.AllAreas))
                {
                    // Move zombie to a valid NavMesh position near the player
                    transform.position = hit.position;
                    climbEndPos = transform.position;
                }
                else
                {
                    // If no valid position found, default to a nearby position
                    transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                    climbEndPos = transform.position;
                }
            }
            else
            {
                // If the zombie is not yet on the same level, stack them as usual
                if (climbingStack.Count == 0)
                {
                    // First zombie starts the stack
                    climbingStack.Push(transform);
                    climbStartPos = transform.position;

                    // Find the closest valid NavMesh position on the platform of the player
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(new Vector3(playerPosition.x, playerPosition.y + 1.0f, playerPosition.z), out hit, 10f, NavMesh.AllAreas))
                    {
                        climbEndPos = hit.position;
                    }
                    else
                    {
                        climbEndPos = new Vector3(playerPosition.x, playerPosition.y + 1.0f, playerPosition.z); // Default to player's height
                    }
                }
                else
                {
                    // Zombies climb on top of the last zombie in the stack
                    climbTarget = climbingStack.Peek();
                    climbingStack.Push(transform);

                    // Calculate climbing position dynamically
                    climbStartPos = transform.position;
                    float heightOffset = 2.5f * 0.7f; // Adjust height for pyramid effect
                    float forwardOffset = climbingStack.Count; // Adjust horizontal offset for pyramid shape

                    // Calculate new position considering stack effect
                    climbEndPos = climbTarget.position
                                  + Vector3.up * heightOffset
                                  + transform.forward * forwardOffset;
                }
            }

            isClimbing = true;
            StartCoroutine(ClimbToPosition());
        }
    }

    IEnumerator ClimbToPosition()
    {
        //animator.SetBool("Climbing", true);

        while (Vector3.Distance(transform.position, climbEndPos) > 0.1f)
        {
            // Smoothly move toward the climbing destination
            transform.position = Vector3.MoveTowards(transform.position, climbEndPos, Time.deltaTime * 0.5f);
            yield return null;
        }

        isClimbing = false;
        //animator.SetBool("Climbing", false);

        // Re-enable NavMeshAgent
        nm.enabled = true;

        // If this zombie is the last in the stack, clear the stack
        if (climbingStack.Peek() == transform)
        {
            climbingStack.Clear();
        }
    }

    #endregion

    #region Attack
    void Attack()
    {
        zombieAnim.SetBool("walking", false);
        zombieAnim.SetBool("alerted", true);
        zombieAnim.SetBool("chasing", true);
        nm.SetDestination(transform.position);
        if (Time.time >= nextTimeToFire)
        {
            zombieAnim.SetTrigger("Attack");
            nextTimeToFire = Time.time + 1f / zombieStats.attackSpeed + 0.2f;
            StartCoroutine(DelayDamage());
        }
    }
    IEnumerator DelayDamage()
    {
        yield return new WaitForSeconds(0f);
        audioSource.PlayOneShot(zombieStats.attackSfx[UnityEngine.Random.Range(0, zombieStats.attackSfx.Count - 1)]);
        if (target == null)
            yield break;

        if (target.tag == enemyTag)
            target.GetComponent<playerHealth>().pv.RPC("TakeDamage", RpcTarget.All, (double)zombieStats.damage);
        else
            target.GetComponent<DeleteObject>().Delete();
    }
    #endregion

    #region DetectTarget
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = 1000;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }
        if (nearestEnemy != null && shortestDistance <= zombieStats.detectDistance + 50)
        {
            Debug.DrawRay(Vector3.forward, transform.position);

            if (Vector3.Angle(Vector3.forward, transform.InverseTransformPoint(nearestEnemy.transform.position)) < zombieStats.fovAngle / 2f)
            {
                target = nearestEnemy.transform;
            }
        }
        else
        {
            target = null;
        }
    }
    #endregion

    #region Alert
    void Alerted()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = 1000;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }
        if (nearestEnemy != null && shortestDistance <= zombieStats.detectDistance + 50)
        {
            Debug.DrawRay(Vector3.forward, transform.position);

            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
            soundAlert.SoundAlerted = false;
        }
    }
    #endregion

    void Destination(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        nm.CalculatePath(destination, path);
        nm.path = path;
    }

    #region Debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(nm.destination, 1);

        if (nm != null && nm.hasPath)
        {
            Gizmos.color = Color.red;
            var path = nm.path;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }

        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -zombieStats.fovAngle / 2f, 0) * transform.forward * (zombieStats.detectDistance + 50);
        Vector3 rightBoundary = Quaternion.Euler(0, zombieStats.fovAngle / 2f, 0) * transform.forward * (zombieStats.detectDistance + 50);

        // Draw lines representing the FOV
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        DrawFOVArc();
    }

    private void DrawFOVArc()
    {
        int segments = 20;  // Number of segments to create the arc
        float angleStep = zombieStats.fovAngle / segments;
        Vector3 previousPoint = transform.position + Quaternion.Euler(0, -zombieStats.fovAngle / 2f, 0) * transform.forward * (zombieStats.detectDistance + 50);

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -zombieStats.fovAngle / 2f + angleStep * i;
            Vector3 nextPoint = transform.position + Quaternion.Euler(0, currentAngle, 0) * transform.forward * (zombieStats.detectDistance + 50);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
    #endregion
}