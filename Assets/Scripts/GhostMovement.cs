using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class GhostMovement : MonoBehaviour
{
    [Header("Balloons")]
    [SerializeField] private GameObject redBalloon;
    [SerializeField] private GameObject pinkBalloon;
    [SerializeField] private GameObject yellowBalloon;

    [Header("Controls")]
    [SerializeField] public float chaseDistance = 10f;
    // public Transform cageTransform;
    bool isStop = false;
    private float nextPlayTime = 0f;
    [SerializeField] public float stopChaseDistance = 15f;
    [SerializeField] public float roamDistance = 40f;
    public AudioSource AudioSource;
    [SerializeField] private AudioClip Audio;
    [SerializeField] private float WaitingTime = 5.0f;
    private Vector3 initialPosition;
    public float volume = 0f;
    Vector3 velocity = Vector3.zero;
    PlayerMovement mPlayer;
    NavMeshAgent navMeshAgent;
    public bool isDead = false;
    private bool isChasing = false;
    private bool isRoaming = false;
    private float roamTimer = 0f;
    private float initialRoamTime = 10f;
    private float lastChaseTime = 0f;
    private float stepInterval = 0f;
    public float rayLength = 100.0f;
    float sphereRadius = 1.0f;
    private List<GameObject> spawnedBalloons = new List<GameObject>();

    [SerializeField] public Animator ghost;

    bool isDoor = false;
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
        SetRandomInitialPosition();
        initialPosition = transform.position;
        navMeshAgent.isStopped = true;
        navMeshAgent.isStopped = false;
        Roam();
        isRoaming = true;
        StartCoroutine(SpawnBalloonWithInterval());
    }

    void Update()
    {
        if (isStop)
        {
            return;
        }
        float distanceToPlayer = Vector3.Distance(mPlayer.transform.position, transform.position);
        float lerpFactor = Mathf.InverseLerp(stopChaseDistance, chaseDistance, distanceToPlayer);
        if (distanceToPlayer <= 0.5f)
        {
            if(isRoaming)
            {
                StopRoaming();
            }
            if (GameObject.FindGameObjectWithTag("Player"))
            {
                isChasing = false;
                isRoaming = false;
            }
        }
        else if (distanceToPlayer <= chaseDistance)
        {
            navMeshAgent.isStopped = false;
            StartChasing();
        }
        else if (distanceToPlayer > stopChaseDistance && isChasing)//if is chasing, player run out, stop
        {
            navMeshAgent.isStopped = false;
            StopChasing();
        }
        else if (isRoaming && !isChasing)//no chasing,roaming
        {
            navMeshAgent.isStopped = false;
            Roam();
        }
        else
        {
            Roam();
        }
        CheckForDoor();
    }
    void CheckForDoor()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position;
        Vector3 rayDirection = transform.forward;
        Debug.DrawRay(rayStart, rayDirection * rayLength, Color.red);
        float sphereCastDistance = rayLength;
        Color debugColor = Color.red;
        Debug.Log("Ray Start: " + rayStart + ", Ray Direction: " + rayDirection);

        DrawSphereCast(rayStart, rayDirection, sphereRadius, sphereCastDistance, debugColor);
        Ray ray = new Ray(rayStart, rayDirection);

        if (Physics.SphereCast(ray, sphereRadius, out hit, rayLength))
        {
            if (hit.collider.CompareTag("Door") && !isDoor)
            {
                Debug.Log("HEREH");
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
    void CloseDoor(DoorController doorController)
    {
        if (doorController != null)
        {
            doorController.ToggleDoor();//close
        }
        doorController.isProcessing = false;
    }
    private void StartChasing()
    {
        if (Time.time >= nextPlayTime)
        {
            AudioSource.Play();
            nextPlayTime = Time.time + Random.Range(1f, 4f);
        }
        if(isRoaming)
        {
            StopRoaming();
        }
        isChasing = true;
        navMeshAgent.isStopped = false;
        ChasePlayer();
    }

    private void StopChasing()
    {
        AudioSource.Stop();
        isChasing = false;//no chasing
        isRoaming = true;//start Roam
    }

    private void ChasePlayer()
    {
        navMeshAgent.SetDestination(mPlayer.gameObject.transform.position);
    }
    private IEnumerator SpawnBalloonWithInterval()
    {
        yield return new WaitForSeconds(Random.Range(20f, 30f));
        if (isRoaming && !isChasing)
        {
            navMeshAgent.isStopped = true;
            yield return new WaitForSeconds(2.0f);
            SpawnRandomBalloon();
            navMeshAgent.isStopped = false;
        }
        StartCoroutine(SpawnBalloonWithInterval());
    }
    private void SpawnRandomBalloon()
    {
        GameObject[] balloons = new GameObject[] { redBalloon, pinkBalloon, yellowBalloon };
        int index = Random.Range(0, balloons.Length);
        Vector3 spawnPosition = transform.position;
        GameObject spawnedBalloon = Instantiate(balloons[index], spawnPosition, Quaternion.identity);
        spawnedBalloons.Add(spawnedBalloon);
    }
    private void SetRandomInitialPosition()
    {

        Vector3 randomPosition = GenerateRandomPosition();
        NavMeshHit hit;
        int attempts = 0;
        while (!NavMesh.SamplePosition(randomPosition, out hit, 5.0f, NavMesh.AllAreas) && attempts < 10)
        {
            randomPosition = GenerateRandomPosition();
            attempts++;
        }
        if (attempts < 10)
        {
            initialPosition = hit.position;
            transform.position = initialPosition;
        }
        else
        {
            Debug.LogError("Failed to find a valid random position on NavMesh.");
        }
    }

    private Vector3 GenerateRandomPosition()
    {
        return new Vector3(Random.Range(-27f, 27f), 1.5f, Random.Range(0f, 27f));
    }


    private void Roam()
    {
        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                StartRandomRoaming();
            }
        }
    }

    private void StartRandomRoaming()
    {
        isRoaming = true;
        if (!navMeshAgent.pathPending)
        {
            Vector3 forwardDirection = transform.forward;
            Vector3 randomDirection = forwardDirection * Random.Range(0.5f * roamDistance, 1.5f * roamDistance);
            Vector3 randomPosition = transform.position + randomDirection;
            randomPosition += new Vector3(Random.Range(-roamDistance, roamDistance), 0, Random.Range(-roamDistance, roamDistance));

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, roamDistance, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
    }
    private void StopRoaming()
    {
        isRoaming = false;
    }
    private IEnumerator StopChasingWithDelay()
    {
        isChasing = false;
        isRoaming = false;

        yield return new WaitForSeconds(WaitingTime);
        isRoaming = true;
    }
    private Vector3 RandomNavMeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }

    private bool HasReachedDestination()
    {
        return !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
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