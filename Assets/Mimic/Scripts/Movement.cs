using UnityEngine;
using UnityEngine.AI;

namespace MimicSpace
{
    public class Movement : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] public float chaseDistance = 10f;
        private float nextPlayTime = 0f;
        [SerializeField] public float stopChaseDistance = 15f;
        [SerializeField] public Material vhsMaterial;
        [SerializeField] public float roamDistance = 40f;
        public AudioSource AudioSource;
        [SerializeField] private AudioClip Audio;
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
        private void Awake()
        {
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.clip = Audio;
            AudioSource.volume = volume;
        }

        private void Start()
        {
            myMimic = GetComponentInChildren<Mimic>();
            mPlayer = FindObjectOfType<PlayerMovement>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            SetRandomInitialPosition();
            initialPosition = transform.position;
            navMeshAgent.isStopped = true;
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

            if (distanceToPlayer <= chaseDistance) //enter range, chase player
            {
                Debug.Log("Chasing Player!!!");
                navMeshAgent.isStopped = false;
                StartChasing();
            }
            else if (distanceToPlayer > stopChaseDistance && isChasing)//if is chasing, player run out, stop
            {
                Debug.Log("Stop Chasing");
                navMeshAgent.isStopped = false;
                StopChasing();
            }
            else if (isRoaming && !isChasing)//no chasing,roaming
            {
                Debug.Log("Start Roam");
                navMeshAgent.isStopped = false;
                Roam();
            }
            else
            {
                Debug.Log("Not Activated");
            }
        }

        private void StartChasing()
        {
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
            RoamAwayFromPlayer();//leave
            // navMeshAgent.isStopped = true;
            isRoaming = true;//start Roam
        }

        private void ChasePlayer()
        {
            navMeshAgent.SetDestination(mPlayer.gameObject.transform.position);
        }

        private void RoamAwayFromPlayer()
        {
            navMeshAgent.isStopped = false;
            roamTimer = initialRoamTime;
            Vector3 directionAwayFromPlayer = transform.position - mPlayer.transform.position;
            Vector3 roamTarget = transform.position + directionAwayFromPlayer.normalized * roamDistance;
            navMeshAgent.SetDestination(roamTarget);
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
    }
}