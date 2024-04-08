using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FPSController
{
    public class WallRun : MonoBehaviour
    {
        FirstPersonController fpsController;
        CameraController cameraController;

        public LayerMask ignoreLayer;

        private float cameraTurnValue;
        private float wallRunTimer;
        private float wallRunDelay;
        private float timeSinceLastWallJump;

        [Space(10), Header("WallRun Settings")]
        [Range(1f, 100f)] [SerializeField] float speedFactor = 5f;
        [Range(1f, 100f)] [SerializeField] float heightFactor = 5f;
        [Range(1f, 100f)] [SerializeField] float maxWallRunTime = 5f;

        [Space(10), Header("WallJump Settings")]
        [Range(1f, 100f)] [SerializeField] float wallJumpDistance = 50;
        [Range(1f, 100f)] [SerializeField] float wallJumpHeight = 10f;
        [Range(0f, 10f)] [SerializeField] float wallJumpCooldown = 0.5f;

        [Space(10), Header("Camera Setting")]
        [Range(0f, 100f)] [SerializeField] float cameraTiltAmount = 15f;
        [Range(1f, 100f)] [SerializeField] float cameraTiltSpeed = 5f;

        private Vector3 wallJumpNormal;

        private bool rightHasHit;
        private bool leftHasHit;
        private bool isRunning;
        private bool canWallJump;

        [Space(10), Header("Public for Other Scripts")]
        //Needed for CameraController
        [SerializeField] [ReadOnly] public float cameraTiltValue;
        [SerializeField] [ReadOnly] public bool isWallRunning;

        void Start()
        {
            fpsController = GetComponent<FirstPersonController>();
            cameraController = GetComponentInChildren<CameraController>();
        }

        void Update()
        {
            isRunning = fpsController.movementInputData.IsRunning;

            RaycastHit rightHit;
            RaycastHit leftHit;

            #region Wall check raycasts
            //Right raycast checks if there is a wall to the immediate right of player. 
            //Gets the wall normal to calculate direction of wall jump.
            if (Physics.Raycast(transform.position, transform.right, out rightHit, .8f, ~ignoreLayer))
            {
                rightHasHit = true;
                leftHasHit = false;
                wallJumpNormal = rightHit.normal;
                cameraTurnValue = -25;
            }
            else
                rightHasHit = false;
            //Left raycast checks if there is a wall to the immediate left of player. 
            //Gets the wall normal to calculate direction of wall jump.
            if (Physics.Raycast(transform.position, -transform.right, out leftHit, .8f, ~ignoreLayer))
            {
                leftHasHit = true;
                rightHasHit = false;
                wallJumpNormal = leftHit.normal;
                cameraTurnValue = 25;
            }
            else
                leftHasHit = false;
            #endregion

            #region WallRun Control
            //If player is grounded, moving forward, running, timer conditions met, and is not falling, they can wall run.
            if (!fpsController.m_isGrounded && fpsController.m_inputVector.y > 0.5f && isRunning &&
                wallRunTimer <= maxWallRunTime && fpsController.m_finalMoveVector.y > -10)
            {
                if (rightHasHit)
                    WallRunRight();
                else if (leftHasHit)
                    WallRunLeft();
                else
                    StopWallRun();
            }
            else
                StopWallRun();
            #endregion

            if (isWallRunning && canWallJump)
            {
                if (fpsController.movementInputData.JumpClicked)
                    JumpOffWall();
            }

            if (timeSinceLastWallJump >= wallJumpCooldown)
                canWallJump = true;
            else
                canWallJump = false;

            if (isWallRunning)
            {
                wallRunTimer += Time.deltaTime;
                wallRunDelay = 0;
            }

            if (wallRunDelay >= 0.8f)
                wallRunTimer = 0;
        }

        void WallRunLeft()
        {
            isWallRunning = true;
            timeSinceLastWallJump += Time.deltaTime;
            cameraTiltValue = Mathf.Lerp(cameraTiltValue, -cameraTiltAmount, cameraTiltSpeed * Time.deltaTime);
            DoWallRun();
        }
        void WallRunRight()
        {
            isWallRunning = true;
            timeSinceLastWallJump += Time.deltaTime;
            cameraTiltValue = Mathf.Lerp(cameraTiltValue, cameraTiltAmount, cameraTiltSpeed * Time.deltaTime);
            DoWallRun();
        }
        void StopWallRun()
        {
            isWallRunning = false;
            wallRunDelay += Time.deltaTime;
            timeSinceLastWallJump = 0;
            if (cameraTiltValue != 0)
                cameraTiltValue = Mathf.Lerp(cameraTiltValue, 0, cameraTiltSpeed * Time.deltaTime);
        }
        void JumpOffWall()
        {
            //Instantly changes the moveDir and y vector of the player, essentially throwing them from the wall's surface.
            timeSinceLastWallJump = 0;
            fpsController.m_finalMoveVector.y = fpsController.m_finalMoveVector.y + wallJumpHeight;
            fpsController.m_finalMoveDir.x = (fpsController.m_finalMoveDir.x) + wallJumpNormal.x * wallJumpDistance;
            fpsController.m_finalMoveDir.z = (fpsController.m_finalMoveDir.z) + wallJumpNormal.z * wallJumpDistance;
            cameraController.m_desiredYaw = cameraController.m_desiredYaw + cameraTurnValue;
        }

        void DoWallRun()
        {
            //Alters the player moveDir to push them is the same direction the wall extends.
            //Changes the y value of the moveVector to increase height or prevent falling.
            Vector3 moveDirNormal = fpsController.m_finalMoveDir.normalized;
            fpsController.m_finalMoveDir.x = Mathf.Lerp(fpsController.m_finalMoveDir.x, moveDirNormal.x * speedFactor,
                3 * Time.deltaTime);
            fpsController.m_finalMoveDir.z = Mathf.Lerp(fpsController.m_finalMoveDir.z, moveDirNormal.z * speedFactor,
                3 * Time.deltaTime);
            fpsController.m_finalMoveVector.y = Mathf.Lerp(fpsController.m_finalMoveVector.y, heightFactor, 3 * Time.deltaTime);
        }
    }

}