using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSController
{
    public class InputHandlerLegacyIS : MonoBehaviour
    {
        private FirstPersonController fpsController;
        private CameraController cameraController;
        private Leaning leaning;
        private Slide slide;
        private Dodge dodge;

        [SerializeField] private bool toggleRun;
        [SerializeField] private bool toggleZoom;
        private bool isLookingAround;

        #region Data
        [Space, Header("Input Data")]
        [SerializeField] private CameraInputData cameraInputData = null;
        [SerializeField] private MovementInputData movementInputData = null;
        #endregion       

        float lastTapTime = 0f;
        int tapCount = 0;

        #region BuiltIn Methods
        void Start()
        {
            fpsController = GetComponent<FirstPersonController>();
            cameraController = GetComponentInChildren<CameraController>();
            leaning = GetComponent<Leaning>();
            slide = GetComponent<Slide>();
            dodge = GetComponent<Dodge>();
            cameraInputData.ResetInput();
            movementInputData.ResetInput();
        }

        void Update()
        {
            //If player is moving slow, forward is not being pressed, or if they hit a wall, automatically stops running.
            if (fpsController.m_currentSpeed <= 1 || fpsController.m_inputVector.y < .5f || fpsController.m_hitWall)
            {
                movementInputData.IsRunning = false;
                movementInputData.RunReleased = true;
            }
            //Can't crouch while running.
            if (movementInputData.IsRunning)
                movementInputData.IsCrouching = false;
            //Running state is controlled by either RunClicked or RunHeld, depending on toggleRun check.
            if (!toggleRun && !slide.isSliding)
                movementInputData.IsRunning = movementInputData.RunHeld;

            //Can only look around if Y has input and player is not airborne or sliding.
            if (!fpsController.m_isGrounded || slide.isSliding || fpsController.m_inputVector.y == 0)
                isLookingAround = false;

            cameraController.isLookingAround = isLookingAround;

            GetCameraInput();
            GetMovementInputData();
        }
        #endregion

        #region Custom Methods
        void GetCameraInput()
        {
            cameraInputData.InputVectorX = Input.GetAxis("Mouse X");
            cameraInputData.InputVectorY = Input.GetAxis("Mouse Y");

            //Controls whether zoom is pressed or held, based on toggleZoom.
            #region Zoom Control
            if (toggleZoom)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    cameraInputData.ZoomClicked = true;
                else
                    cameraInputData.ZoomClicked = false;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    cameraInputData.ZoomClicked = true;
                else
                    cameraInputData.ZoomClicked = false;

                if (Input.GetKeyUp(KeyCode.Z))
                    cameraInputData.ZoomReleased = true;
                else
                    cameraInputData.ZoomReleased = false;
            }
            #endregion

            #region Lean Control
            if (Input.GetKey(KeyCode.Q))
                leaning.LeftLeanInput();
            else
                leaning.isLeaningLeft = false;
            if (Input.GetKey(KeyCode.E))
                leaning.RightLeanInput();
            else
                leaning.isLeaningRight = false;
            if (Input.GetKey(KeyCode.LeftControl))
                leaning.UpLeanInput();
            else
                leaning.isLeaningUp = false;
            #endregion

            //Controls camera over-shoulder look.
            if (Input.GetKey(KeyCode.X))
                isLookingAround = true;
            else
                isLookingAround = false;
        }

        void GetMovementInputData()
        {
            movementInputData.InputVectorX = Input.GetAxisRaw("Horizontal");
            movementInputData.InputVectorY = Input.GetAxisRaw("Vertical");

            //Controls run controls based on toggleRun.
            #region Run Control
            if (toggleRun)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    movementInputData.IsRunning = !movementInputData.IsRunning;
                    movementInputData.RunClicked = true;
                    if (movementInputData.IsCrouching)
                        fpsController.InvokeCrouchRoutine();
                }
                else
                    movementInputData.RunClicked = false;
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift) && !slide.isSliding)
                {
                    movementInputData.RunHeld = true;
                    if (movementInputData.IsCrouching)
                        fpsController.InvokeCrouchRoutine();
                }
                else
                    movementInputData.RunHeld = false;
            }
            #endregion
            
            //With legacy input system, this is how we manage double tap control.
            #region Dodge Control
            float tapSpeed = 0.5f;                       
            if (tapCount != 0)
                lastTapTime += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.C))
            {
                tapCount++;
                lastTapTime = 0;                
            }
            if (tapCount == 2 && lastTapTime < tapSpeed)
                    dodge.DoDodge();

            if (lastTapTime >= tapSpeed)
                tapCount = 0;
            #endregion
            
            movementInputData.JumpClicked = Input.GetKeyDown(KeyCode.Space);

            if (Input.GetKeyDown(KeyCode.C))
                movementInputData.CrouchClicked = true;
            else
                movementInputData.CrouchClicked = false;
        }
        #endregion
    }

}