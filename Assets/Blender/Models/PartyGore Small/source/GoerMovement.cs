using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // 引入 Unity AI 命名空间

public class GoerMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    public float wanderRadius = 10f;
    public float pauseTimeMin = 2f;
    public float pauseTimeMax = 5f;
    private float nextMoveTime;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        MoveToNewRandomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (Time.time >= nextMoveTime)
            {
                MoveToNewRandomPosition();
                animator.SetBool("IsWalking", true);
            }
            else
            {
                animator.SetBool("IsWalking", false);
            }
        }
    }

    void MoveToNewRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
        {
            finalPosition = hit.position;
        }
        agent.SetDestination(finalPosition);

        nextMoveTime = Time.time + Random.Range(pauseTimeMin, pauseTimeMax);
    }
}
