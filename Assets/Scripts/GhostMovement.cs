using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class GhostMovement : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] public float chaseDistance = 15f;
    [SerializeField] public float stopChaseDistance = 8f;
    private float nextPlayTime = 0f;
    public AudioSource AudioSource;
    [SerializeField] private AudioClip Audio;
    public float volume = 0f;
    Vector3 velocity = Vector3.zero;
    PlayerMovement mPlayer;
    NavMeshAgent navMeshAgent;
    private bool isChasing = false;
    [SerializeField] public Animator ghost;

    public float rayLength = 0.001f;
    float sphereRadius = 1f;
    bool isDoor = false;
    bool isCaught = false;

    private void Awake()
    {
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = Audio;
        AudioSource.volume = volume;
    }

    private void Start()
    {
        mPlayer = FindObjectOfType<PlayerMovement>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.isStopped = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.stoppingDistance = 3.0f;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(mPlayer.transform.position, transform.position);
        if (distanceToPlayer <= 3.5f)
        {
            if (GameObject.FindGameObjectWithTag("Player") && !isCaught)
            {
                ghost.SetBool("IsFlying", false);
                ghost.SetBool("IsAttacking", true);
                isChasing = false;
                StartCoroutine(WaitFiveSeconds());
                isCaught = true;
            }//Attack
        }
        else if (distanceToPlayer <= chaseDistance) //enter range, chase player
        {

            isCaught = false;
            Debug.Log("Chasing Player!!!");
            navMeshAgent.isStopped = false;
            mPlayer.chased = true;
            StartChasing();
        }
        else if (distanceToPlayer > stopChaseDistance && isChasing)//if is chasing, player run out, stop
        {
            isCaught = false;
            mPlayer.chased = false;
            Debug.Log("Stop Chasing");
            navMeshAgent.isStopped = false;
            StopChasing();
        }
        CheckForDoor();
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
    IEnumerator WaitFiveSeconds()
    {
        yield return new WaitForSeconds(15.0f);
        float distanceToPlayer = Vector3.Distance(mPlayer.transform.position, transform.position);
        if (distanceToPlayer <= 3.5f)
        {
            float initialDistanceToPlayer = Vector3.Distance(mPlayer.transform.position, transform.position);
            if (initialDistanceToPlayer < 3.5f)
            {
                mPlayer.SetCanMove(false);
                Vector3 directionToPlayer = (mPlayer.transform.position - transform.position).normalized;
                mPlayer.transform.position = transform.position + directionToPlayer * 3.5f;
                mPlayer.fixPos =  mPlayer.transform.position = transform.position + directionToPlayer * 3.5f;
                mPlayer.killed = true;
            }
            ghost.SetBool("IsAttacking", false);
            ghost.SetBool("IsFlying", false);
            mPlayer.cameraControl.canMove = false;
            StartCoroutine(TurnCameraTowards(transform, 2.0f));
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
        if (doorController != null && !doorController.isOpening && !doorController.isProcessing)
        {
            doorController.isProcessing = true;
            doorController.ToggleDoor();//open
            yield return new WaitForSeconds(4.0f);
            CloseDoor(doorController);
            doorController.isProcessing = false;
        }
        isDoor = false;
        doorController.isProcessing = false;
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
    }
    void CloseDoor(DoorController doorController)
    {
        if (doorController != null)
        {
            doorController.ToggleDoor();
        }
        doorController.isProcessing = false;
    }

    private void StartChasing()
    {
        ghost.SetBool("IsAttacking", false);
        ghost.SetBool("IsFlying", true);
        if (Time.time >= nextPlayTime)
        {
            AudioSource.Play();
            nextPlayTime = Time.time + Random.Range(1f, 4f);
        }
        isChasing = true;
        navMeshAgent.isStopped = false;
        ChasePlayer();
    }

    private void StopChasing()
    {
        ghost.SetBool("IsAttacking", true);
        ghost.SetBool("IsFlying", false);
        AudioSource.Stop();
        isChasing = false;//no chasing
        Destroy(gameObject);
    }

    private void ChasePlayer()
    {
        navMeshAgent.SetDestination(mPlayer.gameObject.transform.position);
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