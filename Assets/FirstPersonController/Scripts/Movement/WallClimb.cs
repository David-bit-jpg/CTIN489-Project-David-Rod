using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FPSController
{
    public class WallClimb : MonoBehaviour
    {
        FirstPersonController fpsController;
        CharacterController characterController;
        CameraController cameraController;

        [SerializeField] GameObject surfaceCheckGuide;
        [SerializeField] GameObject surfaceCheckGuide2;

        [Space(10)]
        public LayerMask ignoreLayer;

        RaycastHit headHit;
        RaycastHit aboveHeadHit;
        RaycastHit middleHit;
        RaycastHit feetHit;

        private Vector3 wallJumpNormal;

        private float climbTimer = 0;
        private float minClimbYaw;
        private float maxClimbYaw;

        [Space(10)]
        [Range(1f, 100f)] [SerializeField] float maxClimbTime = 2;
        [Range(1f, 100f)] [SerializeField] float climbSpeed = 4.5f;
        [Range(1f, 100f)] [SerializeField] float mantleSpeed = 6f;
        [Range(1f, 10f)] [SerializeField] float wallJumpStrength = 1.8f;
        [Range(1f, 10f)] [SerializeField] float wallJumpHeightMultiplier = 1.3f;

        [Space(10), Header("Toggle for Use")]
        [SerializeField] public bool wallClimbEnabled = true;
        [SerializeField] public bool mantleEnabled = true;
        [SerializeField] public bool wallJumpEnabled = false;
        
        [Space(10), Header("Public for Other Scripts")]
        [ReadOnly] [SerializeField] public bool isClimbing;
        [ReadOnly] [SerializeField] public bool isMantling;
        [ReadOnly] [SerializeField] public bool isPerformingWallJump;
        private bool canClimb;
        private bool canMantle;
        private bool aboveHeadHasHit;
        private bool headHasHit;
        private bool middleHasHit;
        private bool feetHasHit;
        private bool surfaceCheck1HasHit;
        private bool surfaceCheck2HasHit;
        private bool isFacingSomething;        

        void Start()
        {
            fpsController = GetComponent<FirstPersonController>();
            characterController = GetComponent<CharacterController>();
            cameraController = GetComponentInChildren<CameraController>();
        }

        void Update()
        {
            #region Climb control
            if (wallClimbEnabled)
            {
                //Conditions for allowing player climb. If they are facing a surface, moving forward, mantle conditions haven't been met,
                //they are not falling fast, not jumping off the wall, and maxClimbTime has not been exceeded.
                if (isFacingSomething && fpsController.m_inputVector.y > 0 && !fpsController.m_isGrounded && headHasHit &&
                aboveHeadHasHit && fpsController.m_finalMoveVector.y > -2f && !isPerformingWallJump && climbTimer < maxClimbTime)
                {
                    //Raycast condition for checking if the ledge is climbable, or can be mantled instead.
                    if (!surfaceCheck2HasHit)
                        canClimb = true;
                    else
                        canClimb = false;
                }
                else
                    canClimb = false;

                if (canClimb)
                    Climb();
                else
                    isClimbing = false;
            }

            //Locks camera while climbing. This means the player can't turn around while climbing, unless they stop the climb.
            if (isClimbing)
            {
                if (cameraController.m_desiredYaw < minClimbYaw)
                    cameraController.m_desiredYaw = minClimbYaw;
                if (cameraController.m_desiredYaw > maxClimbYaw)
                    cameraController.m_desiredYaw = maxClimbYaw;
            }

            //Slows speed against walls meaning the player can slide down a wall to make falling slower.
            //Checks if they are facing the wall, holding forwards, and falling.
            if (isFacingSomething && fpsController.m_finalMoveVector.y < -.5f && fpsController.m_inputVector.y > 0)
                fpsController.m_finalMoveVector.y = -4;
            #endregion
            
            #region Mantle control
            if (mantleEnabled)
            {
                //If climb conditions are met, checks if head raycast hits. If not it means there is a mantleable ledge.
                if (isFacingSomething)
                    if (!headHasHit || !aboveHeadHasHit)
                        //Raycast check ensures there is nothing blocking the player above their head.
                        if (!surfaceCheck2HasHit)
                            canMantle = true;
                        else
                            canMantle = false;
                    else
                        canMantle = false;
                else
                    canMantle = false;

                //Mantles if the conditions are met, meaning the player is moving forward and not grounded.
                if (canMantle && fpsController.m_inputVector.y > 0 && !fpsController.m_isGrounded)
                {
                    isMantling = true;
                    Mantle();
                }
                else
                    isMantling = false;
            }
            #endregion

            #region Wall jump control
            if (wallJumpEnabled)
                //If the player is climbing, facing the wall, and not grounded, then they press jump key and jump off the wall.
                if (isClimbing || isFacingSomething && !fpsController.m_isGrounded)
                    if (Input.GetKeyDown(KeyCode.Space))
                        StartCoroutine(JumpOffWall());
            #endregion

            #region Climb timer control
            //If climb limit is reached and can't mantle, cancels climb.
            if (climbTimer > maxClimbTime)
                if (headHasHit)
                    canClimb = false;

            if (fpsController.m_isGrounded)
                climbTimer = 0;
            #endregion
            
            CheckSurface();
            ClimbRaycasts();
        }

        void Climb()
        {
            //When climbing, starts climb timer, establishes camera lock, and moves player up according to climbSpeed.
            isClimbing = true;
            isPerformingWallJump = false;
            climbTimer += Time.deltaTime;
            minClimbYaw = cameraController.m_desiredYaw - 45;
            maxClimbYaw = cameraController.m_desiredYaw + 45;
            fpsController.m_finalMoveVector.y = climbSpeed;
        }

        void Mantle()
        {
            fpsController.m_finalMoveVector.y = mantleSpeed;
        }

        //Moves player back slightly when blocked above.
        void MoveBack()
        {
            fpsController.m_finalMoveDir.x = wallJumpNormal.x * 12f;
            fpsController.m_finalMoveDir.z = wallJumpNormal.z * 12f;
        }

        //Checks if the player is blocked above and if they can move past it by using two raycasts that move upwards and check for hits.
        //If both raycasts hit it means they are blocked above. If only check1 hits it means the player can be moved back.
        void CheckSurface()
        {
            RaycastHit hit1;
            RaycastHit hit2;

            if (Physics.Raycast(surfaceCheckGuide.transform.position, transform.up, out hit1, 5f, ~ignoreLayer))
                surfaceCheck1HasHit = true;
            else
                surfaceCheck1HasHit = false;

            if (Physics.Raycast(surfaceCheckGuide2.transform.position, transform.up, out hit2, 5f, ~ignoreLayer))
                surfaceCheck2HasHit = true;
            else
                surfaceCheck2HasHit = false;

            if (surfaceCheck1HasHit == true && surfaceCheck2HasHit == false && isClimbing)
                MoveBack();
        }

        void ClimbRaycasts()
        {
            #region Raycasts
            float playerHeight = characterController.height;

            //This one is to check if something is above the head but still grabbable.            
            if (Physics.Raycast(transform.position + new Vector3(0, playerHeight + .3f, 0), transform.forward, out aboveHeadHit, 1f, ~ignoreLayer))
                aboveHeadHasHit = true;
            else
                aboveHeadHasHit = false;

            //This on is lined with the players head. Also checks if there is a mantleable ledge.
            if (Physics.Raycast(transform.position + new Vector3(0, playerHeight, 0), transform.forward, out headHit, 1f, ~ignoreLayer))
            {
                headHasHit = true;
            }
            else
                headHasHit = false;

            //Lined with the middel of the player's body. If this one hits and the above two also do, allows for climbing.
            if (Physics.Raycast(transform.position + new Vector3(0, playerHeight / 2, 0), transform.forward, out middleHit, 1f, ~ignoreLayer))
            {
                middleHasHit = true;
            }
            else
                middleHasHit = false;

            //Lined with the middel of the player's body. If this one hits and the top two also do, allows for climbing.
            if (Physics.Raycast(transform.position + new Vector3(0, 0.2f, 0), transform.forward, out feetHit, 1f, ~ignoreLayer))
            {
                feetHasHit = true;
            }
            else
                feetHasHit = false;

            //If no raycasts hit, there is nothing in front of the player.
            if (!headHasHit && !middleHasHit && !feetHasHit)
                isFacingSomething = false;
            else
                isFacingSomething = true;
            #endregion
        }

        IEnumerator JumpOffWall()
        {
            isPerformingWallJump = true;
            climbTimer = 0;
            fpsController.m_finalMoveDir.x = wallJumpNormal.x * wallJumpStrength;
            fpsController.m_finalMoveDir.z = wallJumpNormal.z * wallJumpStrength;
            fpsController.m_finalMoveVector.y = fpsController.jumpSpeed * wallJumpHeightMultiplier;
            yield return new WaitForSeconds(.4f);
            isPerformingWallJump = false;
        }
    }
}
