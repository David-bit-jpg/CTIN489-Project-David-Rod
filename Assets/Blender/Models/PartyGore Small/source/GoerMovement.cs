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
    private GameObject pickedUpBalloon = null;
    private bool hasPickedUpBalloon = false;
    public float chaseEndDistance = 6.0f;
    private bool isMovingToObject = false;
    private GameObject targetObject = null;
    PlayerMovement mPlayer;

    private bool isStick = false;

    [SerializeField] private AudioClip Audio;

    [SerializeField] public float volume = 0.5f;

    public AudioSource AudioSource;

    private bool isCaught = false;
    private void Awake()
    {
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = Audio;
        AudioSource.volume = volume;
    }

    void Start()
    {
        mPlayer = FindObjectOfType<PlayerMovement>();
        agent = GetComponent<NavMeshAgent>();
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsEating", false);
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
        if (animator.GetBool("IsEating")) return;
        if (animator.GetBool("IsWalking") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            animator.Play("Walk");
        }
        if (animator.GetBool("IsChasing") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            animator.Play("Run");
        }
        DetectObjectsOnGround();
        if (isMovingToObject && !agent.pathPending && agent.remainingDistance < 0.5f && !isChasing)
        {
            if (targetObject != null)
            {
                StartCoroutine(ConsumeObject(targetObject));
                targetObject = null;
            }
            isMovingToObject = false;
        }
        else
        {
            if (hasPickedUpBalloon && pickedUpBalloon == null && !isChasing )
            {
                StartChase();
            }
            else if (isChasing && Vector3.Distance(transform.position, playerTransform.position) > chaseEndDistance)
            {
                StopChase();
            }
            else if (Vector3.Distance(transform.position, playerTransform.position) <= 2.0f && isChasing)
            {
                if (GameObject.FindGameObjectWithTag("Player") && !isCaught)
                {
                    // isChasing = false;
                    StartCoroutine(KillPlayer());
                    isCaught = true;
                }
            }
            else if (isChasing)
            {
                ChasePlayer();
            }
            else if (!agent.pathPending && agent.remainingDistance < 0.5f && !isStick)
            {
                if (Time.time >= nextMoveTime)
                {
                    // Debug.Log("Random Roam");
                    animator.SetBool("IsWalking", true);
                    MoveToNewRandomPosition();
                }
                else
                {
                    if (playerTransform != null)
                    {
                        // Debug.Log("Stop and look");
                        TurnTowards(playerTransform.position);
                    }
                    animator.SetBool("IsWalking", false);
                }

                if (Time.time >= currentBalloonSearchTime)
                {
                    // Debug.Log("Time to find balloon");
                    FindNearestBalloon();
                    currentBalloonSearchTime = Time.time + balloonSearchTimer;
                }
                if (currentTargetBalloon != null && !agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    // Debug.Log("Picking up");
                    StartCoroutine(PickupBalloon(currentTargetBalloon));
                    currentTargetBalloon = null;
                }
            }
        }
        CheckForDoor();
    }


    void ChasePlayer()
    {
        if (playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
            ResetAnimationStates();
            animator.SetBool("IsChasing", true);
        }
    }
    void StartChase()
    {
        if (!isChasing && !isStick)
        {
            // Debug.Log("Chase started because balloon broke");
            isChasing = true;
            agent.speed += 1.0f;
            ResetAnimationStates();
            animator.SetBool("IsChasing", true);
            currentBalloonSearchTime = float.MaxValue;
        }
    }

    void StopChase()
    {
        if (isChasing)
        {
            // Debug.Log("Chase stopped because player is far away");
            isChasing = false;
            agent.speed -= 1.0f;
            ResetAnimationStates();
            animator.SetBool("IsWalking", true);
            nextMoveTime = Time.time + Random.Range(pauseTimeMin, pauseTimeMax);
            currentBalloonSearchTime = Time.time + balloonSearchTimer;
            pickedUpBalloon = null;
        }
    }
    void DetectObjectsOnGround()
    {
        float detectionRadius = 10.0f;
        float seeRadius = 4.0f;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        Vector3 playerPos = playerTransform.position;
        float distance = Vector3.Distance(playerPos, transform.position);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("GlowStick"))
            {
                isStick = true;
                GlowStickManager gsm = hitCollider.GetComponent<GlowStickManager>();
                gsm.isTaken = true;
                if (isChasing)
                {
                    StopChase();
                }
                // Debug.Log("Detected " + hitCollider.tag + " within range");
                if (!isMovingToObject)
                {
                    agent.SetDestination(hitCollider.transform.position);
                    isMovingToObject = true;
                    targetObject = hitCollider.gameObject;
                    agent.isStopped = false;
                    break;
                }
            }
            else if (hitCollider.CompareTag("BrokenBalloon") && !isChasing && distance <= seeRadius)
            {
                // Debug.Log("Broken balloon detected. Starting chase.");
                StartChase();
                break;
            }
            else if (hitCollider.CompareTag("BrokenBalloon") && !isChasing)
            {
                // Debug.Log("Detected " + hitCollider.tag + " within range");
                if (!isMovingToObject)
                {
                    agent.SetDestination(hitCollider.transform.position);
                    isMovingToObject = true;
                    targetObject = hitCollider.gameObject;
                    agent.isStopped = false;
                    break;
                }
            }
        }
    }
    IEnumerator KillPlayer()
    {
        AudioSource.Play();
        Debug.Log("Start Kill loop");
        yield return new WaitForSeconds(15.0f);
        float distanceToPlayer = Vector3.Distance(mPlayer.transform.position, transform.position);
        if (distanceToPlayer <= 2.0f && !isStick)
        {
            float initialDistanceToPlayer = Vector3.Distance(mPlayer.transform.position, transform.position);
            if (initialDistanceToPlayer < 2.0f)
            {
                mPlayer.SetCanMove(false);
                Vector3 directionToPlayer = (mPlayer.transform.position - transform.position).normalized;
                mPlayer.transform.position = transform.position + directionToPlayer * 3.5f;
                mPlayer.fixPos = mPlayer.transform.position = transform.position + directionToPlayer * 3.5f;
                mPlayer.killed = true;
            }
            mPlayer.cameraControl.canMove = false;
            StartCoroutine(TurnCameraTowards(transform, 2.0f));
        }
    }
    IEnumerator TurnCameraTowards(Transform target, float duration)
    {
        Transform cameraTransform = mPlayer.cameraControl.transform;
        Quaternion startRotation = cameraTransform.rotation;
        Quaternion endRotation = Quaternion.LookRotation(target.position - cameraTransform.position);

        float time = 0.0f;
        while (time < duration)
        {
            cameraTransform.rotation = Quaternion.Slerp(startRotation, endRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cameraTransform.rotation = endRotation;
        cameraTransform.position = playerTransform.position;
    }

    IEnumerator ConsumeObject(GameObject obj)
    {
        if (!isChasing)
        {
            agent.isStopped = true;
            ResetAnimationStates();
            animator.Play("Eat");
            animator.SetBool("IsEating", true);
            yield return new WaitForSeconds(3.18f);

            obj.SetActive(false);

            ResetAnimationStates();
            animator.SetBool("IsEating", false);
            agent.isStopped = false;

            nextMoveTime = Time.time + Random.Range(pauseTimeMin, pauseTimeMax);
            isMovingToObject = false;
            if(isStick)
            {
                isStick = false;
            }
        }
    }


    IEnumerator PickupBalloon(GameObject balloonChild)
    {
        animator.SetBool("IsWalking", false);
        yield return new WaitForSeconds(2f);
        Debug.Log("Picking up");
        animator.SetBool("IsWalking", true);
        Transform balloonParent = balloonChild.transform;
        pickedUpBalloon = balloonParent.gameObject;
        while (balloonParent.parent != null)
        {
            balloonParent = balloonParent.parent;
        }
        balloonParent.SetParent(transform);
        Break_Ghost bg = balloonParent.GetComponent<Break_Ghost>();
        if(bg)
        {
            bg.isPicked = true;
        }
        balloonParent.localPosition = new Vector3(-1f, 1.75f, 0.1f);
        balloonParent.localRotation = Quaternion.Euler(0, 0, 0);
        hasPickedUpBalloon = true;
        nextMoveTime = Time.time + pauseTimeMin;
        float waitTime = Random.Range(20f, 30f);
        animator.SetBool("IsWalking", true);
        StartCoroutine(DropBalloon(balloonParent.gameObject, waitTime));
    }
    IEnumerator DropBalloon(GameObject balloonParent, float waitTime)
    {
        animator.SetBool("IsWalking", false);
        yield return new WaitForSeconds(waitTime);
        animator.SetBool("IsWalking", true);
        if (balloonParent != null)
        {
            balloonParent.transform.SetParent(null);
            Break_Ghost bg = balloonParent.GetComponent<Break_Ghost>();
            if (bg != null)
            {
                bg.isPicked = false;
            }
            hasPickedUpBalloon = false;
            pickedUpBalloon = null;
        }
        hasPickedUpBalloon = false;
        currentBalloonSearchTime = Time.time + balloonSearchTimer;
        pickedUpBalloon = null;
        nextMoveTime = Time.time + Random.Range(pauseTimeMin, pauseTimeMax);
        currentBalloonSearchTime = Time.time + balloonSearchTimer;
        Debug.Log("Dropping");
        animator.SetBool("IsWalking", true);
    }

    void FindNearestBalloon()
    {
        if (hasPickedUpBalloon)
        {
            return;
        }
        animator.SetBool("IsWalking", true);
        float nearestDistance = Mathf.Infinity;
        GameObject nearestBalloon = null;

        foreach (GameObject balloon in GameObject.FindGameObjectsWithTag("Balloon"))
        {
            Break_Ghost bg = balloon.GetComponent<Break_Ghost>();
            if (bg != null)
            {
                float distance = Vector3.Distance(transform.position, balloon.transform.position);
                if (distance < nearestDistance && !bg.isPicked)
                {
                    nearestDistance = distance;
                    nearestBalloon = balloon;
                }
            }
        }

        if (nearestBalloon != null)
        {
            agent.SetDestination(nearestBalloon.transform.position);
            animator.SetBool("IsWalking", true);
            currentTargetBalloon = nearestBalloon;
            nextMoveTime = float.MaxValue;
        }
    }

    void CheckForDoor()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + new Vector3(0f, 1.0f, 0);
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
        if (isChasing)
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
        if (doorController.isOpened)
        {
            doorController.ToggleDoor();
        }
    }

    void MoveToNewRandomPosition()
    {
        animator.SetBool("IsWalking", true);
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

    void ResetAnimationStates()
    {
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsEating", false);
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