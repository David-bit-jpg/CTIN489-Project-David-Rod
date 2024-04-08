using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FPSController
{
    public class Dodge : MonoBehaviour
    {
        private FirstPersonController fpsController;
        private MovementInputData movementInputData;

        [Range(1f, 10f)] [SerializeField] private float dodgeSpeed = 3f;
        [Range(.1f, 1f)] [SerializeField] private float dodgeDuration = .2f;
        [Range(0f, 10f)] [SerializeField] private float dodgeCooldown = 2f;
        private float cooldownTimer;

        public bool isDodging;

        void Start()
        {
            fpsController = GetComponent<FirstPersonController>();
            movementInputData = fpsController.movementInputData;
            cooldownTimer = dodgeCooldown;
        }

        void Update()
        {
            cooldownTimer += Time.deltaTime;

            //Gets the normal of the movement direction and multiplies that by dodgeAmount.
            if (isDodging)
            {
                Vector3 moveDir = fpsController.m_finalMoveDir.normalized;
                fpsController.m_finalMoveDir = new Vector3(moveDir.x * dodgeSpeed, 1, moveDir.z * dodgeSpeed);                
            }
        }

        public void DoDodge()
        {
            //You can't dodge if you're on cooldown, moving forward, or crouching.
            if (cooldownTimer >= dodgeCooldown && fpsController.m_inputVector.y <= 0 && !movementInputData.IsCrouching)
            {
                StartCoroutine(PerformDodge());
            }                
        }

        //Coroutine needed to override controller script movement.
        IEnumerator PerformDodge()
        {
            cooldownTimer = 0;
            isDodging = true;            
            yield return new WaitForSeconds(dodgeDuration);
            isDodging = false;
        }
    }
}
