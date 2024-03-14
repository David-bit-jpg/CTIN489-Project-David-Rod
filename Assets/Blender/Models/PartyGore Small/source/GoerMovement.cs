using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;


public class GoerMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    public float wanderRadius = 10f;
    public float pauseTimeMin = 2f;
    public float pauseTimeMax = 5f;
    private float nextMoveTime;
    private float balloonSearchTimer = 20.0f;
    private float currentBalloonSearchTime;
    private Transform playerTransform; 
    public float rayLength = 10.0f;
    float sphereRadius = 1f;
    bool isDoor = false;
    bool isChasing = false;
    private GameObject currentTargetBalloon = null;

    private bool hasPickedUpBalloon = false;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator.SetBool("IsWalking", false);
        nextMoveTime = Time.time + Random.Range(pauseTimeMin, pauseTimeMax);
        currentBalloonSearchTime = Time.time + balloonSearchTimer;
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

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
                if (playerTransform != null)
                {
                    TurnTowards(playerTransform.position); 
                }
                animator.SetBool("IsWalking", false);
            }
        }

        if (Time.time >= currentBalloonSearchTime)
        {
            FindNearestBalloon();
            currentBalloonSearchTime = Time.time + balloonSearchTimer; // 重置计时器
        }
        if (currentTargetBalloon != null && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(PickupBalloon(currentTargetBalloon));
            currentTargetBalloon = null; // 重置当前目标气球，避免重复执行
        }
        CheckForDoor();
    }
    IEnumerator PickupBalloon(GameObject balloonChild)
    {
        yield return new WaitForSeconds(2f);
        Transform balloonParent = balloonChild.transform;
        while (balloonParent.parent != null)
        {
            balloonParent = balloonParent.parent;
        }
        balloonParent.SetParent(transform);
        Break_Ghost bg = balloonParent.GetComponent<Break_Ghost>();
        bg.isPicked = true;
        balloonParent.localPosition = new Vector3(-1f, 1.75f, 0.1f);
        balloonParent.localRotation = Quaternion.Euler(0, 0, 0);
        Debug.Log("PICKUP COMPLETE");
        GameObject balloon_Parts = bg.balloon_Parts;
        hasPickedUpBalloon = true;
        nextMoveTime = Time.time + pauseTimeMin;
        float waitTime = Random.Range(20f, 30f);
        StartCoroutine(DropBalloon(balloonParent.gameObject, waitTime));
    }
    IEnumerator DropBalloon(GameObject balloonParent, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        balloonParent.transform.SetParent(null);
        Break_Ghost bg = balloonParent.GetComponent<Break_Ghost>();
        if (bg != null)
        {
            bg.isPicked = false;
        }
        hasPickedUpBalloon = false;
        currentBalloonSearchTime = Time.time + balloonSearchTimer;
        Debug.Log("DROPPED BALLOON");
    }

    void FindNearestBalloon()
    {
        if (hasPickedUpBalloon)
        {
            return;
        }

        float nearestDistance = Mathf.Infinity;
        GameObject nearestBalloon = null;

        foreach (GameObject balloon in GameObject.FindGameObjectsWithTag("Balloon"))
        {
            Break_Ghost bg = balloon.GetComponent<Break_Ghost>();
            float distance = Vector3.Distance(transform.position, balloon.transform.position);
            if (distance < nearestDistance && !bg.isPicked)
            {
                nearestDistance = distance;
                nearestBalloon = balloon;
            }
        }

        if (nearestBalloon != null)
        {
            agent.SetDestination(nearestBalloon.transform.position);
            currentTargetBalloon = nearestBalloon;
            nextMoveTime = float.MaxValue;
        }
    }


    void CheckForDoor()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + new Vector3(0f,1.0f,0);
        Vector3 rayDirection = transform.forward;
        Debug.DrawRay(rayStart, rayDirection * rayLength, Color.red);
        float sphereCastDistance = rayLength;
        Color debugColor = Color.red;
        DrawSphereCast(rayStart, rayDirection, sphereRadius, sphereCastDistance, debugColor);
        Ray ray = new Ray(rayStart, rayDirection);
        if (Physics.SphereCast(ray, sphereRadius, out hit, rayLength))
        {
            if (hit.collider.CompareTag("Door") && !isDoor)
            {
                StartCoroutine(InteractWithDoor(hit));
            }
        }
    }
    IEnumerator InteractWithDoor(RaycastHit hit)
    {
        isDoor = true;
        if(isChasing)
            yield return new WaitForSeconds(1.0f);
        else
            yield return new WaitForSeconds(0.1f);
        DoorController doorController = hit.collider.GetComponent<DoorController>();
        if (doorController != null)
        {
            doorController.ToggleDoor();//open
            StartCoroutine(CloseDoor(doorController));
        }
        yield return new WaitForSeconds(4.0f);
        isDoor = false;
    }
    IEnumerator CloseDoor(DoorController doorController)
    {
        yield return new WaitForSeconds(3.0f);
        if(doorController.isOpened)
        {
            doorController.ToggleDoor();
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

    void TurnTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
        }
    }
    //Debug
    void DrawSphereCast(Vector3 origin, Vector3 direction, float radius, float distance, Color color)
    {
        Debug.DrawRay(origin, direction * distance, color);

        DrawWireSphere(origin, radius, color);

        Vector3 endPosition = origin + direction * distance;
        DrawWireSphere(endPosition, radius, color);
    }
    void DrawWireSphere(Vector3 center, float radius, Color color)
    {
        float angleStep = 10.0f;
        Vector3 prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.up * radius;
        for (float angle = angleStep; angle <= 360.0f; angle += angleStep)
        {
            Vector3 point = center + Quaternion.Euler(0, angle, 0) * Vector3.up * radius;
            Debug.DrawLine(prevPoint, point, color);
            prevPoint = point;
        }

        prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.forward * radius;
        for (float angle = angleStep; angle <= 360.0f; angle += angleStep)
        {
            Vector3 point = center + Quaternion.Euler(angle, 0, 0) * Vector3.forward * radius;
            Debug.DrawLine(prevPoint, point, color);
            prevPoint = point;
        }

        prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.right * radius;
        for (float angle = angleStep; angle <= 360.0f; angle += angleStep)
        {
            Vector3 point = center + Quaternion.Euler(0, 0, angle) * Vector3.right * radius;
            Debug.DrawLine(prevPoint, point, color);
            prevPoint = point;
        }
    }
}