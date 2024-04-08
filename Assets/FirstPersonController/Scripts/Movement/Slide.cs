using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FPSController
{
    public class Slide : MonoBehaviour
    {
        private FirstPersonController fpsController;
        private MovementInputData movementInputData;
        private CharacterController characterController;
        private CameraController cameraController;

        [Space(10)]
        [SerializeField] GameObject center;

        RaycastHit centerHit;
        RaycastHit hillCheckHit;

        [Space(10), Header("Camera Control")]
        [SerializeField] bool lockCameraWhenSliding = true;

        private float slideTimer = 0f;
        private float defaultSlideTimerMax;
        private float timeSinceLastSlide;
        private float minSlideYaw;
        private float maxSlideYaw;
        private float currentVelocity;
        private float centerDistance;
        private float hillCheckDistance;
        private float slopeHeight;

        [Space(10)]
        //maxSlideTime increases depending on downward slope.
        [Range(.1f, 5f)] [SerializeField] float maxSlideTime = 0.7f;
        [Range(0f, 10f)] [SerializeField] float slideCooldown = 1f;
        [Range(1f, 20f)] [SerializeField] float jumpHeightFromSlope = 10;
        
        [Space(10), Header("Public for Other Scripts")]
        [ReadOnly] public bool isSliding;
        [ReadOnly] public bool jumpedFromSlide;
        private bool isCrouching;
        private bool canSlide;
        private bool isDownhill;
        private bool isUphill;
        private bool isMovingFast;

        private Vector3 slideDir;
        private Vector3 downhillSlide;
        private Vector3 moveDirNormal;

        void Start()
        {
            fpsController = GetComponent<FirstPersonController>();
            movementInputData = fpsController.movementInputData;
            characterController = GetComponent<CharacterController>();
            cameraController = GetComponentInChildren<CameraController>();
            defaultSlideTimerMax = maxSlideTime;
            timeSinceLastSlide = slideCooldown;
        }

        void Update()
        {
            isCrouching = movementInputData.IsCrouching;
            slideTimer += Time.deltaTime;
            timeSinceLastSlide += Time.deltaTime;
            slopeHeight = centerDistance - hillCheckDistance;
            currentVelocity = characterController.velocity.magnitude;

            moveDirNormal = fpsController.m_finalMoveDir.normalized;

            #region Slide Conditions
            if (timeSinceLastSlide > slideCooldown && !isCrouching && movementInputData.IsRunning &&
                fpsController.m_isGrounded && !isUphill && fpsController.m_inputVector.y > 0)
            {
                canSlide = true;
            }
            else if (timeSinceLastSlide > slideCooldown && isCrouching && fpsController.m_isGrounded && !isUphill && currentVelocity > 10)
                canSlide = true;
            else
                canSlide = false;

            #endregion

            #region Cancel Slide
            //Conditions to cancel slide. This includes maxSlideTime being reached, being off the ground, or moving slowly.
            if (isSliding)
            {
                movementInputData.IsRunning = false;
                movementInputData.RunHeld = false;
                if (slideTimer > maxSlideTime || !isMovingFast && fpsController.distanceToGround > 10f || fpsController.m_currentSpeed == 0)
                {
                    isSliding = false;
                    maxSlideTime = defaultSlideTimerMax;
                }
                //Conditions to cancel slide, including pressing crouch key and falling.
                if (isSliding && !movementInputData.IsCrouching || fpsController.m_inputVector.y < 0)
                {
                    isSliding = false;
                    maxSlideTime = defaultSlideTimerMax;
                }
            }
            #endregion

            //Camera control limits look direction while sliding so player can't turn around.
            if (lockCameraWhenSliding)
            {
                if (isSliding)
                {
                    if (cameraController.m_desiredYaw < minSlideYaw)
                        cameraController.m_desiredYaw = minSlideYaw;
                    if (cameraController.m_desiredYaw > maxSlideYaw)
                        cameraController.m_desiredYaw = maxSlideYaw;
                    if (cameraController.m_desiredPitch > 30)
                        cameraController.m_desiredPitch = Mathf.Lerp(cameraController.m_desiredPitch, 30, 4 * Time.deltaTime);
                }
            }

            //Player can jump from slide which is set to continue slide upon landing. Player has more jump height from slide by default.
            if (isSliding && movementInputData.JumpClicked)
                StartCoroutine(JumpFromSlide());
            if (fpsController.m_isGrounded)
                jumpedFromSlide = false;

            //Detects if the player is moving fast enough to automatically enter slide if crouched.
            //This serves the purpose of allowing player to slide when falling or after jumping from a slide.
            Vector3 finalMoveVector = fpsController.m_finalMoveVector;
            if (finalMoveVector.x > 5 || finalMoveVector.x < -5 || finalMoveVector.z > 5 || finalMoveVector.z < -5)
                isMovingFast = true;
            else
                isMovingFast = false;

            //Slows the speed at the end of a slide if going downhill and hill angle is not steep enough.
            if (isSliding && slideTimer >= maxSlideTime - 1)
                fpsController.m_finalMoveDir = Vector3.Lerp(fpsController.m_finalMoveDir, new Vector3(0, 0, 0), 1 * Time.deltaTime);

            if (slopeHeight < -.2f && fpsController.distanceToGround < 1f)
                fpsController.stickToGroundForce = 50;
            else
                fpsController.stickToGroundForce = 1;

            CheckHill();
        }

        void LateUpdate()
        {
            //Performs slide if conditions above are met and resets the slideTimer to zero. If slope angle is 
            //steep enough slideTimer remains at zero.
            if (fpsController.movementInputData.CrouchClicked && canSlide)
            {
                DoSlide();
                slideTimer = 0;
            }
            //If the player is moving fast, crouching, and grounded, automatically enters slide.
            if (currentVelocity > 12 && isCrouching && fpsController.m_isGrounded)
            {
                if (isMovingFast || isDownhill)
                {    
                    DoSlide();
                    if (currentVelocity > 15)
                        slideTimer = 0;
                }
            }
        }

        public void DoSlide()
        {
            //Upon entering slide, establishes camera lock and slide direction.
            minSlideYaw = cameraController.m_desiredYaw - 75;
            maxSlideYaw = cameraController.m_desiredYaw + 75;
            fpsController.m_finalMoveDir = slideDir;
            isSliding = true;
            timeSinceLastSlide = 0;
        }

        //Checks the hill in front of the player and dynamically calculates slope and slide speed.
        void CheckHill()
        {
            //Hit1 is centered on the player. Hit2 is based on the player's current move direction. This dynamically calculates the 
            //angle of the slope to adjust the slide time and speed.
            Vector3 hit1Point = centerHit.point;
            Vector3 hit2Point = hillCheckHit.point;
            Vector3 moveDir = fpsController.m_finalMoveDir;

            if (Physics.Raycast(center.transform.position + new Vector3(0, 2, 0), Vector3.down, out centerHit, 50, ~fpsController.ignoreLayer))
            {
                centerDistance = centerHit.distance;
            }
            if (Physics.Raycast(center.transform.position + new Vector3(moveDir.x, 2, moveDir.z), Vector3.down, out hillCheckHit, 50, ~fpsController.ignoreLayer))
            {
                hillCheckDistance = hillCheckHit.distance;
            }

            #region Hill Conditions
            //Determines impact of slope angle. Limits the min and max of the slope factor so the player cannot exceed speeds.
            float slopeFactor = (slopeHeight * -100) / 10;
            if (slopeFactor < 2.5f)
                slopeFactor = 2.5f;
            if (slopeFactor > 8)
                slopeFactor = 8;

            //If downhill, extends slide time and increases slide speed.
            if (centerDistance < hillCheckDistance)
            {
                isUphill = false;
                isDownhill = true;
                slideDir = new Vector3(moveDirNormal.x * 4, -1f, moveDirNormal.z * 4);
                downhillSlide = new Vector3(-slideDir.x * slopeFactor, -1, -slideDir.z * slopeFactor);
                if (isSliding)
                {
                    slideDir = Vector3.Lerp(slideDir, downhillSlide, 2 * Time.deltaTime);
                    fpsController.m_finalMoveDir = slideDir;
                    //If slope is steep enough, slide time is unlimited.
                    if (slopeHeight < -.5f)
                        slideTimer = 0;
                    else
                        maxSlideTime = (defaultSlideTimerMax + -slopeHeight) * 2;
                }
            }
            //If uphill, cancels slide completely. This ensures no improper slide behavior happens when moving up a slope.
            else if (slopeHeight >.2f)
            {
                isUphill = true;
                isDownhill = false;
                isSliding = false;
                slideTimer = maxSlideTime;
                slideDir = new Vector3(moveDirNormal.x * 4, -1f, moveDirNormal.z * 4);
                fpsController.stickToGroundForce = 1;
            }
            //If there is no slope, creates a default slide speed and time.
            else
            {
                isUphill = false;
                isDownhill = false;
                maxSlideTime = defaultSlideTimerMax;
                slideDir = new Vector3(moveDirNormal.x * 4, -1f, moveDirNormal.z * 4);
                fpsController.stickToGroundForce = 1;
            }
            #endregion
        }

        //Coroutine is needed so the grounded condition does not overlap jumpedFromSlide.
        IEnumerator JumpFromSlide()
        {
            Vector3 moveDir = fpsController.m_finalMoveVector;
            //Downward slopes factor in to the jump ability while sliding.
            if (isDownhill)
                fpsController.m_finalMoveVector = new Vector3(moveDir.x, jumpHeightFromSlope, moveDir.z);
            else
            {
                //If just started slide, jump is more impactful. Useful for implementing slide jump mechanics.
                if (slideTimer < 1f)
                    fpsController.m_finalMoveDir = new Vector3((slideDir.x + slideDir.x) / 4, fpsController.jumpSpeed, (slideDir.z + slideDir.z) / 4);
                else
                    fpsController.m_finalMoveDir = new Vector3(slideDir.x, fpsController.jumpSpeed, slideDir.z);
            }

            jumpedFromSlide = true;
            yield return new WaitForSeconds(.05f);
            jumpedFromSlide = true;
        }
    }

}