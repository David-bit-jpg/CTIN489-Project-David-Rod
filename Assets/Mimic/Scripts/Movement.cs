using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace MimicSpace
{
    public class Movement : MonoBehaviour
    {
        [Header("Controls")]
        public Light importantLight;

        public GameObject ghostPlatform;
        [SerializeField] public float chaseDistance = 10f;
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
        public bool isDead = false;
        private bool isChasing = false;
        private bool isRoaming = false;
        private float roamTimer = 0f;
        private float initialRoamTime = 10f;
        private float lastChaseTime = 0f;
        private float stepInterval = 0f;
        private bool isDragging = false;
        private void Awake()
        {
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.clip = Audio;
            AudioSource.volume = volume;
        }

        private void Start()
        {
            GameObject lightGameObject = GameObject.FindGameObjectWithTag("MimicLight");
                if (lightGameObject != null)
            {
                importantLight = lightGameObject.GetComponent<Light>();
            }
            vhsFeature = rendererData.rendererFeatures.Find(feature => feature.name == "FullScreenPassRendererFeature");
            myMimic = GetComponentInChildren<Mimic>();
            mPlayer = FindObjectOfType<PlayerMovement>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            SetRandomInitialPosition();
            initialPosition = transform.position;
            navMeshAgent.isStopped = true;
            Debug.Log("Start Roam");
            navMeshAgent.isStopped = false;
            Roam();
            isRoaming = true;
            // DisableVHSFeature();
        }

        void Update()
        {
            if (isDead)
            {
                return;
            }

            float distanceToPlayer = Vector3.Distance(mPlayer.transform.position, transform.position);
            float lerpFactor = Mathf.InverseLerp(stopChaseDistance, chaseDistance, distanceToPlayer);

            UpdateVHSParameters(lerpFactor);

            if(distanceToPlayer<=0.5f)
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
                Debug.Log("Chasing Player!!!");
                navMeshAgent.isStopped = false;
                StartChasing();
            }
            else if (distanceToPlayer > stopChaseDistance && isChasing && !isDragging)//if is chasing, player run out, stop
            {
                Debug.Log("Stop Chasing");
                navMeshAgent.isStopped = false;
                StopChasing();
                // StartCoroutine(StopChasingWithDelay());
            }
            else if (isRoaming && !isChasing && !isDragging)//no chasing,roaming
            {
                Debug.Log("Start Roam");
                navMeshAgent.isStopped = false;
                Roam();
            }
            else
            {
                Roam();
            }
        }

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
            return new Vector3(Random.Range(-27f, 27f), 0, Random.Range(-27f, 10f));
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
        // private void OnTriggerEnter(Collider other)
        // {
        //     if (other.CompareTag("Player") && !isDragging)
        //     {
        //         isDragging = true;
        //         isChasing = false;
        //         isRoaming = false;
        //         StartCoroutine(FollowMimicWithDelay(other.transform));
        //         // isDragging = false;
        //         // isChasing = false;
        //         // isRoaming = true;
        //     }
        // }

        private IEnumerator FollowMimicWithDelay(Transform playerTransform)
        {
            Color originalColor = Color.white;
            if (importantLight != null)
            {
                originalColor = importantLight.color;
                importantLight.color = Color.red;
            }
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
            if (playerMovement != null)
            {
                playerMovement.SetCanMove(false);
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
            
            yield return new WaitForSeconds(10f);

            isDragging = false;
            
            if (importantLight != null)
            {
                importantLight.color = originalColor;
            }
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

    }
}