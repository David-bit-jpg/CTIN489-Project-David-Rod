using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MimicSpace
{
    /// <summary>
    /// This is a very basic movement script, if you want to replace it
    /// Just don't forget to update the Mimic's velocity vector with a Vector3(x, 0, z)
    /// </summary>
    public class Movement : MonoBehaviour
    {
        [Header("Controls")]
        [Tooltip("Body Height from ground")]
        [Range(0.5f, 5f)]
        public float height = 0.8f;
        public float speed = 5f;
        Vector3 velocity = Vector3.zero;
        public float velocityLerpCoef = 4f;
        Mimic myMimic;
        PlayerMovement mPlayer;
        NavMeshAgent navMeshAgent;
        //GameManager gameManager;
        public bool isDead = false;

        private void Start()
        {
            //gameManager = FindObjectOfType<GameManager>();
            myMimic = GetComponentInChildren<Mimic>();
            mPlayer = FindObjectOfType<PlayerMovement>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            if (isDead)
            {
                return;
            }

            Vector3 dir = mPlayer.gameObject.transform.position - transform.position;

            if(dir.magnitude < 5.0f)
            {
                return;
            }

            velocity = Vector3.Lerp(velocity, new Vector3(dir.x, 0, dir.z).normalized * speed, velocityLerpCoef * Time.deltaTime);

            // Assigning velocity to the mimic to assure great leg placement
            myMimic.velocity = velocity;

            //transform.position = transform.position + velocity * Time.deltaTime;
            navMeshAgent.SetDestination(mPlayer.gameObject.transform.position);
        }
    }

}