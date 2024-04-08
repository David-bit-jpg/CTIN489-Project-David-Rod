using System.Collections;
using Unity.Collections;
using UnityEngine;

namespace FPSController
{
    public class CameraController : MonoBehaviour
    {
        #region Variables
        #region Data
        [Space, Header("Data")]
        [SerializeField] private CameraInputData cameraInputData = null;

        [Space, Header("Custom Classes")]
        [SerializeField] private CameraZoom cameraZoom = null;
        [SerializeField] private CameraSwaying cameraSway = null;
        #endregion

        #region Settings
        [Space, Header("Look Settings")]
        [SerializeField] private Vector2 sensitivity = Vector2.zero;
        [SerializeField] private Vector2 smoothAmount = Vector2.zero;
        [SerializeField] private Vector2 lookAngleMinMax = Vector2.zero;
        #endregion

        #region Private
        private float m_yaw;
        private float m_pitch;
        private float maxYaw;
        private float minYaw;
        //For looking around.
        private float minLookYaw;
        private float maxLookYaw;

        [Space, Header("Public for Other Scripts")]
        public float lockYaw;
        public float m_desiredYaw;
        public float m_desiredPitch;

        [Space(10), Header("Public for InputHandler")]
        private bool hasTurned;
        public bool isLookingAround;

        #region Components   
        private FirstPersonController fpsController;
        private WallClimb wallClimb;
        private Leaning leaning;
        private WallRun wallRun;
        private Slide slide;
        private Transform m_pitchTranform;
        private Camera m_cam;
        #endregion
        #endregion

        #endregion

        void Awake()
        {
            GetComponents();
            InitialValues();
            InitialComponents();
            ChangeCursorState();
        }

        void Update()
        {
            SmoothRotation();
            ApplyRotation();
            CalculateRotation();

            #region Wall Climb Control
            //Establishes look rotation lock when climbing or looking around. Creates min and max yaw so the 
            //camera can't turn too much away from the orginal direction.
            if (wallClimb.isClimbing || isLookingAround)
            {
                if (m_desiredYaw >= maxYaw)
                    m_desiredYaw = maxYaw;
                if (m_desiredYaw <= minYaw)
                    m_desiredYaw = minYaw;
            }
            else
            {
                maxYaw = m_desiredYaw + 100;
                minYaw = m_desiredYaw - 100;
                lockYaw = m_desiredYaw;
            }
            #endregion

            #region Wall Jump Control
            //Turns and adds slowing to rotation speed when jumping from wall.
            if (wallClimb.isPerformingWallJump)
            {
                if (!hasTurned)
                {
                    smoothAmount.x = 10;
                    smoothAmount.y = 10;
                    m_desiredYaw = m_desiredYaw + (180 - (m_desiredYaw - lockYaw));
                    m_desiredPitch = 45;
                    hasTurned = true;
                }
            }
            else
            {
                smoothAmount.x = 1000;
                smoothAmount.y = 1000;
                hasTurned = false;
            }
            #endregion

            #region Air Control
            //Changes the camera smoothing while not grounded. If the player is airborne, they turn slower.
            if (!fpsController.m_isGrounded)
            {
                smoothAmount.x = 20;
                smoothAmount.y = 100;
            }
            else
            {
                smoothAmount.x = 1000;
                smoothAmount.y = 1000;
            }
            #endregion            

            HandleZoom();
        }

        #region Custom Methods
        #region Initial Values
        void GetComponents()
        {
            fpsController = GetComponentInParent<FirstPersonController>();
            wallClimb = GetComponentInParent<WallClimb>();
            leaning = GetComponentInParent<Leaning>();
            wallRun = GetComponentInParent<WallRun>();
            slide = GetComponentInParent<Slide>();
            m_pitchTranform = transform.GetChild(0).transform;
            m_cam = GetComponentInChildren<Camera>();
        }

        void InitialValues()
        {
            m_yaw = transform.eulerAngles.y;
            m_desiredYaw = m_yaw;
        }

        void InitialComponents()
        {
            cameraZoom.Init(m_cam, cameraInputData);
            cameraSway.Init(m_cam.transform);
        }
        #endregion

        void CalculateRotation()
        {
            m_desiredYaw += cameraInputData.InputVector.x * sensitivity.x * Time.deltaTime;
            m_desiredPitch -= cameraInputData.InputVector.y * sensitivity.y * Time.deltaTime;

            m_desiredPitch = Mathf.Clamp(m_desiredPitch, lookAngleMinMax.x, lookAngleMinMax.y);
        }

        //If smoothing is low, rotates camera slower.
        void SmoothRotation()
        {
            m_yaw = Mathf.Lerp(m_yaw, m_desiredYaw, smoothAmount.x * Time.deltaTime);
            m_pitch = Mathf.Lerp(m_pitch, m_desiredPitch, smoothAmount.y * Time.deltaTime);
        }

        void ApplyRotation()
        {
            transform.eulerAngles = new Vector3(0f, m_yaw, 0f);
            m_pitchTranform.localEulerAngles = new Vector3(m_pitch, 0f, 0f + leaning.leanValue + wallRun.cameraTiltValue);
        }

        public void HandleSway(Vector3 _inputVector, float _rawXInput)
        {
            cameraSway.SwayPlayer(_inputVector, _rawXInput);
        }

        //Checks for zoom button press based on the input data.
        void HandleZoom()
        {
            if (cameraInputData.ZoomClicked || cameraInputData.ZoomReleased)
                cameraZoom.ChangeFOV(this);
        }

        public void ChangeRunFOV(bool _returning)
        {
            cameraZoom.ChangeRunFOV(_returning, this);
        }

        //Locks the cursor.
        void ChangeCursorState()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        #endregion
    }
}
