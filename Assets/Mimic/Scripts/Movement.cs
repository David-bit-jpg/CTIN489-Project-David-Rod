using UnityEngine;
using UnityEngine.AI;

namespace MimicSpace
{
    public class Movement : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] public float chaseDistance = 10f;
        [SerializeField] public float stopChaseDistance = 15f;
        [SerializeField] public Material vhsMaterial;
        [SerializeField] public float roamDistance = 20f;
        private Vector3 initialPosition;

        Vector3 velocity = Vector3.zero;
        Mimic myMimic;
        PlayerMovement mPlayer;
        NavMeshAgent navMeshAgent;
        public bool isDead = false;
        private bool isChasing = false;
        private bool isRoaming = false;
        private float roamTimer = 0f;
        private float initialRoamTime = 10f;

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

            if (distanceToPlayer <= chaseDistance)
            {
                StartChasing();
            }
            else if (distanceToPlayer > stopChaseDistance)
            {
                StopChasing();
            }

            if (isChasing)
            {
                ChasePlayer();
            }
            else if (isRoaming)
            {
                Roam();
            }
        }

        private void StartChasing()
        {
            isChasing = true;
            navMeshAgent.isStopped = false;
        }

        private void StopChasing()
        {
            // Debug.Log("StopChasing");
            isChasing = false;
            navMeshAgent.isStopped = true;
            RoamAwayFromPlayer();
            isRoaming = true;
        }

        private void ChasePlayer()
        {
            // Debug.Log("StartChasing");
            navMeshAgent.SetDestination(mPlayer.gameObject.transform.position);
        }

        private void RoamAwayFromPlayer()
        {
            // Debug.Log("Leaving....");
            navMeshAgent.isStopped = false;
            roamTimer = initialRoamTime;
            Vector3 directionAwayFromPlayer = transform.position - mPlayer.transform.position;
            Vector3 roamTarget = transform.position + directionAwayFromPlayer.normalized * roamDistance;
            navMeshAgent.SetDestination(roamTarget);
        }

        private void SetRandomInitialPosition()
        {
            Vector3 randomPosition = new Vector3(Random.Range(-27f, 27f), 0, Random.Range(-27f, 10f));
            NavMeshHit hit;
            while (!NavMesh.SamplePosition(randomPosition, out hit, 5.0f, NavMesh.AllAreas))
            {
                randomPosition = new Vector3(Random.Range(-27f, 27f), 0, Random.Range(-27f, 10f));
            }
            initialPosition = hit.position;
            // if (NavMesh.SamplePosition(randomPosition, out hit, 10.0f, NavMesh.AllAreas))
            // {
            //     initialPosition = hit.position;
            // }
            // else
            // {
            //     initialPosition = transform.position;
            // }
            transform.position = initialPosition;
        }

        private void Roam()
        {
            if (!navMeshAgent.pathPending)
            {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    // StartRandomRoaming();
                    if (!navMeshAgent.hasPath)
                    {
                        StartRandomRoaming();
                    }
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

// private void UpdateVHSParameters(float lerpFactor)
//         {
//             if (vhsMaterial != null)
//             {
//                 float strength = Mathf.Lerp(0.0f, 1.0f, lerpFactor);
//                 float strip = Mathf.Lerp(0.3f, 0.2f, lerpFactor);
//                 float pixelOffset = Mathf.Lerp(0.0f, 40.0f, lerpFactor);
//                 float shake = Mathf.Lerp(0.003f, 0.01f, lerpFactor);
//                 float speed = Mathf.Lerp(0.5f, 1.2f, lerpFactor);
//                 vhsMaterial.SetFloat("_Strength", strength);
//                 vhsMaterial.SetFloat("_StripSize", strip);
//                 vhsMaterial.SetFloat("_PixelOffset", pixelOffset);
//                 vhsMaterial.SetFloat("_Shake", shake);
//                 vhsMaterial.SetFloat("_Speed", speed);
//             }
//         }
//     }
// }
