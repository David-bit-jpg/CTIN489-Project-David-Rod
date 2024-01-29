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
