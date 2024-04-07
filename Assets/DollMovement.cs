using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    private enum State
    {
        Roam,
        Chase,
        StopChase
    }

    private State currentState;
    private Transform playerTransform;
    private float chaseTimer;
    private bool isInteractingWithDoor = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        SwitchState(State.Roam);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Roam:
                UpdateRoamState();
                break;
            case State.Chase:
                UpdateChaseState();
                break;
            case State.StopChase:
                UpdateStopChaseState();
                break;
        }

        if (!isInteractingWithDoor)
        {
            CheckForDoor();
        }
    }

    private void SwitchState(State newState)
    {
        currentState = newState;
        switch (newState)
        {
            case State.Roam:
                agent.destination = GetRandomPoint();
                chaseTimer = Random.Range(10f, 16f);
                break;
            case State.Chase:
                chaseTimer = Random.Range(5f, 11f);
                Debug.Log("Starting Chase! Chase duration: " + chaseTimer + " seconds.");
                break;
            case State.StopChase:
                StartCoroutine(WaitBeforeRoaming());
                break;
        }
    }

    private void UpdateRoamState()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SwitchState(State.Chase);
        }
    }

    private void UpdateChaseState()
    {
        chaseTimer -= Time.deltaTime;
        if (chaseTimer > 0)
        {
            agent.destination = playerTransform.position;
        }
        else
        {
            SwitchState(State.StopChase);
        }
    }

    private void UpdateStopChaseState()
    {
        // The logic for StopChase is handled by the coroutine
    }

    private IEnumerator WaitBeforeRoaming()
    {
        yield return new WaitForSeconds(10f); 
        SwitchState(State.Roam);
    }

    private Vector3 GetRandomPoint()
    {
        float x = Random.Range(-27, 28);
        float z = Random.Range(-27, 28);
        Vector3 point = new Vector3(x, 0, z);
        NavMeshHit hit;
        NavMesh.SamplePosition(point, out hit, 2.0f, NavMesh.AllAreas);
        return hit.position;
    }

    private void CheckForDoor()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + new Vector3(0, 1.0f, 0);
        Vector3 rayDirection = transform.forward;

        DrawSphereCast(rayStart, rayDirection, 0.5f, 3.0f, Color.red);

        if (Physics.SphereCast(rayStart, 0.5f, rayDirection, out hit, 3.0f))
        {
            if (hit.collider.CompareTag("Door"))
            {
                StartCoroutine(InteractWithDoor(hit));
            }
        }
    }
    IEnumerator InteractWithDoor(RaycastHit hit)
    {
        isInteractingWithDoor = true;
            
        yield return new WaitForSeconds(0.1f);
        DoorController doorController = hit.collider.GetComponent<DoorController>();
        if (doorController != null)
        {
            doorController.ToggleDoor();//open
            StartCoroutine(CloseDoor(doorController));
        }
        yield return new WaitForSeconds(4.0f);
        isInteractingWithDoor = false;
    }
    IEnumerator CloseDoor(DoorController doorController)
    {
        yield return new WaitForSeconds(3.0f);
        if (doorController.isOpened)
        {
            doorController.ToggleDoor();
        }
    }

    // Debug drawing for SphereCast visualization
    void DrawSphereCast(Vector3 origin, Vector3 direction, float radius, float distance, Color color)
    {
        Debug.DrawRay(origin, direction * distance, color);

        // Draw the origin sphere
        DrawWireSphere(origin, radius, color);

        Vector3 endPosition = origin + direction * distance;

        // Draw the destination sphere
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
