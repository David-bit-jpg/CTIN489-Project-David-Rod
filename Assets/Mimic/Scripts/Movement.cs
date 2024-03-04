using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace MimicSpace
{
    public class Movement : MonoBehaviour
    {
        [Header("Controls")]
        // public Light importantLight;

        public GameObject ghostPlatform;
        [SerializeField] public float chaseDistance = 10f;
        public Transform cageTransform;
        bool isStop = false;
        [SerializeField] public UniversalRendererData rendererData;
        private ScriptableRendererFeature vhsFeature;
        private float nextPlayTime = 0f;
        [SerializeField] public float stopChaseDistance = 15f;
        [SerializeField] public Material vhsMaterial;
        [SerializeField] public float roamDistance = 40f;
        public AudioSource AudioSource;
        [SerializeField] private AudioClip Audio;
        [SerializeField] private float WaitingTime = 5.0f;
        private Vector3 initialPosition;
        public float volume = 0f;
        Vector3 velocity = Vector3.zero;
        Mimic myMimic;
        PlayerMovement mPlayer;
        NavMeshAgent navMeshAgent;
        // NavMeshAgent navMeshAgent;
        public bool isDead = false;
        private bool isChasing = false;
        private bool isRoaming = false;
        private float roamTimer = 0f;
        private float initialRoamTime = 10f;
        private float lastChaseTime = 0f;
        private float stepInterval = 0f;
        private bool isDragging = false;
        public float rayLength = 0.001f;
        float sphereRadius = 1f;

        bool isDoor = false;
        private void Awake()
        {
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.clip = Audio;
            AudioSource.volume = volume;
        }

        private void Start()
        {
            // GameObject lightGameObject = GameObject.FindGameObjectWithTag("MimicLight");
            //     if (lightGameObject != null)
            // {
            //     importantLight = lightGameObject.GetComponent<Light>();
            // }
            vhsFeature = rendererData.rendererFeatures.Find(feature => feature.name == "FullScreenPassRendererFeature");
            myMimic = GetComponentInChildren<Mimic>();
            mPlayer = FindObjectOfType<PlayerMovement>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            SetRandomInitialPosition();
            initialPosition = transform.position;
            navMeshAgent.isStopped = true;
            // Debug.Log("Start Roam");
            navMeshAgent.isStopped = false;
            Roam();
            isRoaming = true;
            // DisableVHSFeature();
        }

        void Update()
        {
            // CheckAndStopNearCage(); 
            if (isStop)
            {
                return;
            }
            float distanceToPlayer = Vector3.Distance(mPlayer.transform.position, transform.position);
            float lerpFactor = Mathf.InverseLerp(stopChaseDistance, chaseDistance, distanceToPlayer);

            UpdateVHSParameters(lerpFactor);

            if (distanceToPlayer <= 0.5f)
            {
                if (GameObject.FindGameObjectWithTag("Player") && !isDragging)
                {
                    isDragging = true;
                    isChasing = false;
                    isRoaming = false;
                    StartCoroutine(FollowMimicWithDelay(GameObject.FindGameObjectWithTag("Player").transform));
                }
            }
            else if (distanceToPlayer <= chaseDistance && !isDragging) //enter range, chase player
            {
                // Debug.Log("Chasing Player!!!");
                navMeshAgent.isStopped = false;
                StartChasing();
            }
            else if (distanceToPlayer > stopChaseDistance && isChasing && !isDragging)//if is chasing, player run out, stop
            {
                // Debug.Log("Stop Chasing");
                navMeshAgent.isStopped = false;
                StopChasing();
                // StartCoroutine(StopChasingWithDelay());
            }
            else if (isRoaming && !isChasing && !isDragging)//no chasing,roaming
            {
                // Debug.Log("Start Roam");
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
                doorController.ToggleDoor();
            }
            doorController.isProcessing = false;
        }
        // private void CheckAndStopNearCage()
        // {

        //     Vector3 cagePosition = new Vector3(); 
        //     float stoppingDistance = 5f;

        //     if (Vector3.Distance(transform.position, cagePosition) <= stoppingDistance)
        //     {
        //         navMeshAgent.isStopped = true;
        //         isStop = true;
        //     }
        // }

        // private void RestartNavigation(Vector3 newDestination)
        // {
        //     navMeshAgent.isStopped = false;
        //     navMeshAgent.SetDestination(newDestination);
        // }


        private void StartChasing()
        {
            // EnableVHSFeature();
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
            AudioSource.Stop();
            isChasing = false;//no chasing
            isRoaming = true;//start Roam
            // DisableVHSFeature();
        }

        private void ChasePlayer()
        {
            navMeshAgent.SetDestination(mPlayer.gameObject.transform.position);
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
            return new Vector3(Random.Range(-27f, 27f), 0, Random.Range(0f, 27f));
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



        private void UpdateVHSParameters(float lerpFactor)
        {
            if (vhsMaterial != null)
            {
                AudioSource.volume = Mathf.Lerp(0.0f, 0.4f, lerpFactor);
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
        public void EnableVHSFeature()
        {
            if (vhsFeature != null)
            {
                vhsFeature.SetActive(true);
            }
        }

        public void DisableVHSFeature()
        {
            if (vhsFeature != null)
            {
                vhsFeature.SetActive(false);
            }
        }
        private IEnumerator StopChasingWithDelay()
        {
            isChasing = false;
            isRoaming = false;
            // DisableVHSFeature();
            yield return new WaitForSeconds(WaitingTime);
            isRoaming = true;
        }

        private IEnumerator FollowMimicWithDelay(Transform playerTransform)
        {
            // Color originalColor = Color.white;
            // if (importantLight != null)
            // {
            //     originalColor = importantLight.color;
            //     importantLight.color = Color.red;
            // }
            Collider playerCollider = playerTransform.GetComponent<Collider>();
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }
            if (ghostPlatform != null)
            {
                ghostPlatform.GetComponent<Collider>().enabled = true;
            }
            PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
            Transform FlashLightTransform = GameObject.FindGameObjectWithTag("FlashLight").transform;
            FlashManager flashManager = FlashLightTransform.GetComponent<FlashManager>();
            if (playerMovement != null)
            {
                playerMovement.SetCanMove(false);
                flashManager.SetCanMove(false);
                StartCoroutine(ReduceDrainTimeCoroutine(playerMovement, flashManager, 2.0f, 10.0f));

            }

            yield return new WaitForSeconds(0.5f);

            Vector3 destination = RandomNavMeshLocation(roamDistance);
            navMeshAgent.SetDestination(destination);
            float startTime = Time.time;
            while (Time.time - startTime < 10f)
            {
                playerTransform.position = Vector3.Lerp(playerTransform.position, this.transform.position, Time.deltaTime * navMeshAgent.speed);
                yield return null;
            }
            if (playerCollider != null)
            {
                playerCollider.enabled = true;
            }
            if (ghostPlatform != null)
            {
                ghostPlatform.GetComponent<Collider>().enabled = false;
            }
            if (playerMovement != null)
            {
                playerMovement.SetCanMove(true);
            }
            if (flashManager != null)
            {
                flashManager.SetCanMove(true);
            }

            yield return new WaitForSeconds(10f);
            isDragging = false;

        }
        private IEnumerator ReduceDrainTimeCoroutine(PlayerMovement playerMovement, FlashManager flashManager, float targetFactor, float duration)
        {
            if (playerMovement == null || flashManager == null) yield break;

            float startTime = Time.time;
            float endTime = startTime + duration;

            float initialPlayerDrainTime = playerMovement.DrainTime;
            float initialFlashDrainTime = flashManager.DrainTime;

            while (Time.time < endTime)
            {
                float elapsed = Time.time - startTime;
                float progress = elapsed / duration;

                playerMovement.DrainTime = Mathf.Lerp(initialPlayerDrainTime, initialPlayerDrainTime / targetFactor, progress);
                flashManager.DrainTime = Mathf.Lerp(initialFlashDrainTime, initialFlashDrainTime / targetFactor, progress);

                playerMovement.UpdateBatteryBar();
                flashManager.UpdateBatteryBar();

                yield return null;
            }
            playerMovement.DrainTime = initialPlayerDrainTime / targetFactor;
            flashManager.DrainTime = initialFlashDrainTime / targetFactor;
            playerMovement.UpdateBatteryBar();
            flashManager.UpdateBatteryBar();
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
}