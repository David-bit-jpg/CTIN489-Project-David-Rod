using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;


public class AIBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    [SerializeField] private Material vhsMaterial;
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float stopChaseDistance = 15f;

    private ScriptableRendererFeature vhsFeature;

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
    public AudioSource WalkingAudioSource;
    [SerializeField] private AudioClip WalkAudio;
    public AudioSource WarningAudioSource;
    [SerializeField] private AudioClip WarningAudio;
    public float moveSpeed = 1.5f;
    public float pauseSpeed = 0f;
    public float moveTime = 0.5f;
    public float pauseTime = 0.2f;
    private bool warningPlayed = false;
    private float movePauseTimer;
    private bool isWarningFading = false;
    private Coroutine warningFadeCoroutine = null;

    private void Awake()
    {
        WalkingAudioSource = gameObject.AddComponent<AudioSource>();
        WalkingAudioSource.clip = WalkAudio;
        WarningAudioSource = gameObject.AddComponent<AudioSource>();
        WarningAudioSource.clip = WarningAudio;
    }

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

        UpdateMovePauseCycle();
        AdjustVolumeBasedOnDistance();
        UpdateVHSParameters();
    }

    private void UpdateMovePauseCycle()
    {
        if (currentState == State.Roam)
        {
            movePauseTimer -= Time.deltaTime;
            if (movePauseTimer <= 0)
            {
                if (agent.speed == pauseSpeed)
                {
                    // 从停顿切换到移动
                    agent.speed = moveSpeed;
                    movePauseTimer = moveTime;
                }
                else
                {
                    // 从移动切换到停顿
                    agent.speed = pauseSpeed;
                    movePauseTimer = pauseTime;
                    PlayMoveSound();
                }
            }
        }
        if (currentState == State.Chase)
        {
            movePauseTimer -= Time.deltaTime;
            if (movePauseTimer <= 0)
            {
                if (agent.speed == pauseSpeed)
                {
                    // 从停顿切换到移动
                    agent.speed = moveSpeed + 1.0f;
                    movePauseTimer = moveTime;
                }
                else
                {
                    // 从移动切换到停顿
                    agent.speed = pauseSpeed;
                    movePauseTimer = pauseTime;
                    PlayMoveSound();
                }
            }
        }
    }

    private void SwitchState(State newState)
    {
        currentState = newState;
        if(newState != State.Chase)
        {
            warningPlayed = false;
        }
        switch (newState)
        {
            case State.Roam:
                agent.destination = GetRandomPoint();
                chaseTimer = Random.Range(10f, 16f);
                break;
            case State.Chase:
                chaseTimer = 30f;
                Debug.Log("Starting Chase! Chase duration: " + chaseTimer + " seconds.");
                break;
            case State.StopChase:
                StartCoroutine(WaitBeforeRoaming());
                break;
        }
    }

    private void PlayMoveSound()
    {
        if (WalkingAudioSource != null && WalkAudio != null)
        {
            WalkingAudioSource.PlayOneShot(WalkAudio);
        }
    }
    IEnumerator PlayWarningSoundWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (WarningAudioSource != null && WarningAudio != null)
        {
            WarningAudioSource.clip = WarningAudio;
            WarningAudioSource.Play();
            yield return new WaitForSeconds(20f);
            if (warningFadeCoroutine != null)
            {
                StopCoroutine(warningFadeCoroutine);
            }
            warningFadeCoroutine = StartCoroutine(FadeOutWarningAudio(10f));
        }
    }
    IEnumerator FadeOutWarningAudio(float duration)
    {
        isWarningFading = true;
        float startVolume = WarningAudioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            WarningAudioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        WarningAudioSource.volume = 0;
        WarningAudioSource.Stop();
        isWarningFading = false;
    }



    private void AdjustVolumeBasedOnDistance()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        float maxVolumeDistance = 5.0f;
        float minVolumeDistance = 20.0f; 
        float volume = Mathf.Clamp((minVolumeDistance - distance) / (minVolumeDistance - maxVolumeDistance), 0, 0.7f);
        if (!isWarningFading)
        {
            WarningAudioSource.volume = volume;
        }
        WalkingAudioSource.volume = volume/1.5f;
    }




    //update state
    private void UpdateRoamState()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SwitchState(State.Chase);
        }
    }

    private void UpdateChaseState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= chaseDistance)
        {
            if (!warningPlayed)
            {
                StartCoroutine(PlayWarningSoundWithDelay(0.0f));
                warningPlayed = true;
            }
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
        yield return new WaitForSeconds(0.2f);
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
    private void UpdateVHSParameters()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float lerpFactor = Mathf.InverseLerp(stopChaseDistance, chaseDistance, distanceToPlayer);

        if (vhsMaterial != null)
        {
            float strength = Mathf.Lerp(0.0f, 1.0f, lerpFactor);
            float strip = Mathf.Lerp(0.3f, 0.2f, lerpFactor);
            float pixelOffset = Mathf.Lerp(0.0f, 40.0f, lerpFactor);
            float shake = Mathf.Lerp(0.003f, 0.01f, lerpFactor);
            float speed = Mathf.Lerp(0.5f, 1.2f, lerpFactor);
            vhsMaterial.SetFloat("_Strength", strength);
            vhsMaterial.SetFloat("_StripSize", strip);
            vhsMaterial.SetFloat("_PixelOffset", pixelOffset);
            vhsMaterial.SetFloat("_Shake", shake);
            vhsMaterial.SetFloat("_Speed", speed);
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