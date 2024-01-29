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
        private Vector3 initialPosition;

        Vector3 velocity = Vector3.zero;
        Mimic myMimic;
        PlayerMovement mPlayer;
        NavMeshAgent navMeshAgent;
        public bool isDead = false;
        private bool isChasing = false;

        private void Start()
        {
            myMimic = GetComponentInChildren<Mimic>();
            mPlayer = FindObjectOfType<PlayerMovement>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            SetRandomInitialPosition();
            initialPosition = transform.position;
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
            else
            {
                ReturnToInitialPosition();
            }
        }

        private void StartChasing()
        {
            isChasing = true;
            navMeshAgent.isStopped = false;
        }

        private void StopChasing()
        {
            isChasing = false;
            navMeshAgent.isStopped = true;
        }

        private void ChasePlayer()
        {
            navMeshAgent.SetDestination(mPlayer.gameObject.transform.position);
        }

        private void ReturnToInitialPosition()
        {
            if (Vector3.Distance(transform.position, initialPosition) != 0.0f)
            {
                navMeshAgent.SetDestination(initialPosition);
            }
            else
            {
                navMeshAgent.isStopped = true;
            }
        }

        private void SetRandomInitialPosition()
        {
            Vector3 randomPosition = new Vector3(Random.Range(-27f, 27f), 0, Random.Range(-27f, 0f));//random
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 10.0f, NavMesh.AllAreas))
            {
                initialPosition = hit.position;
            }
            else
            {
                initialPosition = transform.position;
            }

            transform.position = initialPosition;
        }

        private void UpdateVHSParameters(float lerpFactor)
        {
            if (vhsMaterial != null)
            {
                float shake = Mathf.Lerp(1.0f, 0.96f, lerpFactor);
                float shake2 = Mathf.Lerp(1.0f, 0.9f, lerpFactor);
                float shake3 = Mathf.Lerp(0.001f, 0.01f, lerpFactor);
                float pixelOffset = Mathf.Lerp(0.0f, 30.0f, lerpFactor);

                vhsMaterial.SetFloat("_Shake", shake);
                vhsMaterial.SetFloat("_Shake2", shake2);
                vhsMaterial.SetFloat("_Shake3", shake3);
                vhsMaterial.SetFloat("_PixelOffset", pixelOffset);
            }
        }
    }
}
